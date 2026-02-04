using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TreeShakeTrigger : MonoBehaviour
{
    public Transform treeVisual;
    public GameObject trashObject;
    public float shakeAmount = 15f;
    public float duration = 0.4f;

    private bool hasShaken = false;

    public string GetInteractionText()
    {
        return hasShaken ? "" : "나무 흔들기 <color=yellow>(E)</color>";
    }

    public void Interact()
    {
        if (hasShaken) return;
        hasShaken = true;

        // 나무 흔들 때 소리 재생
        SoundManager.Instance.Play(SoundKey.Mission1_Tree_Shake);

        StartCoroutine(ShakeTree());
    }

    private IEnumerator ShakeTree()
    {
        if (treeVisual != null)
        {
            treeVisual.DOShakeRotation(duration, shakeAmount, 10, 90f);
        }

        yield return new WaitForSeconds(duration * 0.8f);

        if (trashObject != null)
        {
            trashObject.SetActive(true);

            var rb = trashObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;  // 중력 활성화

            // ? 쓰레기 떨어질 때 소리 재생
            SoundManager.Instance.Play(SoundKey.Mission1_TrashDrop_Fall);
        }

        yield return new WaitForSeconds(0.5f);
        //Destroy(gameObject);  // 트리거 오브젝트 삭제
    }
}

