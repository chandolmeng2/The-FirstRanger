using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 씬 내에서 특정 위치에 배치되는 "목표 오브젝트"를 나타냄
/// 플레이어가 가까이 가면 트리거되며, CompassManager를 통해 나침반에 아이콘을 등록
/// </summary>
public class Objective : MonoBehaviour
{
    [Header("나침반 아이콘 설정")]

    [Tooltip("아이콘의 크기 (나침반에 표시될 이미지 크기)")]
    [SerializeField] private Vector3 _iconScale = Vector3.one;

    [Tooltip("아이콘 색상")]
    [SerializeField] private Color _iconColor = new Color(0, .8f, 1);

    [Tooltip("아이콘 이미지 (스프라이트)")]
    [SerializeField] private Sprite _objectiveIcon;

    [Header("목표 도달 시 실행될 이벤트")]
    [Tooltip("플레이어가 도달했을 때 실행할 이벤트")]
    [SerializeField] private UnityEvent _onCompleteEvents;

    [Header("참조할 컴패스 매니저")]
    [Tooltip("CompassManager를 수동으로 할당할 수 있음 (비워두면 자동으로 찾음)")]
    [SerializeField] private CompassManager compassManager;

    private void Start()
    {
        // CompassManager를 자동으로 찾기 (할당되지 않았을 경우)
        if (compassManager == null)
        {
            compassManager = FindObjectOfType<CompassManager>();
        }

        // 아이콘 생성 및 등록
        CompassObjective createdObjective = compassManager.AddObjectiveForObject(gameObject, _iconColor, _objectiveIcon);

        // 아이콘 크기 설정
        if (createdObjective != null)
        {
            createdObjective.iconScale = _iconScale;
            createdObjective.ObjectiveImage.transform.localScale = _iconScale; // 생성 직후에 바로 적용
        }
    }

    // 플레이어가 해당 오브젝트에 도달했을 때 실행됨
    private void OnTriggerEnter(Collider other)
    {
        _onCompleteEvents.Invoke(); // 유니티 이벤트 실행
        //Destroy(this.gameObject);   // 목표 오브젝트 제거 (한 번만 수행)
        gameObject.SetActive(false);
    }
}
