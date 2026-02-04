using UnityEngine; // Unity 기본 기능 사용
using UnityEngine.AI; // NavMeshAgent 제어
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수 사용
using BehaviorDesigner.Runtime.Tasks; // Task 기반 행동 정의
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // 툴팁 속성 사용

// 카테고리 및 설명 정의
[TaskCategory("Animal/Behavior")]
[TaskDescription("지정 반경 내에서 순찰하며 플레이어가 가까이 오면 순찰을 중단합니다.")]
public class Patrol : Action
{
    [Tooltip("순찰 반경")]
    public SharedFloat patrolRadius = 5f; // 무작위 목표 위치 반경

    [Tooltip("목표 위치 도달 허용 거리")]
    public SharedFloat stoppingDistance = 0.5f; // 목적지 도착 판정 거리

    [Tooltip("플레이어 감지 거리")]
    public SharedFloat detectionRange = 10f; // 감지 범위

    [Tooltip("순찰 속도")]
    public SharedFloat moveSpeed = 2f; // 이동 속도

    private NavMeshAgent navMeshAgent; // 이동 컴포넌트
    private Animator animator; // 애니메이터
    private GameObject player; // 플레이어 오브젝트
    private Vector3 startPosition; // 시작 지점 저장
    private Vector3 targetPosition; // 이동 목표 지점
    private bool isWaiting = false; // 대기 중 여부

    public override void OnStart()
    {
        // 필수 컴포넌트 초기화
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        // 오류 방지 검사
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent가 없습니다.");
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("Player를 찾을 수 없습니다.");
        }

        // 에이전트 설정
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = stoppingDistance.Value;
        navMeshAgent.speed = moveSpeed.Value;

        // 시작 위치 저장
        startPosition = transform.position;

        // 목표 지점 설정
        SetRandomTargetPosition();

        // 애니메이션 설정
        SetWalkingAnimation(true); // 걷기 시작
        Debug.Log("순찰 시작");
    }

    public override TaskStatus OnUpdate()
    {
        // 플레이어가 있고, 감지 범위 안에 있는지 검사
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= detectionRange.Value)
            {
                Debug.Log("플레이어 감지됨 → 순찰 중단");
                SetWalkingAnimation(false); // 걷기 중지
                return TaskStatus.Failure; // 트리 전환
            }
        }

        // 대기 상태면 그대로 유지
        if (isWaiting)
        {
            return TaskStatus.Running;
        }

        // 에이전트가 이동 중이면 애니메이션 유지
        if (navMeshAgent.velocity.magnitude > 0.1f)
        {
            SetWalkingAnimation(true);
        }
        else
        {
            SetWalkingAnimation(false);
        }

        // 도착했으면 일정 시간 대기
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitAtPosition(1.0f));
            return TaskStatus.Running;
        }

        return TaskStatus.Running;
    }

    private void SetRandomTargetPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius.Value;
        Vector3 candidate = startPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

        // NavMesh 위 유효 위치 샘플
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolRadius.Value, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
            navMeshAgent.SetDestination(targetPosition);
            SetWalkingAnimation(true);
            Debug.Log("새 목표 지점: " + targetPosition);
        }
        else
        {
            Debug.LogWarning("유효한 순찰 위치 찾기 실패");
        }
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking); // 걷기 여부 설정
            animator.SetBool("isRunning", false);     // 도망 아님 명시
        }
    }

    private System.Collections.IEnumerator WaitAtPosition(float waitTime)
    {
        isWaiting = true;
        SetWalkingAnimation(false); // 대기 중엔 걷기 중지
        navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        navMeshAgent.isStopped = false;
        SetRandomTargetPosition(); // 다음 위치 설정
    }
}
