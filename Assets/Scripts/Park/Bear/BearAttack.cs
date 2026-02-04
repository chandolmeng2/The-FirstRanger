using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Bear/Behavior")]
[TaskDescription("플레이어가 공격 범위 내에 있을 때 곰이 한 번만 공격합니다.")]
public class BearAttack : Action
{
    [Tooltip("플레이어를 공격할 수 있는 범위")]
    public SharedFloat attackRange = 2f; // 공격 범위

    [Tooltip("공격 간 대기 시간")]
    public SharedFloat attackCooldown = 2f; // 공격 대기 시간

    private GameObject player; // 플레이어 오브젝트
    private Animator animator; // 애니메이터 컴포넌트
    private float lastAttackTime = -Mathf.Infinity; // 마지막 공격 시간 기록
    private bool isAttacking = false; // 공격 상태 플래그

    /// <summary>
    /// 태스크 시작 시 호출됩니다.
    /// </summary>
    public override void OnStart()
    {
        // 태그를 사용하여 플레이어 객체를 찾음
        player = GameObject.FindGameObjectWithTag("Player");
        animator = gameObject.GetComponent<Animator>();

        if (player == null)
        {
            Debug.LogError("플레이어 객체를 찾을 수 없습니다!");
        }

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 매 프레임 호출되는 함수입니다.
    /// </summary>
    /// <returns>Task 상태 (Success, Failure, 또는 Running)</returns>
    public override TaskStatus OnUpdate()
    {
        // 플레이어가 없는 경우 태스크 실패 반환
        if (player == null)
        {
            return TaskStatus.Failure;
        }

        // 곰과 플레이어 사이의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 공격 대기 시간이 지나지 않았다면 대기
        if (Time.time - lastAttackTime < attackCooldown.Value)
        {
            Debug.Log("공격 쿨다운 중...");
            return TaskStatus.Failure;
        }

        // 플레이어가 공격 범위 내에 있을 경우
        if (distanceToPlayer <= attackRange.Value && !isAttacking)
        {
            // 공격 실행
            Debug.Log("플레이어를 공격합니다!");
            lastAttackTime = Time.time; // 마지막 공격 시간 갱신
            isAttacking = true;

            // 공격 애니메이션 실행
            SetAttackingAnimation(true);

            return TaskStatus.Running; // 공격 중 상태 유지
        }

        // 플레이어가 공격 범위를 벗어난 경우
        Debug.Log("플레이어가 공격 범위를 벗어났습니다.");
        return TaskStatus.Failure; // 실패 반환
    }

    /// <summary>
    /// 공격 애니메이션 상태를 제어합니다.
    /// </summary>
    private void SetAttackingAnimation(bool isAttacking)
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", isAttacking);
            Debug.Log($"공격 애니메이션 상태 변경: {isAttacking}");
        }
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출되어 공격 로직을 실행합니다.
    /// </summary>
    public void PerformAttack()
    {
        Debug.Log("곰이 플레이어에게 피해를 입혔습니다!");
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출되어 공격 상태를 종료합니다.
    /// </summary>
    public void EndAttack()
    {
        isAttacking = false;
        SetAttackingAnimation(false); // 공격 애니메이션 종료
        Debug.Log("공격 종료");
    }

    /// <summary>
    /// 태스크 종료 시 호출됩니다.
    /// </summary>
    public override void OnEnd()
    {
        isAttacking = false;
        SetAttackingAnimation(false); // 공격 애니메이션 중지
        Debug.Log("공격 태스크 종료");
    }
}
