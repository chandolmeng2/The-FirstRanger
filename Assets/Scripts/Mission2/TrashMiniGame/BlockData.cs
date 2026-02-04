using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "TrashPuzzle/BlockData")]
public class BlockData : ScriptableObject
{
    public string blockName;
    public Vector2Int[] occupiedCells; // (0,0) 기준 상대 좌표로 셀 구성

    public List<Vector2Int> GetOffsets()
    {
        return new List<Vector2Int>(occupiedCells);
    }
}