using System.Collections;
using UnityEngine;

public class IllegalNPCAI : MonoBehaviour
{
    private IllegalNPC npc;
    private Transform targetPlayer;
    private bool isTalking = false;

    public Transform visualRoot;

    [SerializeField] private float facingOffsetY = -15f;

    private void Awake()
    {
        npc = GetComponent<IllegalNPC>();
    }

    public void StartIllegalAction()
    {
        npc.animator.Play("IllegalAction");
    }

    public void EnterDialogueIdle()
    {
        npc.animator.applyRootMotion = false;
        npc.animator.Play("Idle");

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
            isTalking = true;

            // NPC를 즉시 회전시켜 플레이어 바라보게
            Vector3 dir = player.transform.position - visualRoot.position;
            dir.y = 0f;
            if (dir != Vector3.zero)
                visualRoot.forward = dir.normalized;

        }
    }


    public void EndDialogue()
    {
        isTalking = false;
        targetPlayer = null;
    }

    public void PlayApologize()
    {
        if (!npc.hasSurrendered)
        {
            npc.animator.SetTrigger("Apologize");
            npc.hasSurrendered = true;
            npc.isActive = false;
            EndDialogue();
        }
    }

    public void PlayRunAway()
    {
        if (!npc.hasEscaped)
        {
            // 범법자 반응 애니메이션 강제 종료
            npc.animator.ResetTrigger("ReactAngry");
            npc.animator.ResetTrigger("ReactSurprised");

            npc.animator.SetTrigger("Run");

            // 1. 플레이어 방향 계산
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - transform.position;
                toPlayer.y = 0f;

                if (toPlayer != Vector3.zero)
                {
                    // 2. 플레이어 방향의 반대 방향으로 NPC 회전
                    Vector3 runDirection = -toPlayer.normalized;
                    transform.forward = runDirection;

                    // 3. 이동 시작 (예: Rigidbody 또는 Translate 방식)
                    StartCoroutine(RunAwayRoutine(runDirection));
                }
            }
            npc.hasEscaped = true;
            npc.isActive = false;
            EndDialogue();
        }
    }

    private IEnumerator RunAwayRoutine(Vector3 direction)
    {
        float runDuration = 3f;
        float elapsed = 0f;
        float speed = 3f; // 원하는 속도

        while (elapsed < runDuration)
        {
            transform.position += direction * speed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }



    private void LateUpdate()
    {
        if (isTalking && targetPlayer != null)
        {
            Vector3 dir = targetPlayer.position - transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = lookRot * Quaternion.Euler(0f, facingOffsetY, 0f); // 보정 각도 추가
            }
                
        }
    }

    public void PlaySurprised()
    {
        npc.animator.SetTrigger("ReactSurprised");
    }

    public void PlayAngry()
    {
        npc.animator.SetTrigger("ReactAngry");
    }


}
