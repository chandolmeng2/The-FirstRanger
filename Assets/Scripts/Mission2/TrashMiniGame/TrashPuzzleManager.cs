using System.Collections.Generic;
using UnityEngine;

public class TrashPuzzleManager : MonoBehaviour
{
    public static TrashPuzzleManager Instance;

    [Header("Hierarchy에 미리 배치된 패널들")]
    public List<GameObject> trashMiniGamePanels;  // 예: TrashMiniGame0, TrashMiniGame1, ...

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 퍼즐 인덱스에 해당하는 GameObject 패널 반환
    /// </summary>
    public GameObject GetPanelByIndex(int index)
    {
        if (index >= 0 && index < trashMiniGamePanels.Count)
            return trashMiniGamePanels[index];
        return null;
    }

    /// <summary>
    /// 퍼즐 인덱스에 해당하는 TrashPuzzleGameController 반환
    /// </summary>
    public TrashPuzzleGameController GetControllerByIndex(int index)
    {
        if (index >= 0 && index < trashMiniGamePanels.Count)
        {
            var panel = trashMiniGamePanels[index];
            return panel.GetComponentInParent<TrashPuzzleGameController>();
        }

        return null;
    }

}
