using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;
    public CanvasGroup fadePanel;

    public static bool IsFading { get; private set; } = false;

    void Awake()
    {
        Instance = this;

        // 항상 활성화된 상태 유지, 초기 알파는 0
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;  // UI 클릭 방지 해제
        }
    }
    public IEnumerator FadeIn(float duration)
    {
        IsFading = true;
        fadePanel.transform.SetAsLastSibling(); //항상 최상단으로
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = true;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            fadePanel.alpha = t / duration;
            yield return null;
        }
        fadePanel.alpha = 1f;
        IsFading = false;
    }


    public IEnumerator FadeOut(float duration)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
        fadePanel.interactable = false;

        IsFading = false;
    }
}

