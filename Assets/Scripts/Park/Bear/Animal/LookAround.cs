using UnityEngine; // Unity 기본 기능
using BehaviorDesigner.Runtime; // Behavior Designer 전역 변수
using BehaviorDesigner.Runtime.Tasks; // 태스크 기능
using TooltipAttribute = BehaviorDesigner.Runtime.Tasks.TooltipAttribute; // 툴팁 어트리뷰트 지정

[TaskCategory("Animal/Behavior")]
[TaskDescription("주위를 둘러봅니다 (좌우로 회전합니다).")]
public class LookAround : Action
{
    [Tooltip("한 방향으로 회전할 각도")]
    public SharedFloat lookAngle = 45f; // 좌우 회전 각도

    [Tooltip("한 방향으로 회전하는 데 걸리는 시간")]
    public SharedFloat lookDuration = 1f; // 회전 시간

    private float timer = 0f; // 타이머
    private Quaternion originalRotation; // 원래 회전
    private Quaternion targetRotation; // 목표 회전
    private int direction = 1; // 회전 방향 (1: 오른쪽, -1: 왼쪽)

    public override void OnStart()
    {
        // 시작 시 원래 회전값 저장
        originalRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + direction * lookAngle.Value, 0);
        timer = 0f;

        Debug.Log("👀 주위를 둘러보기 시작");
    }

    public override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;

        // 회전 진행
        float t = Mathf.Clamp01(timer / lookDuration.Value);
        transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, t);

        if (t >= 1f)
        {
            // 방향을 바꾸고 초기화 (한 번만 하면 return Success)
            if (direction == 1)
            {
                direction = -1;
                originalRotation = transform.rotation;
                targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + direction * lookAngle.Value, 0);
                timer = 0f;
                return TaskStatus.Running;
            }
            else
            {
                Debug.Log("✅ 주위를 둘러보기 완료");
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        Debug.Log("🛑 둘러보기 종료");
    }
}
