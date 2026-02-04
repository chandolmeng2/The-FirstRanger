using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class Mission1Manager : MonoBehaviour, IMissionManager
{
    public int index = 1; // �̼� ��ȣ
    public int expValue = 60; // �̼� ����ġ
    // ���� �߰� �ڵ� : �ν��Ͻ�ȭ�ؼ� ResultManager���� �����ϰԲ� �� 
    public static Mission1Manager Instance { get; private set; }
    public bool gotoResult = false;
    public float checkTime;
    private MissionPanel missionPanel;
    private bool hasLoadedResult = false;

    public int Index => index;
    public int ExpValue => expValue;
    public bool GotoResult => gotoResult;
    public float CheckTime => checkTime;
    //

    [Header("����")]
    [SerializeField] private int targetCount = 3;
    [SerializeField] private string resultSceneName;  // �ν����Ϳ��� ���� // ���� �߰� �ڵ� : ������ ���� clear -> result�� �̸� �ٲپ����� �����ϼ�
    [SerializeField] private GameObject missionCompleteText; // UI �ؽ�Ʈ ������Ʈ (��Ȱ��ȭ ����)
    [SerializeField] private GameObject goalTriggerObject; // �繫�� Ʈ���� ������Ʈ

    private int currentCount = 0;
    public bool missionCompleted = false; //�繫�� ���� ���� Ȯ�ο� // 250528 �ۺ�� �������ǳ�?

    public int TargetCount => targetCount;
    public int CurrentCount => currentCount;
    public bool MissionCompleted => missionCompleted;
    public string ResultSceneName => resultSceneName; // ���� �߰� �ڵ� : ������ ���� clear -> result�� �̸� �ٲپ����� �����ϼ�

    // ���� �߰� �ڵ� : �̱��� �ν��Ͻ� ����
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
    //
    void Start()
    {
        missionPanel = FindObjectOfType<MissionPanel>();

        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);

        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(false); // ó������ ��Ȱ��ȭ

    }

    void Update()
    {
        if(!hasLoadedResult && missionPanel.remainingTime <= 0)
        {
            hasLoadedResult = true;
            missionPanel.remainingTime = 0;
            CallResult();
        }
    }
    public void AddTrashCount()
    {
        currentCount++;

        if (currentCount >= targetCount && !missionCompleted)
        {
            missionCompleted = true;
            Debug.Log("������ ���� �Ϸ�! �繫�Ƿ� ���ư�����.");
            StartCoroutine(ShowMissionCompleteMessage());
        }
    }

    private IEnumerator ShowMissionCompleteMessage()
    {
        if (missionCompleteText != null)
            missionCompleteText.SetActive(true);

        // ��ǥ ������Ʈ Ȱ��ȭ
        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);
    }

    public void CallResult()
    {
        checkTime = missionPanel.remainingTime;
        missionPanel.isTimer = false;
        SceneTransitionManager.Instance.ResultLoadScene(ResultSceneName);
    }
}
