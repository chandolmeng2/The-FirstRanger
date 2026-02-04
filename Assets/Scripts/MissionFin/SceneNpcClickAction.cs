using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNpcClickAction : MonoBehaviour
{
    [Header("Scene Restriction (옵션)")]
    [SerializeField] private bool restrictToScenes = true;
    [SerializeField] private List<string> allowedSceneNames = new List<string>(); // 비워두면 현재 씬 이름 자동 사용

    [Header("References")]
    [SerializeField] private Camera playerCamera;                 // 비워두면 Camera.main
    [SerializeField] private TimedMissionController mission;      // 비워두면 자동 탐색

    [Header("Click Action")]
    [SerializeField] private float clickRange = 20f;
    [SerializeField] private LayerMask npcClickMask = ~0;         // NPC 레이어 지정 권장
    [SerializeField] private float rewardWeight = 1f;             // 보상 가중치
    [SerializeField] private bool consumeNpc = true;              // 처리 후 대상 비활성화(중복 방지)

    private readonly HashSet<IllegalNPC> handled = new HashSet<IllegalNPC>();

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (mission == null) mission = FindObjectOfType<TimedMissionController>();

        if (restrictToScenes)
        {
            string cur = SceneManager.GetActiveScene().name;
            if (allowedSceneNames.Count == 0) allowedSceneNames.Add(cur); // 현재 씬만 허용
            enabled = allowedSceneNames.Contains(cur);
        }
    }

    void Update()
    {
        // 다른 UI/퍼즐 중에는 입력 무시 (ActionController와 동일한 가드)
        if (FadeController.IsFading ||
            PuzzleUIManager.IsPuzzleActive ||
            BookCodex.codexActivated ||
            CodexScanController.IsScanning ||
            MissionPanel.IsOpen)
            return;

        if (Input.GetMouseButtonDown(0))
            TryClick();
    }

    void TryClick()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, clickRange, npcClickMask))
            return;

        var npc = hit.collider.GetComponentInParent<IllegalNPC>();
        if (npc == null || !npc.isActive || handled.Contains(npc))
            return;

        handled.Add(npc);
        if (consumeNpc) npc.isActive = false; // 필요하면 SetActive(false) 등으로 교체 가능

        mission?.ReportEventOutcome(true, rewardWeight); // 저지율 바 상승
        // TODO: 이펙트/사운드 등 피드백을 원하면 여기서 추가
    }
}
