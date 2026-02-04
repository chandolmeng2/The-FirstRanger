using UnityEngine;

[System.Serializable]
public class PerkData
{
    public string perkId;        // 고유 ID (예: "Strength1")
    public string perkName;      // 이름 (예: "근력 I")
    [TextArea]
    public string description;   // 설명 (UI 표시용)
    public int cost = 1;         // 필요 포인트
    public string prerequisite;  // 선행 퍽 ID (없으면 "")

    [HideInInspector]
    public bool unlocked = false; // 해금 여부 (실제 게임 플레이 중 반영됨)
}
