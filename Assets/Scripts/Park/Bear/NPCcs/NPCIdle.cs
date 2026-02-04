using UnityEngine; // Unity 엔진의 기본 기능 사용
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수 사용
using BehaviorDesigner.Runtime.Tasks; // Behavior Designer 태스크 기반 클래스 사용
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 충돌 방지용 명시적 정의

// NPC가 Idle 상태일 때 수행하는 태스크
public class NPCIdle : Action
{
    [Tooltip("플레이어의 Transform")]
    public Transform player;

    [Tooltip("NPC 상단에 표시될 힌트 UI 오브젝트(월드 스페이스)")]
    public GameObject hintUI;

    [Tooltip("UI를 표시할 거리 (미터 단위)")]
    public float showRange = 3f;

    [Tooltip("NPC의 애니메이터 컴포넌트")]
    public Animator animator;

    [Header("애니메이션 재생 옵션")]
    [Tooltip("true면 Animator의 Default State(기본 상태)를 사용하고, false면 idleOverride를 사용합니다.")]
    public bool useAnimatorDefault = true;

    [Tooltip("기본 상태 대신 특정 상태로 강제 재생하고 싶을 때 사용 (비어있으면 무시)")]
    public SharedString idleOverride; // 예: "SitIdle"

    [Header("힌트 UI 바라보기 옵션")]
    [Tooltip("힌트 UI가 플레이어 대신 카메라를 바라보게 할지 여부")]
    public bool faceCamera = true;

    [Tooltip("수평(Yaw)만 맞출지 여부 (고개가 위/아래로 꺾이지 않게)")]
    public bool yawOnly = true;

    [Tooltip("힌트 UI 전방 보정(Y). 거꾸로 보이면 180으로")]
    public float uiFacingOffsetY = 180f;

    [Tooltip("E 키를 누르면 true로 설정되는 전역 변수")]
    public SharedBool isTalking;

    // 태스크 시작 시 1회 호출
    public override void OnStart()
    {
        if (hintUI != null) hintUI.SetActive(false);

        if (animator == null) return;

        if (useAnimatorDefault)
        {
            // 필요시 재생 보장:
            // animator.Play(0, 0, 0f);
            return;
        }

        if (string.IsNullOrEmpty(idleOverride.Value)) return;

        int baseLayer = 0;
        int stateHash = Animator.StringToHash(idleOverride.Value);
        if (animator.HasState(baseLayer, stateHash))
        {
            animator.Play(stateHash);
        }
        else
        {
            var npcName = gameObject != null ? gameObject.name : "NULL";
            Debug.LogWarning($"[NPCIdle] '{idleOverride.Value}' 상태를 찾지 못했습니다. 기본 상태를 사용합니다. (NPC={npcName})");
        }
    }

    // 매 프레임 호출
    public override TaskStatus OnUpdate()
    {
        if (player == null) return TaskStatus.Running;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= showRange)
        {
            if (hintUI != null && !hintUI.activeSelf) hintUI.SetActive(true);

            if (hintUI != null)
            {
                // ▶ 바라볼 대상 선택: 카메라 우선(설정에 따라)
                Vector3 targetPos;
                if (faceCamera && Camera.main != null) targetPos = Camera.main.transform.position;
                else targetPos = player.position;

                Vector3 dir = targetPos - hintUI.transform.position;
                if (yawOnly) dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    rot *= Quaternion.Euler(0f, uiFacingOffsetY, 0f); // 전방 보정(뒤집힘 방지)
                    hintUI.transform.rotation = rot;
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                isTalking.Value = true;
                if (hintUI != null) hintUI.SetActive(false);
                return TaskStatus.Failure; // 대화 시퀀스로 이동
            }
        }
        else
        {
            if (hintUI != null && hintUI.activeSelf) hintUI.SetActive(false);
        }

        return TaskStatus.Running;
    }
}
