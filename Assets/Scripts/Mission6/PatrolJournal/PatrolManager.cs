using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PatrolManager : MonoBehaviour
{
    public static PatrolManager Instance;

    [System.Serializable]
    public class PatrolStage
    {
        public GameObject patrolZone;       // 순찰 지점 트리거 오브젝트
        public GameObject interactTarget;   // 상호작용 대상 오브젝트
        public string enterMessage;         // 접근 시 출력할 텍스트
        public string interactMessage;      // E키 상호작용 후 출력할 텍스트
    }

    public List<PatrolStage> patrolStages = new List<PatrolStage>();
    private int currentIndex = 0;

    private void Awake() => Instance = this;

    void Start()
    {
        // 모든 순찰 지점과 상호작용 오브젝트 비활성화
        foreach (var stage in patrolStages)
        {
            if (stage.patrolZone != null)
                stage.patrolZone.SetActive(false);

            if (stage.interactTarget != null)
                stage.interactTarget.SetActive(false);
        }

        // 첫 번째 순찰 지점만 활성화
        ActivateCurrentStage();
    }

    public PatrolStage GetCurrentStage()
    {
        if (currentIndex < patrolStages.Count)
            return patrolStages[currentIndex];
        return null;
    }

    public void CompleteCurrentStage()
    {
        var current = patrolStages[currentIndex];

        // 현재 순찰 지점 오브젝트는 바로 삭제 (트리거)
        if (current.patrolZone != null)
            Destroy(current.patrolZone);
        if (current.interactTarget != null)
            Destroy(current.interactTarget);

        currentIndex++;

        if (currentIndex < patrolStages.Count)
        {
            // 다음 순찰 시작
            ActivateCurrentStage();

            // 이전 상호작용 오브젝트 삭제 (다음 거랑 안 겹치게)
            if (current.interactTarget != null)
                Destroy(current.interactTarget);
        }
        else
        {
            // 마지막 interactMessage 이후 최종 메시지를 보여주기 위해 코루틴 사용
            if (current.interactTarget != null)
                Destroy(current.interactTarget, 3f); // 메시지가 3초라면 그 후 삭제

            StartCoroutine(ShowFinalMessageAfterDelay(3f));
        }
    }

    private IEnumerator ShowFinalMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIManager.Instance.ShowMessage("모든 순찰을 완료했다. 사무실로 돌아가자.", 3f);
    }



    private void ActivateCurrentStage()
    {
        var stage = patrolStages[currentIndex];
        stage.patrolZone.SetActive(true);
        stage.interactTarget.SetActive(true);
    }

    public bool IsAllPatrolComplete()
    {
        return currentIndex >= patrolStages.Count;
    }


}
