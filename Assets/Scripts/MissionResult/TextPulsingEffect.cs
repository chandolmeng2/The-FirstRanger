using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TextPulsingEffect : MonoBehaviour
{
    [SerializeField] private Text targetText1;
    [SerializeField] private TextMeshProUGUI targetText2;

    void Start()
    {
        // Text�� ������� �ݺ������� ����̰� ���� + GameObject �ı� �� �ڵ� Tween ����
        targetText1.DOFade(0.2f, 0.8f)
                  .SetLoops(-1, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine)
                  .SetLink(gameObject); // �� GameObject�� �ı��Ǹ� Tween�� �ڵ� ����

        targetText2.DOFade(0.2f, 0.8f)
                  .SetLoops(-1, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine)
                  .SetLink(gameObject); // �� GameObject�� �ı��Ǹ� Tween�� �ڵ� ����
    }
}
