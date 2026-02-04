using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Tutorial3Manager : MonoBehaviour
{
    // 목표 지점
    public Transform[] rallyPoints;
    public Transform npcToMove;
    private bool isMoving = true;
    public Animator npcAnimator;
    public Transform playerTransform;
    public bool tuto_end = false;
    public GameObject[] interactPoints;
    public DialogueManager2 dmanager;
    public bool isPhase1 = false;
    public bool isPhase2 = false;
    public bool isPhase3 = false; // 다이얼로그를 위함임
    public CodexScanController csc;
    public BookCodex bc;
    public float lookSpeed = 2.0f;
    public Transform lookPoint;
    public Transform playerCameraTransform;
    public bool isTalking = false;

    // NPC 이동 속도
    public float speed = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        csc.enabled = false;
        bc.enabled = false;
        // 코루틴으로 StartTutorial()을 시작
        StartCoroutine(StartTutorial());
    }

    void Update()
    {
        if (isTalking)
        {
            LookAtNPC();
        }
    }
    IEnumerator StartTutorial()
    {
        while (isMoving)
        {
            npcToMove.position = Vector3.MoveTowards(
                npcToMove.position,
                rallyPoints[0].position,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(npcToMove.position, rallyPoints[0].position) < 0.1f)
            {
                npcAnimator.SetBool("isStop", true);
                npcToMove.LookAt(playerTransform);
                isMoving = false;
            }

            yield return null; // 한프레임씩 실행하기 위해 필요한 것
        }

        yield return StartCoroutine(StartTutorial1());
    }

    IEnumerator StartTutorial1()
    {
        StopCoroutine(StartTutorial());
        interactPoints[0].SetActive(true);
        isPhase1 = true;
        yield return null;
    }

    public IEnumerator Phase1()
    {
        interactPoints[0].SetActive(false);
        Debug.Log("페이즈1 시작");

        PlayerController.IsDialogueActive = true; // 튜토리얼 시작 알림
        SoundManager.Instance.StopWalkingLoop();  // 혹시 켜져 있던 발소리 정지

        while (!dmanager.isPhase1Over)
        {
            yield return null;
        }
        isPhase1 = false;
        isPhase2 = true;

        PlayerController.IsDialogueActive = false; // 대화/이벤트 끝 알림

        StartCoroutine(movetoPhase2());
    }

        IEnumerator movetoPhase2()
        {
        isMoving = true;
        npcAnimator.SetBool("isStop", false);
        npcToMove.LookAt(rallyPoints[1].position);

        while (isMoving)
            {
                npcToMove.position = Vector3.MoveTowards(
                    npcToMove.position,
                    rallyPoints[1].position,
                    speed * Time.deltaTime
                );

                if (Vector3.Distance(npcToMove.position, rallyPoints[1].position) < 0.1f)
                {
                    npcAnimator.SetBool("isStop", true);
                    npcToMove.LookAt(playerTransform);
                    isMoving = false;
                }

                yield return null; // 한프레임씩 실행하기 위해 필요한 것
            }

        interactPoints[1].SetActive(true);
    }
    public IEnumerator Phase2()
    {
        Debug.Log("페이즈2 시작");

        PlayerController.IsDialogueActive = true; // 튜토리얼 시작 알림
        SoundManager.Instance.StopWalkingLoop();  // 혹시 켜져 있던 발소리 정지

        interactPoints[1].SetActive(false);
        csc.enabled = true;
        while (!dmanager.isPhase2Over)
        {
            yield return null;
        }
        isPhase2 = false;

        PlayerController.IsDialogueActive = false; // 대화/이벤트 끝 알림

        while (!csc.isRegistered)
        {
            yield return null;
        }
        Debug.Log("촬영 성공");
        csc.enabled = false;
        isPhase2 = false;
        isPhase3 = true;     

        StartCoroutine(movetoPhase3());
    }

    IEnumerator movetoPhase3()
    {
        interactPoints[2].SetActive(true);
        yield return null;
    }

    public IEnumerator Phase3()
    {
        PlayerController.IsDialogueActive = true; // 튜토리얼 시작 알림
        SoundManager.Instance.StopWalkingLoop();  // 혹시 켜져 있던 발소리 정지

        interactPoints[2].SetActive(false);
        while (!dmanager.isPhase3Over){
            yield return null;
        }
        interactPoints[3].SetActive(true);

        PlayerController.IsDialogueActive = false; // 대화/이벤트 끝 알림

        bc.enabled = true;
        tuto_end = true;
        isMoving = true;
        npcAnimator.SetBool("isRunning", true);
        npcToMove.LookAt(rallyPoints[2].position);
        while (isMoving)
        {
            speed = 3.0f;
            npcToMove.position = Vector3.MoveTowards(
                npcToMove.position,
                rallyPoints[2].position,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(npcToMove.position, rallyPoints[2].position) < 0.1f)
            {
                npcAnimator.SetBool("isRunning", false);
                npcToMove.LookAt(playerTransform);
                isMoving = false;
            }

            yield return null; // 한프레임씩 실행하기 위해 필요한 것
        }
    }

    public void LookAtNPC()
    {
        if (playerCameraTransform != null && lookPoint != null)
        {
            // NPC를 바라보는 방향 벡터 계산
            Vector3 direction = lookPoint.position - playerCameraTransform.position;

            // 목표 회전값 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 현재 회전값에서 목표 회전값으로 부드럽게 보간
            playerCameraTransform.rotation = Quaternion.Slerp(playerCameraTransform.rotation, targetRotation, lookSpeed * Time.deltaTime);
        }
    }
}
