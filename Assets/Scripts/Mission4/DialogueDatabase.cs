using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IllegalIdleLineEntry
{
    public string npcId;
    public string[] idleLines; // 3초마다 랜덤으로 나올 말풍선 대사들
}

[System.Serializable]
public class DialogueTurn
{
    public string line1;
    public string line2;
    public string line3;
    public int correctIndex;

    public string npcReply1;
    public string npcReply2;
    public string npcReply3;
}

[System.Serializable]
public class DialogueEntry
{
    public string npcId;
    public List<DialogueTurn> turns = new List<DialogueTurn>();

    public string finalPlayerLine;
    public string finalNPCReplySuccess;
    public string finalNPCReplyFail;
}

[CreateAssetMenu(menuName = "Dialogue/Database")]
public class DialogueDatabase : ScriptableObject
{
    public List<DialogueEntry> entries;

    public List<IllegalIdleLineEntry> illegalIdleLines;

    // 대사 불러오기 함수
    public string[] GetIdleLines(string npcId)
    {
        foreach (var entry in illegalIdleLines)
        {
            if (entry.npcId == npcId)
                return entry.idleLines;
        }
        return null;
    }

    private DialogueEntry GetEntry(string npcId)
    {
        return entries.Find(e => e.npcId == npcId);
    }

    public DialogueTurn GetTurn(string npcId, int turnIndex)
    {
        var entry = GetEntry(npcId);
        if (entry == null || turnIndex < 0 || turnIndex >= entry.turns.Count) return null;
        return entry.turns[turnIndex];
    }

    public string GetFinalPlayerLine(string npcId)
    {
        return GetEntry(npcId)?.finalPlayerLine ?? "";
    }

    public string GetFinalNPCReplySuccess(string npcId)
    {
        return GetEntry(npcId)?.finalNPCReplySuccess ?? "";
    }

    public string GetFinalNPCReplyFail(string npcId)
    {
        return GetEntry(npcId)?.finalNPCReplyFail ?? "";
    }
}