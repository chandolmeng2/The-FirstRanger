using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class JournalOption
{
    public Toggle toggle;
    public string text;
}

public class PatrolJournalUI : MonoBehaviour
{
    public static PatrolJournalUI Instance;

    [SerializeField] private GameObject journalPanel;
    [SerializeField] private List<JournalOption> options;
    [SerializeField] private List<string> correctAnswers; // 정답 텍스트
    [SerializeField] private Text resultText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button completeButton;

    private bool hasCompleted = false;

    private void Awake()
    {
        Instance = this;
        journalPanel.SetActive(false);

        // AddListener에는 반드시 void 반환 메서드!
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
    }

    public void Open()
    {
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        // 마우스 조작 준비
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerController.IsJournalOpen = true;

        yield return StartCoroutine(FadeController.Instance.FadeIn(1.2f));

        journalPanel.SetActive(true);
        resultText.gameObject.SetActive(false);

        foreach (var option in options)
        {
            option.toggle.isOn = false;

            // 사운드 연결
            option.toggle.onValueChanged.RemoveAllListeners(); // 중복 방지
            option.toggle.onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                    SoundManager.Instance.Play(SoundKey.Mission6_CheckMark);
            });
        }

        yield return StartCoroutine(FadeController.Instance.FadeOut(1.2f));
    }

    public void OnCloseButtonClicked()
    {
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        yield return StartCoroutine(FadeController.Instance.FadeIn(1.2f));

        journalPanel.SetActive(false);

        yield return StartCoroutine(FadeController.Instance.FadeOut(1.2f));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.IsJournalOpen = false;
    }

    public void OnCompleteButtonClicked()
    {
        StartCoroutine(OnCompleteRoutine());
    }

    private IEnumerator OnCompleteRoutine()
    {
        int score = 0;
        int total = options.Count;

        foreach (var option in options)
        {
            bool isChecked = option.toggle.isOn;
            string trimmed = option.text.Trim();
            bool isCorrect = correctAnswers.Contains(trimmed);

            if (isCorrect && isChecked)
                score += 1;
            else if (isCorrect && !isChecked)
                score -= 1;
            else if (!isCorrect && isChecked)
                score -= 1;
            else if (!isCorrect && !isChecked)
                score += 1;

            Debug.Log($"선택: [{trimmed}] / 정답여부: {isCorrect} / 체크됨: {isChecked}");
        }

        string grade = GetGrade(score, total);
        Debug.Log($"점수: {score} / {total}");

        resultText.text = $"등급: <b>{grade}</b>";
        resultText.gameObject.SetActive(true);

        hasCompleted = (grade == "S");

        // S등급일 경우 성공 효과음 추가
        if (grade == "S")
        {
            SoundManager.Instance.Play(SoundKey.Mission6_Check_S);
        }

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeController.Instance.FadeIn(1.2f));
        journalPanel.SetActive(false);
        yield return StartCoroutine(FadeController.Instance.FadeOut(1.2f));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.IsJournalOpen = false;

        // 여기서 Mission6Manager에 클리어 알림
        if (Mission6Manager.Instance != null)
        {
            Mission6Manager.Instance.OnJournalCompleted();
        }
    }

    private string GetGrade(int score, int total)
    {
        if (score == total) return "S";
        if (score == total - 1) return "A";
        return "B";
    }



    public bool IsJournalCompleted()
    {
        return hasCompleted;
    }

    public bool IsOpen()
    {
        return journalPanel.activeSelf;
    }
}

