using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IllegalDialogueUI : MonoBehaviour
{
    public static IllegalDialogueUI Instance;

    [Header("UI 오브젝트")]
    public GameObject panel;
    [SerializeField] private Image persuadeGauge;

    [Header("선택 버튼들")]
    public Button choiceButton1;
    public Button choiceButton2;
    public Button choiceButton3;
    public Button finalAttemptButton;

    [Header("버튼 텍스트")]
    [SerializeField] private Text choiceButton1Text;
    [SerializeField] private Text choiceButton2Text;
    [SerializeField] private Text choiceButton3Text;
    [SerializeField] private Text finalAttemptButtonText;

    public Text turnInfoText;      // 턴 + 도망 확률 표시용
    public Text successChanceText; // 설득 버튼 옆 성공 확률 표시용

    private Coroutine gaugeRoutine;

    private bool isFinalAttempting = false;

    private Coroutine escapeChanceRoutine;
    public bool IsGaugeAnimationDone { get; private set; } = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Hide();

        // 클릭 이벤트 연결 + 사운드
        choiceButton1.onClick.AddListener(() => {
            SoundManager.Instance.Play(SoundKey.UIClick_Button);
            IllegalInteractionManager.Instance.OnChoiceSelected(0);
        });
        choiceButton2.onClick.AddListener(() => {
            SoundManager.Instance.Play(SoundKey.UIClick_Button);
            IllegalInteractionManager.Instance.OnChoiceSelected(1);
        });
        choiceButton3.onClick.AddListener(() => {
            SoundManager.Instance.Play(SoundKey.UIClick_Button);
            IllegalInteractionManager.Instance.OnChoiceSelected(2);
        });
        finalAttemptButton.onClick.AddListener(() => {
            SoundManager.Instance.Play(SoundKey.UIClick_Button);
            IllegalInteractionManager.Instance.OnFinalPersuasionAttempt();
        });
    }

    public void ShowChoices()
    {
        panel.SetActive(true);
        choiceButton1.gameObject.SetActive(true);
        choiceButton2.gameObject.SetActive(true);
        choiceButton3.gameObject.SetActive(true);
        finalAttemptButton.gameObject.SetActive(true);
    }

    public void SetDialogueChoices(string text1, string text2, string text3)
    {
        choiceButton1Text.text = text1;
        choiceButton2Text.text = text2;
        choiceButton3Text.text = text3;
    }

    public void SetButtonsInteractable(bool interactable)
    {
        choiceButton1.interactable = interactable;
        choiceButton2.interactable = interactable;
        choiceButton3.interactable = interactable;
        finalAttemptButton.interactable = interactable;
    }

    public void UpdateGauge(float targetValue)
    {
        if (gaugeRoutine != null) StopCoroutine(gaugeRoutine);
        gaugeRoutine = StartCoroutine(AnimateGauge(targetValue));
    }

    public void SetGaugeImmediately(float value)
    {
        if (gaugeRoutine != null) StopCoroutine(gaugeRoutine);
        persuadeGauge.fillAmount = Mathf.Clamp01(value / 100f);
        IsGaugeAnimationDone = true;

        successChanceText.text = $"성공 확률: {Mathf.RoundToInt(value)}%"; // ? 바로 반영
    }


    private IEnumerator AnimateGauge(float targetValue)
    {
        IsGaugeAnimationDone = false;

        float startValue = persuadeGauge.fillAmount;
        float endValue = Mathf.Clamp01(targetValue / 100f);

        float startSuccessChance = Mathf.RoundToInt(startValue * 100f);
        float endSuccessChance = Mathf.RoundToInt(endValue * 100f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 1.0f;

            // 게이지 바 애니메이션
            float gaugeFill = Mathf.Lerp(startValue, endValue, t);
            persuadeGauge.fillAmount = gaugeFill;

            // 성공 확률 텍스트 애니메이션
            float currentSuccess = Mathf.Lerp(startSuccessChance, endSuccessChance, t);
            successChanceText.text = $"성공 확률: {Mathf.RoundToInt(currentSuccess)}%";

            yield return null;
        }

        persuadeGauge.fillAmount = endValue;
        successChanceText.text = $"성공 확률: {Mathf.RoundToInt(endSuccessChance)}%";

        IsGaugeAnimationDone = true;
    }


    public void Hide()
    {
        panel.SetActive(false);
    }

    public void ShowSpeech(string playerLine, string npcLine)
    {
        if (!string.IsNullOrEmpty(playerLine))
        {
            var playerSpeech = GameObject.FindWithTag("Player").GetComponentInChildren<SpeechBubbleController>();
            playerSpeech.ShowSpeech(playerLine);
        }
        if (!string.IsNullOrEmpty(npcLine))
        {
            var npc = IllegalInteractionManager.Instance.CurrentNPC;
            npc?.GetComponentInChildren<SpeechBubbleController>()?.ShowSpeech(npcLine);
        }
    }

    public void HideAllSpeech()
    {
        GameObject.FindWithTag("Player")?.GetComponentInChildren<SpeechBubbleController>()?.HideSpeech();
        IllegalInteractionManager.Instance.CurrentNPC?.GetComponentInChildren<SpeechBubbleController>()?.HideSpeech();
    }


    public void UpdateTurnAndChances(int current, int max, float escapeChance, float successChance)
    {
        // 텍스트만 갱신 (표시는 여기서 안 함)
        turnInfoText.text = $"턴 {current} / {max} \n 도망 확률: {Mathf.RoundToInt(escapeChance)}%";
        successChanceText.text = $"성공 확률: {Mathf.RoundToInt(successChance)}%";
    }


    public void AnimateEscapeChance(float fromValue, float toValue, int currentTurn, int maxTurn)
    {
        if (escapeChanceRoutine != null) StopCoroutine(escapeChanceRoutine);
        escapeChanceRoutine = StartCoroutine(AnimateEscapeChanceRoutine(fromValue, toValue, currentTurn, maxTurn));
    }



    private IEnumerator AnimateEscapeChanceRoutine(float from, float to, int current, int max)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.0f;
            float value = Mathf.Lerp(from, to, t);
            turnInfoText.text = $"턴 {current} / {max} \n 도망 확률: {Mathf.RoundToInt(value)}%";
            yield return null;
        }

        turnInfoText.text = $"턴 {current} / {max} \n 도망 확률: {Mathf.RoundToInt(to)}%";
        // turnInfoText.gameObject.SetActive(true); ← 항상 표시하는 부분 제거 또는 주석
    }

    public void HideChoiceButtons()
    {
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        choiceButton3.gameObject.SetActive(false);
        finalAttemptButton.gameObject.SetActive(false);
    }

    public void ShowChoiceButtons()
    {
        choiceButton1.gameObject.SetActive(true);
        choiceButton2.gameObject.SetActive(true);
        choiceButton3.gameObject.SetActive(true);
        finalAttemptButton.gameObject.SetActive(true);
    }

    // ? 3가지 대화 선택 버튼만 숨기기
    public void HideDialogueChoiceButtons()
    {
        choiceButton1.gameObject.SetActive(false);
        choiceButton2.gameObject.SetActive(false);
        choiceButton3.gameObject.SetActive(false);
    }

    // ? 3가지 대화 선택 버튼만 보이기
    public void ShowDialogueChoiceButtons()
    {
        choiceButton1.gameObject.SetActive(true);
        choiceButton2.gameObject.SetActive(true);
        choiceButton3.gameObject.SetActive(true);
    }

    public void ShowTurnInfoTemporary(int current, int max, float escapeChance, float successChance, float duration = 3f)
    {
        if (escapeChanceRoutine != null)
            StopCoroutine(escapeChanceRoutine);

        escapeChanceRoutine = StartCoroutine(ShowTurnInfoRoutine(current, max, escapeChance, successChance, duration));
    }

    private IEnumerator ShowTurnInfoRoutine(int current, int max, float escapeChance, float successChance, float duration)
    {
        turnInfoText.text = $"턴 {current} / {max} \n 도망 확률: {Mathf.RoundToInt(escapeChance)}%";
        turnInfoText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        turnInfoText.gameObject.SetActive(false);
    }



}
