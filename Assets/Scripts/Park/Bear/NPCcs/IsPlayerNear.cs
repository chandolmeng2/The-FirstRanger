using UnityEngine; // Unity 기본 기능
using BehaviorDesigner.Runtime; // BD 전역 변수 사용
using BehaviorDesigner.Runtime.Tasks; // BD 태스크 클래스
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 중복 방지용 지정

public class IsPlayerNear : Conditional
{
    public SharedTransform player; // 플레이어의 Transform (SharedTransform 형태로 받아옴)
    public SharedFloat detectionRange = 5f; // 감지 거리 설정 (기본값은 5미터)

    private Transform npcTransform; // 현재 NPC의 Transform 참조

    public override void OnStart()
    {
        // NPC 자신의 Transform 캐싱
        npcTransform = transform;
    }

    public override TaskStatus OnUpdate()
    {
        // 플레이어 또는 NPC Transform이 없는 경우 실패 반환
        if (player.Value == null || npcTransform == null)
        {
            return TaskStatus.Failure;
        }

        // NPC와 플레이어 사이의 거리 계산
        float distance = Vector3.Distance(npcTransform.position, player.Value.position);

        // 거리가 detectionRange 이내이면 성공 반환, 아니면 실패 반환
        if (distance <= detectionRange.Value)
        {
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
    }
}
