using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;  // DOTween 네임스페이스 추가
using UnityEngine.UI;  

public class HoverMissionPanelHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("마우스 오버 시 보여줄 UI 패널")]
    public GameObject panelToShow;

    [Header("Scale Animation Settings")]
    public float scaleUp = 1.1f;
    public float duration = 0.25f;


    [Header("클리어 표시 & 잠금 표시")]
    public GameObject checkd;           // 미션 완료 표시
    public GameObject locked;           // 잠긴 미션

    public GameObject checking;         // 미션 완료 동기화
    public GameObject locking;          // 잠긴 미션 동기화

    private Vector3 originalScale;
    [SerializeField] private GameObject continueMessage; // 패널 안의 '계속' 메시지 오브젝트


    private void Start()
    {
        if (panelToShow != null)
        {
            panelToShow.SetActive(false);
            originalScale = panelToShow.transform.localScale;
            panelToShow.transform.localScale = Vector3.zero; // 시작 시 완전히 작게

            var cg = panelToShow.GetComponent<CanvasGroup>();
            if (!cg) cg = panelToShow.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false; // 입력 통과
            cg.interactable = false; // 선택 불가

            checkd.SetActive(false);
            locked.SetActive(false);
            if (continueMessage) continueMessage.SetActive(false); // 기본 비활성
        }
    }


    private void Update()
    {
        GameObject change = panelToShow.transform.GetChild(0).gameObject;
        GameObject text = panelToShow.transform.GetChild(2).gameObject;

        Image image = change.GetComponent<Image>();
        image.color = gameObject.GetComponent<Image>().color;

        if (checking.activeInHierarchy)
        {
            checkd.SetActive(true);

        }
        else if (!checking.activeInHierarchy)
        {
            checkd.SetActive(false);
        }
        if (locking.activeInHierarchy)
        {
            locked.SetActive(true);
            text.SetActive(false);
        }
        else if (!locking.activeInHierarchy)
        {
            locked.SetActive(false);
            text.SetActive(true);
        }


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            panelToShow.transform.DOScale(originalScale * scaleUp, duration).SetEase(Ease.OutBack);

            bool isLocked = (locking && locking.activeInHierarchy) || (locked && locked.activeInHierarchy);
            if (continueMessage) continueMessage.SetActive(!isLocked);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (panelToShow != null)
        {
            // 작아지면서 사라짐
            panelToShow.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).OnComplete(() =>
            {
                panelToShow.SetActive(false);
                panelToShow.transform.localScale = originalScale; // 다시 원래 크기로 리셋
                if (continueMessage) continueMessage.SetActive(false); // 호버 끝나면 꺼두기

            });
        }
    }
}


