using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 씬 내 타이머/스케줄/성공-실패 판정 담당
public class TimedMissionController : MonoBehaviour, IMissionManager
{
    [System.Serializable]
    public class EventEntry
    {
        public GameObject EventPrefab;
        public float StartTime;
        public Vector3 SpawnPosition;
        public Vector3 SpawnEuler;
    }

    [Header("IMissionManager fields")]
    [SerializeField] private int index = 0;
    [SerializeField] private int expValue = 0;

    [Header("Mission Settings")]
    [SerializeField] private float missionDurationSeconds = 180f;
    [SerializeField] private List<EventEntry> events = new List<EventEntry>();
    [SerializeField] private bool autoEndWhenAllResolved = true;
    [SerializeField] private bool pauseTimerDuringQTE = true;

    [Header("Optional UI")]

    [SerializeField] private Image timerFill; // 채워질 바(Image)
    [SerializeField] private Text timerText;  // 있던 텍스트 필드 쓰면 OK (없으면 생략 가능)

    [Header("Suppression Gauge (저지율 바)")]
    [SerializeField, Range(0f, 1f)] float gauge = 0f;      // 시작 저지율(0~1)
    [SerializeField, Range(0f, 1f)] float winThreshold = 1f; // 굿엔딩 임계치(1=100%)
    [SerializeField] float rewardPerCorrect = 0.1f;       // 올바른 조치 1회당 +10%
    [SerializeField] float penaltyPerWrong = 0.1f;       // 잘못된 조치 1회당 -10%
    [SerializeField] float passiveDecayPerSec = 0f;       // 초당 자연 감소(압박감; 예: 0.01f)
    [SerializeField] bool winInstantlyAtThreshold = true; // 임계치 도달 즉시 성공
    [SerializeField] bool loseInstantlyAtZero = false;// 0 되면 즉시 실패(선택)


    // IMissionManager 구현
    public int Index => index;
    public int ExpValue => expValue;
    // 결과 화면으로 넘어가도 되는가? (미션 종료 시 true)
    public bool GotoResult { get; private set; }
    // 체크 기준 시간(여기선 제한시간 의미로 사용)
    public float CheckTime => missionDurationSeconds;

    // 상태
    public float RemainingTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    readonly List<MissionEvent> _active = new List<MissionEvent>();
    int _total, _resolved;
    Coroutine _timerCo, _schedCo;

    public void Start()
    {
        StartMission();
    }

    public void StartMission()
    {
        Cleanup();
        GotoResult = false;
        RemainingTime = Mathf.Max(1f, missionDurationSeconds);
        _total = events != null ? events.Count : 0;
        _resolved = 0;
        IsRunning = true;
        IsPaused = false;

        if (timerFill) timerFill.fillAmount = gauge;  // 바는 저지율로 사용

        if (events != null) events.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        _timerCo = StartCoroutine(TimerTick());
        _schedCo = StartCoroutine(Scheduler());
        UpdateTimerUI();
    }

    public void PauseMission(bool pause)
    {
        if (!IsRunning) return;
        IsPaused = pause;
    }

    // QTE 연동용 (선택)
    public void OnQTEStart() { if (pauseTimerDuringQTE) PauseMission(true); }
    public void OnQTEEnd() { if (pauseTimerDuringQTE) PauseMission(false); }

    IEnumerator TimerTick()
    {
        while (IsRunning && RemainingTime > 0f)
        {
            if (!IsPaused)
            {
                RemainingTime -= Time.deltaTime;

                // ▶ 패시브 하락(선택): 압박감 연출
                if (passiveDecayPerSec > 0f)
                {
                    gauge = Mathf.Clamp01(gauge - passiveDecayPerSec * Time.deltaTime);
                    UpdateProgressUI();
                    if (loseInstantlyAtZero && gauge <= 0f) { EndFail(); yield break; }
                }

                UpdateTimerUI(); // 텍스트만 갱신
            }
            yield return null;
        }

        if (IsRunning)
        {
            // 시간이 다 됐을 때: 저지율 임계치 충족여부로 판단해도 됨
            if (gauge >= winThreshold) EndSuccess();
            else EndFail();
        }
    }


    IEnumerator Scheduler()
    {
        if (events == null || events.Count == 0) yield break;

        float elapsed = 0f;
        int i = 0;

        while (IsRunning && i < events.Count)
        {
            if (!IsPaused)
            {
                elapsed += Time.deltaTime;

                while (i < events.Count && elapsed >= events[i].StartTime)
                {
                    SpawnEvent(events[i]);
                    i++;
                }
            }
            yield return null;
        }
    }

    void SpawnEvent(EventEntry e)
    {
        if (!e.EventPrefab) return;

        var rot = Quaternion.Euler(e.SpawnEuler);
        var go = Instantiate(e.EventPrefab, e.SpawnPosition, rot);

        var evt = go.GetComponent<MissionEvent>();
        if (!evt) { Destroy(go); return; }

        evt.Initialize(this);
        _active.Add(evt);
        evt.Begin();
    }

    public void EventResolved(MissionEvent evt)
    {
        if (!_active.Contains(evt)) return;
        _resolved++;
        _active.Remove(evt);

        if (autoEndWhenAllResolved && _resolved >= _total && RemainingTime > 0f)
            EndSuccess();
    }

    void EndSuccess()
    {
        if (!IsRunning) return;
        IsRunning = false;
        StopCo();
        AbortAll();

        // 저장: 전역 MissionManager 사용 (네가 이미 보유)
        MissionManager.Instance.OnMissionClear(Index);

        GotoResult = true;
    }

    void EndFail()
    {
        if (!IsRunning) return;
        IsRunning = false;
        StopCo();
        AbortAll();

        GotoResult = true;
    }

    void AbortAll()
    {
        foreach (var e in _active) e.Abort();
        _active.Clear();
    }

    void StopCo()
    {
        if (_timerCo != null) StopCoroutine(_timerCo);
        if (_schedCo != null) StopCoroutine(_schedCo);
        _timerCo = _schedCo = null;
    }

    void Cleanup()
    {
        StopCo();
        AbortAll();
    }

    void UpdateTimerUI()
    {
        if (!timerText) return;
        float t = Mathf.Max(0f, RemainingTime);
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }


    void UpdateProgressUI()
    {
        if (timerFill) timerFill.fillAmount = Mathf.Clamp01(gauge);
    }

    public void ReportEventOutcome(bool correct, float weight = 1f)
    {
        if (!IsRunning) return;

        if (correct) gauge += rewardPerCorrect * Mathf.Max(0f, weight);
        else gauge -= penaltyPerWrong * Mathf.Max(0f, weight);

        gauge = Mathf.Clamp01(gauge);
        UpdateProgressUI();

        if (winInstantlyAtThreshold && gauge >= winThreshold) EndSuccess();
        if (loseInstantlyAtZero && gauge <= 0f) EndFail();
    }


}
