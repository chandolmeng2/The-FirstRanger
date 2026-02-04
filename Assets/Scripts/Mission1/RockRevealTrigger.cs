using UnityEngine;
using System.Collections;
using DG.Tweening;

public class RockRevealTrigger : MonoBehaviour
{
    [Header("움직일 오브젝트")]
    public Transform rockObject;
    public Transform trashObject;

    [Header("움직임 설정")]
    public float rockDownDistance = 1.5f;
    public float trashUpDistance = 1.0f;
    public float downDuration = 1.0f;
    public float shakeDuration = 0.5f;
    public float trashDelay = 0.2f;

    private bool hasActivated = false;

    public string GetInteractionText()
    {
        return hasActivated ? "" : "바위를 밀어보기 <color=yellow>(E)</color>";
    }

    public void Interact()
    {
        if (hasActivated) return;

        hasActivated = true;
        StartCoroutine(RevealTrashRoutine());
    }

    private IEnumerator RevealTrashRoutine()
    {
        // 1. 바위 흔들림 (Shake)
        if (rockObject != null)
        {
            rockObject.DOShakeRotation(shakeDuration, strength: new Vector3(0, 20f, 0), vibrato: 10, randomness: 90f);
            
            SoundManager.Instance.Play(SoundKey.Mission1_Rock_Shake); // 바위를 건들 때 나는 효과음
        }

        yield return new WaitForSeconds(shakeDuration + 0.1f);

        // 2. 바위 아래로 이동
        if (rockObject != null)
        {
            rockObject.DOMoveY(rockObject.position.y - rockDownDistance, downDuration).SetEase(Ease.InOutSine);
        }

        yield return new WaitForSeconds(trashDelay);

        // 3. 쓰레기 위로 등장
        if (trashObject != null)
        {
            trashObject.gameObject.SetActive(true);
            trashObject.DOMoveY(trashObject.position.y + trashUpDistance, downDuration * 0.8f).SetEase(Ease.OutBack);

            SoundManager.Instance.Play(SoundKey.Mission1_Trash_Appear); // 쓰레기가 튀어나올 때 나는 효과음
        }

        // 4. 트리거 삭제
        yield return new WaitForSeconds(downDuration + 0.3f);
        Destroy(gameObject);
    }
}


