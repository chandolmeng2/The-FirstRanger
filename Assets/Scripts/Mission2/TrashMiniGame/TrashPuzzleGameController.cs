using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class TrashPuzzleGameController : MonoBehaviour
{
    public System.Action onGameEnd;

    public Button resetButton;
    public Button clearButton;
    public Button closeButton;

    public GameObject clearPanel;
    public GameObject trashMiniGamePanel;

    public GameObject Cleared;
    public GameObject noCleared;

    private CanvasGroup clearCanvasGroup;
    private List<BlockDragHandler> allBlocks = new List<BlockDragHandler>();
    private bool isCleared = false;

    [HideInInspector]
    public GameObject targetPuzzleItem;

    private void Awake()
    {
        Debug.Log("[TrashPuzzleGameController] Awake 호출됨");
        allBlocks = new List<BlockDragHandler>(
            trashMiniGamePanel.GetComponentsInChildren<BlockDragHandler>(true)
        );

        clearCanvasGroup = clearPanel.GetComponent<CanvasGroup>();

        clearPanel.SetActive(false);
        clearCanvasGroup.alpha = 0;
    }

    private void Start()
    {
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetAllBlocks);

        clearButton.onClick.RemoveAllListeners();
        clearButton.onClick.AddListener(CheckGameClear);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(CloseGame);
    }

    public void ResetAllBlocks()
    {
        if (isCleared) return;
        
        for (int i = allBlocks.Count - 1; i >= 0; i--)
        {
            if (allBlocks[i] == null)
            {
                allBlocks.RemoveAt(i);
                continue;
            }
            allBlocks[i].ResetToInitialState();
        }
        TrashPuzzleGrid.Instance.ResetGrid();

        SoundManager.Instance.Play(SoundKey.Mission2_UIClick_Button); // 리셋 효과음
    }

    public void CheckGameClear()
    {
        if (isCleared) return;

        foreach (var block in allBlocks)
        {
            if (!block.IsPlaced())
            {
                Debug.Log("아직 배치되지 않은 블록 있음");
                
                SoundManager.Instance.Play(SoundKey.Mission2_UIClick_Button_Fail); // 실패 효과음
                return;
            }
        }

        isCleared = true;
        ShowClearPanelWithTween();

        SoundManager.Instance.Play(SoundKey.Mission2_UIClick_Button); // 성공 효과음
    }

    private void ShowClearPanelWithTween()
    {
        clearPanel.SetActive(true);
        clearCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                onGameEnd?.Invoke(); // ← 이게 없으면 ActionController가 알 수 없음
                StartCoroutine(FadeAndClose());
            });

        resetButton.interactable = false;
        clearButton.interactable = false;
        closeButton.interactable = false;

        // ? 미션2 퍼즐 클리어 처리 추가
        var mission2 = FindObjectOfType<Mission2Manager>();
        if (mission2 != null)
        {
            mission2.ClearTrashPuzzle();
        }

        // ? 미션 패널 갱신
        if (MissionPanel.Instance != null)
        {
            MissionPanel.Instance.RefreshText();
        }

        Debug.Log($"[TrashPuzzleGameController] targetPuzzleItem: {(targetPuzzleItem == null ? "NULL" : targetPuzzleItem.name)}");
        if (targetPuzzleItem != null)
        {
            Debug.Log($"[TrashPuzzleGameController] Destroying {targetPuzzleItem.name}");
            Destroy(targetPuzzleItem);
        }
    }

    private IEnumerator FadeAndClose()
    {
        yield return FadeController.Instance.FadeIn(1.0f);

        trashMiniGamePanel.SetActive(false);

        var controller = FindObjectOfType<ActionController>();
        if (controller != null)
        {
            controller.ForceCancelPuzzle();
            controller.EndPuzzle();
        }
        if (isCleared)
        {
            noCleared.SetActive(false);
            Cleared.SetActive(true);
        }
        yield return new WaitForSeconds(0.3f);
        yield return FadeController.Instance.FadeOut(1.0f);
    }

    private void CloseGame()
    {
        if (isCleared) return;
        StartCoroutine(FadeAndClose());
    }
}
