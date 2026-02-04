using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;   // 인코딩 추가
using UnityEngine;

public class DataManager : SingletonBehaviour<DataManager>, IManager
{
    [System.Serializable]
    private class MissionDataWrapper
    {
        public List<MissionData> list;
    }

    [System.Serializable]
    public class ExpSaveData
    {
        public int totalExp;
    }

    [System.Serializable]
    public class PerkWrapper
    {
        public int perkPoints;
        public List<PerkData> perks;
    }

    [System.Serializable]
    public class PlayerStatsData
    {
        public float interactionRange;
        public float runSpeed;
        public float speechChance;
        public float findChance;
        public float rarePlantChance;
    }

    private string savePath;
    private string expPath;
    private string perkDataPath;
    private string playerStatsPath;

    public void Initialize()
    {
        savePath = Path.Combine(Application.persistentDataPath, "mission_save.json");
        expPath = Path.Combine(Application.persistentDataPath, "exp_save.json");
        perkDataPath = Path.Combine(Application.persistentDataPath, "perk_data.json");
        playerStatsPath = Path.Combine(Application.persistentDataPath, "player_stats.json");

        if (!File.Exists(savePath))
        {
            var defaultMissions = new Dictionary<int, MissionData>();

            // 예시: 1~4, 6번 미션
            for (int i = 1; i <= 4; i++)
                defaultMissions[i] = new MissionData { index = i, IsClear = false };
            defaultMissions[6] = new MissionData { index = 6, IsClear = false };

            SaveMissionData(defaultMissions);
        }

        if (!File.Exists(expPath)) SaveExp(0);
        if (!File.Exists(perkDataPath)) CreateDefaultPerkFile();
        if (!File.Exists(playerStatsPath)) CreateDefaultPlayerStatsFile();
    }

    public void DeleteAllSaveData()
    {
        try
        {
            if (File.Exists(savePath)) File.Delete(savePath);         // 미션
            if (File.Exists(expPath)) File.Delete(expPath);           // 경험치
            if (File.Exists(perkDataPath)) File.Delete(perkDataPath); // 퍼크
            if (File.Exists(playerStatsPath)) File.Delete(playerStatsPath); // 플레이어 스탯

            Debug.Log("모든 세이브 데이터 삭제 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"세이브 데이터 삭제 중 오류 발생: {e.Message}");
        }
    }


    #region 미션 저장/불러오기

    public void SaveMissionData(Dictionary<int, MissionData> missionDict)
    {
        List<MissionData> dataList = missionDict.Values.ToList();
        MissionDataWrapper wrapper = new MissionDataWrapper { list = dataList };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, json, Encoding.UTF8);   // UTF-8 강제
        Debug.Log($"미션 데이터 저장 완료: {savePath}");
    }

    public Dictionary<int, MissionData> LoadMissionData()
    {
        Dictionary<int, MissionData> result = new();

        if (!File.Exists(savePath))
        {
            Debug.Log("미션 저장 파일 없음 → 초기 상태 생성");

            for (int i = 1; i <= 4; i++)
                result[i] = new MissionData { index = i, IsClear = false };
            

            return result;
        }

        string json = File.ReadAllText(savePath, Encoding.UTF8);
        MissionDataWrapper wrapper = JsonUtility.FromJson<MissionDataWrapper>(json);

        foreach (var data in wrapper.list)
            result[data.index] = data;

        Debug.Log("미션 데이터 불러오기 완료");
        return result;
    }

    #endregion

    #region 경험치 저장/불러오기

    public void SaveExp(int exp)
    {
        ExpSaveData data = new ExpSaveData { totalExp = exp };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(expPath, json, Encoding.UTF8);   // UTF-8 강제
        Debug.Log($"경험치 저장 완료: {expPath}");
    }

    public int LoadExp()
    {
        if (!File.Exists(expPath))
        {
            Debug.Log("경험치 저장 파일 없음 → 0으로 시작");
            return 0;
        }

        string json = File.ReadAllText(expPath, Encoding.UTF8);
        ExpSaveData data = JsonUtility.FromJson<ExpSaveData>(json);
        Debug.Log($"경험치 불러오기 완료: {data.totalExp}");
        return data.totalExp;
    }

