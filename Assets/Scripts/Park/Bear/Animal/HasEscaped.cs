using UnityEngine; // Unity 기본 기능 사용
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수 기능
using BehaviorDesigner.Runtime.Tasks; // 태스크 베이스 클래스
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // 툴팁 별칭 정의

// Behavior Designer에서 "Animal/Conditions"로 표시됨
[TaskCategory("Animal/Conditions")]
[TaskDescription("플레이어로부터 안전 거리 이상 멀어졌는지 확인합니다.")]
public class HasEscaped : Conditional
{
    [Tooltip("도망 성공 거리 (이 거리 이상 떨어지면 성공으로 판단)")]
    public SharedFloat safeDistance = 12f; // 안전 거리 기준

    private GameObject player; // 플레이어 오브젝트 참조

    // 태스크가 처음 실행될 때 호출됨
    public override void OnStart()
    {
        // 태그가 "Player"인 오브젝트를 찾아 저장
        player = GameObject.FindGameObjectWithTag("Player");

        // 플레이어를 찾지 못했을 경우 경고 출력
        if (player == null)
        {
            Debug.LogWarning("⚠️ 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    // 매 프레임마다 실행되는 메인 로직
    public override TaskStatus OnUpdate()
    {
        // 플레이어가 없으면 실패
        if (player == null)
        {
            return TaskStatus.Failure;
        }

        // 플레이어와 NPC 사이의 거리 계산
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // 디버그 출력
        Debug.Log($"[HasEscaped] 현재 거리: {distance}, 도망 기준 거리: {safeDistance.Value}");

        // 거리가 안전 거리 이상이면 성공
        if (distance >= safeDistance.Value)
        {
            return TaskStatus.Success;
        }

        // 아직 안전 거리에 도달하지 않았으면 실패
        return TaskStatus.Failure;
    }
}
