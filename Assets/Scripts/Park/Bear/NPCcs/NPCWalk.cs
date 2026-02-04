using UnityEngine; // Unity 기능
using BehaviorDesigner.Runtime; // BD 전역 변수
using BehaviorDesigner.Runtime.Tasks; // BD Task 사용
using System.Collections; // 코루틴용
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 이름 충돌 방지

public class NPCWalk : Action
{
    public enum MoveMode { Random, Waypoint } // 이동 모드: 랜덤 또는 Waypoint

    [Tooltip("이동 모드 선택")]
    public MoveMode moveMode = MoveMode.Random; // 기본값: 랜덤

    [Tooltip("플레이어 Transform")]
    public SharedTransform player;

    [Tooltip("이동 속도")]
    public float moveSpeed = 1f;

    [Tooltip("랜덤 이동 반경")]
    public float moveRadius = 5f;

    [Tooltip("Waypoint 리스트")]
    public SharedTransformList waypoints;

    [Tooltip("말풍선 UI 오브젝트")]
    public SharedGameObject speechBubbleUI;

    [Tooltip("플레이어 감지 거리")]
    public float detectionRange = 5f;

    [Tooltip("말풍선 UI 위치 오프셋")]
    public Vector3 uiOffset = new Vector3(0, 2.0f, 0);

    private Transform npcTransform; // NPC Transform
    private Vector3 targetPosition; // 이동 목표
    private int currentWaypointIndex = 0; // 현재 Waypoint 인덱스
    private bool isRespawning = false; // 리스폰 중 여부

    // 트리 시작 시 초기화
    public override void OnStart()
    {
        npcTransform = transform; // NPC의 Transform 저장
        SetNewTarget(); // 첫 타겟 설정
    }

    // 매 프레임 실행
    public override TaskStatus OnUpdate()
    {
        if (npcTransform == null || player.Value == null || isRespawning)
            return TaskStatus.Running;

        // 목표 위치로 이동
        float step = moveSpeed * Time.deltaTime;
        npcTransform.position = Vector3.MoveTowards(npcTransform.position, targetPosition, step);

        // 목표 지점 도달 시
        if (Vector3.Distance(npcTransform.position, targetPosition) < 0.2f)
        {
            // Waypoint 모드일 경우
            if (moveMode == MoveMode.Waypoint && waypoints != null && waypoints.Value.Count > 0)
            {
                // 마지막 Waypoint에 도달했을 때
                if (currentWaypointIndex == 0)
                {
                    // 리스폰 처리 시작
                    isRespawning = true;
                    BehaviorManager.instance.StartCoroutine(RespawnAtFirstPoint());
                    return TaskStatus.Running;
                }

                SetNewTarget(); // 다음 타겟 설정
            }
            else
            {
                SetNewTarget(); // 랜덤 모드일 경우
            }
        }

        // 플레이어 거리 체크 후 말풍선 UI 표시
        float distance = Vector3.Distance(npcTransform.position, player.Value.position);

        if (speechBubbleUI.Value != null)
        {
            if (distance <= detectionRange)
            {
                if (!speechBubbleUI.Value.activeSelf)
                    speechBubbleUI.Value.SetActive(true);

                speechBubbleUI.Value.transform.position = npcTransform.position + uiOffset;

                if (Camera.main != null)
                {
                    speechBubbleUI.Value.transform.LookAt(Camera.main.transform);
                    speechBubbleUI.Value.transform.Rotate(0, 180, 0);
                }
            }
            else
            {
                if (speechBubbleUI.Value.activeSelf)
                    speechBubbleUI.Value.SetActive(false);
            }
        }

        return TaskStatus.Running;
    }

    // 다음 타겟 위치 설정
    private void SetNewTarget()
    {
        if (moveMode == MoveMode.Random)
        {
            Vector2 randomPoint = Random.insideUnitCircle * moveRadius;
            targetPosition = new Vector3(randomPoint.x, npcTransform.position.y, randomPoint.y);
        }
        else if (moveMode == MoveMode.Waypoint && waypoints != null && waypoints.Value.Count > 0)
        {
            Transform target = waypoints.Value[currentWaypointIndex];
            targetPosition = new Vector3(target.position.x, npcTransform.position.y, target.position.z);
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Value.Count;
        }
    }

    // 마지막 도착 후 사라졌다가 처음 위치로 복귀하는 코루틴
    private IEnumerator RespawnAtFirstPoint()
    {
        // 말풍선도 끄기
        if (speechBubbleUI.Value != null)
            speechBubbleUI.Value.SetActive(false);

        // NPC 비활성화
        npcTransform.gameObject.SetActive(false);

        // 1초 대기
        yield return new WaitForSeconds(1f);

        // 위치 초기화
        npcTransform.position = waypoints.Value[0].position;

        // 인덱스 초기화
        currentWaypointIndex = 1; // 0은 지금 갔으니까 다음 순서부터

        // NPC 다시 활성화
        npcTransform.gameObject.SetActive(true);

        // 다음 타겟 지정
        SetNewTarget();

        isRespawning = false;
    }
}
