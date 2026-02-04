using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public LineDrawer lineDrawer;
    public Camera uiCamera;

    private bool isDragging = false;
    private string currentColor = "";
    private Cell startCell;
    private List<Cell> currentPath = new List<Cell>();

    [SerializeField] private Image clearPanel;
    [SerializeField] private Button resetButton;

    private Dictionary<string, List<Cell>> paths = new Dictionary<string, List<Cell>>();
    private Dictionary<string, bool> isColorConnected = new Dictionary<string, bool>();

    private bool isPuzzleCleared = false;

    private void Start()
    {
        InitializeColors();
    }

    private void InitializeColors()
    {
        Dot[] allDots = FindObjectsOfType<Dot>();

        foreach (Dot dot in allDots)
        {
            string color = dot.colorType;

            if (!isColorConnected.ContainsKey(color))
            {
                isColorConnected[color] = false;
                paths[color] = new List<Cell>();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPuzzleCleared) return;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Cell cell = result.gameObject.GetComponent<Cell>();

            if (cell != null && cell.dot != null)
            {
                string color = cell.dot.colorType;
                // 이미 연결된 색상은 건들지 못하게 막기
                if (isColorConnected.ContainsKey(color) && isColorConnected[color])
                {
                    Debug.Log($"{color}는 이미 연결됨 (수정 불가)");
                    return;
                }
                currentColor = color;
                isDragging = true;
                startCell = cell;

                if (!paths.ContainsKey(color))
                {
                    paths[color] = new List<Cell>();
                    isColorConnected[color] = false;
                }

                UndoColorPath(color);

                paths[color].Add(startCell);
                currentPath = paths[color];

                Debug.Log($"?? {color} 드래그 시작");
                return;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isPuzzleCleared) return;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Cell cell = result.gameObject.GetComponent<Cell>();
            if (cell == null || currentPath.Contains(cell)) continue;

            var lastCell = currentPath[currentPath.Count - 1];

            if (!IsAdjacent(cell.gridPos, lastCell.gridPos))
            {
                Debug.Log("? 인접하지 않음");
                continue;
            }

            // ? 이미 다른 색상에 의해 점유된 셀은 통과 금지
            if (cell.IsOccupiedByOther(currentColor))
            {
                Debug.Log($"? {cell.gridPos}는 {cell.usedByColor}에 의해 점유 중입니다.");
                continue;
            }

            if (cell.dot != null && cell.dot.colorType != currentColor)
            {
                Debug.Log($"? 다른 색상의 도트를 지나칠 수 없습니다: {cell.dot.colorType}");
                continue;
            }

            currentPath.Add(cell);

            // ? 셀 배경 색상 변경
            Image cellImage = cell.GetComponent<Image>();
            if (cellImage != null)
                cellImage.color = GetColor(currentColor);

            // ? DotVisual 색상 변경
            Transform visualDot = cell.transform.Find("DotVisual");
            if (visualDot != null)
            {
                Image visualDotImage = visualDot.GetComponent<Image>();
                if (visualDotImage != null)
                    visualDotImage.color = GetColor(currentColor);
            }

            Vector2 start = lastCell.transform.position;
            Vector2 end = cell.transform.position;
            lineDrawer.DrawLine(start, end, currentColor);

            // 셀/도트 색상 적용 (생략 가능)

            if (cell.dot != null && cell.dot.colorType == currentColor)
            {
                isColorConnected[currentColor] = true;
                isDragging = false;
                foreach (var c in currentPath)
                {
                    c.usedByColor = currentColor;

                    // ? 반짝임 효과
                    if (c.dot != null)
                    {
                        c.dot.PlayGlowEffect(); // 빛나는 연출 함수 호출
                    }
                }

                Debug.Log($"? {currentColor} 연결 성공!");

                // 연결 성공 효과음
                SoundManager.Instance.Play(SoundKey.Mission2_Puzzle2_LineConnect);

                CheckPuzzleClear();
            }

            break;
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || isPuzzleCleared) return;

        var path = paths[currentColor];
        Cell last = path[path.Count - 1];

        if (last.dot == null || last.dot.colorType != currentColor)
        {
            Debug.Log($"? {currentColor} 연결 실패. 초기화");

            // ?? 실시간 경로의 색상도 초기화 (usedByColor 없이)
            foreach (var cell in currentPath)
            {
                // 시작 지점 Dot이 있는 셀은 초기화하면 안 됨
                if (cell.dot != null && cell.dot.colorType == currentColor)
                    continue;

                // 셀 배경색 초기화
                Image cellImage = cell.GetComponent<Image>();
                if (cellImage != null)
                    cellImage.color = Color.white;

                // DotVisual 초기화
                Transform visualDot = cell.transform.Find("DotVisual");
                if (visualDot != null)
                {
                    Image visualDotImage = visualDot.GetComponent<Image>();
                    if (visualDotImage != null)
                        visualDotImage.color = Color.white;
                }

                // Dot 초기화
                if (cell.dot != null)
                {
                    Image dotImage = cell.dot.GetComponent<Image>();
                    if (dotImage != null)
                        dotImage.color = Color.white;
                }
            }


            lineDrawer.ClearLine(currentColor);
            paths[currentColor].Clear();
            isColorConnected[currentColor] = false;
        }

        isDragging = false;
    }


    public void UndoColorPath(string color)
    {
        if (!paths.ContainsKey(color)) return;

        foreach (var c in paths[color])
        {
            // 연결된 Dot을 가진 셀은 초기화하지 않음
            if (c.dot != null && c.dot.colorType == color)
                continue;

            if (c.usedByColor != color) continue;

            c.usedByColor = null;

            // 셀 초기화
            Image cellImage = c.GetComponent<Image>();
            if (cellImage != null)
                cellImage.color = Color.white;

            Transform visualDot = c.transform.Find("DotVisual");
            if (visualDot != null)
            {
                Image visualDotImage = visualDot.GetComponent<Image>();
                if (visualDotImage != null)
                    visualDotImage.color = Color.white;
            }

            if (c.dot != null)
            {
                Image dotImage = c.dot.GetComponent<Image>();
                if (dotImage != null)
                    dotImage.color = Color.white;
            }
        }


        lineDrawer.ClearLine(color);
    }


    public void ResetAllColors()
    {
        if (isPuzzleCleared) return;

        foreach (var color in paths.Keys)
        {
            UndoColorPath(color);
            paths[color].Clear();
            isColorConnected[color] = false;
        }
        Debug.Log("?? 모든 색상 초기화 완료");

        SoundManager.Instance.Play(SoundKey.Mission2_UIClick_Button);

        isDragging = false;
        currentColor = "";
    }

    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    private void CheckPuzzleClear()
    {
        foreach (var entry in isColorConnected)
        {
            if (!entry.Value)
            {
                Debug.Log($"? 아직 연결되지 않은 색상: {entry.Key}");
                return;
            }
        }

        Debug.Log("?? 퍼즐 클리어!");

        isPuzzleCleared = true;
        if (resetButton != null)
            resetButton.interactable = false;

        // 번쩍 연출 (흰색 알파 → 1 → 0)
        if (clearPanel != null)
        {
            clearPanel.color = new Color(1, 1, 1, 0); // 초기 알파 0
            clearPanel.DOFade(1f, 0.1f) // 빠르게 밝아지고
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    clearPanel.DOFade(0f, 0.5f); // 부드럽게 사라짐
                });
        }

        FindObjectOfType<PuzzleUIManager>()?.ShowClearAndAutoClose();
    }

    private Color GetColor(string color)
    {
        return color switch
        {
            "Red" => Color.red,
            "Blue" => Color.blue,
            "Green" => Color.green,
            "Yellow" => Color.yellow,
            _ => Color.white
        };
    }
}