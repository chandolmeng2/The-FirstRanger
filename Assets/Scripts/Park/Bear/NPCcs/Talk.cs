using UnityEngine; // 유니티 기본 네임스페이스
using UnityEngine.UI; // 버튼/이미지 등 UI 컴포넌트 사용
using TMPro; // TextMeshPro 사용
using System.Collections.Generic; // List 사용
using BehaviorDesigner.Runtime; // BD 전역변수
using BehaviorDesigner.Runtime.Tasks; // BD 태스크 기반
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 별칭(사용자 규칙)


public class Talk : Action // BD 액션 태스크
{
    [Tooltip("대화 UI 패널 전체(켜고/끄는 루트 패널)")]
    public GameObject dialoguePanel; // 패널 레퍼런스

    [Tooltip("대화 본문 텍스트(TextMeshProUGUI)")]
    public TextMeshProUGUI dialogueText; // 본문 텍스트

    [Tooltip("NPC 이름 텍스트(TextMeshProUGUI)")]
    public TextMeshProUGUI nameText; // 이름 텍스트

    [Tooltip("닫기/넘기기 버튼(클릭 시 Space와 동일 동작)")]
    public Button closeButton; // 버튼 레퍼런스

    [Tooltip("플레이어 Transform")]
    public Transform player; // 플레이어 위치

    [Tooltip("NPC Transform(루트)")]
    public Transform npc; // NPC 루트

    [Tooltip("회전시킬 실제 대상(비워두면 npc 사용)")]
    public Transform rotateTarget; // 회전 타깃

    [Tooltip("NPC 애니메이터")]
    public Animator animator; // 애니메이터

    [Tooltip("플레이어 조작 스크립트(대화 중 비활성)")]
    public MonoBehaviour playerControllerScript; // 이동/회전 스크립트

    [Tooltip("기본 카메라(필요 시 커서 락/해제만 처리)")]
    public Camera mainCamera; // 메인 카메라

    [Tooltip("전역 변수: 단일 문자열 대사. <next>로 페이지 구분 가능")]
    public SharedString sharedDialogueText; // 기존 단일 대사(호환용)

    [Tooltip("전역 변수: 이름")]
    public SharedString sharedNameText; // 이름

    [Tooltip("대화 시작 시 NPC가 플레이어를 바라보게 할지")]
    public bool facePlayerOnStart = true; // 시작 시 회전 여부

    [Tooltip("수평(Yaw)만 맞출지")]
    public bool yawOnly = true; // Yaw만

    [Tooltip("모델 전방 보정 각도 Y(등 보이면 180)")]
    public float facingOffsetY = 180f; // 전방 보정

    [Header("Talk 애니메이션 옵션")] // 헤더 표시
    [Tooltip("Talk 트리거 이름(있으면 우선 사용)")]
    public SharedString talkTrigger; // 트리거명

    [Tooltip("트리거가 없을 때 재생할 상태 이름")]
    public SharedString talkStateOverride; // 상태명

    [Tooltip("둘 다 없으면 기본 상태 유지 허용")]
    public bool allowAnimatorDefaultWhenMissing = true; // 기본허용

    [Header("대화 페이징 옵션")] // 페이징 섹션
    [Tooltip("대사 페이지 리스트(각 요소가 한 페이지). 비어있으면 sharedDialogueText를 <next>로 분할")]
    public List<string> dialoguePages = new List<string>(); // 페이지 리스트

    [Tooltip("페이지 구분 토큰(예: <next>)")]
    public string splitToken = "<next>"; // 구분자

    [Tooltip("이름/본문 기본 정렬 강제(TopLeft 권장)")]
    public bool forceTopLeftLayout = true; // 정렬강제

    [Tooltip("본문 여백(좌,상,우,하)")]
    public Vector4 bodyMargin = new Vector4(32, 8, 32, 24); // 여백

    private bool finished = false; // 대화 종료 플래그
    private int pageIndex = 0; // 현재 페이지 인덱스
    private Transform rot; // 회전 타깃 캐시

    public override void OnStart() // 태스크 시작 시
    {
        finished = false; // 종료 초기화
        pageIndex = 0; // 페이지 0부터
        rot = rotateTarget != null ? rotateTarget : npc; // 회전 타깃 결정

        // 패널 표시
        if (dialoguePanel) dialoguePanel.SetActive(true); // 패널 켜기

        // 이름 표시
        if (nameText)
        {
            if (forceTopLeftLayout) nameText.alignment = TextAlignmentOptions.TopLeft; // 정렬 강제
            nameText.text = sharedNameText.Value; // 이름 텍스트 세팅
        }

        // 페이지 소스 구성(리스트가 비었으면 sharedDialogueText에서 분할)
        if (dialoguePages == null || dialoguePages.Count == 0) // 리스트 비었으면
        {
            var raw = sharedDialogueText.Value ?? string.Empty; // 원본 문자열
            raw = raw.Replace("\\n", "\n").Replace("<br>", "\n"); // 줄바꿈 치환
            if (!string.IsNullOrEmpty(splitToken) && raw.Contains(splitToken)) // 토큰으로 분할
            {
                var parts = raw.Split(new string[] { splitToken }, System.StringSplitOptions.None); // 분할
                dialoguePages = new List<string>(parts); // 리스트로 저장
            }
            else
            {
                dialoguePages = new List<string> { raw }; // 한 페이지만 사용
            }
        }

        // 본문 초기 페이지 출력
        ApplyPage(); // 0페이지 적용

        // 버튼 이벤트: 클릭하면 Space와 동일하게 다음/닫기
        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
            closeButton.onClick.AddListener(AdvanceOrClose); // 새 리스너 등록
        }

