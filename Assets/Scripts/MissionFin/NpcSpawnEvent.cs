using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// 지정한 범위(원형/사각형) 안에서 랜덤 위치로 NPC를 생성하는 이벤트.
/// - TimedMissionController가 StartTime에 이 프리팹을 한 번 생성
/// - Begin() 시, totalCount 만큼 interval 간격으로 랜덤 스폰
/// - NavMesh 옵션을 켜면 "걸을 수 있는" 위치만 샘플링
public class NpcSpawnEvent : MissionEvent
{
    [Header("NPC")]
    public GameObject npcPrefab;               // 생성할 NPC 프리팹
    public Transform parentForSpawned;         // 생성된 NPC 묶을 부모(선택)

    [Header("Area (중심 기준)")]
    public Transform areaCenter;               // 기준점(비우면 이 이벤트 오브젝트의 Transform 사용)
    public bool useCircle = true;              // 원형(true) / 직사각형(false)
    public float radius = 10f;                 // 원형 반경
    public Vector2 rectSize = new Vector2(20f, 20f); // 직사각형 크기(x=폭, y=깊이)

    [Header("Placement Options")]
    public bool useNavMesh = true;             // NavMesh 위에만 배치
    public float navMeshMaxSampleDistance = 3f;
    public int navMeshSampleTries = 20;        // 한 개체당 샘플 재시도 횟수
    public bool alignToGround = true;          // NavMesh 미사용 시 지면에 Raycast로 Y 맞춤
    public LayerMask groundMask = ~0;
    public float groundRayStartHeight = 100f;
    public float groundRayLength = 300f;

    [Header("Timing")]
    public int totalCount = 5;                 // 총 몇 개 생성
    public float startDelay = 0f;              // 시작 지연(초)
    public float intervalBetweenSpawns = 0f;   // 생성 간 간격(초). 0이면 한 프레임에 몰아서 생성

    [Header("Lifecycle")]
    public bool resolveAfterSpawn = true;      // 전부 생성 끝나면 이벤트 완료 처리
    public bool destroySpawnedOnAbort = false; // 중단 시 생성 NPC 파괴할지

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Coroutine co;

    public override void Begin()
    {
        if (!npcPrefab)
        {
            Debug.LogWarning("[NpcSpawnEvent] npcPrefab이 비어 있습니다.");
            ResolveAndNotify();
            return;
        }
        co = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        var center = areaCenter ? areaCenter.position : transform.position;
        int count = Mathf.Max(1, totalCount);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = useCircle ? RandomInCircle(center, radius)
                                    : RandomInRect(center, rectSize);

            // NavMesh 우선
            if (useNavMesh)
            {
                bool ok = false;
                for (int t = 0; t < navMeshSampleTries; t++)
                {
                    Vector3 tryPos = useCircle ? RandomInCircle(center, radius)
                                               : RandomInRect(center, rectSize);
                    if (NavMesh.SamplePosition(tryPos, out var hit, navMeshMaxSampleDistance, NavMesh.AllAreas))
                    {
                        pos = hit.position;
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                {
                    // 실패하면 원래 pos 사용(옵션) — 필요 없다면 continue로 스킵 가능
                }
            }
            else if (alignToGround)
            {
                // 단순 지면 맞춤
                Vector3 origin = pos + Vector3.up * groundRayStartHeight;
                if (Physics.Raycast(origin, Vector3.down, out var hit, groundRayLength, groundMask))
                    pos.y = hit.point.y;
            }

            var go = Instantiate(npcPrefab, pos, Quaternion.identity,
                                 parentForSpawned ? parentForSpawned : null);
            spawned.Add(go);

            if (intervalBetweenSpawns > 0f && i < count - 1)
                yield return new WaitForSeconds(intervalBetweenSpawns);
        }

        if (resolveAfterSpawn) ResolveAndNotify();
    }

    Vector3 RandomInCircle(Vector3 center, float r)
    {
        var v = Random.insideUnitCircle * Mathf.Max(0f, r);
        return new Vector3(center.x + v.x, center.y, center.z + v.y);
    }

    Vector3 RandomInRect(Vector3 center, Vector2 size)
    {
        float x = Random.Range(-Mathf.Abs(size.x) * 0.5f, Mathf.Abs(size.x) * 0.5f);
        float z = Random.Range(-Mathf.Abs(size.y) * 0.5f, Mathf.Abs(size.y) * 0.5f);
        return new Vector3(center.x + x, center.y, center.z + z);
    }

    void ResolveAndNotify()
    {
        if (IsResolved) return;
        Resolve();                   // MissionEvent 베이스: _mission.EventResolved(this)
        Destroy(gameObject, 0.05f);  // 이벤트 컨트롤러 정리
    }

    public override void Abort()
    {
        if (co != null) StopCoroutine(co);

        if (destroySpawnedOnAbort)
        {
            foreach (var go in spawned) if (go) Destroy(go);
        }
        spawned.Clear();
        base.Abort();
    }

    // 디버그용: 씬 뷰에서 범위 가시화
    void OnDrawGizmosSelected()
    {
        var c = areaCenter ? areaCenter.position : transform.position;
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.2f);
        if (useCircle)
        {
            // 원형 그리기(수평면)
            const int seg = 32;
            Vector3 prev = c + new Vector3(radius, 0, 0);
            for (int i = 1; i <= seg; i++)
            {
                float ang = i * Mathf.PI * 2f / seg;
                Vector3 cur = c + new Vector3(Mathf.Cos(ang) * radius, 0, Mathf.Sin(ang) * radius);
                Gizmos.DrawLine(prev, cur);
                prev = cur;
            }
        }
        else
        {
            Vector3 half = new Vector3(rectSize.x * 0.5f, 0, rectSize.y * 0.5f);
            Vector3 a = c + new Vector3(-half.x, 0, -half.z);
            Vector3 b = c + new Vector3(half.x, 0, -half.z);
            Vector3 d = c + new Vector3(-half.x, 0, half.z);
            Vector3 e = c + new Vector3(half.x, 0, half.z);
            Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, e); Gizmos.DrawLine(e, d); Gizmos.DrawLine(d, a);
        }
    }
}
