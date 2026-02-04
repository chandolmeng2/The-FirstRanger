using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class Mission4Manager : MonoBehaviour, IMissionManager
{
    public int index = 4; // �̼� ��ȣ
    public int expValue = 80; // �̼� ����ġ

    public static Mission4Manager Instance { get; private set; }
    public bool gotoResult = false;
    public float checkTime;

    private MissionPanel missionPanel;
    private bool hasLoadedResult = false;

    public int Index => index;
    public int ExpValue => expValue;
    public bool GotoResult => gotoResult;
    public float CheckTime => checkTime;

    [Header("����")]
    [SerializeField] private int targetCount = 1; // ������ 1��� ��ġ�ϸ� Ŭ����
    [SerializeField] private string resultSceneName;
    [SerializeField] private GameObject missionCompleteText;
    [SerializeField] private GameObject goalTriggerObject;

    private int currentCount = 0;
    public bool missionCompleted = false;

    public int TargetCount => targetCount;
    public int CurrentCount => currentCount;
    public bool MissionCompleted => missionCompleted;
    public string ResultSceneName => resultSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        missionPanel = FindObjectOfType<MissionPanel>();
        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);
        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(false);
    }

    void Update()
    {
        if (!hasLoadedResult && missionPanel != null && missionPanel.remainingTime <= 0)
        {
            hasLoadedResult = true;
            missionPanel.remainingTime = 0;
            CallResult();
        }
    }

    /// <summary>
    /// ������ ���� ���� �� ȣ�� (CriminalSpawnerManager �Ǵ� IllegalInteractionManager���� ȣ��)
    /// </summary>
    public void AddPersuadedCount()
    {
        currentCount++;
        Debug.Log($"[Mission4] 설득 성공 처리됨 -> 카운트 {currentCount}/{targetCount}");

        if (currentCount >= targetCount && !missionCompleted)
        {
            missionCompleted = true;
            Debug.Log("[Mission4] 목표 달성! goalTrigger 활성화 진행");
            StartCoroutine(ShowMissionCompleteMessage());
        }
    }

    private IEnumerator ShowMissionCompleteMessage()
    {
        if (missionCompleteText != null)
            missionCompleteText.SetActive(true);

        if (goalTriggerObject != null)
        {
            goalTriggerObject.SetActive(true);
            Debug.Log("[Mission4] goalTrigger 활성화됨");
        }

        yield return new WaitForSeconds(4f);

        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);
    }

    public void CallResult()
    {
        if (missionPanel != null)
        {
            checkTime = missionPanel.remainingTime;
            missionPanel.isTimer = false;
        }
        SceneTransitionManager.Instance.ResultLoadScene(ResultSceneName);
    }
}