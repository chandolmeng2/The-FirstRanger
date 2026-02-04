using UnityEngine;

public class IllegalNPC : MonoBehaviour
{
    public string npcId;                      // 고유 ID, 예: "npc_fire_001"
    public int prefabIndex;

    public bool isActive = true;              // 단속 가능 여부
    [HideInInspector] public bool hasSurrendered = false;
    [HideInInspector] public bool hasEscaped = false;
    public Animator animator;

    [Header("초기 설득 게이지 설정")]
    [Range(0f, 100f)]
    public float initialPersuadeGauge = 50f;

    [Header("말풍선 관련")]
    [SerializeField] private DialogueDatabase dialogueDatabase; // SO 연결용

    private SpeechBubbleController bubble;

    private void Start()
    {
        bubble = GetComponentInChildren<SpeechBubbleController>();

        if (dialogueDatabase != null)
        {
            var lines = dialogueDatabase.GetIdleLines(npcId);

            if (lines != null && lines.Length > 0)
            {
                bubble.StartLoopingSpeech(lines, 3f, 5f); // 3초마다 5초간 출력
            }
        }
        else
        {
            Debug.LogWarning($"[IllegalNPC] {name}에 DialogueDatabase가 연결되어 있지 않습니다.");
        }
    }
}
