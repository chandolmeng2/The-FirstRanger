using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Animal/Conditions")]
[TaskDescription("NavMeshAgent가 현재 목적지에 도착했는지 확인합니다.")]
public class HasReachedDestination : Conditional
{
    private NavMeshAgent agent; // 에이전트 캐시

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent 가져오기
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null)
        {
            Debug.LogWarning("⚠️ NavMeshAgent가 없습니다.");
            return TaskStatus.Failure;
        }

        // 목적지 도착 판정 (경로 계산 완료 + 거리가 거의 0)
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
        {
            Debug.Log("✅ 목적지 도착 확인");
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}
