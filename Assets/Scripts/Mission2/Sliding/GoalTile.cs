using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GoalTile : MonoBehaviour
{
    private Image image;
    private Vector3 originalScale;

    void Start()
    {
        image = GetComponent<Image>();
        originalScale = transform.localScale;
    }

    public void PlayReachedEffect()
    {
        // 눌림 + 색 반짝임
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.5f);
        image.DOColor(Color.white, 0.2f).SetLoops(2, LoopType.Yoyo);
    }
}
