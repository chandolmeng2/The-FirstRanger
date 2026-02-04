using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CompassManager는 나침반 UI를 관리하고,
/// 씬 내의 목표(Objective)를 나침반에 아이콘으로 표시하는 역할을 함
/// </summary>
public class CompassManager : MonoBehaviour
{
    [Header("나침반 UI 구성 요소")]
    [Tooltip("나침반의 RawImage (UV를 조절해 회전 효과를 구현)")]
    public RawImage CompassImage;

    [Tooltip("목표 아이콘들이 배치될 부모 RectTransform")]
    public RectTransform CompassObjectiveParent;

    [Tooltip("CompassObjective 프리팹")]
    public GameObject CompassObjectivePrefab;

    // 현재 등록된 모든 Objective 목록
    private readonly List<CompassObjective> _currentObjectives = new List<CompassObjective>();

    // 시작 시 목표들을 주기적으로 정렬
    private IEnumerator Start()
    {
        WaitForSeconds updateDelay = new WaitForSeconds(1);

        while (enabled)
        {
            SortCompassObjectives();
            yield return updateDelay;
        }
    }

    // 매 프레임마다 나침반의 회전 방향을 업데이트
    private void LateUpdate() => UpdateCompassHeading();

    // 나침반의 UV 좌표를 조절하여 방향을 맞춰줌
    private void UpdateCompassHeading()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        // 플레이어의 방향(y축 회전값)을 0~1 범위로 정규화
        Vector2 compassUvPosition = Vector2.right * (player.transform.rotation.eulerAngles.y / 360f);

        // 나침반 RawImage의 UV를 조정하여 회전 효과 구현
        CompassImage.uvRect = new Rect(compassUvPosition, Vector2.one);
    }

    // 현재 씬 내 모든 목표 아이콘들을 거리 기준으로 정렬
    private void SortCompassObjectives()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // WorldGameObject가 유효한 아이콘만 필터링 후 거리 기준 내림차순 정렬
        CompassObjective[] orderedObjectives = _currentObjectives
            .Where(o => o.WorldGameObject != null)
            .OrderByDescending(o => Vector3.Distance(player.transform.position, o.WorldGameObject.position))
            .ToArray();

        // 나침반 내에서 UI 순서를 재정렬
        for (int i = 0; i < orderedObjectives.Length; i++)
        {
            orderedObjectives[i].UpdateUiIndex(i);
        }
    }

    // 새로운 목표 아이콘을 나침반에 추가하고 반환
    public CompassObjective AddObjectiveForObject(GameObject compassObjectiveGameObject, Color color, Sprite sprite)
    {
        // 프리팹을 생성하고 CompassObjective 설정
        CompassObjective newObj = Instantiate(CompassObjectivePrefab, CompassObjectiveParent, false)
            .GetComponent<CompassObjective>()
            .Configure(compassObjectiveGameObject, color, sprite, this);

        // 목록에 추가
        _currentObjectives.Add(newObj);

        return newObj;
    }
}
