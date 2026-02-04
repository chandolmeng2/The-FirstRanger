using UnityEngine;

public class PlayerStats : SingletonBehaviour<PlayerStats>
{
    public float interactionRange;
    public float runSpeed;
    public float speechChance;
    public float findChance;
    public float rarePlantChance;

    public void ResetToDefault()
    {
        interactionRange = 3f;
        runSpeed = 5f;
        speechChance = 0f;
        findChance = 0f;
        rarePlantChance = 0f;
    }

    public void Save()
    {
        DataManager.Instance.SavePlayerStats(this);
    }

    public void Load()
    {
        var data = DataManager.Instance.LoadPlayerStats();

        interactionRange = data.interactionRange;
        runSpeed = data.runSpeed;
        speechChance = data.speechChance;
        findChance = data.findChance;
        rarePlantChance = data.rarePlantChance;

        Debug.Log($"�÷��̾� ���� �ҷ���: Range={interactionRange}, Speed={runSpeed}");
    }
}
