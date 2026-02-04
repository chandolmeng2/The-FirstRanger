using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Bear/Conditions")]
[TaskDescription("플레이어가 공격 범위 내에 있는지 확인합니다.")]
public class CanAttack : Conditional
{
    [Tooltip("플레이어를 공격할 수 있는 범위")]
    public SharedFloat attackRange = 2f; // 공격 범위

    [Tooltip("추적 상태로 돌아가는 허용 범위")]
    public SharedFloat extraChaseRange = 2.5f; // 공격 상태에서 추적으로 돌아가는 허용 범위

    private GameObject player;

    public override void OnStart()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("플레이어 객체를 찾을 수 없습니다!");
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (player == null)
        {
            return TaskStatus.Failure;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 공격 범위 내에 있으면 Success
        if (distanceToPlayer <= attackRange.Value)
        {
            Debug.Log("플레이어가 공격 범위 내에 있습니다.");
            return TaskStatus.Success;
        }

        // 플레이어가 허용 범위를 벗어나면 Failure
        if (distanceToPlayer > extraChaseRange.Value)
        {
            Debug.Log("플레이어가 허용 범위를 벗어났습니다. 추적으로 전환.");
            return TaskStatus.Failure;
        }

        return TaskStatus.Failure;
    }
}
