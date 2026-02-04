using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Bear/Behavior")]
[TaskDescription("플레이어가 감지 범위 내에 있을 때 추적합니다.")]
public class BearChase : Action
{
    [Tooltip("추적 속도")]
    public SharedFloat chaseSpeed = 6f; // 추적 속도 (달리기 속도)

    [Tooltip("플레이어를 공격할 수 있는 범위")]
    public SharedFloat attackRange = 2f; // 공격 범위

    [Tooltip("추적을 중단할 거리")]
    public SharedFloat stopChaseDistance = 15f; // 추적 중단 거리

    private NavMeshAgent navMeshAgent; // NavMeshAgent 컴포넌트
    private Animator animator; // Animator 컴포넌트
    private GameObject player; // 플레이어 오브젝트

    /// <summary>
    /// 태스크가 시작될 때 호출됩니다.
    /// </summary>
    public override void OnStart()
    {
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent가 없습니다. 추적을 중단합니다.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator가 없습니다. 애니메이션 실행 불가.");
        }

        if (player == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다.");
            return;
        }

        // 추적 속도 설정
        navMeshAgent.speed = chaseSpeed.Value;
        navMeshAgent.isStopped = false;

        // 달리기 애니메이션 활성화
        SetRunningAnimation(true);
        Debug.Log("플레이어 추적 시작");
    }

    /// <summary>
    /// 매 프레임 호출되어 추적을 처리합니다.
    /// </summary>
    /// <returns>Task 상태 (Running 또는 Failure)</returns>
    public override TaskStatus OnUpdate()
    {
        if (player == null)
        {
            Debug.LogError("플레이어가 없습니다. 추적 실패.");
            SetRunningAnimation(false); // 달리기 애니메이션 비활성화
            return TaskStatus.Failure; // 플레이어가 없으면 순찰로 복귀
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 플레이어가 공격 범위 내에 있을 경우 추적 종료
        if (distanceToPlayer <= attackRange.Value)
        {
            Debug.Log("플레이어가 공격 범위 내에 있습니다. 추적 종료.");
            SetRunningAnimation(false); // 달리기 애니메이션 비활성화
            return TaskStatus.Failure; // Selector가 CanAttack으로 전환되도록 실패 반환
        }

        // 플레이어가 추적 중단 범위를 벗어난 경우 추적 중단
        if (distanceToPlayer > stopChaseDistance.Value)
        {
            Debug.Log("플레이어가 추적 중단 범위를 벗어났습니다. 추적 종료.");
            SetRunningAnimation(false); // 달리기 애니메이션 비활성화
            return TaskStatus.Failure; // 추적 중단
        }

        // 플레이어가 공격 범위 밖에 있지만 추적 범위 내에 있는 경우 계속 추적
        navMeshAgent.SetDestination(player.transform.position);

        // 애니메이션 상태를 안정적으로 유지
        UpdateAnimationState();

        Debug.Log($"플레이어 추적 중: 남은 거리 {distanceToPlayer}");
        return TaskStatus.Running; // 추적 상태 유지
    }

    /// <summary>
    /// 태스크가 종료될 때 호출됩니다.
    /// </summary>
    public override void OnEnd()
    {
        SetRunningAnimation(false); // 태스크 종료 시 달리기 애니메이션 비활성화
        navMeshAgent.isStopped = true;
        Debug.Log("플레이어 추적 종료");
    }

    /// <summary>
    /// 달리기 애니메이션 상태를 설정합니다.
    /// </summary>
    /// <param name="isRunning">달리기 상태 여부</param>
    private void SetRunningAnimation(bool isRunning)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isWalking", false); // 달리기 시 걷기 해제
            Debug.Log($"달리기 애니메이션 상태 변경: {isRunning}");
        }
    }

    /// <summary>
    /// 애니메이션 상태를 업데이트합니다.
    /// </summary>
    private void UpdateAnimationState()
    {
        // NavMeshAgent의 속도를 기반으로 애니메이션 상태를 갱신
        if (navMeshAgent.velocity.magnitude > 0.1f)
        {
            SetRunningAnimation(true); // 이동 중일 때 달리기 애니메이션 활성화
        }
        else
        {
            SetRunningAnimation(false); // 멈췄을 때 달리기 애니메이션 비활성화
        }
    }
}
