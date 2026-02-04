using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IllegalInteractionManager : MonoBehaviour
{
    public static IllegalInteractionManager Instance;

    public Camera playerCamera;
    public Camera illegalDialogueCamera;

    private IllegalNPC currentNPC;
    private float persuadeGauge = 50f;

    private int maxTurnLimit;
    private int currentTurn;

    [SerializeField] private DialogueDatabase dialogueDB;

    [SerializeField] private GameObject compassUI; // 나침반 UI 연결
    [SerializeField] private GameObject crossHair; // 크로스헤어 UI 연결

    public IllegalNPC CurrentNPC => currentNPC;

    private bool isFinalAttempting = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void BeginInteraction(IllegalNPC npc)
    {
        MissionPanel.Instance.isBlockTab = true;
        compassUI.SetActive(false); // 나침반 숨기기
        crossHair.SetActive(false);

        currentNPC = npc;

        // 말풍선 멈추기
        npc.GetComponentInChildren<SpeechBubbleController>()?.StopLoopingSpeech();

        // 말풍선 위치를 offset 기준으로 강제 전환
        npc.GetComponentInChildren<SpeechBubbleController>()?.EnableOffsetFollow();
        
        persuadeGauge = npc.initialPersuadeGauge; // ? 여기서 NPC의 초기 게이지 적용
        IllegalDialogueUI.Instance.SetGaugeImmediately(persuadeGauge); // UI 반영

        maxTurnLimit = 5;  // ? 무조건 5턴 고정
        currentTurn = 0;

        npc.GetComponent<IllegalNPCAI>().EnterDialogueIdle();

        UpdateUI(); // ? 수치 초기화 후 UI 정확히 갱신

        IllegalDialogueUI.Instance.ShowChoices();
        ShowDialogueChoices();

        playerCamera.enabled = false;
        illegalDialogueCamera.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // UI 초기화 확실히
        IllegalDialogueUI.Instance.ShowDialogueChoiceButtons();
        IllegalDialogueUI.Instance.SetButtonsInteractable(true);

        // 플레이어 애니메이션 강제 Idle 진입
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var anim = player.GetComponentInChildren<Animator>();
            anim.SetFloat("Speed", 0f);
            anim.SetBool("isRunning", false);
        }
    }



    private void ShowDialogueChoices()
    {
        var turn = dialogueDB.GetTurn(currentNPC.npcId, currentTurn);
        if (turn != null)
        {
            IllegalDialogueUI.Instance.SetDialogueChoices(turn.line1, turn.line2, turn.line3);
        }
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        StartCoroutine(HandleDialogueTurn(choiceIndex));
    }

    private IEnumerator HandleDialogueTurn(int choiceIndex)
    {
        IllegalDialogueUI.Instance.HideDialogueChoiceButtons();

        var turn = dialogueDB.GetTurn(currentNPC.npcId, currentTurn);
        currentTurn++;

        bool isCorrect = (turn != null && choiceIndex == turn.correctIndex);

        string playerLine = choiceIndex switch
        {
            0 => turn.line1,
            1 => turn.line2,
            2 => turn.line3,
            _ => ""
        };

        string npcReply = choiceIndex switch
        {
            0 => turn.npcReply1,
            1 => turn.npcReply2,
            2 => turn.npcReply3,
            _ => ""
        };

        // 1. 플레이어 대사 출력
        IllegalDialogueUI.Instance.ShowSpeech(playerLine, "");
        yield return new WaitForSeconds(2f);
        IllegalDialogueUI.Instance.HideAllSpeech();

        // 2. 게이지 반영
        persuadeGauge += isCorrect ? 25f : -15f;
        persuadeGauge = Mathf.Clamp(persuadeGauge, 0f, 100f);
        IllegalDialogueUI.Instance.UpdateGauge(persuadeGauge);

        // 정답/오답 사운드는 성공/실패 판정이 나지 않을 때만 재생
        bool reachedSuccess = (persuadeGauge >= 100f);
        bool reachedFailure = (persuadeGauge <= 0f || currentTurn >= maxTurnLimit);

        if (!reachedSuccess && !reachedFailure)
        {
            if (isCorrect)
            {
                SoundManager.Instance.Play(SoundKey.Mission4_Persuade_Correct);
                currentNPC.GetComponent<IllegalNPCAI>().PlaySurprised();
            }
            else
            {
                SoundManager.Instance.Play(SoundKey.Mission4_Persuade_Wrong);
                currentNPC.GetComponent<IllegalNPCAI>().PlayAngry();
            }
        }

        // ? 3. 성공 조건 검사 (100% 도달 시 NPC 대사 생략)
        if (persuadeGauge >= 100f)
        {
            yield return StartCoroutine(HandleSuccess());
            yield break;
        }

        // 실패 조건 검사 (도망/턴초과)
        if (persuadeGauge <= 0f || currentTurn >= maxTurnLimit)
        {
            StartCoroutine(HandleFailure());
            yield break;
        }

        // 5. 도망 여부 판정
        float escapeChance = GetEscapeChanceByTurn();
        if (Random.value < (escapeChance / 100f))
        {
            StartCoroutine(HandleFailure());
            yield break;
        }

        // 4. NPC 반응 출력 (100%가 아닐 때만)
        IllegalDialogueUI.Instance.ShowSpeech("", npcReply);

        // 애니메이션 재생
        var npcAI = currentNPC.GetComponent<IllegalNPCAI>();
        if (isCorrect)
        {
            npcAI.PlaySurprised();  // 정답: 놀람
        }
        else
        {
            npcAI.PlayAngry();      // 오답: 분노
        }

        yield return new WaitForSeconds(2f);
        IllegalDialogueUI.Instance.HideAllSpeech();

        // 6. 실패 or 턴 초과
        if (persuadeGauge <= 0f || currentTurn >= maxTurnLimit)
        {
            StartCoroutine(HandleFailure());
        }
        else
        {
            UpdateUI();
            IllegalDialogueUI.Instance.ShowTurnInfoTemporary(
                currentTurn,
                maxTurnLimit,
                GetEscapeChanceByTurn(),
                persuadeGauge
            );

            ShowDialogueChoices();
            IllegalDialogueUI.Instance.ShowDialogueChoiceButtons();
            IllegalDialogueUI.Instance.SetButtonsInteractable(true);
        }
    }



    public void OnFinalPersuasionAttempt()
    {
        if (!isFinalAttempting)
        {
            isFinalAttempting = true;
            StartCoroutine(HandleFinalPersuasion());
        }
    }

    private IEnumerator HandleFinalPersuasion()
    {
        IllegalDialogueUI.Instance.SetButtonsInteractable(false);

        IllegalDialogueUI.Instance.HideDialogueChoiceButtons(); // ? 대화 버튼 숨기기 추가

        float chance = persuadeGauge;
        bool success = Random.value < (chance / 100f);

        Debug.Log($"[설득 결과] NPC: {currentNPC.npcId}, 게이지: {persuadeGauge}, 성공 여부: {success}");

        string playerLine = dialogueDB.GetFinalPlayerLine(currentNPC.npcId);
        string npcLine = success
            ? dialogueDB.GetFinalNPCReplySuccess(currentNPC.npcId)
            : dialogueDB.GetFinalNPCReplyFail(currentNPC.npcId);

        IllegalDialogueUI.Instance.ShowSpeech(playerLine, "");
        yield return new WaitForSeconds(2f);
        IllegalDialogueUI.Instance.HideAllSpeech();

        if (success)
        {
            currentNPC.GetComponent<IllegalNPCAI>().PlayApologize();
            IllegalDialogueUI.Instance.ShowSpeech("", npcLine);
            yield return new WaitForSeconds(2f);
            IllegalDialogueUI.Instance.HideAllSpeech();
            yield return StartCoroutine(FadeOutAndCleanup());
        }
        else
        {
            StartCoroutine(HandleFailure());
        }

        isFinalAttempting = false;
    }

    private IEnumerator HandleSuccess()
    {
        SoundManager.Instance.Play(SoundKey.Mission4_Persuade_Success); // 성공 사운드

        currentNPC.GetComponent<IllegalNPCAI>().PlayApologize();
        string reply = dialogueDB.GetFinalNPCReplySuccess(currentNPC.npcId);
        IllegalDialogueUI.Instance.ShowSpeech("", reply);
        yield return new WaitForSeconds(2f);
        IllegalDialogueUI.Instance.HideAllSpeech();

        yield return StartCoroutine(FadeOutAndCleanup());
    }

    private IEnumerator FadeOutAndCleanup()
    {
        float fadeDuration = 0.4f;         // 어두워지고 밝아지는 속도
        float holdDuration = 1.2f;         // 검정 화면 유지 시간

        yield return FadeController.Instance.FadeIn(fadeDuration);
        IllegalDialogueUI.Instance.Hide();

        // 설득 성공 시 -> 영구 제거 처리
        if (currentNPC != null)
            CriminalSpawnerManager.Instance.MarkAsPermanentlyRemoved(currentNPC.gameObject);

        // Mission4Manager에 설득 성공 카운트 추가
        if (Mission4Manager.Instance != null)
            Mission4Manager.Instance.AddPersuadedCount();

        if (currentNPC != null) Destroy(currentNPC.gameObject);

        yield return new WaitForSeconds(holdDuration); // 검정 화면 유지 시간 추가
        EndInteraction();
        yield return FadeController.Instance.FadeOut(fadeDuration);
    }

    private IEnumerator HandleFailure()
    {
        SoundManager.Instance.Play(SoundKey.Mission4_Persuade_Fail); // 실패 사운드

        currentNPC.GetComponent<IllegalNPCAI>().PlayRunAway();

        string finalFailLine = dialogueDB.GetFinalNPCReplyFail(currentNPC.npcId);
        IllegalDialogueUI.Instance.ShowSpeech("", finalFailLine);

        float duration = 2f;
        float elapsed = 0f;

        Vector3 camPosition = illegalDialogueCamera.transform.position;

        while (elapsed < duration)
        {
            if (currentNPC != null)
            {
                Vector3 lookTarget = currentNPC.transform.position + Vector3.up * 1.5f;
                illegalDialogueCamera.transform.position = camPosition;
                illegalDialogueCamera.transform.LookAt(lookTarget);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return FadeController.Instance.FadeIn(0.3f);
        IllegalDialogueUI.Instance.Hide();

        // 설득 실패(도망감) → 다시 등장 가능하되 한 번 제외
        if (currentNPC != null)
            CriminalSpawnerManager.Instance.MarkAsRanAway(currentNPC.gameObject);

        if (currentNPC != null) Destroy(currentNPC.gameObject);

        yield return new WaitForSeconds(1f); // ? 검정 화면 유지
        EndInteraction();
        yield return FadeController.Instance.FadeOut(0.3f);
    }

    private void EndInteraction()
    {
        MissionPanel.Instance.isBlockTab = false; // TAB키 방지 해제
        compassUI.SetActive(true); // 나침반 다시 표시
        crossHair.SetActive(true);
        illegalDialogueCamera.enabled = false;
        playerCamera.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FindObjectOfType<PlayerController>().enabled = true;
    }

    private float GetEscapeChanceByTurn()
    {
        return GetEscapeChanceByRemainingTurn(maxTurnLimit - currentTurn);
    }

    private void UpdateUI()
    {
        float newEscapeChance = GetEscapeChanceByTurn(); // 현재 턴 기반
        float successChance = Mathf.Clamp(persuadeGauge, 0f, 100f);
        int displayedTurn = currentTurn + 1;

        // 이전 턴 기준 도망 확률
        float prevEscapeChance = GetEscapeChanceByRemainingTurn(maxTurnLimit - (currentTurn - 1));

        IllegalDialogueUI.Instance.AnimateEscapeChance(
            prevEscapeChance, newEscapeChance, displayedTurn, maxTurnLimit);

        IllegalDialogueUI.Instance.UpdateGauge(successChance);
    }


    private float GetEscapeChanceByRemainingTurn(int remainingTurns)
    {
        return remainingTurns switch
        {
            5 => 20f,
            4 => 25f,
            3 => 33f,
            2 => 50f,
            1 => 100f,
            _ => 100f
        };
    }


}
