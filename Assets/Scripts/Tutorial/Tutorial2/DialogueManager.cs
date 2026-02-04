using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위함

public class DialogueManager : MonoBehaviour
{
    // 대화 패널 UI
    public GameObject dialoguePanel;
    // 대화 내용을 표시할 TextMeshProUGUI 컴포넌트
    public TextMeshProUGUI dialogueText;
    public SequenceManager manager;
    private bool dialogueTrigger = false;
    private int dialogueCount = 0;
    public GameObject[] points;
    public Camera playerCamera;

    // 대화 데이터 (하드코딩된 예시)
    private string[] dialogueLines = {
        "처음 뵙겠습니다. 국립공원 레인저 사무실에 \n 오신 것을 환영해요.",
        "저는 선배 레인저고, 오늘 사무실 시설 안내해드릴게요.",
        "여기는 임무 보드입니다.",
        "여기서 임무를 시작할 수 있습니다.",
        "여기는 개인 정비실입니다.",
        "여기서 구매한 장비나 얻은 퍽을 커스터마이징할 수 있습니다.",
        "지금부턴 밖에 나가서 배울 것들을 배우겠습니다. \n 뒤로 나가시면 됩니다."
    };

    private int currentLineIndex = 0;

    void Start()
    {
        // 게임 시작 시 대화창은 비활성화 상태
        dialoguePanel.SetActive(false);
    }

    // 대화를 시작하는 함수
    public void StartDialogue()
    {
        // 대화 패널 활성화
        dialoguePanel.SetActive(true);
        currentLineIndex = 0;
        PlayerController.IsDialogueActive = true; // 대화 시작 알림
        SoundManager.Instance.StopWalkingLoop();  // 혹시 켜져 있던 발소리 강제 정지
        DisplayNextLine();
    }

    void Update()
    {
        if (manager.isDialogueStarted && !dialogueTrigger)
        {
            StartDialogue();
            dialogueTrigger = true;

        }
        // 대화창이 활성화된 상태에서만 입력 처리
        if (dialoguePanel.activeSelf)
        {
            // 스페이스 바를 누르면 다음 대화로 넘어감
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DisplayNextLine();
            }
        }
    }

    private void DisplayNextLine()
    {
        // 모든 대화가 끝났는지 확인
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }
        if (dialogueCount == 2)
        {
            playerCamera.transform.position = points[1].transform.position;
            playerCamera.transform.rotation = points[1].transform.rotation;
        }

        if (dialogueCount == 4)
        {
            playerCamera.transform.position = points[2].transform.position;
            playerCamera.transform.rotation = points[2].transform.rotation;
        }

        if (dialogueCount == 6)
        {
            playerCamera.transform.position = points[0].transform.position;
            playerCamera.transform.rotation = points[0].transform.rotation;
        }

        // 여기서 효과음 재생
        SoundManager.Instance.Play(SoundKey.UIClick_Dialogue);

        // 현재 대화 내용을 UI에 표시
        dialogueText.text = dialogueLines[currentLineIndex];
        currentLineIndex++;
        dialogueCount++;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        manager.isDialogueStarted = false;
        manager.moveNextScene = true;
        PlayerController.IsDialogueActive = false; // 대화 끝 알림
    }
}
