using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Mission3Manager : MonoBehaviour, IMissionManager
{
    public int index = 3; // 미션 번호
    public int expValue = 60; // 미션 경험치
    // 현규 추가 코드 : 인스턴스화해서 ResultManager에서 참조하게끔 함 
    public static Mission3Manager Instance { get; private set; }
    public bool gotoResult = false;
    public float checkTime;
    private MissionPanel missionPanel;
    private bool hasLoadedResult = false;
    //

    [Header("설정")]
    [SerializeField] private int targetCount = 3;
    [SerializeField] private string resultSceneName;  // 인스펙터에서 설정 // 현규 추가 코드 : 종혁씨 여기 clear -> result로 이름 바꾸었으니 참고하쇼
    [SerializeField] private GameObject missionCompleteText; // UI 텍스트 오브젝트 (비활성화 상태)
    [SerializeField] private GameObject goalTriggerObject; // 사무실 트리거 오브젝트

    [SerializeField] private GameObject[] racoonPrefabs; // 서로 다른 라쿤 프리팹들
    [SerializeField] private Transform[] spawnPoints; // 생성 위치
    
    private GameObject currentRacoon;  // 현재 라쿤 참조용
    private int lastSpawnIndex = -1;

    private int currentCount = 0; // 성공한 수
    public bool missionCompleted = false; //사무실 도달 조건 확인용 // 250528 퍼블릭 돌려도되나?
    [SerializeField] private GameObject compassRootUI;


    public int TargetCount => targetCount;
    public int CurrentCount => currentCount;
    public bool MissionCompleted => missionCompleted;
    public string ResultSceneName => resultSceneName; // 현규 추가 코드 : 종혁씨 여기 clear -> result로 이름 바꾸었으니 참고하쇼

    public int Index => index;
    public int ExpValue => expValue;
    public bool GotoResult => gotoResult;
    public float CheckTime => checkTime;

    // 현규 추가 코드 : 싱글톤 인스턴스 설정
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
            goalTriggerObject.SetActive(false); // 처음에는 비활성화

        SpawnNextRacoon();  // 첫 라쿤 생성
        compassRootUI.SetActive(true);
    }

    void Update()
    {
        if(!hasLoadedResult && missionPanel.remainingTime <= 0)
        {
            hasLoadedResult = true;
            missionPanel.remainingTime = 0;
            CallResult();
        }

        if (compassRootUI != null)
        {
            bool shouldHide = QTEManager.IsQTEActive;
            compassRootUI.SetActive(!shouldHide);
        }
    }

    public void OnRacoonCaught(bool success)
    {
        if (currentRacoon != null)
            Destroy(currentRacoon);

        if (success)
            currentCount++;

        if (currentCount >= targetCount)
        {
            missionCompleted = true;
            Debug.Log("야생 동물 구출 완료! 사무실로 돌아가세요.");
            StartCoroutine(ShowMissionCompleteMessage());
            return;
        }

        SpawnNextRacoon();
    }

    private void SpawnNextRacoon() // 지정된 위치에서 라쿤이 생성, 라쿤 잡기(QTE)에 실패하면 다른 지정된 위치에서 라쿤 생성, 다 잡을 때까지(3마리) 반복
    {
        int spawnIndex;

        if (spawnPoints.Length == 1)
        {
            spawnIndex = 0; // 하나밖에 없으면 그냥 그것 사용
        }
        else
        {
            // 후보 리스트 생성 (lastSpawnIndex 제외)
            List<int> candidates = new List<int>();
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (i != lastSpawnIndex)
                    candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                // 만약 모든 위치를 다 썼거나 후보가 없으면 다시 다 허용
                for (int i = 0; i < spawnPoints.Length; i++)
                    candidates.Add(i);
            }

            spawnIndex = candidates[Random.Range(0, candidates.Count)];
        }
        lastSpawnIndex = spawnIndex;

        GameObject prefab = racoonPrefabs[spawnIndex]; // 각 위치마다 다른 ML 모델
        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        currentRacoon = Instantiate(prefab, spawnPos, Quaternion.identity);

        var agent = currentRacoon.GetComponent<RacoonAgentWithQTE>();
        if (agent != null)
        {
            agent.player = GameObject.FindWithTag("Player")?.transform;
            agent.qteManager = FindObjectOfType<QTEManager>();
            agent.ResetAgent();
        }
    }

    private IEnumerator ShowMissionCompleteMessage()
    {
        if (missionCompleteText != null)
            missionCompleteText.SetActive(true);

        // 목표 오브젝트 활성화
        if (goalTriggerObject != null)
            goalTriggerObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        if (missionCompleteText != null)
            missionCompleteText.SetActive(false);
    }

    public void CallResult()
    {
        checkTime = missionPanel.remainingTime;
        SceneTransitionManager.Instance.ResultLoadScene(ResultSceneName);
    }
}