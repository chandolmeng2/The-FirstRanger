using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Codex : MonoBehaviour
{
    public static Codex instance;

    public static bool codexActivated = false;

    [SerializeField]
    private GameObject go_CodexBase; // Codex_Base 패널

    [SerializeField]
    private GameObject go_SlotsParent; // Grid Setting

    private CodexSlot[] slots;

    [SerializeField]
    private GameObject crosshair; // Crosshair 오브젝트

    [SerializeField]
    private ItemData[] codexItems; // 등록 가능한 아이템들 (필수는 아님)
    private ActionController actionController;

    private HashSet<string> discoveredItemNames = new HashSet<string>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    void Start()
    {
        slots = go_SlotsParent.GetComponentsInChildren<CodexSlot>();
    }

    void Update()
    {
        TryOpenCodex();
    }

    private void TryOpenCodex()
    {
        // 퍼즐창이 열렸거나 애니메이션 중이면 도감 열지 않음
        if (PuzzleUIManager.IsPuzzleActive || CodexScanController.IsScanning)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            codexActivated = !codexActivated;

            if (codexActivated)
                OpenCodex();
            else
                CloseCodex();
        }
    }

    private void OpenCodex()
    {
        go_CodexBase.SetActive(true);

        // 마우스 보이기
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshair != null)
            crosshair.SetActive(false);
    }

    private void CloseCodex()
    {
        go_CodexBase.SetActive(false);

        // 마우스 잠그기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null)
            crosshair.SetActive(true);

        // 툴팁 강제 종료
        SlotToolTip toolTip = FindObjectOfType<SlotToolTip>();
        if (toolTip != null)
            toolTip.HideToolTip();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (crosshair == null)
        {
            crosshair = GameObject.Find("Crosshair");
        }

        // Mission1 씬에서만 있을 수 있으므로 이때 찾기
        actionController = FindObjectOfType<ActionController>();
    }

    // 아이템 등록 함수
    public void RegisterToCodex(ItemData _item)
    {
        if (discoveredItemNames.Contains(_item.itemName))
            return;

        if (_item.itemType != ItemType.Codex)
            return;

        discoveredItemNames.Add(_item.itemName);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                slots[i].AddItem(_item);
                return;
            }
        }
    }
}
