using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("Bear/Conditions")]
[TaskDescription("플레이어가 감지 범위 내에 있는지 확인합니다.")]
public class CanChase : Conditional
{
    [Tooltip("플레이어를 감지할 수 있는 범위")]
    public SharedFloat detectionRange = 10f; // 감지 범위

    private GameObject player; // 플레이어 객체

    /// <summary>
    /// 태스크 시작 시 호출되는 함수입니다.
    /// </summary>
    public override void OnStart()
    {
        // 태그를 사용하여 플레이어 객체를 찾음
        player = GameObject.FindGameObjectWithTag("Player");

        // 플레이어 객체가 없을 경우 경고 메시지 출력
        if (player == null)
        {
            Debug.LogError("플레이어 객체를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 조건 평가 함수입니다.
    /// </summary>
    /// <returns>플레이어가 감지 범위 내에 있으면 Success, 아니면 Failure</returns>
    public override TaskStatus OnUpdate()
    {
        // 플레이어가 없는 경우 조건 실패
        if (player == null)
        {
            Debug.LogWarning("플레이어가 없습니다.");
            return TaskStatus.Failure;
        }

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 플레이어가 감지 범위 내에 있는지 확인
        if (distanceToPlayer <= detectionRange.Value)
        {
            Debug.Log("플레이어가 감지 범위 내에 있습니다.");
            return TaskStatus.Success; // 조건 충족
        }

        Debug.Log("플레이어가 감지 범위를 벗어났습니다.");
        return TaskStatus.Failure; // 조건 불충족
    }
}
