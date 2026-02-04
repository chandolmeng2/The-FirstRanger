using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class MissionPanel : MonoBehaviour
{
    public static MissionPanel Instance;
    public static bool IsOpen { get; private set; } = false;

    public RectTransform missionPanel; // 임무 패널을 슬라이드 인/아웃할 RectTransform
    public RectTransform expPanel; // 경험치 패널 RectTransform
    public float slideDuration = 0.5f; // 슬라이드 애니메이션 시간
    public float offscreenPosition = -500f; // 화면 밖 위치 (왼쪽)
    public float onscreenPosition = 0f; // 화면 안 위치 (왼쪽 끝)

    public bool isTimer; //타이머 정지
    public bool isBlockTab = false; //TAB키 방지

    private bool isPanelVisible = false; // 패널이 보이는 상태인지 여부

    [Header("미션 정보 UI")]
    public Text txtObjective;
    public Text txtTrashCount;
    public Text txtTimer;

    [Header("미션 매니저")]
    public MonoBehaviour missionManager; // Mission1Manager 또는 Mission2Manager 또는 Mission3Manager 
    public float missionTime = 180f; // 총 3분

    public float remainingTime; //남은 시간

    //미션2 내용
    private readonly string[] puzzleNames = new string[]
    {
        "전기 점검",
        "배관 점검",
        "보일러 점검",
        "경보 시스템 점검",
        "쓰레기 정리",
    };


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        isTimer = true;
        // 패널을 시작할 때 화면 밖에 위치시키기
        missionPanel.anchoredPosition = new Vector2(offscreenPosition, missionPanel.anchoredPosition.y);
        expPanel.anchoredPosition = new Vector2(offscreenPosition, expPanel.anchoredPosition.y);
        remainingTime = missionTime;
    }

    public IEnumerator ShowExpPanel()
    {
        expPanel.DOAnchorPosX(onscreenPosition, slideDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(2f);
        expPanel.DOAnchorPosX(offscreenPosition, slideDuration).SetEase(Ease.OutQuad);
    }   

    private void Update()
    {
        // 퍼즐 중에는 패널 작동안함
        if (PuzzleUIManager.IsPuzzleActive) return;

        // 시간 감소
        if (remainingTime > 0f && isTimer)
            remainingTime -= Time.deltaTime;

        // 특정 키를 눌렀을 때
        if (Input.GetKey(KeyCode.Tab) && !BookCodex.codexActivated && !CodexScanController.IsScanning && !isBlockTab)
        {
            if (!isPanelVisible)
            {
                missionPanel.DOAnchorPosX(onscreenPosition, slideDuration).SetEase(Ease.OutQuad);
                expPanel.DOAnchorPosX(onscreenPosition, slideDuration).SetEase(Ease.OutQuad);
                isPanelVisible = true;
                MissionPanel.IsOpen = true; // (만약 정적 변수 쓰는 경우)
            }

            UpdateMissionText();
        }
        else
        {
            if (isPanelVisible)
            {
                missionPanel.DOAnchorPosX(offscreenPosition, slideDuration).SetEase(Ease.InQuad);
                expPanel.DOAnchorPosX(offscreenPosition, slideDuration).SetEase(Ease.InQuad);
                isPanelVisible = false;
                MissionPanel.IsOpen = false;
            }
        }

    }

    public void RefreshText()
    {
        UpdateMissionText(); // 내부 호출
    }

    // 텍스트 정보 업데이트
    private void UpdateMissionText()
    {
        if (txtObjective != null)
        {
            if (missionManager is Mission2Manager m2) //미션2 임무
            {
                txtObjective.text = $"업무 목표: 모든 시설물 점검 {m2.ClearedCount}/5 해결\n";

                for (int i = 0; i < puzzleNames.Length; i++)
                {
                    bool isCleared;
                    if (i < 4)
                        isCleared = m2.IsPuzzleCleared(i);
                    else
                        isCleared = m2.IsTrashPuzzleCleared();  // 아래에서 만들 것

                    string status = isCleared ? "<color=#ffff00>점검 완료</color>" : "미완료";
                    txtObjective.text += $"\n- {puzzleNames[i]}: {status}";
                }
            }

            else if (missionManager is Mission3Manager m3)//미션3 임무
            {
                txtObjective.text = "업무 목표: 야생 동물 구출";
            }

            else if (missionManager is Mission4Manager m4)//미션4 임무
            {
                txtObjective.text = "업무 목표: 범법자 퇴치";
            }

            else if (missionManager is Mission6Manager m6)//미션6 임무
            {
                txtObjective.text = "업무 목표: 야간 순찰";
            }

            else if (missionManager is Mission1Manager m1)//미션6 임무
            {
                txtObjective.text = "업무 목표: 쓰레기 수거";
            }

            else //튜토리얼 임무
            {
                txtObjective.text = "업무 목표: 국립공원 레인저 업무 배우기";
            }
        }

        if (txtTrashCount != null)
        {
            if (missionManager is Mission1Manager m1) //미션1 패널 내용
                txtTrashCount.text = "남은 수거: " + (m1.TargetCount - m1.CurrentCount);

            else if (missionManager is Mission3Manager m3) // 미션 3 추가
                txtTrashCount.text = "남은 개체 수: " + (m3.TargetCount - m3.CurrentCount);
        }

        if (txtTimer != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            txtTimer.text = $"남은 시간: {minutes:00}:{seconds:00}";
        }
    }
}