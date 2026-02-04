using UnityEngine;
using TMPro;
using DG.Tweening;

public class WarningUI : MonoBehaviour
{
    public static WarningUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string message, float duration = 3f)
    {
        messageText.text = message;
        panel.SetActive(true);

        // CanvasGroup으로 페이드 처리
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        DOTween.Kill(panel); // 중복 실행 방지
        cg.DOFade(0f, duration).OnComplete(() =>
        {
            panel.SetActive(false);
        }).SetId(panel);
    }
}
