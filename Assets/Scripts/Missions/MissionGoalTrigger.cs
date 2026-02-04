using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionGoalTrigger : MonoBehaviour
{
    private Mission1Manager mission1Manager;
    private Mission2Manager mission2Manager;
    private Mission3Manager mission3Manager;
    private Mission4Manager mission4Manager;
    private Mission6Manager mission6Manager;
    private MissionPanel missionPanel;

    private void Start()
    {
        mission1Manager = FindObjectOfType<Mission1Manager>();
        mission2Manager = FindObjectOfType<Mission2Manager>();
        mission3Manager = FindObjectOfType<Mission3Manager>();
        mission4Manager = FindObjectOfType<Mission4Manager>();
        mission6Manager = FindObjectOfType<Mission6Manager>();
        missionPanel = FindObjectOfType<MissionPanel>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && mission1Manager != null && mission1Manager.MissionCompleted)
        {
            mission1Manager.gotoResult = true;  // 현규 추가 코드 : 트리거 발동되면 미션매니저의 불값 변경
            mission1Manager.CallResult();
        }

        // 미션2가 있을 경우
        if (other.CompareTag("Player") && mission2Manager != null && mission2Manager.MissionCompleted)
        {
            mission2Manager.gotoResult = true;
            mission2Manager.CallResult();
        }

        // 미션3가 있을 경우
        if (other.CompareTag("Player") && mission3Manager != null && mission3Manager.MissionCompleted)
        {
            mission3Manager.gotoResult = true;
            mission3Manager.CallResult();
        }

        // 미션4 
        if (mission4Manager != null && mission4Manager.MissionCompleted)
        {
            mission4Manager.gotoResult = true;
            mission4Manager.CallResult();
        }

        if (mission6Manager != null && mission6Manager.MissionCompleted)
        {
            mission6Manager.gotoResult = true;
            mission6Manager.CallResult();
        }
    }
}
