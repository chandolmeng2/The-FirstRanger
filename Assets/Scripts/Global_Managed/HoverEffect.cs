using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour,
                                   IPointerEnterHandler, IPointerExitHandler
{
    /* ───────────── Inspector ───────────── */
    [Header("Targets")]
    public Image highlightBG;          // 반투명 하이라이트 배경
    public Text buttonText;           // 버튼 텍스트

    [Header("Settings")]
    public float highlightAlpha = 0.3f;
    public float scaleMultiplier = 1.2f;
    public float duration = 0.2f;

    /* ───────────── 내부 상태 ───────────── */
    private Vector3 originalBGScale;
    private Vector3 originalTextScale;

    private Tween fadeTween;
    private Tween bgScaleTween;
    private Tween textScaleTween;

    /* ───────────── 초기 설정 ───────────── */
    private void Start()
    {
        if (highlightBG != null)
        {
            originalBGScale = highlightBG.transform.localScale;
            highlightBG.gameObject.SetActive(false);
        }

        if (buttonText != null)
            originalTextScale = buttonText.transform.localScale;
    }

    /* ───────────── Hover 진입 ───────────── */
    public void OnPointerEnter(PointerEventData eventData)
    {
        fadeTween?.Complete();
        bgScaleTween?.Kill();
        textScaleTween?.Kill();

        highlightBG.gameObject.SetActive(true);

        fadeTween = highlightBG
            .DOFade(highlightAlpha, duration)
            .From(highlightBG.color.a);

        bgScaleTween = highlightBG.transform
            .DOScale(originalBGScale * scaleMultiplier, duration)
            .SetEase(Ease.OutBack);

        textScaleTween = buttonText.transform
            .DOScale(originalTextScale * scaleMultiplier, duration)
            .SetEase(Ease.OutBack);
    }

    /* ───────────── Hover 종료 ───────────── */
    public void OnPointerExit(PointerEventData eventData)
    {
        fadeTween?.Kill();
        bgScaleTween?.Kill();
        textScaleTween?.Kill();

        fadeTween = highlightBG
            .DOFade(0f, duration)
            .OnComplete(() => highlightBG.gameObject.SetActive(false));

        bgScaleTween = highlightBG.transform
            .DOScale(originalBGScale, duration)
            .SetEase(Ease.InBack);

        textScaleTween = buttonText.transform
            .DOScale(originalTextScale, duration)
            .SetEase(Ease.InBack);
    }

    /* ───────────── 버튼이 비활성화될 때 자동 초기화 ───────────── */
    private void OnDisable() => ResetVisual();

    /* ───────────── 시각 상태 초기화 ───────────── */
    private void ResetVisual()
    {
        fadeTween?.Kill();
        bgScaleTween?.Kill();
        textScaleTween?.Kill();

        if (highlightBG != null)
        {
            var c = highlightBG.color;
            highlightBG.color = new Color(c.r, c.g, c.b, 0f);   // 알파 0
            highlightBG.transform.localScale = originalBGScale;
            highlightBG.gameObject.SetActive(false);
        }

        if (buttonText != null)
            buttonText.transform.localScale = originalTextScale;
    }
}
