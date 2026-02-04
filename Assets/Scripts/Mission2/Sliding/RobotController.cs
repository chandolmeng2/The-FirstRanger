using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class RobotController : MonoBehaviour
{
    public GridManager gridManager;
    public SlidingManager slidingManager;
    public GameObject arrowButtonPrefab;
    public Transform arrowParent;

    public float moveDurationPerCell = 0.2f;
    private bool isMoving = false;

    //로봇이 골에 도달 시 연출 효과들
    private Image robotImage;
    private Outline robotOutline;
    private Color originalColor;
    private bool isOnGoal = false;
    private Color goalColor; // 도달한 Goal의 색

    public Vector2Int currentGridPos;
    private Vector2Int startGridPos;

    private Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (slidingManager == null)
            slidingManager = FindObjectOfType<SlidingManager>();

        if (arrowParent == null)
        {
            GameObject found = GameObject.Find("ArrowContainer");
            if (found != null)
                arrowParent = found.transform;
            else
                Debug.LogError("ArrowContainer를 찾을 수 없습니다.");
        }

        RectTransform rt = GetComponent<RectTransform>();

        currentGridPos = gridManager.WorldToGrid(rt.anchoredPosition);
        startGridPos = currentGridPos; // ? 초기 위치 저장

        rt.anchoredPosition = gridManager.GridToWorld(currentGridPos);

        // SlidingManager에 자신 등록
        slidingManager.RegisterRobot(this);

        robotImage = GetComponent<Image>();
        robotOutline = GetComponent<Outline>(); // Outline 필수
        if (robotImage != null)
            originalColor = robotImage.color;

        if (robotOutline != null)
            robotOutline.enabled = false;
    }

    public void OnClickRobot()
    {
        if (isMoving) return;
        if (slidingManager != null && slidingManager.IsTransitioning()) return; //스테이지 클리어 시 입력 차단

        SoundManager.Instance.Play(SoundKey.Mission2_Puzzle2_LineConnect); // 로봇 클릭

        ClearArrows();
        ShowAvailableArrows();
    }

    void ShowAvailableArrows()
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int arrowPos = currentGridPos + dir;

            if (!gridManager.IsInBounds(arrowPos)) continue;

            bool blocked = gridManager.IsBlocked(arrowPos);
            bool occupied = slidingManager.IsOccupied(arrowPos, this);

            if (!blocked && !occupied)
            {
                CreateArrow(arrowPos, dir);
            }
        }
    }


    void CreateArrow(Vector2Int gridPos, Vector2Int dir)
    {
        Vector2 worldPos = gridManager.GridToWorld(gridPos);

        float offsetRatio = 0.1f; // 셀의 35% 정도 거리로 설정 (튜닝 가능)
        float offsetMagnitude = gridManager.cellSize * offsetRatio;
        Vector2 offset = (Vector2)dir * offsetMagnitude;

        Vector2 adjustedPos = worldPos - offset;


        GameObject arrow = Instantiate(arrowButtonPrefab, arrowParent);
        arrow.GetComponent<RectTransform>().anchoredPosition = adjustedPos;
        arrow.transform.localScale = Vector3.zero; // 시작 크기 0

        arrow.GetComponent<ArrowButton>().Initialize(this, dir);
        arrow.tag = "Arrow";

        float angle = 0;
        if (dir == Vector2Int.up) angle = 0;
        else if (dir == Vector2Int.down) angle = 180;
        else if (dir == Vector2Int.left) angle = 90;
        else if (dir == Vector2Int.right) angle = -90;

        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 등장 애니메이션
        arrow.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }


    public void MoveInDirection(Vector2Int dir)
    {
        if (isMoving) return;
        StartCoroutine(SlideMoveCoroutine(dir));
    }

    IEnumerator SlideMoveCoroutine(Vector2Int dir)
    {
        isMoving = true;
        ClearArrows();

        List<Vector2Int> path = CalculatePathUntilArrowOrGoal(currentGridPos, dir);
        if (path.Count == 0)
        {
            isMoving = false;
            yield break;
        }

        SoundManager.Instance.Play(SoundKey.Mission2_Sliding_RobotMove);// 이동 시작 효과음

        Vector2Int lastPos = path[path.Count - 1];
        float totalDuration = path.Count * moveDurationPerCell;
        Vector2 targetWorldPos = gridManager.GridToWorld(lastPos);

        transform.DOLocalMove(targetWorldPos, totalDuration).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(totalDuration);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int prev = (i == 0) ? currentGridPos : path[i - 1];
            Vector2Int now = path[i];

            // 로봇이 떠난 셀이 장애물이면 벽으로 변환
            if (gridManager.GetMapValue(prev) == "O")
            {
                gridManager.SetMapValue(prev, "W");
                gridManager.ReplaceObstacleWithWall(prev);
            }

            currentGridPos = now;
            GetComponent<RectTransform>().anchoredPosition = gridManager.GridToWorld(now);

            UpdateGoalState(); // 매 이동 후 Goal 상태 체크
        }

        // ?? Goal 위인지 확인 후 상태 업데이트
        slidingManager.CheckGameClear();

        // ?? 화살표 셀인 경우 다음 방향으로 이동
        if (gridManager.IsArrowTile(currentGridPos))
        {
            yield return new WaitForSeconds(0.05f);
            Vector2Int newDir = gridManager.GetArrowDirection(currentGridPos);
            StartCoroutine(SlideMoveCoroutine(newDir));
            yield break;
        }

        isMoving = false;
    }

    List<Vector2Int> CalculatePathUntilArrowOrGoal(Vector2Int start, Vector2Int dir)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;

        while (true)
        {
            Vector2Int next = current + dir;

            if (!gridManager.IsInBounds(next) || gridManager.IsBlocked(next))
                break;

            // ? 다른 로봇이 있는 경우 → 멈춤
            if (slidingManager.IsOccupied(next, this))
                break;

            path.Add(next);
            current = next;

            if (gridManager.IsGoalTile(current)) break;
            if (gridManager.IsArrowTile(current)) break;
        }

        return path;
    }


    void ClearArrows()
    {
        foreach (Transform child in arrowParent)
        {
            if (child.CompareTag("Arrow"))
                Destroy(child.gameObject);
        }
    }

    public void ResetToStart()
    {
        currentGridPos = startGridPos;
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = gridManager.GridToWorld(currentGridPos);
    }

    void UpdateGoalState()
    {
        bool nowOnGoal = gridManager.IsGoalTile(currentGridPos);

        if (nowOnGoal && !isOnGoal)
        {
            PlayGoalReachedEffect();
        }
        else if (!nowOnGoal && isOnGoal)
        {
            ResetVisual();
        }

        isOnGoal = nowOnGoal;
    }

    void PlayGoalReachedEffect()
    {
        // ?? Goal 색 가져오기
        goalColor = gridManager.GetGoalColorAt(currentGridPos); // 새 함수 필요

        if (robotImage != null)
        {
            robotImage.DOColor(goalColor, 0.3f); // Goal과 같은 색으로 변함
            robotImage.DOFade(0.5f, 0.1f).SetLoops(4, LoopType.Yoyo); // 반짝임
        }

        // Glow 켜기
        if (robotOutline != null)
        {
            robotOutline.effectColor = goalColor;
            robotOutline.enabled = true;
        }

        // 튕김 + 회전
        transform.DORotate(new Vector3(0, 0, 15), 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);

        // Goal 바닥 반응
        gridManager.TriggerGoalEffectAt(currentGridPos);
    }
    void ResetVisual()
    {
        // Glow 끄기
        if (robotOutline != null)
            robotOutline.enabled = false;

        // 색상 복구
        if (robotImage != null)
            robotImage.color = originalColor;

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
