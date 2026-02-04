using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTEManager : MonoBehaviour
{    
    [Header("UI")]
    public GameObject keyBoxPrefab; // 키 패드 모양 프리팹
    public Transform keyContainer;
    public Slider timerSlider; // 시간 타이머
    public GameObject QTEPanel; // QTE 전체 UI 패널

    [Header("Sprites")]
    // 검정 키
    public Sprite QSprite;
    public Sprite WSprite;
    public Sprite ESprite;
    public Sprite RSprite;
    public Sprite ASprite;
    public Sprite SSprite;
    public Sprite DSprite;
    public Sprite FSprite;

    // 하얀 키
    public Sprite QHighlightSprite;
    public Sprite WHighlightSprite;
    public Sprite EHighlightSprite;
    public Sprite RHighlightSprite;
    public Sprite AHighlightSprite;
    public Sprite SHighlightSprite;
    public Sprite DHighlightSprite;
    public Sprite FHighlightSprite;

    // 딕셔너리로 저장
    private Dictionary<string, Sprite> keySpriteMap;
    private Dictionary<string, Sprite> keyHighlightSpriteMap;

    // 리스트로 저장
    private List<string> currentKeySequence;
    private List<Image> keyBoxImages = new List<Image>();

    // 각 성공, 실패 불러들이기
    private System.Action onSuccessCallback;
    private System.Action onFailCallback;

    [Header("Settings")]
    public float qteDuration = 3f; // 제한 시간
    private float timeLeft;
    private int currentIndex = 0;
    private bool isQTEActive = false;

    public static bool IsQTEActive { get; private set; } = false; // 외부 접근가능한 프로퍼티

    private string[] availableKeys = { "Q", "W", "E", "R", "A", "S", "D", "F" }; // 키 구성

    void Start()
    {
        InitSpriteMaps();
        if (QTEPanel != null) QTEPanel.SetActive(false); // 시작 시 꺼놓기
    }

    void Update()
    {
        if (!isQTEActive) return;

        timeLeft -= Time.deltaTime;
        timerSlider.value = timeLeft / qteDuration;

        if (timeLeft <= 0f)
        {
            FailQTE();
        }

        HandleInput();
    }

    public void TriggerQTE(System.Action onSuccess, System.Action onFail) // 트리거 켜지면 성공 OR 실패 확인
    {
        onSuccessCallback = onSuccess;
        onFailCallback = onFail;
        StartQTE();
    }

    void StartQTE()
    {
        if (QTEPanel != null)
            QTEPanel.SetActive(true); // 패널 켜기

        isQTEActive = true;
        IsQTEActive = true;
        timeLeft = qteDuration;
        timerSlider.value = 1f;
        currentIndex = 0;

        foreach (Transform child in keyContainer)
        {
            Destroy(child.gameObject);
        }
        keyBoxImages.Clear();

        int randomLength = Random.Range(5, 10); // 5 이상 10 미만 → 5~9 사이

        currentKeySequence = GenerateRandomKeySequence(randomLength); // 5~9 개 나옴
        foreach (string key in currentKeySequence)
        {
            GameObject box = Instantiate(keyBoxPrefab, keyContainer);
            Image img = box.GetComponent<Image>();
            img.sprite = keySpriteMap[key];
            img.color = Color.white;
            keyBoxImages.Add(img);
        }

        HighlightCurrentKey();
    }

    void HandleInput()
    {
        if (Input.anyKeyDown && currentIndex < currentKeySequence.Count)
        {
            string input = Input.inputString.ToUpper();

            if (string.IsNullOrEmpty(input) || input.Length != 1)
                return;

            if (input == currentKeySequence[currentIndex])
            {
                //마지막 성공만 제외한 버튼 잘 누름 효과음
                if (currentIndex < currentKeySequence.Count - 1)
                {
                    SoundManager.Instance.Play(SoundKey.Mission3_QTE_CorrectKey);
                }

                keyBoxImages[currentIndex].color = Color.green;
                currentIndex++;

                if (currentIndex >= currentKeySequence.Count)
                    SuccessQTE();
                else
                    HighlightCurrentKey();
            }
            else
            {
                // 잘못된 키 입력 효과음
                SoundManager.Instance.Play(SoundKey.Mission3_QTE_WrongKey);
            }
        }
    }

    void HighlightCurrentKey() // 현재 키는 하얀색, 맞춘 것은 초록색, 남은 것은 검정색
    {
        for (int i = 0; i < keyBoxImages.Count; i++)
        {
            string key = currentKeySequence[i];
            Image img = keyBoxImages[i];

            if (i < currentIndex)
            {
                img.sprite = keyHighlightSpriteMap[key];
                img.color = Color.green;
            }
            else if (i == currentIndex)
            {
                img.sprite = keyHighlightSpriteMap[key];
                img.color = Color.white;
            }
            else
            {
                img.sprite = keySpriteMap[key];
                img.color = Color.white;
            }
        }
    }

    private void SuccessQTE()
    {
        Debug.Log("QTE 성공!");
        isQTEActive = false;
        IsQTEActive = false;

        if (QTEPanel != null)
            QTEPanel.SetActive(false); // 패널 끄기

        onSuccessCallback?.Invoke(); // 성공 넘기기
    }

    private void FailQTE()
    {
        Debug.Log("QTE 실패!");
        isQTEActive = false;
        IsQTEActive = false;

        if (QTEPanel != null)
            QTEPanel.SetActive(false); // 패널 끄기

        onFailCallback?.Invoke(); // 실패 넘기기
    }

    void InitSpriteMaps()
    {
        keySpriteMap = new Dictionary<string, Sprite>()
        {
            { "Q", QSprite }, { "W", WSprite }, { "E", ESprite }, { "R", RSprite },
            { "A", ASprite }, { "S", SSprite }, { "D", DSprite }, { "F", FSprite }
        };

        keyHighlightSpriteMap = new Dictionary<string, Sprite>()
        {
            { "Q", QHighlightSprite }, { "W", WHighlightSprite }, { "E", EHighlightSprite }, { "R", RHighlightSprite },
            { "A", AHighlightSprite }, { "S", SHighlightSprite }, { "D", DHighlightSprite }, { "F", FHighlightSprite }
        };
    }

    List<string> GenerateRandomKeySequence(int length) // 키 배열 랜덤으로 생성
    {
        List<string> sequence = new List<string>();
        for (int i = 0; i < length; i++)
        {
            int idx = Random.Range(0, availableKeys.Length);
            sequence.Add(availableKeys[idx]);
        }
        return sequence;
    }
}
