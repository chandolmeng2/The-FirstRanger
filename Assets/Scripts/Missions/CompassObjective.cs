using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이 클래스는 나침반(Compass)에 표시될 목표 오브젝트 하나를 제어
/// 플레이어와의 거리, 방향에 따라 아이콘의 위치/크기/표시 여부를 조절
/// </summary>
public class CompassObjective : MonoBehaviour
{
    // 나침반에 표시될 이미지 (아이콘)
    public Image ObjectiveImage;

    [Header("아이콘 크기 설정")]
    public Vector3 iconScale = Vector3.one; // 아이콘의 기본 크기 (인스펙터에서 조절 가능)

    // 현재 나침반에 표시되어야 하는지 여부
    public bool IsCompassObjectiveActive { get; private set; }

    // UI 위치 계산용
    private RectTransform _rectTransform;

    // 실제 목표가 되는 월드상의 게임 오브젝트 위치
    public Transform WorldGameObject { get; private set; }

    // 거리 제한 상수
    [SerializeField] private float MinVisiblityRange = 2f;
    [SerializeField] private float MaxVisiblityRange = 100f;

    // 나침반 매니저 참조 (싱글톤 사용하지 않고 직접 참조)
    private CompassManager compassManager;
    
    private void Start()
    {
        // 씬에서 CompassManager를 찾아 자동 할당 (안 되어 있을 경우 대비)
        if (compassManager == null)
        {
            compassManager = FindObjectOfType<CompassManager>();
        }
    }

    // 목표를 설정하고 초기화하는 함수
    public CompassObjective Configure(GameObject worldGameObject, Color color, Sprite sprite, CompassManager manager)
    {
        compassManager = manager;
        WorldGameObject = worldGameObject.transform;
        _rectTransform = GetComponent<RectTransform>();

        // 아이콘 색상 및 이미지 설정
        ObjectiveImage.color = color;
        if (sprite != null)
        {
            ObjectiveImage.sprite = sprite;
        }

        // 아이콘 크기 설정
        ObjectiveImage.transform.localScale = iconScale;

        // 초기 위치 계산
        UpdateCompassPosition();

        return this;
    }

    // 매 프레임의 LateUpdate에서 방향 위치 갱신
    private void LateUpdate() => UpdateCompassPosition();

    //플레이어의 방향에 따라 나침반상의 위치를 갱신하는 함수
    public void UpdateCompassPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (WorldGameObject == null || !IsCompassObjectiveActive || compassManager == null || player == null)
            return;

        Vector3 playerForward = player.transform.forward;
        float angle = Vector3.SignedAngle(
            playerForward,
            GetObjectiveDirection(WorldGameObject, player.transform),
            Vector3.up
        ) / 180f;

        // 나침반 UI의 가로 길이를 기준으로 위치 계산
        _rectTransform.localPosition = Vector2.right * angle * (compassManager.CompassImage.rectTransform.sizeDelta.x / 2f);
    }

    // 아이콘이 나타날지 여부에 따라 부드럽게 스케일 조절
    private void Update()
    {
        Vector3 targetScale = (IsCompassObjectiveActive && WorldGameObject != null) ? iconScale : Vector3.zero;
        ObjectiveImage.transform.localScale = Vector3.Lerp(ObjectiveImage.transform.localScale, targetScale, Time.deltaTime * 8f);
    }

    //정적 함수: 두 Transform 사이의 방향 각도 계산하는 함수
    public static float GetObjectiveAngle(Transform worldObjectiveTransform)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return -1f;

        return Vector3.SignedAngle(
            player.transform.forward,
            GetObjectiveDirection(worldObjectiveTransform, player.transform),
            Vector3.up
        ) / 180f;
    }

    //목표와 플레이어 간의 방향 벡터 계산 (Y축 기준 수평만)
    private static Vector3 GetObjectiveDirection(Transform objectiveTransform, Transform sourceTransform)
    {
        return (new Vector3(
            objectiveTransform.position.x,
            sourceTransform.position.y,
            objectiveTransform.position.z
        ) - sourceTransform.position).normalized;
    }

    //UI 우선순위 갱신 및 표시 여부 업데이트
    public void UpdateUiIndex(int newIndex)
    {
        _rectTransform.SetSiblingIndex(newIndex);
        UpdateVisibility();
    }

    // 플레이어와의 거리에 따라 나침반에서 표시할지 여부 결정
    private void UpdateVisibility()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float currentDistance = Vector3.Distance(WorldGameObject.position, player.transform.position);

        IsCompassObjectiveActive = currentDistance < MaxVisiblityRange &&
                                   currentDistance > MinVisiblityRange;
    }
}
