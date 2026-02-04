// SlidingManager.cs with Shake + Fade Transition
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SlidingManager : MonoBehaviour
{
    public GridManager gridManager;
    public Text goalProgressText;
    public GameObject robotPrefab;
    public Transform gridParent;
    public GameObject puzzleUI;
    public float stageDelay = 3f;

    public Image fadeImage; // ? 페이드 이미지 (검은색 이미지 필요)

    private List<RobotController> robots = new List<RobotController>();
    private List<Vector2Int> robotStartPositions = new List<Vector2Int>();
    private bool isInitialized = false;

    public List<string[,]> mapStages = new List<string[,]>();
    public int currentStageIndex = 0;

    private bool isTransitioning = false;

    void Start()
    {
        mapStages = gridManager.GetAllStages();
        LoadStage(0);
        ResetGame();
        Invoke("Initialize", 0.1f);

        fadeImage.color = new Color(0, 0, 0, 1); // 완전 검정
        fadeImage.DOFade(0, 2f); // 2초 동안 서서히 밝아짐
    }

    void Initialize()
    {
        robots = new List<RobotController>(FindObjectsOfType<RobotController>());
        CheckGameClear();
    }

    public void RegisterRobot(RobotController robot)
    {
        if (!robots.Contains(robot))
        {
            robots.Add(robot);
            if (!isInitialized)
                robotStartPositions.Add(robot.currentGridPos);
        }
    }

    public bool IsOccupied(Vector2Int pos, RobotController self)
    {
        foreach (var robot in robots)
        {
            if (robot != self && robot.currentGridPos == pos)
                return true;
        }
        return false;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    public void CheckGameClear()
    {
        int reached = CountRobotsOnGoals();
        int total = gridManager.goalPositions.Count;

        UpdateGoalUI(reached, total);

        if (reached == total && total > 0)
        {
            Debug.Log($"?? Stage {currentStageIndex + 1} 클리어!");
            StartCoroutine(ProceedToNextStageAfterDelay());

            // 마지막 스테이지일 때만 종료 처리
            if (currentStageIndex == mapStages.Count - 1)
            {
                PuzzleUIManager ui = FindObjectOfType<PuzzleUIManager>();
                if (ui != null)
                {
                    ui.ShowClearAndAutoClose();
                }
            }
        }
    }

    IEnumerator ProceedToNextStageAfterDelay()
    {
        isTransitioning = true;

        // 0.5초 기다린 후 퍼즐 흔들림 효과 (모든 스테이지 공통)
        yield return new WaitForSeconds(0.5f);
        if (puzzleUI != null)
        {
            puzzleUI.transform.DOShakePosition(0.4f, 20f, 20, 90f);
        }
        // 흔들림이 끝나고 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 화면 어둡게 (Fade Out)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            yield return fadeImage.DOFade(1f, 2f).WaitForCompletion();
        }

        yield return new WaitForSeconds(0.5f);

        currentStageIndex++;

        if (currentStageIndex < mapStages.Count)
        {
            LoadStage(currentStageIndex);
            ResetGame();

            // ? Fade In
            if (fadeImage != null)
            {
                yield return fadeImage.DOFade(0f, 2f).WaitForCompletion();
                fadeImage.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("? 모든 퍼즐 스테이지 완료!");
            yield return new WaitForSeconds(stageDelay);
            if (puzzleUI != null)
            {
                puzzleUI.SetActive(false);
            }
        }

        isTransitioning = false;
    }

    public void LoadStage(int index)
    {
        if (index >= 0 && index < mapStages.Count)
        {
            gridManager.SetMap(mapStages[index]);
        }
    }

    private int CountRobotsOnGoals()
    {
        int count = 0;
        foreach (Vector2Int goal in gridManager.goalPositions)
        {
            foreach (RobotController robot in robots)
            {
                if (robot.currentGridPos == goal)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    private void UpdateGoalUI(int reached, int total)
    {
        if (goalProgressText != null)
        {
            goalProgressText.text = $"도달 목표: {reached}/{total}";
        }
    }

    public void OnClickReset()
    {
        if (!isTransitioning)
        {
            SoundManager.Instance.Play(SoundKey.Mission2_UIClick_Button); // 리셋 사운드
            ResetGame();
        }
    }

    public void ResetGame()
    {
        ClearAllArrows();

        foreach (var robot in robots)
        {
            if (robot != null)
                Destroy(robot.gameObject);
        }
        robots.Clear();

        gridManager.ResetMap();
        isInitialized = true;
        CheckGameClear();
    }

    public void ClearAllArrows()
    {
        GameObject arrowContainer = GameObject.Find("ArrowContainer");
        if (arrowContainer != null)
        {
            foreach (Transform child in arrowContainer.transform)
            {
                if (child.CompareTag("Arrow"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
