using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    public bool isTriggered = false;
    private bool isCoroutineStarted = false;
    public Transform npcCameraTarget;
    public Camera playerCamera;
    private float cameraMoveDuration = 1.0f;
    public PlayerController playerController;
    public bool isDialogueStarted = false;
    public bool moveNextScene = false;
    public Animator npcAnimator;
    private bool notRepeated = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered && !isCoroutineStarted)
        {
            StartCoroutine(StartDialogueCamera());
            isCoroutineStarted = true;
        }

        if (!isDialogueStarted && moveNextScene && !notRepeated)
        {
            StartCoroutine(EndDialogueCamera());
        }
    }

    public IEnumerator StartDialogueCamera()
    {
        // 플레이어 컨트롤러 비활성화
        playerController.enabled = false;
        // 카메라 이동 시작
        yield return StartCoroutine(MoveCameraToTarget());
        // 카메라 이동이 완료되면 대화 시스템 시작

    }

    private IEnumerator MoveCameraToTarget()
    {
        // 원래 카메라 위치와 회전 값 저장
        Vector3 originalPosition = playerCamera.transform.position;
        Quaternion originalRotation = playerCamera.transform.rotation;

        // NPC 카메라 타겟의 최종 위치와 회전
        Vector3 targetPosition = npcCameraTarget.position;
        Quaternion targetRotation = npcCameraTarget.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < cameraMoveDuration)
        {
            // Lerp를 사용하여 위치와 회전을 부드럽게 보간
            playerCamera.transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / cameraMoveDuration));
            playerCamera.transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, (elapsedTime / cameraMoveDuration));

            elapsedTime += Time.deltaTime;
            yield return null; // 한 프레임 대기
        }

        // 정확한 위치와 회전으로 최종 설정
        playerCamera.transform.position = targetPosition;
        playerCamera.transform.rotation = targetRotation;

        isDialogueStarted = true;
    }

    

    public IEnumerator EndDialogueCamera()
    {
        notRepeated = true;
        // 플레이어 카메라의 부모(예: 플레이어 게임 오브젝트)를 원래 위치로 설정
        // 플레이어 카메라가 플레이어 게임 오브젝트의 자식인 경우
        playerCamera.transform.localPosition = new Vector3(0f, 1.63f, 0.4f);
        playerCamera.transform.localRotation = Quaternion.identity;

        // 플레이어 컨트롤러 재활성화
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        yield return null; // 한 프레임 대기

        // 씬 전환 사운드 재생
        SoundManager.Instance.Play(SoundKey.SceneTransition);

        SceneTransitionManager.Instance.LoadScene("TutorialScene3");
    }
}
