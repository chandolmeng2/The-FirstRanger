using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BookCodex : MonoBehaviour
{
    public static BookCodex instance;
    public static bool codexActivated = false;

    private ActionController actionController;

    [Header("UI 연결")]
    public GameObject codexCanvas;               // 도감 전체 UI (SetActive용)
    public GameObject slotPrefab;                // CodexSlot 프리팹 (Hierarchy에서 사용 안 함)
    public Transform slotParent;                 // Grid Setting
    public Image detailImage;                    // 오른쪽 페이지 아이템 이미지
    public Text detailName;                      // 오른쪽 페이지 이름
    public Text detailDescription;               // 오른쪽 페이지 설명
    public GameObject crosshair;                 // 십자선
    public Text detailRarityText;                // 도감물 희귀도 텍스트

    [SerializeField] private ItemData[] allCodexItems;

    private CodexCategory currentCategory = CodexCategory.Nature;
    private HashSet<ItemData> discoveredItems = new HashSet<ItemData>();
    private bool isCodexOpen = false;

    //페이지 넘김 효과
    [SerializeField] private CanvasGroup leftPageGroup;
    [SerializeField] private CanvasGroup rightPageGroup;
    [SerializeField] private float pageTurnDuration = 0.3f;

    private ItemData newlyRegisteredItem = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !PuzzleUIManager.IsPuzzleActive && !QTEManager.IsQTEActive)
        {
            ToggleCodex();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // crosshair 재연결
        if (crosshair == null)
            crosshair = GameObject.Find("Crosshair");

        // actionController 재연결
        actionController = FindObjectOfType<ActionController>();

        codexActivated = false;
        isCodexOpen = false;
        codexCanvas.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        if (crosshair != null)
            crosshair.SetActive(true);
    }


    private void ToggleCodex()
    {
        isCodexOpen = !isCodexOpen;
        codexActivated = isCodexOpen; // 상태 동기화

        codexCanvas.SetActive(isCodexOpen);

        Cursor.lockState = isCodexOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isCodexOpen;

        if (crosshair != null)
            crosshair.SetActive(!isCodexOpen);

        if (isCodexOpen)
            RefreshPage();
    }


    public void OnClickTab(int categoryIndex)
    {
        if ((CodexCategory)categoryIndex == currentCategory)
            return;

        // 페이지 넘기기 연출
        StartCoroutine(PlayPageTurn((CodexCategory)categoryIndex));
    }

    private IEnumerator PlayPageTurn(CodexCategory newCategory)
    {
        // 페이드 아웃
        leftPageGroup.DOFade(0f, pageTurnDuration);
        rightPageGroup.DOFade(0f, pageTurnDuration);
        yield return new WaitForSeconds(pageTurnDuration);

        // 카테고리 전환
        currentCategory = newCategory;
        RefreshPage();

        // 페이드 인
        leftPageGroup.DOFade(1f, pageTurnDuration);
        rightPageGroup.DOFade(1f, pageTurnDuration);
    }



    public void RefreshPage()
    {
        BookSlot[] slots = slotParent.GetComponentsInChildren<BookSlot>();

        // 현재 카테고리의 아이템 추출
        List<ItemData> currentCategoryItems = new List<ItemData>();
        foreach (ItemData item in allCodexItems)
        {
            if (item.codexCategory == currentCategory)
                currentCategoryItems.Add(item);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            BookSlot slot = slots[i];

            if (i < currentCategoryItems.Count)
            {
                ItemData item = currentCategoryItems[i];
                bool isDiscovered = discoveredItems.Contains(item);
                slot.SetSlot(item, isDiscovered);

                // 신규 등록이라면 등장 애니메이션
                if (newlyRegisteredItem != null && item == newlyRegisteredItem)
                {
                    slot.AnimateRegisteredEffect(); // ? 빛나는 효과
                }

            }
            else
            {
                slots[i].SetEmptySlot(); // 미등록 또는 빈 슬롯
            }
        }

        ShowItemDetails(null);
    }

    public void ShowItemDetails(ItemData item)
    {
        if (item == null)
        {
            detailImage.enabled = false;
            detailName.text = "";
            detailDescription.text = "";
            detailRarityText.text = "";
            return;
        }

        detailImage.sprite = item.icon;
        detailImage.enabled = true;
        detailName.text = item.itemName;
        detailDescription.text = item.description;
        detailRarityText.text = GetRarityString(item.rarity);
    }

    private string GetRarityString(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Rare: return "<color=blue>희귀한</color>";
            case Rarity.Unique: return "<color=#DAA520>특별한</color>";
            default: return "일반";
        }
    }

    public void RegisterToCodex(ItemData item)
    {
        if (discoveredItems.Contains(item)) return;
        if (item.itemType != ItemType.Codex) return;

        discoveredItems.Add(item);
        newlyRegisteredItem = item; // ? 애니메이션 대상 설정
        Debug.Log("도감에 등록됨: " + item.itemName);
        StartCoroutine(MissionPanel.Instance.ShowExpPanel());
        ExpManager.Instance.AddExp(10);
    }    
}
