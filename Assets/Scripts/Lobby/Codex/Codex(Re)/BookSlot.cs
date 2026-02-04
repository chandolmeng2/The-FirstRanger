using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BookSlot : MonoBehaviour
{
    public ItemData item;
    public Image iconImage;
    public Button slotButton;

    public Sprite unknownIcon; // ← Inspector에서 ? 이미지 등록

    public GameObject glowEffectRare;
    public GameObject glowEffectUnique;

    public void SetSlot(ItemData data, bool isDiscovered)
    {
        item = data;

        if (isDiscovered)
        {
            iconImage.sprite = item.icon;
            slotButton.interactable = true;

            glowEffectRare.SetActive(item.rarity == Rarity.Rare);
            glowEffectUnique.SetActive(item.rarity == Rarity.Unique);
        }
        else
        {
            iconImage.sprite = unknownIcon;
            slotButton.interactable = false;
            glowEffectRare.SetActive(false);
            glowEffectUnique.SetActive(false);
        }

        iconImage.enabled = true;
    }

    public void OnClickSlot()
    {
        if (item != null && slotButton.interactable)
            BookCodex.instance.ShowItemDetails(item);
    }

    public void SetEmptySlot()
    {
        item = null;
        iconImage.sprite = unknownIcon;
        slotButton.interactable = false;
        iconImage.enabled = true;

        if (glowEffectRare != null) glowEffectRare.SetActive(false);
        if (glowEffectUnique != null) glowEffectUnique.SetActive(false);
    }

    public void AnimateRegisteredEffect()
    {
        // 1. 팡 하고 튀는 효과 (살짝 스케일 업 & 복원)
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.5f);

        // 2. 반짝이는 효과 (알파값 번쩍)
        iconImage.DOFade(1f, 0.15f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

}


