using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExpData
{
    public const int RankValue1 = 100;
    public const int RankValue2 = 200;
    public const int RankValue3 = 300;
}

public class ExpManager : SingletonBehaviour<ExpManager>, IManager
{
    public enum Rank
    {
        rank1,
        rank2,
        rank3,
    }

    private int _exp;
    public Action OnAddExp;
    public Action OnRankUp;

    // 이벤트 사용 예제
    //ExpManager.Instance.OnRankUp += OnRankUp;
    //private void OnRankUp()
    //{
    //    // ui 생성
    //}

    public int GetMaxExp()
    {
        switch (CurrentRank())
        {
            case Rank.rank1:
                return ExpData.RankValue1;
            case Rank.rank2:
                return ExpData.RankValue2 - ExpData.RankValue1;
            case Rank.rank3:
                return ExpData.RankValue3 - ExpData.RankValue2;
            default:
                return 0;
        }
    }

    public void Initialize()
    {
        _exp = DataManager.Instance.LoadExp();
    }

    public void AddExp(int value)
    {
        Debug.Log($"[EXP] 기존: {_exp}, 추가: {value}");
        var before = CurrentRank();
        _exp += value;
        var after = CurrentRank();
        Debug.Log($"[EXP] 저장될 최종 경험치: {_exp}");

        OnAddExp?.Invoke();
        if (before != after)
            OnRankUp?.Invoke();

        DataManager.Instance.SaveExp(_exp);
    }

    public int GetExp()
    {
        return _exp;
    }

    public int GetRankExp()
    {
        switch (CurrentRank())
        {
            case Rank.rank1:
                return _exp;
            case Rank.rank2:
                return _exp - ExpData.RankValue1;
            case Rank.rank3:
                return _exp - ExpData.RankValue2;
            default:
                return 0;
        }

        
    }

    public Rank GetRank()
    {
        return CurrentRank();
    }

    private Rank CurrentRank()
    {
        if (_exp < ExpData.RankValue1)
            return Rank.rank1;
        else if (_exp < ExpData.RankValue2)
            return Rank.rank2;
        else
            return Rank.rank3;  // RankValue2(200) 이상은 모두 최고 랭크
    }

}
