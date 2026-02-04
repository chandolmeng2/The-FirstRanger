using System.Collections.Generic;
using UnityEngine;

public class CriminalSpawnerManager : MonoBehaviour
{
    public static CriminalSpawnerManager Instance;

    [Header("스폰 가능한 범법자 프리팹")]
    public List<GameObject> criminalPrefabs;

    private HashSet<int> permanentlyRemoved = new HashSet<int>();  // 설득 성공 → 영구 제거
    private HashSet<int> currentlySpawned = new HashSet<int>();    // 현재 존재 중

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 소환 가능한 범법자를 무작위로 선택 (제외 조건 고려)
    /// </summary>
    public GameObject GetRandomAvailableCriminal()
    {
        List<int> available = new List<int>();
        for (int i = 0; i < criminalPrefabs.Count; i++)
        {
            if (permanentlyRemoved.Contains(i)) continue;
            if (currentlySpawned.Contains(i)) continue;
            available.Add(i);
        }

        if (available.Count == 0) return null;

        int chosen = available[Random.Range(0, available.Count)];
        currentlySpawned.Add(chosen);
        return criminalPrefabs[chosen];
    }

    public void RegisterSpawned(int prefabIndex)
    {
        currentlySpawned.Add(prefabIndex);
    }

    public void Unregister(GameObject instance)
    {
        IllegalNPC npc = instance.GetComponent<IllegalNPC>();
        if (npc != null)
        {
            int index = npc.prefabIndex;
            currentlySpawned.Remove(index);
        }
    }

    public void MarkAsPermanentlyRemoved(GameObject instance)
    {
        IllegalNPC npc = instance.GetComponent<IllegalNPC>();
        if (npc != null)
        {
            int index = npc.prefabIndex;
            permanentlyRemoved.Add(index);
            currentlySpawned.Remove(index);
        }
    }

    public void MarkAsRanAway(GameObject instance)
    {
        // 도망간 경우에는 다시 스폰 가능해야 하므로 currentlySpawned만 제거
        IllegalNPC npc = instance.GetComponent<IllegalNPC>();
        if (npc != null)
        {
            int index = npc.prefabIndex;
            currentlySpawned.Remove(index);
        }
    }
}