using System.Collections.Generic;
using UnityEngine;

public class ClickActionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;                 // 비워두면 Camera.main
    [SerializeField] private TimedMissionController mission;      // 비워두면 자동 탐색
    [SerializeField] private float clickRange = 20f;

    [Header("Layer Routing (레이어에 따라 분기)")]
    [SerializeField] private LayerMask npcMask;                   // 예: "NPC"
    [SerializeField] private LayerMask animalMask;                // 예: "Animal"
    [SerializeField] private LayerMask fireMask;                  // 예: "Fire"

    [Header("Rewards & Penalties (게이지 가중치)")]
    [SerializeField] private float npcRewardWeight = 1f;
    [SerializeField] private float animalRewardWeight = 1f;
    [SerializeField] private float fireRewardWeight = 1f;         // 100% 진압 시 보상
    [SerializeField] private float wrongClickPenalty = 0f;        // 잘못 클릭 시 패널티(원하면 사용)

    [Header("NPC Settings")]
    [SerializeField] private bool npcDisableAfter = true;         // 처리 후 비활성화
    [SerializeField] private float npcDisableDelay = 1.2f;
    [SerializeField] private string npcAnimTrigger = "Apprehend"; // 있으면 트리거
    [SerializeField] private bool npcPlayAudio = true;

    [Header("Animal Settings")]
    [SerializeField] private bool animalDestroyAfter = true;
    [SerializeField] private float animalDestroyDelay = 1f;
    [SerializeField] private string animalAnimTrigger = "Rescue";
    [SerializeField] private bool animalPlayAudio = true;

    [Header("Fire Settings")]
    [SerializeField] private float fireExtinguishPerClick = 0.25f; // 4번 클릭 = 완전 진압
    [SerializeField] private bool fireRequireExtinguisher = false; // 도구 필요할 때
    [SerializeField] private float noExtinguisherPenalty = 0f;     // 도구 없을 때 패널티

    // 내부 상태
    readonly HashSet<object> _handledOnce = new();                // 중복 처리 방지(NPC/동물 등)
    readonly Dictionary<Transform, float> _fireProgress = new();  // 화재별 진행도

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main;
        if (!mission) mission = FindObjectOfType<TimedMissionController>();
    }

    void Update()
    {
        // 기존 가드(입력 잠금 상황 방지) — 네 프로젝트 기준
        if (FadeController.IsFading ||
            PuzzleUIManager.IsPuzzleActive ||
            BookCodex.codexActivated ||
            CodexScanController.IsScanning ||
            MissionPanel.IsOpen)
            return;

        if (Input.GetMouseButtonDown(0)) ProcessClick();
    }

    void ProcessClick()
    {
        if (!playerCamera) return;

        var combined = npcMask | animalMask | fireMask;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, clickRange, combined))
        {
            if (wrongClickPenalty > 0f) mission?.ReportEventOutcome(false, wrongClickPenalty);
            return;
        }

        int layer = hit.collider.gameObject.layer;

        if (IsInMask(layer, npcMask)) HandleNpc(hit);
        else if (IsInMask(layer, animalMask)) HandleAnimal(hit);
        else if (IsInMask(layer, fireMask)) HandleFire(hit);
        else if (wrongClickPenalty > 0f) mission?.ReportEventOutcome(false, wrongClickPenalty);
    }

    bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    // ---------- NPC ----------
    void HandleNpc(RaycastHit hit)
    {
        var npc = hit.collider.GetComponentInParent<IllegalNPC>();
        if (!npc || !npc.isActive || _handledOnce.Contains(npc)) return;

        _handledOnce.Add(npc);
        npc.isActive = false; // 더 이상 단속 대상 아님

        // 연출(있으면)
        var anim = hit.collider.GetComponentInParent<Animator>();
        if (anim && !string.IsNullOrEmpty(npcAnimTrigger)) anim.SetTrigger(npcAnimTrigger);
        var audio = hit.collider.GetComponentInParent<AudioSource>();
        if (npcPlayAudio && audio) audio.Play();

        if (npcDisableAfter) StartCoroutine(DisableAfter(npc.gameObject, npcDisableDelay));
        mission?.ReportEventOutcome(true, npcRewardWeight);
    }

    // ---------- 동물 ----------
    void HandleAnimal(RaycastHit hit)
    {
        var root = hit.collider.GetComponentInParent<Transform>();
        if (!root || _handledOnce.Contains(root)) return;

        _handledOnce.Add(root);

        // 연출(있으면)
        var anim = root.GetComponentInChildren<Animator>();
        if (anim && !string.IsNullOrEmpty(animalAnimTrigger)) anim.SetTrigger(animalAnimTrigger);
        var audio = root.GetComponentInChildren<AudioSource>();
        if (animalPlayAudio && audio) audio.Play();

        if (animalDestroyAfter) StartCoroutine(DestroyAfter(root.gameObject, animalDestroyDelay));
        mission?.ReportEventOutcome(true, animalRewardWeight);
    }

    // ---------- 화재 ----------
    void HandleFire(RaycastHit hit)
    {
        if (fireRequireExtinguisher && !PlayerHasExtinguisher())
        {
            if (noExtinguisherPenalty > 0f) mission?.ReportEventOutcome(false, noExtinguisherPenalty);
            return;
        }

        // 화재 루트 찾기(파티클 루트 기준)
        Transform fireRoot = GetFireRoot(hit.collider.transform);
        if (!fireRoot) fireRoot = hit.collider.transform;

        float cur = _fireProgress.TryGetValue(fireRoot, out var v) ? v : 0f;
        cur = Mathf.Clamp01(cur + Mathf.Max(0.01f, fireExtinguishPerClick));
        _fireProgress[fireRoot] = cur;

        // 시각 피드백(파티클 약화/정지)
        var allFX = fireRoot.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in allFX)
        {
            var main = ps.main;
            main.simulationSpeed = Mathf.Lerp(0.4f, 1f, 1f - cur); // 대충 약해지는 느낌
            if (cur >= 1f) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 완전 진압 시에만 보상 (원하면 중간 보상으로 바꿔도 됨)
        if (cur >= 1f)
            mission?.ReportEventOutcome(true, fireRewardWeight);
    }

    Transform GetFireRoot(Transform t)
    {
        var ps = t.GetComponentInParent<ParticleSystem>();
        return ps ? ps.transform : null;
    }

    bool PlayerHasExtinguisher()
    {
        // TODO: 인벤토리 연동이 있다면 체크. 일단 true 반환.
        return true;
    }

    System.Collections.IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) go.SetActive(false);
    }

    System.Collections.IEnumerator DestroyAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go) Destroy(go);
    }
}
