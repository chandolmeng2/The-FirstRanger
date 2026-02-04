using UnityEngine;
using System.Collections;

public class Mission6Manager : MonoBehaviour, IMissionManager
{
    public int index = 6; // 미션 번호
    public int expValue = 60; // 미션 경험치

    public static Mission6Manager Instance { get; private set; }

    public bool gotoResult = false;
    public float checkTime; // 타이머 없음, IMissionManager 때문에 남김

    public int Index => index;
    public int ExpValue => expValue;
    public bool GotoResult => gotoResult;
    public float CheckTime => checkTime;

    [Header("설정")]
    [SerializeField] private string resultSceneName;
    [SerializeField] private GameObject missionCompleteText;
    [SerializeField] private GameObject goalTriggerObject;

    [Header("상태")]
    [SerializeField] private bool missionCompleted = false; // Inspector에서 토글 가능

    public bool MissionCompleted
    {
        get => missionCompleted;
        set
        {
            if (!missionCompleted && value) // false -> true 될 때만
            {
                missionCompleted = true;
                Debug.Log("MissionCompleted 수동 토글됨 → GoalTrigger 활성화");
                if (missionCompleteText != null)
                    missionCompleteText.SetActive(true);
                if (goalTriggerObject != null)
                    goalTriggerObject.SetActive(true);
            }
            else
            {
                missionCompleted = value;
            }
        }
    }

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
        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);

        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(false);
    }

    public void OnJournalCompleted()
    {
        MissionCompleted = true;
    }

    public void CallResult()
    {
        SceneTransitionManager.Instance.ResultLoadScene(ResultSceneName);
    }
}
