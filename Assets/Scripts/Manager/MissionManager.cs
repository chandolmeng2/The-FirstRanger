using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IManager
{
    void Initialize();
}

public interface IMissionManager
{
    int Index { get; }
    int ExpValue { get; }
    bool GotoResult { get; }
    float CheckTime { get; }
    GameObject gameObject { get; }
}

public class MissionManager : SingletonBehaviour<MissionManager>, IManager
{
    
    private Dictionary<int, MissionData> datas = new();

    public void Initialize()
    {
        Load();
    }

    public bool IsMissionClear(int index)
    {
        if (datas != null && datas.ContainsKey(index))
            return datas[index].IsClear;

        return false;
    }

    private void Load()
    {
        var loadData = DataManager.Instance.LoadMissionData();
        datas = loadData;
    }
    
    /// <summary>
    /// �̼��� ������ �� ȣ�� -> ����
    /// </summary>
    public void OnMissionClear(int index)
    {
        if (datas.ContainsKey(index))
        {
            datas[index].IsClear = true;
        }
        else
        {
            datas.Add(index, new MissionData { index = index, IsClear = true });
        }

        DataManager.Instance.SaveMissionData(datas);
        Debug.Log($"미션 {index} 클리어 저장 완료");
    }
}
