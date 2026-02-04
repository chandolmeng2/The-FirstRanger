using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashDropTrigger : MonoBehaviour
{
    [Header("떨어질 쓰레기 오브젝트")]
    public GameObject trashObject;      // 떨어질 쓰레기 프리팹
    public float dropDelay = 0.5f;      // 쓰레기 떨어지는 딜레이
    public bool destroyAfterTrigger = true;  // 상호작용 후 삭제 여부

    private bool hasTriggered = false;

    public string GetInteractionText()
    {
        return hasTriggered ? "" : "무언가 이상한 흔적이 있다. 눌러보자. <color=yellow>(E)</color>";
    }

    public void Interact()
    {
        if (hasTriggered) return;

        hasTriggered = true;

        // 상호작용 순간 효과음 (버튼/흔적 눌렀을 때 소리)
        SoundManager.Instance.Play(SoundKey.Mission1_TrashDrop_Interact);

        StartCoroutine(DropTrashRoutine());
    }

    private IEnumerator DropTrashRoutine()
    {
        yield return new WaitForSeconds(dropDelay);

        if (trashObject != null)
        {
            trashObject.SetActive(true);

            Rigidbody rb = trashObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false; // 중력 적용

            // 쓰레기가 떨어질 때 효과음
            SoundManager.Instance.Play(SoundKey.Mission1_TrashDrop_Fall);
        }

        // 상호작용 오브젝트 삭제
        if (destroyAfterTrigger)
        {
            yield return new WaitForSeconds(0.2f); // 잠시 여유
            Destroy(gameObject);
        }
    }
}