        // 애니메이션 처리(트리거→상태→기본)
        if (animator)
        {
            bool played = false; // 재생여부
            if (!string.IsNullOrEmpty(talkTrigger.Value) &&
                HasParameter(animator, talkTrigger.Value, AnimatorControllerParameterType.Trigger)) // 트리거 존재
            {
                animator.SetTrigger(talkTrigger.Value); // 트리거 발사
                played = true; // 재생됨
            }
            else if (!string.IsNullOrEmpty(talkStateOverride.Value)) // 상태명 지정됨
            {
                int hash = Animator.StringToHash(talkStateOverride.Value); // 해시
                if (animator.HasState(0, hash)) // 상태 존재 여부
                {
                    animator.Play(hash); // 상태 재생
                    played = true; // 재생됨
                }
                else
                {
                    Debug.LogWarning($"[Talk] 상태 '{talkStateOverride.Value}'를 찾지 못했습니다. (NPC={gameObject.name})"); // 경고
                }
            }
            // played==false이고 allowAnimatorDefaultWhenMissing==true면 기본상태 유지 // 주석만
        }

        // 커서/컨트롤 잠금 변경
        Cursor.lockState = CursorLockMode.None; // 마우스 잠금 해제
        Cursor.visible = true; // 마우스 보이기
        if (mainCamera) mainCamera.enabled = true; // 카메라 활성(필요 시)
        if (playerControllerScript) playerControllerScript.enabled = false; // 조작 비활성

        // NPC가 플레이어 바라보게
        if (facePlayerOnStart && rot && player) // 옵션과 레퍼런스 확인
        {
            Vector3 dir = player.position - rot.position; // 방향 벡터
            if (yawOnly) dir.y = 0f; // 수평만
            if (dir.sqrMagnitude > 0.0001f) // 유효 방향이면
            {
                var target = Quaternion.LookRotation(dir.normalized, Vector3.up) // 정면 회전
                            * Quaternion.Euler(0f, facingOffsetY, 0f); // 보정 각도 적용
                rot.rotation = target; // 회전 반영
            }
        }
    }

    public override TaskStatus OnUpdate() // 매 프레임 업데이트
    {
        // Space를 누르면 다음 페이지 또는 닫기
        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스 다운 감지
        {
            AdvanceOrClose(); // 넘기기/닫기 처리
        }

        // 끝났다면 성공 반환하여 시퀀스 진행
        if (finished) // 종료 플래그 확인
        {
            return TaskStatus.Success; // 태스크 성공
        }

        return TaskStatus.Running; // 계속 실행
    }

    private void AdvanceOrClose() // 넘기기 또는 닫기 공통 처리
    {
        // 다음 페이지가 남아있다면 페이지 인덱스 증가
        if (pageIndex < dialoguePages.Count - 1) // 아직 마지막 전이라면
        {
            pageIndex++; // 다음 페이지로
            ApplyPage(); // 화면 반영
        }
        else // 마지막 페이지 이후면 닫기
        {
            CloseDialogue(); // 닫기 처리
        }
    }

    private void ApplyPage() // 현재 페이지 내용을 UI에 적용
    {
        if (!dialogueText) return; // 본문 레퍼런스 없으면 반환

        if (forceTopLeftLayout) // 정렬 강제 옵션
        {
            //dialogueText.alignment = TextAlignmentOptions.TopLeft; // 상단좌측 정렬
            dialogueText.enableWordWrapping = true; // 줄바꿈
            dialogueText.overflowMode = TextOverflowModes.Overflow; // 오버플로우 허용
            //dialogueText.margin = bodyMargin; // 여백 적용
        }

        // 페이지 텍스트 꺼내어 줄바꿈 토큰 치환
        string raw = dialoguePages[Mathf.Clamp(pageIndex, 0, dialoguePages.Count - 1)]; // 안전 인덱스
        raw = (raw ?? string.Empty).Replace("\\n", "\n").Replace("<br>", "\n"); // 줄바꿈 치환
        dialogueText.text = raw; // 본문 적용
    }

    private void CloseDialogue() // 대화 닫기
    {
        // 패널 끄기
        if (dialoguePanel) dialoguePanel.SetActive(false); // 패널 비활성

        // 커서/컨트롤 복구
        Cursor.lockState = CursorLockMode.Locked; // 마우스 잠금
        Cursor.visible = false; // 마우스 숨김
        if (mainCamera) mainCamera.enabled = true; // 카메라 유지
        if (playerControllerScript) playerControllerScript.enabled = true; // 조작 복구

        finished = true; // 종료 플래그 설정
    }

    private bool HasParameter(Animator anim, string param, AnimatorControllerParameterType type) // 파라미터 존재 확인
    {
        foreach (var p in anim.parameters) // 모든 파라미터 순회
            if (p.type == type && p.name == param) return true; // 일치하면 true
        return false; // 없으면 false
    }
}
