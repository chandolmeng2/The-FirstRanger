// GridManager.cs
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public GameObject robotPrefab;
    public GameObject obstaclePrefab;
    public GameObject wallPrefab;
    public GameObject goalPrefab;
    public GameObject arrowTilePrefab;
    public Transform gridParent;
    public SlidingManager slidingManager;

    public float cellSize = 80f;
    public Vector2 gridOrigin = Vector2.zero;
    public int width = 10;
    public int height = 5;

    public List<Vector2Int> goalPositions = new List<Vector2Int>();
    private string[,] mapData;
    private string[,] originalMapData;

    void Start()
    {
        SetMap(stage1);
    }
    //P==플레이어, W== 벽, E==이동공간(빈공간)
    //G==골인지점, O==장애물(한 번 지나가면 벽(W)으로 바뀜)
    //U,D,L,R == 위,아래,왼,오른쪽으로 강제이동
    //스테이지는 new string[행, 열]
    private string[,] stage1 = new string[6, 6] {
        { "P", "E", "W", "E", "E", "E"},
        { "E", "E", "E", "E", "E", "E" },
        { "E", "W", "E", "E", "W", "E" },
        { "E", "W", "E", "E", "E", "E" },
        { "E", "E", "E", "E", "E", "E" },
        { "E", "E", "W", "E", "G", "W" }
    };

    private string[,] stage2 = new string[6, 6] {
        { "W", "G", "E", "E", "E", "W"},
        { "E", "E", "E", "E", "E", "E"},
        { "E", "E", "E", "W", "W", "E"},
        { "E", "W", "E", "E", "E", "E"},
        { "P", "E", "E", "E", "E", "W"},
        { "E", "E", "W", "E", "E", "E"}
    };

    private string[,] stage3 = new string[6, 6] {
        { "E", "E", "W", "E", "E", "W"},
        { "O", "E", "E", "E", "O", "E"},
        { "O", "E", "U", "G", "E", "O"},
        { "O", "P", "W", "E", "E", "E"},
        { "O", "O", "W", "E", "E", "E"},
        { "O", "O", "W", "O", "W", "E"}
    };

    public List<string[,]> GetAllStages()
    {
        return new List<string[,]> { stage1, stage2, stage3 };
    }

    public void SetMap(string[,] map)
    {
        height = map.GetLength(0); // 행 수
        width = map.GetLength(1);  // 열 수

        mapData = (string[,])map.Clone();
        originalMapData = (string[,])map.Clone();
        ResetMap();
    }

    public void ResetMap()
    {
        mapData = (string[,])originalMapData.Clone();

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        goalPositions.Clear();
        RenderMap();
    }

    public void RenderMap()
    {
        gridOrigin = Vector2.zero;

        float offsetX = -(width * cellSize) / 2f + (cellSize / 2f);
        float offsetY = -(height * cellSize) / 2f + (cellSize / 2f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int flippedY = height - 1 - y;
                Vector2 worldPos = gridOrigin + new Vector2(x * cellSize, y * cellSize) + new Vector2(offsetX, offsetY);
                GameObject cell = Instantiate(cellPrefab, gridParent);
                cell.GetComponent<RectTransform>().anchoredPosition = worldPos;

                string code = mapData[flippedY, x];

                if (code == "P")
                {
                    GameObject robot = Instantiate(robotPrefab, gridParent);
                    robot.GetComponent<RectTransform>().anchoredPosition = worldPos;

                    var rc = robot.GetComponent<RobotController>();
                    rc.gridManager = this;
                    rc.slidingManager = slidingManager;

                    slidingManager.RegisterRobot(rc);
                }
                else if (code == "O")
                    Instantiate(obstaclePrefab, gridParent).GetComponent<RectTransform>().anchoredPosition = worldPos;
                else if (code == "W")
                    Instantiate(wallPrefab, gridParent).GetComponent<RectTransform>().anchoredPosition = worldPos;
                else if (code == "G")
                {
                    GameObject goal = Instantiate(goalPrefab, gridParent);
                    goal.GetComponent<RectTransform>().anchoredPosition = worldPos;

                    goalPositions.Add(new Vector2Int(x, y));

                    // GoalTile 컴포넌트가 있어야 효과 작동
                    if (goal.GetComponent<GoalTile>() == null)
                    {
                        goal.AddComponent<GoalTile>(); // 없으면 자동으로 붙이기 (선택 사항)
                    }
                }

                else if (code == "U" || code == "D" || code == "L" || code == "R")
                {
                    GameObject arrow = Instantiate(arrowTilePrefab, gridParent);
                    arrow.GetComponent<RectTransform>().anchoredPosition = worldPos;

                    float rot = code == "U" ? 0 : code == "D" ? 180 : code == "L" ? 90 : -90;
                    arrow.transform.rotation = Quaternion.Euler(0, 0, rot);
                }
            }
        }
    }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        float offsetX = -(width * cellSize) / 2f + (cellSize / 2f);
        float offsetY = -(height * cellSize) / 2f + (cellSize / 2f);
        return gridOrigin + new Vector2(gridPos.x * cellSize, gridPos.y * cellSize) + new Vector2(offsetX, offsetY);
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        float offsetX = -(width * cellSize) / 2f + (cellSize / 2f);
        float offsetY = -(height * cellSize) / 2f + (cellSize / 2f);
        Vector2 local = worldPos - gridOrigin - new Vector2(offsetX, offsetY);
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);

        return new Vector2Int(x, y);
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsBlocked(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return true;
        int flippedY = height - 1 - pos.y;
        string code = mapData[flippedY, pos.x];
        return code == "W";
    }

    public string GetMapValue(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return null;
        int flippedY = height - 1 - pos.y;
        return mapData[flippedY, pos.x];
    }

    public void SetMapValue(Vector2Int pos, string value)
    {
        if (IsInBounds(pos))
        {
            int flippedY = height - 1 - pos.y;
            mapData[flippedY, pos.x] = value;
        }
    }

    public void ReplaceObstacleWithWall(Vector2Int pos)
    {
        Vector2 worldPos = GridToWorld(pos);

        foreach (Transform child in gridParent)
        {
            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt != null && Vector2.Distance(rt.anchoredPosition, worldPos) < 1f && child.name.Contains("Obstacle"))
            {
                Destroy(child.gameObject);
                break;
            }
        }

        GameObject wall = Instantiate(wallPrefab, gridParent);
        wall.GetComponent<RectTransform>().anchoredPosition = worldPos;
    }

    public bool IsArrowTile(Vector2Int pos)
    {
        if (!IsInBounds(pos)) return false;
        int flippedY = height - 1 - pos.y;
        string code = mapData[flippedY, pos.x];
        return code == "U" || code == "D" || code == "L" || code == "R";
    }

    public Vector2Int GetArrowDirection(Vector2Int pos)
    {
        int flippedY = height - 1 - pos.y;
        string code = mapData[flippedY, pos.x];
        if (code == "U") return Vector2Int.up;
        if (code == "D") return Vector2Int.down;
        if (code == "L") return Vector2Int.left;
        if (code == "R") return Vector2Int.right;
        return Vector2Int.zero;
    }

    public bool IsGoalTile(Vector2Int pos)
    {
        foreach (var goal in goalPositions)
        {
            if (goal == pos)
                return true;
        }
        return false;
    }

    public void TriggerGoalEffectAt(Vector2Int pos)
    {
        Vector2 worldPos = GridToWorld(pos);

        foreach (Transform child in gridParent)
        {
            if (Vector2.Distance(child.GetComponent<RectTransform>().anchoredPosition, worldPos) < 1f)
            {
                GoalTile goal = child.GetComponent<GoalTile>();
                if (goal != null)
                {
                    goal.PlayReachedEffect();
                }
            }
        }
    }

    public Color GetGoalColorAt(Vector2Int pos)
    {
        Vector2 worldPos = GridToWorld(pos);

        foreach (Transform child in gridParent)
        {
            if (Vector2.Distance(child.GetComponent<RectTransform>().anchoredPosition, worldPos) < 1f)
            {
                // GoalTile이 붙어 있는 오브젝트만 대상으로 제한
                GoalTile goalTile = child.GetComponent<GoalTile>();
                if (goalTile != null)
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null)
                        return img.color;
                }
            }
        }

        return Color.yellow; // fallback
    }



}