using UnityEngine;
using System.Collections;

public class Mission2Manager : MonoBehaviour, IMissionManager
{
    public int index = 2; // �̼� ��ȣ
    public int expValue = 30; // �̼� ����ġ
    public static Mission2Manager Instance { get; private set; }
    private MissionPanel missionPanel;
    private bool hasLoadedResult = false;
    public float checkTime;  // �̼� ���� �� ���� �ð� �����
    public bool gotoResult = false;


    [Header("����")]
    [SerializeField] private string resultSceneName;
    [SerializeField] private GameObject missionCompleteText;
    [SerializeField] private GameObject goalTriggerObject;

    private bool[] puzzleCleared = new bool[5];
    private bool missionCompleted = false;

    private bool trashPuzzleCleared = false;

    public string ResultSceneName => resultSceneName;
    public bool MissionCompleted => missionCompleted;

    public int Index => index;
    public int ExpValue => expValue;
    public bool GotoResult => gotoResult;
    public float CheckTime => checkTime;

    [Header("���� �� ��Ȱ��ȭ�� UI")]
    [SerializeField] private GameObject compassRootUI;


    public int ClearedCount
    {
        get
        {
            int count = 0;
            foreach (var cleared in puzzleCleared)
            {
                if (cleared) count++;
            }

            if (trashPuzzleCleared) count++;

            return count;
        }
    }

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

    private void Start()
    {
        missionPanel = FindObjectOfType<MissionPanel>();

        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);

        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(false);
    }

    private void Update()
    {
        // ���� �ð� �ʰ� �� �ڵ� ��� ��ȯ
        if (!hasLoadedResult && missionPanel.remainingTime <= 0)
        {
            hasLoadedResult = true;
            missionPanel.remainingTime = 0;
            CallResult();
        }

        // ���� ���� �����ؼ� ��ħ�� ǥ�� ���� ����
        if (compassRootUI != null)
        {
            bool shouldHide = PuzzleUIManager.IsPuzzleActive || PuzzleUIManager.IsLocked;
            if (compassRootUI.activeSelf == !shouldHide)
                return;

            compassRootUI.SetActive(!shouldHide);
        }      
    }


    public void ClearPuzzle(int index)
    {
        if (index < 0 || index >= puzzleCleared.Length) return;
        if (puzzleCleared[index]) return; // �̹� Ŭ������ �����̸� ����

        puzzleCleared[index] = true;

        Debug.Log($"���� {index + 1} Ŭ�����. �� Ŭ���� ��: {ClearedCount}/5");

        if (ClearedCount >= 5 && !missionCompleted)
        {
            missionCompleted = true;
            StartCoroutine(ShowMissionCompleteMessage());
        }
    }

    private IEnumerator ShowMissionCompleteMessage()
    {
        if (missionCompleteText != null)
            missionCompleteText.SetActive(true);

        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);
    }

    public bool IsPuzzleCleared(int index)
    {
        if (index < 0 || index >= puzzleCleared.Length) return false;
        return puzzleCleared[index];
    }

    public void ClearTrashPuzzle()
    {
        if (trashPuzzleCleared) return;
        trashPuzzleCleared = true;
        Debug.Log("������ ���� ���� Ŭ�����!");

        if (ClearedCount >= 5 && !missionCompleted)
        {
            missionCompleted = true;
            Debug.Log("��� ���� Ŭ����! �繫�Ƿ� ���ư�����.");
            StartCoroutine(ShowMissionCompleteMessage());
        }
    }

    public bool IsTrashPuzzleCleared()
    {
        return trashPuzzleCleared;
    }

    public void CallResult()
    {
        if (missionPanel != null)
        {
            checkTime = missionPanel.remainingTime;
        }
        SceneTransitionManager.Instance.ResultLoadScene(resultSceneName);
    }
}

