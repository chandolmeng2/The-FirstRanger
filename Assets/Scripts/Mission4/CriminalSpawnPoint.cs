using UnityEngine;

[ExecuteAlways]
public class CriminalSpawnPoint : MonoBehaviour
{
    [Header("플레이어 거리 기반 스폰 조건")]
    public float spawnDistanceThreshold = 10f;

    [Header("스폰 쿨타임 및 확률")]
    public float spawnCooldown = 10f;
    [Range(0f, 1f)] public float spawnChance = 0.3f;

    private GameObject player;
    private float cooldownTimer = 0f;
    private bool isCooling = false;
    private bool wasInRange = false;

    private GameObject currentSpawnedCriminal;
    private float lastLoggedTime = -1f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (player == null) return;

        // 현재 스폰된 NPC가 사라졌는지 체크 (도망, 설득 성공 등으로 삭제됨)
        if (currentSpawnedCriminal != null && !currentSpawnedCriminal.activeInHierarchy)
        {
            CriminalSpawnerManager.Instance.Unregister(currentSpawnedCriminal);
            Destroy(currentSpawnedCriminal);
            currentSpawnedCriminal = null;
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);
        bool isInRange = dist < spawnDistanceThreshold;

        // 플레이어가 범위 안으로 들어온 경우 → 쿨타임 초기화
        if (isInRange)
        {
            if (!wasInRange)
                Debug.Log($"[스폰 포인트: {name}] ▶ 플레이어 범위 재진입 → 쿨타임 초기화");

            cooldownTimer = 0f;
            isCooling = false;
            lastLoggedTime = -1f;
            wasInRange = true;
            return;
        }

        // 플레이어가 범위 밖으로 나간 경우 → 쿨타임 시작
        if (wasInRange)
        {
            Debug.Log($"[스폰 포인트: {name}] ◀ 플레이어 범위 벗어남 → 쿨타임 시작");
            isCooling = true;
            cooldownTimer = 0f;
            lastLoggedTime = -1f;
        }

        wasInRange = false;

        if (isCooling && currentSpawnedCriminal == null)
        {
            cooldownTimer += Time.deltaTime;
            float timeLeft = Mathf.Max(0f, spawnCooldown - cooldownTimer);

            if (Mathf.Floor(timeLeft) != lastLoggedTime)
            {
                lastLoggedTime = Mathf.Floor(timeLeft);
                Debug.Log($"[스폰 포인트: {name}] 남은 쿨타임: {lastLoggedTime}초");
            }

            if (cooldownTimer >= spawnCooldown)
            {
                cooldownTimer = 0f;
                TrySpawn();
            }
        }
    }

    private void TrySpawn()
    {
        if (currentSpawnedCriminal != null) return;

        float r = Random.value;
        if (r < spawnChance)
        {
            GameObject prefab = CriminalSpawnerManager.Instance.GetRandomAvailableCriminal();
            if (prefab != null)
            {
                int index = CriminalSpawnerManager.Instance.criminalPrefabs.IndexOf(prefab);
                GameObject spawned = Instantiate(prefab, transform.position, Quaternion.identity);

                var npc = spawned.GetComponent<IllegalNPC>();
                if (npc != null)
                    npc.prefabIndex = index;

                CriminalSpawnerManager.Instance.RegisterSpawned(index);
                currentSpawnedCriminal = spawned;

                Debug.Log($"[스폰 포인트: {name}] 범법자 스폰됨: {prefab.name}");
            }
            else
            {
                Debug.Log($"[스폰 포인트: {name}] 스폰 가능한 범법자가 없음!");
            }
        }
        else
        {
            Debug.Log($"[스폰 포인트: {name}] 확률 실패 → 다시 쿨타임 시작");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        Gizmos.DrawSphere(transform.position, 0.5f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, spawnDistanceThreshold);
    }
#endif
}

