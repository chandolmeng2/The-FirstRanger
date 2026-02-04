using UnityEngine; // Unity 기본 기능
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수 시스템
using BehaviorDesigner.Runtime.Tasks; // 태스크 관련 기능
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 어트리뷰트 별칭 지정

// Behavior Designer에서 "Animal/Conditions" 카테고리로 표시되도록 지정
[TaskCategory("Animal/Conditions")]
[TaskDescription("플레이어가 지정된 거리 이내에 있는지 감지합니다.")]
public class IsPlayerNear2 : Conditional
{
    [Tooltip("플레이어를 감지할 거리입니다. 이 거리 이내에 있으면 감지됩니다.")]
    public SharedFloat detectionRange = 3f; // 플레이어 감지 거리 (기본값 5m)

    private GameObject player; // 플레이어 오브젝트를 저장할 변수

    // 태스크 시작 시 호출되는 함수
    public override void OnStart()
    {
        // 태그가 "Player"인 게임 오브젝트를 찾아 player 변수에 저장
        player = GameObject.FindGameObjectWithTag("Player");

        // 플레이어를 찾지 못한 경우 콘솔에 경고 출력
        if (player == null)
        {
            Debug.LogWarning("⚠️ 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }

    // 매 프레임마다 호출되어 조건 검사
    public override TaskStatus OnUpdate()
    {
        // 플레이어가 존재하지 않으면 실패 반환
        if (player == null)
        {
            return TaskStatus.Failure;
        }

        // 플레이어와 현재 객체 사이의 거리 계산
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // 거리 정보 디버그 출력
        Debug.Log($"[IsPlayerNear] 거리: {distance}, 감지범위: {detectionRange.Value}");

        // 감지 거리 이내에 있는 경우 성공 반환
        if (distance <= detectionRange.Value)
        {
            return TaskStatus.Success;
        }

        // 감지 거리 밖이면 실패 반환
        return TaskStatus.Failure;
    }
}
