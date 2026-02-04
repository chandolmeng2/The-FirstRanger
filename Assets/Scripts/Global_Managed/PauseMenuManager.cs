using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    // UI
    public CanvasGroup pauseMenuPanel;
    public CanvasGroup mainMenuPanel;
    public CanvasGroup settingMenuPanel;
    public CanvasGroup shutoffMenuPanel;
    public CanvasGroup settingSavePanel;
    public GameObject[] menuButtons;

    // 유틸
    private bool isGamePaused = false;
    private bool isEscLocked = false;
    private PlayerController playerController;
    private GameObject player;
    private GameObject crosshair;

    // 현재 열려 있는 서브 패널
    private CanvasGroup currentSubPanel = null;
    public static PauseMenuManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // 중복 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InitPanel(pauseMenuPanel);
        InitPanel(mainMenuPanel);
        InitPanel(settingMenuPanel);
        InitPanel(shutoffMenuPanel);
        InitPanel(settingSavePanel);

        if (playerController == null)
        {
            Debug.LogError("FirstPersonController is not found on the player GameObject.");
        }

        crosshair = GameObject.Find("Crosshair");
        if (crosshair == null)
        {
            Debug.LogWarning("Crosshair not found in the scene.");
        }
    }

    void Update()
    {
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
            return;

        if (isEscLocked) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                if (currentSubPanel != null)
                {
                    HidePanel(currentSubPanel, ShowMenuButtons);
                    currentSubPanel = null;
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                ShowPauseMenu();
            }
        }
    }

    void InitPanel(CanvasGroup cg)
    {
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignPlayerController();

        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(true);
        }

        crosshair = GameObject.Find("Crosshair");
        if (crosshair != null)
        {
            crosshair.SetActive(!isGamePaused);
        }
    }

    private void AssignPlayerController()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    public void ShowPauseMenu()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        isGamePaused = true;
        ShowPanel(pauseMenuPanel);

        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isGamePaused = false;
        isEscLocked = true;

        pauseMenuPanel.DOFade(0f, 0.3f).OnComplete(() =>
        {
            pauseMenuPanel.gameObject.SetActive(false);
            isEscLocked = false;
        });

        currentSubPanel = null;

        if (crosshair != null)
        {
            crosshair.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMainMenuButtonClick()
    {
        HideMenuButtons();
        ShowPanel(mainMenuPanel);
        currentSubPanel = mainMenuPanel;
    }

    public void OnSettingMenuButtonClick()
    {
        HideMenuButtons();
        ShowPanel(settingMenuPanel);
        currentSubPanel = settingMenuPanel;
    }

    public void OnShutOffButtonClick()
    {
        HideMenuButtons();
        ShowPanel(shutoffMenuPanel);
        currentSubPanel = shutoffMenuPanel;
    }

    public void OnMainMenuYesButtonClick()
    {
        if (SceneManager.GetActiveScene().name == "Mission1Scene")
        {
            Destroy(Mission1Manager.Instance.gameObject);
        }
        SceneTransitionManager.Instance.LoadScene("MainMenu");
    }

    public void OnMainMenuNoButtonClick()
    {
        HidePanel(mainMenuPanel, ShowMenuButtons);
        currentSubPanel = null;
    }

    public void OnSettingMenuSaveButtonClick()
    {
        ShowPanel(settingSavePanel);

        DOVirtual.DelayedCall(1.5f, () =>
        {
            HidePanel(settingSavePanel, ShowMenuButtons);
            HidePanel(settingMenuPanel, ShowMenuButtons);
            currentSubPanel = null;
        });
    }

    public void OnSettingMenuCancelButtonClick()
    {
        HidePanel(settingMenuPanel, ShowMenuButtons);
        currentSubPanel = null;
    }

    public void OnShutOffYesButtonClick()
    {
        DOTween.KillAll();
        DOTween.Clear(true);
        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void OnShutOffNoButtonClick()
    {
        HidePanel(shutoffMenuPanel, ShowMenuButtons);
        currentSubPanel = null;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void ShowPanel(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.DOFade(1, 0.3f);
    }

    void HidePanel(CanvasGroup panel, TweenCallback onComplete = null)
    {
        panel.DOFade(0, 0.3f).OnComplete(() =>
        {
            panel.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void ShowMenuButtons()
    {
        foreach (var btn in menuButtons)
        {
            btn.SetActive(true);
        }
    }

    public void HideMenuButtons()
    {
        foreach (var btn in menuButtons)
        {
            btn.SetActive(false);
        }
    }
}