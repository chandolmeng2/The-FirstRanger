using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위함

public class DialogueManager2 : MonoBehaviour
{
    // 대화 패널 UI
    public GameObject dialoguePanel1;
    public GameObject dialoguePanel2;
    public GameObject dialoguePanel3;
    // 대화 내용을 표시할 TextMeshProUGUI 컴포넌트
    public TextMeshProUGUI dialogueText1;
    public TextMeshProUGUI dialogueText2;
    public TextMeshProUGUI dialogueText3;
    public Tutorial3Manager manager;
    private bool dialogueTrigger1 = false;
    private bool dialogueTrigger2 = false;
    private bool dialogueTrigger3 = false;
    private int dialogueCount = 0;
    public PlayerController playerController;
    public Animator playerAnimator;
    public GameObject[] guidePanel;
    public bool isPhase1Over = false;
    public bool isPhase2Over = false;
    public bool isPhase3Over = false;
    public bool goDialogue1 = false;
    public bool goDialogue2 = false;
    public bool goDialogue3 = false;

    // 대화 데이터 (하드코딩된 예시)
    private string[] dialogueLines1 = {
        "우선 앞으로 임무를 수행할 때, 위 나침반을 통해 \n 목표를 확인할 수 있습니다.",
        "각 아이콘은 목표를 표시합니다, ",
        "목표를 향해 바라보며 찾아가면 됩니다.",
        "다음으로 이동할게요. 따라 오세요."
    };

    private string[] dialogueLines2 = {
        "임무를 하면서, 식물을 발견하면 조사해야 합니다.",
        "F키를 누르면 망원경을 들어서 조사를 할 수 있습니다.",
        "해당 물체를 3초간 바라보고 \'F\'키를 누르면 \n 조사에 성공합니다.",
        "어디 한 번 실습을 해볼까요?"
    };

    private string[] dialogueLines3 = {
        "좋아요. 제대로 조사하셨군요?",
        "확인하려면 'C'키를 눌러 도감을 열어보면 \n 조사한 식물이 등록됩니다.",
        "이런..! 지금 몇 시죠? TAB키를 누르면 \n 현재 목표와 남은 시간이 보입니다.",
        "얼마 남지 않았어요! 얼른 뛰어서 사무실로 \n 복귀합시다!"
    };

    private int currentLineIndex = 0;

    void Start()
    {
        // 게임 시작 시 대화창은 비활성화 상태
        dialoguePanel1.SetActive(false);
        dialoguePanel2.SetActive(false);
        dialoguePanel3.SetActive(false);
    }

    void Update()
    {
        if (goDialogue1 && !dialogueTrigger1)
        {
            StartDialogue1();
            dialogueTrigger1 = true;
        }

        if (goDialogue2 && !dialogueTrigger2)
        {
            StartDialogue2();
            dialogueTrigger2 = true;
        }

        if (goDialogue3 && !dialogueTrigger3)
        {
            StartDialogue3();
            dialogueTrigger3 = true;
        }

        if (dialoguePanel1.activeSelf || dialoguePanel2.activeSelf || dialoguePanel3.activeSelf)
        {
            playerController.enabled = false;
            playerAnimator.enabled = false;
            if (manager.isPhase1)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    DisplayNextLine1();
                }
            }else if (manager.isPhase2)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    DisplayNextLine2();
                }
            }else if (manager.isPhase3)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    DisplayNextLine3();
                }
            }
                
        }
    }

    #region
    private void StartDialogue1()
    {
        // 대화 패널 활성화
        dialoguePanel1.SetActive(true);
        currentLineIndex = 0;
        dialogueCount = 0;
        DisplayNextLine1();
    }

    private void DisplayNextLine1()
    {
        // 모든 대화가 끝났는지 확인
        if (currentLineIndex == 4)
        {
            EndDialogue1();
            return;
        }
        else
        {
            // 대사 넘어갈 때 사운드 재생
            SoundManager.Instance.Play(SoundKey.UIClick_Dialogue);

            dialogueText1.text = dialogueLines1[currentLineIndex];
        }

        if (dialogueCount == 1)
        {
            guidePanel[0].SetActive(true);
        }
        if (dialogueCount == 2)
        {
            guidePanel[0].SetActive(false);
            guidePanel[1].SetActive(true);
        }
        if (dialogueCount == 3)
        {
            guidePanel[1].SetActive(false);
        }
        currentLineIndex++;
        dialogueCount++;
    }
    
    private void EndDialogue1()
    {
        manager.isTalking = false;
        playerController.enabled = true;
        playerAnimator.enabled = true;
        dialoguePanel1.SetActive(false);
        isPhase1Over = true;
    }
    #endregion
    #region
    private void StartDialogue2()
    {
        // 대화 패널 활성화
        dialoguePanel2.SetActive(true);
        currentLineIndex = 0;
        dialogueCount = 0;
        DisplayNextLine2();
    }

    private void DisplayNextLine2()
    {
        // 모든 대화가 끝났는지 확인
        if (currentLineIndex == 4)
        {
            
            EndDialogue2();
            return;
        }
        else
        {
            SoundManager.Instance.Play(SoundKey.UIClick_Dialogue); // 사운드 추가
            dialogueText2.text = dialogueLines2[currentLineIndex];
        }

        if (dialogueCount == 0)
        {
            guidePanel[2].SetActive(true);
        }
        if (dialogueCount == 1)
        {
            guidePanel[2].SetActive(false);
            guidePanel[3].SetActive(true);
        }
        if (dialogueCount == 2)
        {
            guidePanel[3].SetActive(false);
            guidePanel[4].SetActive(true);
        }
        if (dialogueCount == 3)
        {
            guidePanel[4].SetActive(false);
        }
        currentLineIndex++;
        dialogueCount++;
    }

    private void EndDialogue2()
    {
        manager.isTalking = false;
        playerController.enabled = true;
        playerAnimator.enabled = true;
        dialoguePanel2.SetActive(false);
        isPhase2Over = true;
    }
    #endregion
    #region
    private void StartDialogue3()
    {
        // 대화 패널 활성화
        dialoguePanel3.SetActive(true);
        currentLineIndex = 0;
        dialogueCount = 0;
        DisplayNextLine3();
    }

    private void DisplayNextLine3()
    {
        // 모든 대화가 끝났는지 확인
        if (currentLineIndex == 4)
        {
            EndDialogue3();
            return;
        }
        else
        {
            SoundManager.Instance.Play(SoundKey.UIClick_Dialogue); // 사운드 추가
            dialogueText3.text = dialogueLines3[currentLineIndex];
        }

        if (dialogueCount == 1)
        {
            guidePanel[5].SetActive(true);
        }
        if (dialogueCount == 2)
        {
            guidePanel[5].SetActive(false);
            guidePanel[6].SetActive(true);
        }
        if (dialogueCount == 3)
        {
            guidePanel[6].SetActive(false);
        }
        currentLineIndex++;
        dialogueCount++;
    }

    private void EndDialogue3()
    {
        manager.isTalking = false;
        playerController.enabled = true;
        playerAnimator.enabled = true;
        dialoguePanel3.SetActive(false);
        isPhase3Over = true;
    }
    #endregion
}
