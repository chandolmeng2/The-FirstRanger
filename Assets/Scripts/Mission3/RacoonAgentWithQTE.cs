using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RacoonAgentWithQTE : Agent
{
    public Transform player;
    private Animator animator;
    public float agentSpeed = 3f;
    public float detectionRadius = 25f; // 플레이어 감지 기준(이지만, 결국 학습이 잘 안된듯)
    public QTEManager qteManager;

    [Header("Movement Bounds")]
    public Vector3 areaCenter; // 반경 중심
    public float areaRadius = 30f; // 중심으로부터 움직일 수 있는 반경 거리

    private Rigidbody agentRb;
    private bool isEscaping = false;
    private bool isInQTE = false;
    private bool isFrozen = false;

    private bool forceEscapeOverride = false; // ML뿐만 아니라 스크립트로 일부 조정(플레이어 방향 및 위치 파악)

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void ResetAgent() // 라쿤 생성 전 리셋
    {
        transform.position = areaCenter;
        transform.rotation = Quaternion.identity;
        isEscaping = false;
        isInQTE = false;
        forceEscapeOverride = false;

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public override void CollectObservations(VectorSensor sensor) // 관측 센서(머신러닝에서 가장 중요한 것, 일종의 뇌?)
    {
        sensor.AddObservation(transform.position); 
        sensor.AddObservation(player.position);
        sensor.AddObservation(Vector3.Distance(transform.position, player.position)); // 총 7개(3+3+1) 이하로 유지
    }

    // 이 함수는 ML(머신러닝) 부분인데 현재 플레이어의 방향, 위치 탐지를 정확하게 못하는 듯 하여 강제 스크립트를 추가함
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isEscaping || isInQTE || isFrozen) return; // 도망 상태가 아니거나, QTE 도중이거나, 강제 정지이면 동작 중단

        UpdateEscapeOverride(); // 플레이어가 가까우면 강제 스크립트 도주로 전환

        Vector3 move;
        if (forceEscapeOverride && player != null)
        {
            move = (transform.position - player.position).normalized; // ML 대신 스크립트 사용
        }
        else
        {
            move = new Vector3(actions.ContinuousActions[0], 0f, actions.ContinuousActions[1]).normalized; // ML 사용
        }
        Vector3 nextPos = transform.position + move.normalized * agentSpeed * Time.deltaTime;
        agentRb.MovePosition(ClampToArea(nextPos)); // 지정된 반경으로 이동 범위 제한

        if (move != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.2f); // 회전
            if (animator != null) animator.SetBool("isRunning", true); // 애니메이션은 달리는 것으로(샘플에 있어서 씀)
        }
        else
        {
            if (animator != null) animator.SetBool("isRunning", false); // 멈추면 달리기 안함
        }
    }

    private void Update() // 감지 범위 내에 들어오면 그 때부터 도주 시작
    {
        if (isInQTE || isEscaping) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
            isEscaping = true;
    }

    private void UpdateEscapeOverride() // 얘가 이제 강제 스크립트를 위한 함수이며, 정면 방향으로 3f 안에 플레이어가 있으면 그 반대 방향으로 도주
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= 3f)
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, direction.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            {
                if (hit.transform == player)
                {
                    forceEscapeOverride = true;
                    return;
                }
            }
        }

        forceEscapeOverride = false;
    }

    public void TriggerQTE() // QTE 트리거 발동
    {
        if (isInQTE || !isEscaping) return;

        isInQTE = true;
        isFrozen = true; // 너구리 얼음

        var playerController = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false; // 플레이어 컨트롤 정지

        // 발소리 강제 정지
        SoundManager.Instance.StopWalkingLoop();

        qteManager.TriggerQTE(OnQTESuccess, OnQTEFail); // 성공 OR 실패 데이터 공유
    }

    private void OnQTESuccess()
    {
        isFrozen = false; // 너구리 땡

        var playerController = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = true; // QTE 끝났으므로 플레이어 컨트로 재실행

        // 포획 성공 효과음
        SoundManager.Instance.Play(SoundKey.Mission3_QTE_Success);

        Mission3Manager.Instance.OnRacoonCaught(true);
        Destroy(gameObject);  // 너구리 제거
    }

    private void OnQTEFail()
    {
        isFrozen = false; // 너구리 땡

        var playerController = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = true; // QTE 끝났으므로 플레이어 컨트로 재실행

        Mission3Manager.Instance.OnRacoonCaught(false);
        Destroy(gameObject);  // 너구리 제거
    }

    private Vector3 ClampToArea(Vector3 pos) // 반경 지역 정의
    {
        Vector3 offset = pos - areaCenter;
        offset.y = 0;
        if (offset.magnitude > areaRadius)
            pos = areaCenter + offset.normalized * areaRadius;

        pos.y = areaCenter.y;
        return pos;
    }

    
}