    #endregion

    #region 퍼크 저장/불러오기

    private void CreateDefaultPerkFile()
    {
        var defaultPerks = new List<PerkData>
        {
            new PerkData { perkId="Strength1", perkName="근력 I", description="상호작용 거리 +2 증가", cost=1, prerequisite="", unlocked=false },
            new PerkData { perkId="Strength2", perkName="근력 II", description="상호작용 거리 +4 증가", cost=1, prerequisite="Strength1", unlocked=false },
            new PerkData { perkId="Run1", perkName="달리기 I", description="달리기 속도 +2 증가", cost=1, prerequisite="", unlocked=false },
            new PerkData { perkId="Run2", perkName="달리기 II", description="달리기 속도 +4 증가", cost=1, prerequisite="Run1", unlocked=false },
            new PerkData { perkId="Speech1", perkName="화술 I", description="설득 확률 +10% 증가", cost=1, prerequisite="", unlocked=false },
            new PerkData { perkId="Speech2", perkName="화술 II", description="설득 확률 +20% 증가", cost=1, prerequisite="Speech1", unlocked=false },
            new PerkData { perkId="Luck1", perkName="행운 I", description="식물/NPC 찾기 확률 +10%", cost=1, prerequisite="", unlocked=false },
            new PerkData { perkId="Luck2", perkName="행운 II", description="희귀 식물 발견 확률 증가", cost=1, prerequisite="Luck1", unlocked=false },
            new PerkData { perkId="Druid", perkName="드루이드", description="식물 발견 확률 +30%", cost=1, prerequisite="Luck2", unlocked=false }
        };

        PerkWrapper wrapper = new PerkWrapper { perks = defaultPerks };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(perkDataPath, json, Encoding.UTF8);   // UTF-8 강제
        Debug.Log("기본 퍼크 파일 생성 완료");
    }

    public List<PerkData> LoadPerkList(out int perkPoints)
    {
        if (!File.Exists(perkDataPath))
        {
            Debug.Log("퍼크 데이터 없음 → 기본값으로 생성");
            CreateDefaultPerkFile();
        }

        string json = File.ReadAllText(perkDataPath, Encoding.UTF8);
        PerkWrapper wrapper = JsonUtility.FromJson<PerkWrapper>(json);

        perkPoints = wrapper.perkPoints; // 불러오기
        return wrapper.perks ?? new List<PerkData>();
    }

    public void SavePerkList(List<PerkData> perks, int perkPoints)
    {
        PerkWrapper wrapper = new PerkWrapper
        {
            perkPoints = perkPoints,
            perks = perks
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(perkDataPath, json, Encoding.UTF8);
        Debug.Log("퍼크 데이터 저장 완료");
    }


    #endregion

    #region 스탯 저장

    private void CreateDefaultPlayerStatsFile()
    {
        PlayerStatsData data = new PlayerStatsData
        {
            interactionRange = 3f,
            runSpeed = 5f,
            speechChance = 0f,
            findChance = 0f,
            rarePlantChance = 0f
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(playerStatsPath, json, Encoding.UTF8);
        Debug.Log("기본 플레이어 스탯 파일 생성 완료");
    }

    public void SavePlayerStats(PlayerStats stats)
    {
        PlayerStatsData data = new PlayerStatsData
        {
            interactionRange = stats.interactionRange,
            runSpeed = stats.runSpeed,
            speechChance = stats.speechChance,
            findChance = stats.findChance,
            rarePlantChance = stats.rarePlantChance
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(playerStatsPath, json, Encoding.UTF8);
        Debug.Log($"플레이어 스탯 저장 완료: {playerStatsPath}");
    }

    public PlayerStatsData LoadPlayerStats()
    {
        if (!File.Exists(playerStatsPath))
        {
            Debug.Log("플레이어 스탯 저장 파일 없음 → 기본값 사용");
            return null;
        }

        string json = File.ReadAllText(playerStatsPath, Encoding.UTF8);
        PlayerStatsData data = JsonUtility.FromJson<PlayerStatsData>(json);
        Debug.Log("플레이어 스탯 불러오기 완료");
        return data;
    }
    #endregion
}