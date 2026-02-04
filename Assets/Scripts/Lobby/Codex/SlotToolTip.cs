using UnityEngine;
using UnityEngine.UI;

public class SlotToolTip : MonoBehaviour
{
    [Header("툴팁 기본 UI")]
    [SerializeField] private GameObject go_Base;        // 툴팁 패널 (Base_Outer)
    [SerializeField] private Text txt_ItemName;         // 아이템 이름 텍스트
    [SerializeField] private Text txt_ItemDesc;         // 아이템 설명 텍스트
    [SerializeField] private Image iconImage;           // 아이템 이미지

    [Header("위치 조정")]
    [SerializeField] private float width = 0.1f;
    [SerializeField] private float height = 1f;

    /// <summary>
    /// 툴팁 보여주기 (아이템 데이터와 위치를 전달받음)
    /// </summary>
    public void ShowToolTip(ItemData _item, Vector3 _pos)
    {
        go_Base.SetActive(true);

        // 툴팁 위치 보정
        _pos += new Vector3(
            go_Base.GetComponent<RectTransform>().rect.width * width,
            -go_Base.GetComponent<RectTransform>().rect.height * height,
            0f);

        go_Base.transform.position = _pos;

        // 텍스트 채우기
        txt_ItemName.text = _item.itemName;
        txt_ItemDesc.text = _item.description;
        iconImage.sprite = _item.icon;          // ← 이미지 설정
        iconImage.enabled = (_item.icon != null); // ← 아이콘 없을 경우 안 보이게
    }

    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void HideToolTip()
    {
        go_Base.SetActive(false);
    }
}
