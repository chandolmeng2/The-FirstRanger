using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;  // Text를 사용하기 위해 추가

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("페이드 설정")]
    public float fadeDuration = 1f;

    // 직접 참조할 패널
    private CanvasGroup fadePanel;
    private CanvasGroup locationPanel;
    private Text locationText;  // TextMeshProUGUI -> Text로 변경

    public bool IsTransitioning { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 자식에서 패널 찾기
            fadePanel = transform.Find("Canvas/FadePanel")?.GetComponent<CanvasGroup>();
            locationPanel = transform.Find("Canvas/LocationTextPanel")?.GetComponent<CanvasGroup>();
            locationText = locationPanel?.GetComponentInChildren<Text>();  // Text 컴포넌트로 참조

            // 초기 상태 설정
            if (fadePanel != null)
            {
                fadePanel.alpha = 0f;
                fadePanel.blocksRaycasts = false;
                fadePanel.gameObject.SetActive(false);
            }

            if (locationPanel != null)
            {
                locationPanel.alpha = 0f;
                locationPanel.blocksRaycasts = false;
                locationPanel.gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public void ResultLoadScene(string sceneName)
    {
        StartCoroutine(ResultTransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        IsTransitioning = true;

        // 1. 페이드 패널 페이드 인
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            fadePanel.gameObject.SetActive(true);
            yield return fadePanel.DOFade(1f, fadeDuration).WaitForCompletion();
        }

        // 2. 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 씬 로딩 진행 중
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 3. 씬 로딩 완료 후 잠시 대기 (페이드 아웃이 씬 로딩 후 진행되도록 대기)
        yield return new WaitForSeconds(0.3f);

        // 씬이 로딩 완료되었으므로 이제 씬을 활성화
        asyncLoad.allowSceneActivation = true;

        // 4. 씬이 완전히 로드되고 활성화된 후에 페이드 아웃
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 5. 페이드 패널 페이드 아웃
        if (fadePanel != null)
        {
            yield return fadePanel.DOFade(0f, fadeDuration).WaitForCompletion();
            fadePanel.gameObject.SetActive(false);
            fadePanel.blocksRaycasts = false;
        }

        // 6. 위치 텍스트 표시
        if (!IsMenuScene())
        {
            if (sceneName.ToLower().Contains("tutorial")) //씬 이름에 튜토리얼 들어가면 걸림
            {
                yield break;
            }
            ShowLocationText(GetSceneLocationName(sceneName));
        }
    }

    private IEnumerator ResultTransitionRoutine(string sceneName)
    {
        IsTransitioning = true;

        // 1. 페이드 패널 페이드 인
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            fadePanel.gameObject.SetActive(true);
            yield return fadePanel.DOFade(1f, fadeDuration).WaitForCompletion();
        }

        // 2. 씬 비동기 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 씬 로딩 진행 중
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 3. 씬 로딩 완료 후 잠시 대기 (페이드 아웃이 씬 로딩 후 진행되도록 대기)
        yield return new WaitForSeconds(0.3f);

        // 씬이 로딩 완료되었으므로 이제 씬을 활성화
        asyncLoad.allowSceneActivation = true;

        // 4. 씬이 완전히 로드되고 활성화된 후에 페이드 아웃
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 5. 페이드 패널 페이드 아웃
        if (fadePanel != null)
        {
            yield return fadePanel.DOFade(0f, fadeDuration).WaitForCompletion();
            fadePanel.gameObject.SetActive(false);
            fadePanel.blocksRaycasts = false;
        }
    }

    private void ShowLocationText(string locationName, float duration = 2f)
    {
        StartCoroutine(LocationTextRoutine(locationName, duration));
    }

    private IEnumerator LocationTextRoutine(string locationName, float duration)
    {
        if (locationPanel != null && locationText != null)
        {
            locationText.text = locationName;
            locationPanel.alpha = 0f;
            locationPanel.gameObject.SetActive(true);

            yield return locationPanel.DOFade(1f, 0.5f).WaitForCompletion();
            yield return new WaitForSeconds(duration);
            yield return locationPanel.DOFade(0f, 0.5f).WaitForCompletion();

            locationPanel.gameObject.SetActive(false);
            IsTransitioning = false;
        }
    }

    private string GetSceneLocationName(string sceneName)
    {
        switch (sceneName)
        {
            case "LobbyScene": return "사무실";
            case "Mission1Scene": return "쓰레기 줍기";
            case "Mission2Scene": return "자재 수리";
            case "Mission3Scene": return "야생동물 포획";
            case "Mission4Scene": return "범법자 검거";
            case "Mission6Scene": return "야간 순찰";
            default: return "할당안되었으니까 할당해라 애송이";
        }
    }

    private bool IsMenuScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 메뉴 씬들에 대한 이름을 지정 (예: "MainMenu", "Settings", "PauseMenu" 등)
        if (currentScene == "MainMenu")
        {
            return true;  // 메뉴 씬인 경우 true 반환
        }

        return false;  // 그 외 씬은 false 반환
    }
}
