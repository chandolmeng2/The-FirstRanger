using UnityEngine;                                                     // Unity 기본 엔진 네임스페이스 사용
using BehaviorDesigner.Runtime;                                        // Behavior Designer 전역/공유 변수 접근을 위해 사용
using BehaviorDesigner.Runtime.Tasks;                                  // Behavior Designer 태스크 기반 클래스를 위해 사용
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 이름 충돌 방지를 위한 별칭 지정

// 플레이어가 가까이 오면 말풍선 UI를 켜고, 멀어지면 끄는 단순 대기(Idle) 태스크
public class NPCIdle2 : Action                                         // Behavior Designer의 Action 태스크를 상속
{
    [Tooltip("플레이어 Transform (SharedTransform)")]                  // 인스펙터에서 설명 표시
    public SharedTransform player;                                     // 플레이어 Transform을 공유 변수로 받음

    [Tooltip("말풍선 UI 오브젝트 (SharedGameObject)")]                 // 인스펙터에서 설명 표시
    public SharedGameObject speechBubbleUI;                            // 켜고/끄기 할 말풍선 UI 오브젝트

    [Tooltip("플레이어 감지 거리 (이 거리 이하일 때 UI 표시)")]          // 인스펙터에서 설명 표시
    public float detectionRange = 5f;                                  // UI 표시 임계 거리 값

    [Tooltip("말풍선 UI의 월드 좌표 오프셋 (NPC 기준)")]                // 인스펙터에서 설명 표시
    public Vector3 uiOffset = new Vector3(0f, 2f, 0f);                 // 말풍선이 머리 위에 뜨도록 하는 위치 보정 값

    [Tooltip("카메라를 향해 말풍선이 항상 바라보도록 빌보드 처리할지 여부")] // 인스펙터에서 설명 표시
    public bool billboardToCamera = true;                              // 카메라 빌보드 기능 on/off 스위치

    [Tooltip("Idle 애니메이션을 재생할 Animator (선택)")]               // 인스펙터에서 설명 표시
    public Animator animator;                                          // 선택적으로 연결할 Animator 컴포넌트

    [Tooltip("진입 시 재생할 Idle 스테이트 이름 (비워두면 아무 것도 안 함)")] // 인스펙터에서 설명 표시
    public string idleStateName = "NPC_Idle";                          // 애니메이션 레이어의 Idle 상태 이름

    private Transform npcTransform;                                     // 매 프레임 transform 접근 비용을 줄이기 위한 캐시
    private Camera mainCam;                                             // 메인 카메라 캐시 (빌보드용)

    public override void OnStart()                                      // 태스크가 시작될 때 한 번 호출되는 콜백
    {
        npcTransform = transform;                                       // 자신(NPC)의 Transform을 캐싱
        mainCam = Camera.main;                                          // 메인 카메라 참조를 캐싱 (null일 수도 있음)

        if (speechBubbleUI != null && speechBubbleUI.Value != null)     // 말풍선 UI 공유 변수와 실제 오브젝트가 존재하는지 확인
        {
            speechBubbleUI.Value.SetActive(false);                      // 시작 시에는 기본적으로 말풍선을 꺼둠
        }

        if (animator != null && !string.IsNullOrEmpty(idleStateName))   // Animator가 있고 Idle 스테이트 이름이 지정되어 있다면
        {
            animator.CrossFadeInFixedTime(idleStateName, 0.1f);         // 부드럽게 Idle 스테이트로 전환 (트리거 반복 방지)
        }
    }

    public override TaskStatus OnUpdate()                               // 매 프레임마다 호출되는 콜백
    {
        if (npcTransform == null)                                       // NPC Transform이 유효하지 않다면
            return TaskStatus.Running;                                  // 특별히 할 일 없이 러닝 유지

        if (player == null || player.Value == null)                     // 플레이어 참조가 비어 있으면
        {
            HideBubble();                                               // 말풍선을 숨기고
            return TaskStatus.Running;                                  // 계속 대기 상태 유지
        }

        float distance = Vector3.Distance(                              // NPC와 플레이어 간 거리를 계산
            npcTransform.position,                                      // NPC 위치
            player.Value.position                                       // 플레이어 위치
        );                                                              // 계산 끝

        if (distance <= detectionRange)                                 // 거리가 감지 범위 이하면
        {
            ShowBubble();                                               // 말풍선을 보이게 하고
            UpdateBubbleTransform();                                    // 말풍선 위치/방향을 갱신
        }
        else                                                            // 감지 범위를 벗어나면
        {
            HideBubble();                                               // 말풍선을 숨김
        }

        return TaskStatus.Running;                                      // 이 태스크는 계속 실행되므로 항상 Running 반환
    }

    public override void OnEnd()                                        // 태스크가 종료될 때 한 번 호출되는 콜백
    {
        HideBubble();                                                   // 종료 시 말풍선을 확실히 숨겨 깔끔하게 정리
    }

    private void ShowBubble()                                           // 말풍선 표시 함수
    {
        if (speechBubbleUI != null && speechBubbleUI.Value != null)     // 말풍선 오브젝트가 유효하면
        {
            if (!speechBubbleUI.Value.activeSelf)                       // 현재 비활성화 상태라면
                speechBubbleUI.Value.SetActive(true);                   // 활성화하여 보이게 함
        }
    }

    private void HideBubble()                                           // 말풍선 숨김 함수
    {
        if (speechBubbleUI != null && speechBubbleUI.Value != null)     // 말풍선 오브젝트가 유효하면
        {
            if (speechBubbleUI.Value.activeSelf)                        // 현재 활성화 상태라면
                speechBubbleUI.Value.SetActive(false);                  // 비활성화하여 숨김
        }
    }

    private void UpdateBubbleTransform()                                // 말풍선 위치/방향 갱신 함수
    {
        if (speechBubbleUI == null || speechBubbleUI.Value == null)     // 말풍선 오브젝트가 없으면
            return;                                                     // 더 할 일이 없으므로 리턴

        speechBubbleUI.Value.transform.position =                       // 말풍선의 월드 좌표를
            npcTransform.position + uiOffset;                           // NPC 위치에 오프셋을 더한 곳으로 설정

        if (billboardToCamera && mainCam != null)                       // 빌보드 옵션이 켜져 있고 카메라가 유효하면
        {
            speechBubbleUI.Value.transform.LookAt(mainCam.transform);   // 카메라를 바라보도록 회전
            speechBubbleUI.Value.transform.Rotate(0f, 180f, 0f);        // LookAt이 뒤집히는 경우를 보정하기 위해 Y축 180도 회전
        }
    }
}
