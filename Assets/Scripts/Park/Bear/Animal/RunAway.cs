using UnityEngine; // Unity 엔진 기능
using UnityEngine.AI; // NavMeshAgent 기능 사용
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수 시스템
using BehaviorDesigner.Runtime.Tasks; // 태스크 관련 기능
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // Tooltip 어트리뷰트 별칭 지정

[TaskCategory("Animal/Behavior")] // Behavior Designer 트리 카테고리 지정
[TaskDescription("플레이어가 가까이 오면 한 번 좌표를 찍고 도망가고, 장애물로 인해 도망 방향이 막히면 방향을 보정합니다.")]
public class RunAway : Action
{
    [Tooltip("도망 속도")]
    public SharedFloat runSpeed = 25f; // NavMeshAgent의 도망 속도

    [Tooltip("플레이어로부터 이 거리 이상 도망가면 성공 처리")]
    public SharedFloat safeDistance = 12f; // 도망 성공 거리

    [Tooltip("플레이어로부터 도망치는 목표 거리")]
    public SharedFloat fleeDistance = 4f; // 도망 좌표 거리

    private NavMeshAgent navMeshAgent; // 이동 제어용 NavMeshAgent
    private GameObject player; // 플레이어 오브젝트
    private Animator animator; // 애니메이션 제어용
    private Vector3 fleeTarget; // 도망 목적지 좌표
    private bool hasFleeTarget = false; // 좌표가 설정되었는지 여부
    private int maxAttempts = 5; // 보정 최대 시도 횟수
    private float angleIncrement = 30f; // 각도 증가 단위 (도 단위)

    // 태스크 시작 시 호출
    public override void OnStart()
    {
        navMeshAgent = GetComponent<NavMeshAgent>(); // NavMeshAgent 가져오기
        animator = GetComponent<Animator>(); // Animator 가져오기
        player = GameObject.FindGameObjectWithTag("Player"); // 태그로 플레이어 찾기

        if (navMeshAgent == null)
        {
            Debug.LogError("❌ NavMeshAgent가 없습니다.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ Player를 찾을 수 없습니다.");
            return;
        }

        navMeshAgent.isStopped = false; // 이동 시작
        navMeshAgent.speed = runSpeed.Value; // 속도 설정

        if (animator != null)
        {
            animator.SetBool("isRunning", true); // 도망 애니메이션
            animator.SetBool("isWalking", false); // 걷기 중지
        }

        SetFleeTarget(); // 도망 좌표 설정
    }

    // 매 프레임마다 실행
    public override TaskStatus OnUpdate()
    {
        if (player == null || navMeshAgent == null)
        {
            return TaskStatus.Failure; // 필수 요소 누락 시 실패
        }

        float distance = Vector3.Distance(transform.position, player.transform.position); // 플레이어와 거리 계산

        if (distance >= safeDistance.Value)
        {
            Debug.Log("✅ 도망 성공, 안전 거리 확보");
            return TaskStatus.Success; // 안전 거리 확보 시 성공
        }

        if (!hasFleeTarget)
        {
            SetFleeTarget(); // 도망 좌표 재설정
        }

        return TaskStatus.Running; // 계속 도망 중
    }

    // 태스크 종료 시 호출
    public override void OnEnd()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true; // 에이전트 정지
        }

        if (animator != null)
        {
            animator.SetBool("isRunning", false); // 애니메이션 정지
        }

        hasFleeTarget = false; // 초기화
        Debug.Log("🛑 도망 종료");
    }

    // 도망 좌표 설정 함수 (보정 로직 포함)
    private void SetFleeTarget()
    {
        Vector3 directionAway = (transform.position - player.transform.position).normalized; // 기본 반대 방향
        float radius = fleeDistance.Value; // 목표 거리 반영
        bool found = false; // 성공 여부 저장

        for (int i = 0; i < maxAttempts; i++) // 여러 방향으로 보정 시도
        {
            float angle = angleIncrement * i; // 회전 각도 설정

            // 시계 방향 회전 방향
            Vector3 rotatedDir = Quaternion.Euler(0, angle, 0) * directionAway;
            Vector3 potentialTarget = transform.position + rotatedDir * radius;

            if (NavMesh.SamplePosition(potentialTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                fleeTarget = hit.position;
                navMeshAgent.SetDestination(fleeTarget);
                hasFleeTarget = true;
                Debug.Log($"📍 도망 좌표 설정 완료 (보정됨): {fleeTarget}");
                found = true;
                break;
            }

            // 반시계 방향 회전 방향
            rotatedDir = Quaternion.Euler(0, -angle, 0) * directionAway;
            potentialTarget = transform.position + rotatedDir * radius;

            if (NavMesh.SamplePosition(potentialTarget, out hit, 2f, NavMesh.AllAreas))
            {
                fleeTarget = hit.position;
                navMeshAgent.SetDestination(fleeTarget);
                hasFleeTarget = true;
                Debug.Log($"📍 도망 좌표 설정 완료 (보정됨): {fleeTarget}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning("⚠️ 도망 좌표 설정 실패: NavMesh에서 유효하지 않음");
        }
    }
}
