using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleUIManager : MonoBehaviour
{
    public static bool IsPuzzleActive { get; private set; } = false;
    public static bool IsLocked { get; private set; } = false;

    public List<GameObject> gamePanels;
    public GameObject crosshair;
    public GameObject clearImage;
    public GameObject closeButton;

    private GameObject targetPuzzleItem;

    private PlayerController playerController;


    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }

    // 퍼즐 켜기
    public void ShowPuzzle(int index)
    {
        StartCoroutine(FadeAndShow(index));
    }

    private IEnumerator FadeAndShow(int index)
    {
        SetPlayerControl(false); // 페이드인 전에 조작 비활성화

        yield return FadeController.Instance.FadeIn(1f); // 공통 페이드 인

        for (int i = 0; i < gamePanels.Count; i++)
            gamePanels[i].SetActive(i == index);

        // 슬라이딩 퍼즐이라면 시작 초기화 호출
        if (gamePanels[index].name.Contains("SlidingPuzzle")) // 또는 이름 확인 방식
        {
            SlidingManager sm = FindObjectOfType<SlidingManager>();
            sm?.ResetGame(); // 또는 StartPuzzle()처럼 새로 만든 함수 호출
        }

        IsPuzzleActive = true;

        if (crosshair != null)
            crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return new WaitForSeconds(0.2f);
        yield return FadeController.Instance.FadeOut(1f); // 공통 페이드 아웃
    }


    // 퍼즐 끄기 (X 버튼 또는 클리어 후 호출됨)
    // ? withFadeIn 옵션 추가
    public void HideAll(bool withFadeIn = false)
    {
        StartCoroutine(FadeAndHide(withFadeIn));
    }

    private IEnumerator FadeAndHide(bool withFadeIn)
    {
        if (withFadeIn)
            yield return FadeController.Instance.FadeIn(1f);

        foreach (var panel in gamePanels)
            panel.SetActive(false);

        IsPuzzleActive = false;

        if (crosshair != null)
            crosshair.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetPlayerControl(true); // 조작 다시 가능

        yield return FadeController.Instance.FadeOut(1f);
    }



    // 퍼즐 클리어 시 호출됨
    public void ShowClearAndAutoClose()
    {
        if (IsLocked) return;
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        IsLocked = true;

        if (clearImage != null)
            clearImage.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (clearImage != null)
            clearImage.SetActive(false);

        yield return FadeController.Instance.FadeIn(1f);

        if (targetPuzzleItem != null)
        {
            ItemObject itemObject = targetPuzzleItem.GetComponent<ItemObject>();
            if (itemObject != null &&
                (itemObject.itemData.itemType == ItemType.LinePuzzle ||
                 itemObject.itemData.itemType == ItemType.SlidingPuzzle))
            {
                int index = itemObject.itemData.puzzleIndex;

                var manager = FindObjectOfType<Mission2Manager>();
                manager?.ClearPuzzle(index);

                Destroy(targetPuzzleItem);
                targetPuzzleItem = null;
            }
        }

        // withFadeIn = false로 호출 (이미 위에서 FadeIn 했음)
        HideAll(withFadeIn: false);
        IsLocked = false;
    }

    public void SetTargetPuzzleItem(GameObject item)
    {
        targetPuzzleItem = item;
    }


}
