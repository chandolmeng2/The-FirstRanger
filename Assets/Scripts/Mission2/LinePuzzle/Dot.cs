using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Dot : MonoBehaviour
{
    public string colorType;
    public bool isStartDot = false;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetDot(string color, bool isStart)
    {
        colorType = color;
        isStartDot = isStart;
        image.color = GetColorByName(color);
    }

    /// <summary>
    /// 연결 성공 시 반짝이는 효과
    /// </summary>
    public void PlayGlowEffect()
    {
        image.color = GetColorByName(colorType);
        image.DOFade(0.2f, 0f); // 처음부터 alpha 0.3로 고정
        transform.localScale = Vector3.one * 0.1f; // 아주 작게 시작

        Sequence seq = DOTween.Sequence();

        // ?? 천천히 커짐 (0.5초)
        seq.Append(transform.DOScale(12f, 0.4f))

           // ?? 빠르게 줄어듦 (0.2초)
           .Append(transform.DOScale(0f, 0.1f))

           .OnComplete(() =>
           {
               transform.localScale = Vector3.one;
               image.color = GetColorByName(colorType);
               image.DOFade(1f, 0.05f); // 최종적으로 원래 밝기 복구
           });
    }





    private Color GetColorByName(string color)
    {
        return color switch
        {
            "Red" => Color.red,
            "Blue" => Color.blue,
            "Green" => Color.green,
            "Yellow" => Color.yellow,
            "Orange" => new Color(1f, 0.5f, 0f),
            "Cyan" => Color.cyan,
            _ => Color.white
        };
    }
}
