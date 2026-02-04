using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    private bool hasPressed = false;
    private bool showingResult = false;

    [Header("UI")]
    public Text resultText;
    public GameObject continueText;
    public GameObject successIcon;
    public GameObject failIcon;
    public GameObject xpBar;
    public Image xpFillImage;
    public float lineDelay = 2f;

    [Header("캐릭터")]
    public GameObject resultCharacter;
    public Animator characterAnimator;

    [SerializeField] private Image hidePanel;

    public float xpAnimDuration = 1.0f;

    [SerializeField] private GameObject perkPointPanel;
    [SerializeField] private TextMeshProUGUI perkPointText;

    void Start()
    {
        showingResult = true;
        successIcon.SetActive(false);
        failIcon.SetActive(false);
        xpBar.SetActive(false);

        StartCoroutine(ShowResultText());
    }

    void Update()
    {
        if (!hasPressed && Input.anyKeyDown && !showingResult)
        {
            hasPressed = true;
            SceneTransitionManager.Instance.LoadScene("LobbyScene");

            IMissionManager mission = GetActiveMission();
            if (mission != null)
                Destroy(mission.gameObject);
        }
    }

    private IMissionManager GetActiveMission()
    {
        return FindObjectsOfType<MonoBehaviour>().OfType<IMissionManager>().FirstOrDefault();
    }

    IEnumerator ShowResultText()
    {
        IMissionManager mission = GetActiveMission();
        if (mission == null)
        {
            Debug.LogWarning("No active mission manager found.");
            yield break;
        }

        float time = mission.CheckTime;
        bool isSuccess = mission.GotoResult;
        int missionIndex = mission.Index;
        int expAmount = mission.ExpValue;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        string timeFormatted = $"{minutes:00}:{seconds:00}";

        // 1. 임무 보고
        resultText.text = "<size=150>임무 보고</size>\n";
        yield return new WaitForSeconds(lineDelay);

        FadeOutPanel();

        // 2. 임무 수행 여부
        resultText.text += "임무 수행 여부:\n";
        yield return new WaitForSeconds(lineDelay);

        // 3. 수행 결과 아이콘 표시
        if (isSuccess)
        {
            characterAnimator.CrossFade("Cheer", 0.2f);
            successIcon.SetActive(true);
        }
        else
        {
            characterAnimator.CrossFade("Sad", 0.2f);
            failIcon.SetActive(true);
        }
        yield return new WaitForSeconds(lineDelay);

        // 4. 남은 시간
        resultText.text += $"남은 시간: {timeFormatted}\n";
        yield return new WaitForSeconds(lineDelay);

        // 5. 획득 경험치
        if (isSuccess)
            resultText.text += $"획득 경험치: +{expAmount} Exp";
        else
            resultText.text += "획득 경험치: +0 Exp";
        yield return new WaitForSeconds(lineDelay);

        // 6. XP 바
        xpFillImage.fillAmount = 0f;
        xpBar.SetActive(true);
        yield return new WaitForSeconds(lineDelay);

        if (isSuccess)
        {
            yield return AnimateExpGain(expAmount, missionIndex);
        }
        else
        {
            yield return new WaitForSeconds(xpAnimDuration);
        }

        yield return new WaitForSeconds(0.5f);

        continueText.SetActive(true);
        showingResult = false;
    }

    /// <summary>
    /// 경험치 애니메이션 (레벨업 고려)
    /// </summary>
    IEnumerator AnimateExpGain(int expAmount, int missionIndex)
    {
        int currentExp = ExpManager.Instance.GetRankExp();  // 랭크 내 경험치
        int expToAdd = expAmount;

        while (expToAdd > 0)
        {
            int maxExp = ExpManager.Instance.GetMaxExp();
            int expToLevelUp = maxExp - currentExp;

            float beforeRatio = (float)currentExp / maxExp;

            if (expToAdd >= expToLevelUp)
            {
                // 현재 경험치 → 1까지 애니메이션
                yield return xpFillImage.DOFillAmount(1f, xpAnimDuration)
                    .SetEase(Ease.OutCubic)
                    .WaitForCompletion();

                ExpManager.Instance.AddExp(expToLevelUp);

                MissionManager.Instance.OnMissionClear(missionIndex);

                PerkManager.Instance.perkPoints += 2;
                ShowPerkPointUI(2);

                expToAdd -= expToLevelUp;
                currentExp = 0;

                xpFillImage.fillAmount = 0f;  // 레벨업 직후 초기화
            }
            else
            {
                float afterRatio = (float)(currentExp + expToAdd) / maxExp;
                yield return xpFillImage.DOFillAmount(afterRatio, xpAnimDuration)
                    .SetEase(Ease.OutCubic)
                    .WaitForCompletion();

                ExpManager.Instance.AddExp(expToAdd);

                MissionManager.Instance.OnMissionClear(missionIndex);

                expToAdd = 0;
            }
        }
    }


    private void ShowPerkPointUI(int amount)
    {
        if (perkPointPanel == null || perkPointText == null) return;

        // 처음 활성화
        perkPointPanel.SetActive(true);
        perkPointText.text = $"+{amount} 특성 포인트!";

        // 알파 초기화
        CanvasGroup cg = perkPointPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = perkPointPanel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
    }


    void FadeOutPanel()
    {
        Color color = hidePanel.color;
        color.a = 1f;
        hidePanel.color = color;

        hidePanel.DOFade(0f, 1.0f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => hidePanel.gameObject.SetActive(false));
    }
}
