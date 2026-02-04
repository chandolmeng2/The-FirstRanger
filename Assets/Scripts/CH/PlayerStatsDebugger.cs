using UnityEngine;
using TMPro;

public class PlayerStatsDebugger : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    void Update()
    {
        if (debugText == null) return;

        string text = "";

        if (PerkManager.Instance != null)
        {
            text += $"특성 포인트: {PerkManager.Instance.perkPoints}\n";
        }
        else
        {
            text += "PerkManager 없음\n";
        }

        if (PlayerStats.Instance != null)
        {
            text += $"상호작용 거리: {PlayerStats.Instance.interactionRange} ";
            text += $"이동 속도: {PlayerStats.Instance.runSpeed}";
        }
        else
        {
            text += "PlayerStats 없음";
        }

        debugText.text = text;
    }
}
