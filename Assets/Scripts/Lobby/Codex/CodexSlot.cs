using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CodexSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;                 // 등록된 도감 아이템
    public Image itemImage;              // 슬롯에 표시할 이미지

    [SerializeField]
    private SlotToolTip codexToolTip;    // 툴팁 UI (Inspector에서 연결)

    void Start()
    {
        if (codexToolTip == null)
            codexToolTip = FindObjectOfType<SlotToolTip>(); // 자동 연결
    }

    // 도감 아이템 등록
    public void AddItem(ItemData _item)
    {
        item = _item;
        itemImage.sprite = item.icon;
        itemImage.enabled = true;
        SetColor(1f); // 불투명하게 설정
    }

    // 이미지 투명도 설정
    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // 마우스를 슬롯에 올렸을 때 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && codexToolTip != null)
            codexToolTip.ShowToolTip(item, transform.position);
    }

    // 마우스가 벗어났을 때 툴팁 숨기기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (codexToolTip != null)
            codexToolTip.HideToolTip();
    }
}

