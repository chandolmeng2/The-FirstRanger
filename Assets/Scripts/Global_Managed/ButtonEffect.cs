using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Text buttonText; // 일반 UI 텍스트
    private TMP_Text tmpText; // TextMeshPro UI 텍스트
    private Color originalColor;
    private Color darkColor = new Color(0.5f, 0.5f, 0.5f); // 어두운 색 (회색)

    void Start()
    {
        // 일반 UI Text 컴포넌트 가져오기
        buttonText = GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }

        // TextMeshPro(TMP) UI 텍스트 가져오기
        tmpText = GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            originalColor = tmpText.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 시 텍스트 색상 어둡게 변경
        if (buttonText != null) buttonText.color = darkColor;
        if (tmpText != null) tmpText.color = darkColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 버튼에서 손을 떼면 원래 색상으로 복구
        if (buttonText != null) buttonText.color = originalColor;
        if (tmpText != null) tmpText.color = originalColor;
    }
}
