using System.Collections.Generic;
using UnityEngine;

public class PerkManager : SingletonBehaviour<PerkManager>, IManager
{
    public List<PerkData> perkList = new List<PerkData>();
    public int perkPoints = 3; // 시작 포인트 (테스트용)

    public void Initialize()
    {
        int loadedPoints;
        perkList = DataManager.Instance.LoadPerkList(out loadedPoints);
        perkPoints = loadedPoints;

        // 저장된 데이터 기반으로 능력치 다시 적용
        foreach (var perk in perkList)
        {
            if (perk.unlocked)
            {
                Debug.Log($"[PerkManager] {perk.perkName} 적용 전 → Range: {PlayerStats.Instance.interactionRange}, Speed: {PlayerStats.Instance.runSpeed}");
                ApplyPerkEffect(perk);
                Debug.Log($"[PerkManager] {perk.perkName} 적용 후 → Range: {PlayerStats.Instance.interactionRange}, Speed: {PlayerStats.Instance.runSpeed}");
            }
        }

    }

    public bool UnlockPerk(string perkId)
    {
        var perk = perkList.Find(p => p.perkId == perkId);
        if (perk == null || perk.unlocked) return false;

        if (perkPoints < perk.cost)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(perk.prerequisite))
        {
            var prereq = perkList.Find(p => p.perkId == perk.prerequisite);
            if (prereq == null || !prereq.unlocked)
            {
                return false;
            }
        }

        perk.unlocked = true;
        perkPoints -= perk.cost;

        DataManager.Instance.SavePerkList(perkList, perkPoints);

        ApplyPerkEffect(perk);

        Debug.Log($"해금 완료: {perk.perkName}");
        return true;
    }

    private void ApplyPerkEffect(PerkData perk)
    {
        switch (perk.perkId)
        {
            case "Strength1":
                PlayerStats.Instance.interactionRange += 2f;
                break;
            case "Strength2":
                PlayerStats.Instance.interactionRange += 4f;
                break;
            case "Run1":
                PlayerStats.Instance.runSpeed += 2f;
                break;
            case "Run2":
                PlayerStats.Instance.runSpeed += 4f;
                break;
            case "Speech1":
                PlayerStats.Instance.speechChance += 0.1f;
                break;
            case "Speech2":
                PlayerStats.Instance.speechChance += 0.2f;
                break;
            case "Luck1":
                PlayerStats.Instance.findChance += 0.1f;
                break;
            case "Luck2":
                PlayerStats.Instance.rarePlantChance += 0.2f;
                break;
            case "Druid":
                PlayerStats.Instance.findChance += 0.3f;
                break;
        }

        PlayerStats.Instance.Save();
    }
}
