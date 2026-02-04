using System.Collections.Generic;
using UnityEngine;

public class TrashPuzzleGrid : MonoBehaviour
{
    public static TrashPuzzleGrid Instance;

    public RectTransform gridArea;
    public RectTransform blockRoot;
    public Vector2 cellSize = new Vector2(100, 100);
    public int gridWidth = 8;
    public int gridHeight = 8;

    private bool[,] occupied;

    private void Awake()
    {
        Instance = this;
        occupied = new bool[gridWidth, gridHeight];
    }

    public bool TryPlaceBlock(List<Vector2Int> occupiedCells, RectTransform blockTransform, Vector2 localPoint)
    {
        Vector2Int gridOrigin = WorldToGrid(localPoint);

        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (Vector2Int offset in occupiedCells)
        {
            Vector2Int cellPos = gridOrigin + offset;
            if (!IsInsideGrid(cellPos) || occupied[cellPos.x, cellPos.y])
            {
                return false;
            }
            positions.Add(cellPos);
        }

        foreach (var pos in positions)
            occupied[pos.x, pos.y] = true;

        blockTransform.anchoredPosition = GridToWorld(gridOrigin);
        return true;
    }

    private Vector2Int WorldToGrid(Vector2 localPos)
    {
        float totalCellW = cellSize.x + 5f;
        float totalCellH = cellSize.y + 5f;

        float gridOriginX = -gridArea.rect.width / 2f;
        float gridOriginY = gridArea.rect.height / 2f;

        float xFromLeft = localPos.x - gridOriginX;
        float yFromTop = gridOriginY - localPos.y;

        int x = Mathf.FloorToInt(xFromLeft / totalCellW);
        int y = Mathf.FloorToInt(yFromTop / totalCellH);

        return new Vector2Int(x, y);
    }

    private Vector2 GridToWorld(Vector2Int gridPos)
    {
        float totalCellW = cellSize.x + 5f;
        float totalCellH = cellSize.y + 5f;

        float startX = -gridArea.rect.width / 2f;
        float startY = gridArea.rect.height / 2f;

        float x = startX + gridPos.x * totalCellW;
        float y = startY - gridPos.y * totalCellH;

        return new Vector2(x, y);
    }

    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < gridWidth && pos.y < gridHeight;
    }

    public void UpdateGhost(RectTransform ghostTransform, List<Vector2Int> occupiedCells, Vector2 localPoint)
    {
        Vector2Int gridOrigin = WorldToGrid(localPoint);

        foreach (Vector2Int offset in occupiedCells)
        {
            Vector2Int cell = gridOrigin + offset;
            if (!IsInsideGrid(cell) || occupied[cell.x, cell.y])
            {
                ghostTransform.gameObject.SetActive(false);
                return;
            }
        }

        ghostTransform.gameObject.SetActive(true);
        ghostTransform.anchoredPosition = GridToWorld(gridOrigin);
    }

    public void ResetGrid()
    {
        occupied = new bool[gridWidth, gridHeight];
    }
}