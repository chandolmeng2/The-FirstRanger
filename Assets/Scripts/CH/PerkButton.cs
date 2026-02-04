using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string perkId;
    public float holdTime = 1.5f;

    public Image iconImage;
    public Image progressBar; // ���û���

    [Header("Hover Panel Settings (���� �г�)")]
    public GameObject panelToShow;           // Hover �� ������ ���� �г�
    public TextMeshProUGUI perkNameText;     // ���� �г��� ��ũ �̸� �ؽ�Ʈ
    public TextMeshProUGUI perkDescText;     // ���� �г��� ��ũ ���� �ؽ�Ʈ
    public float scaleUp = 1.1f;
    public float duration = 0.25f;

    private static Vector3 originalScale;    // ���� �г��� ���� ũ��
    private static bool initialized = false; // �ʱ�ȭ �� ���� ����

    private float timer;
    private bool isHolding;
    private float progress = 0f;
    public float decaySpeed = 1f;

    void Start()
    {
        RefreshUI();

        // ���� �г� �ʱ�ȭ�� �� �� ����
        if (!initialized && panelToShow != null)
        {
            originalScale = panelToShow.transform.localScale;
            panelToShow.SetActive(false);
            panelToShow.transform.localScale = Vector3.zero;
            initialized = true;
        }
    }

    void Update()
    {
        if (PerkBoardInteraction.Instance == null || !PerkBoardInteraction.Instance.IsOpen)
            return;

        if (isHolding)
        {
            progress += Time.deltaTime / holdTime;
        }
        else
        {
            // 내려가기
            progress -= Time.deltaTime / holdTime * decaySpeed;
        }

        // 0~1 범위 클램프
        progress = Mathf.Clamp01(progress);

        // UI 적용
        if (progressBar != null)
            progressBar.fillAmount = progress;

        // 가득 차면 해금
        if (progress >= 1f)
        {
            isHolding = false;
            CompleteUnlock();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PerkBoardInteraction.Instance == null || !PerkBoardInteraction.Instance.IsOpen)
            return;

        var perk = PerkManager.Instance.perkList.Find(p => p.perkId == perkId);

        // 이미 해금된 경우
        if (perk != null && perk.unlocked) return;

        // 포인트 부족 → 아예 진행 안 함
        if (PerkManager.Instance.perkPoints < perk.cost)
        {
            WarningUI.Instance?.Show("포인트가 부족합니다!");
            return;
        }

        // 선행 스킬 필요하지만 해금 안 된 경우 → 아예 진행 안 함
        if (!string.IsNullOrEmpty(perk.prerequisite))
        {
            var prereq = PerkManager.Instance.perkList.Find(p => p.perkId == perk.prerequisite);
            if (prereq == null || !prereq.unlocked)
            {
                WarningUI.Instance?.Show("선행 스킬을 먼저 해금해야 합니다!");
                return;
            }
        }

        // 조건 만족 시에만 진행
        isHolding = true;
        timer = 0f;

        if (progressBar != null)
            progressBar.fillAmount = 0f;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        timer = 0f;

        if (progressBar != null)
            progressBar.fillAmount = 0f;

        RefreshUI();

    }

    private void CompleteUnlock()
    {
        if (PerkManager.Instance.UnlockPerk(perkId))
        {
            PlayerStats.Instance.Save();
            Debug.Log($"[PerkButton] {perkId} 해금됨");

            // 버튼 흔들기 모션
            transform.DOShakeScale(
                0.5f,              // duration (0.5초 동안 흔들림)
                strength: 0.2f,    // 크기 변화 강도
                vibrato: 10,       // 흔들림 횟수
                randomness: 90f,   // 랜덤 방향
                fadeOut: true      // 점점 줄어드는 효과
            );
        }

        RefreshUI();
    }


    private bool IsUnlocked()
    {
        var perk = PerkManager.Instance.perkList.Find(p => p.perkId == perkId);
        return perk != null && perk.unlocked;
    }

    // Hover ���� �� �г� ���� + ���� ����
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (panelToShow != null)
        {
            var perk = PerkManager.Instance.perkList.Find(p => p.perkId == perkId);
            if (perk != null)
            {
                if (perkNameText != null) perkNameText.text = perk.perkName;
                if (perkDescText != null) perkDescText.text = perk.description;
            }

            panelToShow.SetActive(true);
            panelToShow.transform.DOScale(originalScale * scaleUp, duration).SetEase(Ease.OutBack);
        }
    }

    // Hover ���� �� �г� �ݱ�
    public void OnPointerExit(PointerEventData eventData)
    {
        if (panelToShow != null)
        {
            panelToShow.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).OnComplete(() =>
            {
                panelToShow.SetActive(false);
                panelToShow.transform.localScale = originalScale;
            });
        }
    }
    private void RefreshUI()
    {
        if (IsUnlocked())
        {
            iconImage.color = Color.green; // �رݵ� ��ũ �� �ʷϻ�
            if (progressBar != null)
                progressBar.fillAmount = 1f;
        }
        else
        {
            iconImage.color = Color.gray; // ���� ��� ��ũ �� ȸ��
            if (progressBar != null)
                progressBar.fillAmount = 0f;
        }
    }

}
