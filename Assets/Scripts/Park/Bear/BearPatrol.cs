using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Bear/Behavior")]
[TaskDescription("곰이 플레이어를 감지하지 못했을 때 순찰하며 이동합니다.")]
public class BearPatrol : Action
{
    [Tooltip("순찰 반경")]
    public SharedFloat patrolRadius = 5f; // 순찰 반경

    [Tooltip("목표 위치에 도달했다고 판단할 거리")]
    public SharedFloat stoppingDistance = 0.5f; // 목표 위치에 도달했다고 간주하는 거리

    [Tooltip("플레이어를 감지할 범위")]
    public SharedFloat detectionRange = 10f; // 플레이어 감지 범위

    [Tooltip("곰의 이동 속도")]
    public SharedFloat moveSpeed = 2f; // 곰의 이동 속도

    private NavMeshAgent navMeshAgent; // NavMeshAgent 컴포넌트
    private Animator animator; // Animator 컴포넌트
    private GameObject player; // 플레이어 오브젝트
    private Vector3 startPosition; // 곰의 시작 위치
    private Vector3 targetPosition; // 곰이 이동할 목표 위치
    private bool isWaiting = false; // 대기 상태 여부

    /// <summary>
    /// 태스크가 시작될 때 호출됩니다.
    /// </summary>
    public override void OnStart()
    {
        // NavMeshAgent와 Animator 초기화
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent가 없습니다. 순찰을 중단합니다.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator가 없습니다. 애니메이션 실행 불가.");
        }

        if (player == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다.");
        }

        // NavMeshAgent 초기 설정
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = stoppingDistance.Value;
        navMeshAgent.speed = moveSpeed.Value; // 곰의 이동 속도 설정

        // 시작 위치 저장 및 첫 번째 목표 설정
        startPosition = transform.position;
        SetRandomTargetPosition();
        SetWalkingAnimation(true);
        Debug.Log($"곰 순찰 시작 (속도: {moveSpeed.Value})");
    }

    /// <summary>
    /// 매 프레임 호출되어 순찰을 처리합니다.
    /// </summary>
    /// <returns>Task 상태 (Running 또는 Failure)</returns>
    public override TaskStatus OnUpdate()
    {
        if (player != null)
        {
            // 플레이어와의 거리 계산
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // 플레이어가 감지 범위 내에 있으면 순찰 종료
            if (distanceToPlayer <= detectionRange.Value)
            {
                Debug.Log("플레이어를 감지했습니다! 순찰 종료.");
                return TaskStatus.Failure; // Selector가 BearChase로 전환되도록 실패 반환
            }
        }

        // 대기 중일 경우 대기 유지
        if (isWaiting)
        {
            return TaskStatus.Running;
        }

        // NavMeshAgent가 이동 중인지 확인하여 애니메이션 상태 업데이트
        if (navMeshAgent.velocity.magnitude > 0.1f && !isWaiting)
        {
            SetWalkingAnimation(true); // 이동 중이면 걷기 애니메이션 활성화
        }
        else
        {
            SetWalkingAnimation(false); // 멈춰있으면 걷기 애니메이션 비활성화
        }

        // 목표 위치에 도달했는지 확인
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitAtPosition(1.0f)); // 1초 대기
            return TaskStatus.Running; // 대기 상태 유지
        }

        Debug.Log($"곰 순찰 중: 현재 위치 {transform.position}, 목표 위치 {targetPosition}");
        return TaskStatus.Running;
    }

    /// <summary>
    /// 무작위 목표 위치를 설정합니다.
    /// </summary>
    private void SetRandomTargetPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius.Value;
        Vector3 potentialTarget = startPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

        if (NavMesh.SamplePosition(potentialTarget, out NavMeshHit hit, patrolRadius.Value, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
            navMeshAgent.SetDestination(targetPosition);
            SetWalkingAnimation(true); // 새로운 목표 설정 시 걷기 애니메이션 활성화
            Debug.Log($"새로운 순찰 목표 설정: {targetPosition}");
        }
        else
        {
            Debug.LogWarning("유효한 목표 위치를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 걷기 애니메이션 상태를 설정합니다.
    /// </summary>
    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isRunning", false); // 걷기 시 달리기 해제
            Debug.Log($"걷기 애니메이션 상태 변경: {isWalking}");
        }
    }

    /// <summary>
    /// 목표 위치에 도달한 후 대기합니다.
    /// </summary>
    private System.Collections.IEnumerator WaitAtPosition(float waitTime)
    {
        isWaiting = true;
        SetWalkingAnimation(false); // 대기 중에는 걷기 애니메이션 비활성화
        navMeshAgent.isStopped = true;
        Debug.Log($"곰이 {waitTime}초 동안 대기합니다.");
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        navMeshAgent.isStopped = false;
        SetRandomTargetPosition();
    }
}
