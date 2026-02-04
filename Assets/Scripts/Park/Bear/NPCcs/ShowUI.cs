using UnityEngine; // Unity 기본 기능 사용
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수 사용
using BehaviorDesigner.Runtime.Tasks; // BD 태스크 클래스 사용
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 중복 방지용 별칭

public class ShowUI : Action
{
    [Tooltip("플레이어의 Transform")]
    public SharedTransform player; // 플레이어 Transform 참조

    [Tooltip("UI 오브젝트 (말풍선)")]
    public SharedGameObject speechBubbleUI; // NPC 머리 위 UI 오브젝트

    [Tooltip("UI 표시 감지 범위 (기본 5m)")]
    public SharedFloat detectionRange = 5f; // 감지 거리 설정

    [Tooltip("UI 위치 오프셋 (머리 위 높이 조정)")]
    public Vector3 uiOffset = new Vector3(0, 2.0f, 0); // UI가 NPC 머리 위로 뜨도록 위치 보정

    private Transform npcTransform; // NPC 자신의 Transform 저장

    // 태스크가 시작될 때 실행
    public override void OnStart()
    {
        npcTransform = transform; // NPC 자신의 Transform 캐싱
    }

    // 매 프레임마다 실행
    public override TaskStatus OnUpdate()
    {
        // 플레이어 또는 UI가 설정되지 않았다면 실패
        if (player.Value == null || speechBubbleUI.Value == null || npcTransform == null)
        {
            return TaskStatus.Failure;
        }

        // NPC와 플레이어 사이의 거리 계산
        float distance = Vector3.Distance(npcTransform.position, player.Value.position);

        // 거리가 detectionRange 이하 → UI 켜기
        if (distance <= detectionRange.Value)
        {
            // UI가 꺼져 있다면 켜기
            if (!speechBubbleUI.Value.activeSelf)
            {
                speechBubbleUI.Value.SetActive(true);
            }

            // UI를 NPC 머리 위 위치로 이동
            speechBubbleUI.Value.transform.position = npcTransform.position + uiOffset;

            // 카메라 방향으로 UI가 항상 보이도록 회전
            if (Camera.main != null)
            {
                speechBubbleUI.Value.transform.LookAt(Camera.main.transform);
                speechBubbleUI.Value.transform.Rotate(0, 180, 0); // UI가 뒤집히지 않도록 Y축 회전
            }
        }
        else
        {
            // 감지 범위를 벗어나면 UI 비활성화
            if (speechBubbleUI.Value.activeSelf)
            {
                speechBubbleUI.Value.SetActive(false);
            }
        }

        // 항상 Success 반환 → Selector가 다음 조건으로 넘어가지 않게 유지
        return TaskStatus.Success;
    }
}
