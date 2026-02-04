using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPos; // (x, y) 위치
    public Dot dot; // 있으면 점 정보, 없으면 null
    public string usedByColor = null;

    public void Init(Vector2Int pos)
    {
        gridPos = pos;
        dot = GetComponentInChildren<Dot>();
    }

    public bool IsOccupiedByOther(string color)
    {
        return !string.IsNullOrEmpty(usedByColor) && usedByColor != color;
    }
}
