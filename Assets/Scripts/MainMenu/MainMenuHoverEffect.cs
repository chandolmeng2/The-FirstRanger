using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainMenuHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Targets")]
    public Image highlightBG;
    public Text buttonText;

    [Header("Settings")]
    public float highlightAlpha = 0.3f;
    public float scaleMultiplier = 1.2f;
    public float duration = 0.2f;

    Vector3 originalBGScale;
    Vector3 originalTextScale;

    Tween fadeTween;
    Tween bgScaleTween;
    Tween textScaleTween;

    void Start()
    {
        if (highlightBG != null)
        {
            originalBGScale = highlightBG.transform.localScale;
            highlightBG.gameObject.SetActive(false);
        }

        if (buttonText != null)
            originalTextScale = buttonText.transform.localScale;
    }

    /* ---------- POINTER ENTER ---------- */
    public void OnPointerEnter(PointerEventData eventData)
    {
        /* 1. 종료 또는 즉시 완료 */
        fadeTween?.Complete();
        bgScaleTween?.Kill();
        textScaleTween?.Kill();

        /* 2. 보이도록 준비 */
        highlightBG.gameObject.SetActive(true);

        /* 3. 배경 페이드 인 */
        fadeTween = highlightBG
            .DOFade(highlightAlpha, duration)
            .From(highlightBG.color.a);        // 현재 알파에서 목표까지

        /* 4. 배경 스케일 업 */
        bgScaleTween = highlightBG.transform
            .DOScale(originalBGScale * scaleMultiplier, duration)
            .SetEase(Ease.OutBack);

        /* 5. 텍스트 스케일 업 */
        textScaleTween = buttonText.transform
            .DOScale(originalTextScale * scaleMultiplier, duration)
            .SetEase(Ease.OutBack);
    }

    /* ---------- POINTER EXIT ---------- */
    public void OnPointerExit(PointerEventData eventData)
    {
        /* 1. 진행 중인 트윈 정리 */
        fadeTween?.Kill();
        bgScaleTween?.Kill();
        textScaleTween?.Kill();

        /* 2. 배경 페이드 아웃 + 비활성화 */
        fadeTween = highlightBG
            .DOFade(0f, duration)
            .OnComplete(() => highlightBG.gameObject.SetActive(false));

        /* 3. 배경·텍스트 스케일 다운 */
        bgScaleTween = highlightBG.transform
            .DOScale(originalBGScale, duration)
            .SetEase(Ease.InBack);

        textScaleTween = buttonText.transform
            .DOScale(originalTextScale, duration)
            .SetEase(Ease.InBack);
    }
}