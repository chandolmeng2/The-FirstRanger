using DG.Tweening;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject[] menuButtons;
    public CanvasGroup confirmationPanel;
    public CanvasGroup settingsPanel;
    public CanvasGroup logo;
    public CanvasGroup settingsSavePanel;

    private bool isTransitioning = false;
    [SerializeField] private GameObject continueButton;

    void Start()
    {
        foreach (GameObject button in menuButtons)
        {
            button.SetActive(false);
        }

        InitPanel(confirmationPanel);
        InitPanel(settingsPanel);
        InitPanel(settingsSavePanel);

        // �ΰ�� ���̵� �θ� (���� ���̵�� SceneTransitionManager�� ó����)
        logo.alpha = 0;
        logo.gameObject.SetActive(true);
        logo.DOFade(1, 3f).OnComplete(() =>
        {
            ShowMenuButtons();
            CheckContinueButton();   // 버튼 활성화 여부 체크
        });
    }

    private void CheckContinueButton()
    {
        string expPath = System.IO.Path.Combine(Application.persistentDataPath, "exp_save.json");

        // exp 파일이 존재하고, 값이 0 초과일 때만 Continue 허용
        if (File.Exists(expPath))
        {
            int expValue = DataManager.Instance.LoadExp();
            continueButton.SetActive(expValue > 0);
        }
        else
        {
            continueButton.SetActive(false);
        }
    }
    void InitPanel(CanvasGroup panel)
    {
        panel.alpha = 0;
        panel.gameObject.SetActive(false);
    }

    void ShowMenuButtons()
    {
        if (isTransitioning) return;

        foreach (GameObject button in menuButtons)
        {
            button.SetActive(true);
            button.transform.localScale = new Vector3(0, 0, 1);
            button.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public void OnNewGameButtonClick()
    {
        if (isTransitioning) return;

        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);

        isTransitioning = true;

        HideMenuButtons(() => ShowPanel(confirmationPanel));
    }

    public void OnSettingsButtonClick()
    {
        if (isTransitioning) return;

        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);

        isTransitioning = true;

        HideMenuButtons(() => ShowPanel(settingsPanel));
    }

    public void OnNoButtonClick()
    {
        if (!confirmationPanel.gameObject.activeSelf) return;

        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);

        HidePanel(confirmationPanel, ShowMenuButtons);
        isTransitioning = false;
    }

    public void OnSettingNoButtonClick()
    {
        if (!settingsPanel.gameObject.activeSelf) return;

        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);

        HidePanel(settingsPanel, ShowMenuButtons);
        isTransitioning = false;
    }

    public void OnYesButtonClick()
    {
        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);
        DataManager.Instance.DeleteAllSaveData();
        SceneTransitionManager.Instance.LoadScene("TutorialScene1");
    }

    public void OnContinueButtonClick()
    {
        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);
        SceneTransitionManager.Instance.LoadScene("LobbyScene");
    }

    public void OnSettingsSaveButtonClick()
    {
        if (!settingsPanel.gameObject.activeSelf) return;

        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);

        ShowPanel(settingsSavePanel);

        DOVirtual.DelayedCall(1.5f, () =>
        {
            HidePanel(settingsSavePanel, ShowMenuButtons);
            HidePanel(settingsPanel, ShowMenuButtons);
            isTransitioning = false;
        });
    }

    public void OnShutOffButtonClick() //���� ����� �� ���� �ٲ�ߵ�!!
    {
        // ��ư Ŭ�� ����
        SoundManager.Instance.Play(SoundKey.UIClick_Button);

        //�̰� �ΰ��� ��Ȱ��ȭ ����Ҷ� �������
        DOTween.KillAll();       
        DOTween.Clear(true);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

    void HideMenuButtons(TweenCallback onComplete)
    {
        foreach (GameObject button in menuButtons)
        {
            button.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }

        DOVirtual.DelayedCall(0.3f, () =>
        {
            foreach (GameObject button in menuButtons)
            {
                button.SetActive(false);
            }
            onComplete?.Invoke();
        });
    }
}
