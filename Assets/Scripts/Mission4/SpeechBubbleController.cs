using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbleController : MonoBehaviour
{
    public GameObject bubbleObject;
    public Text bubbleText; // Text로 변경

    [SerializeField] private Vector3 offset = new Vector3(0, 2.0f, 0); // 인스펙터에서 조절 가능
    [SerializeField] private Transform target; // 말풍선을 따라다니게 할 대상 (예: Player 또는 Head Bone)

    private Coroutine hideCoroutine;
    private Coroutine loopCoroutine;

    private bool isForcedFollowOffset = false;

    private void Start()
    {
        // target이 인스펙터에서 지정되지 않은 경우, 자신의 부모를 따라감
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }

        isForcedFollowOffset = false; // 초기에는 수동 위치 유지
    }


    private void LateUpdate()
    {
        if (target != null && isForcedFollowOffset)
        {
            transform.position = target.position + offset;
        }
    }


    public void ShowSpeech(string text, float duration = 2f)
    {
        if (bubbleObject == null || bubbleText == null) return;

        // 기존 코루틴 정지
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        bubbleText.text = text;              // 먼저 텍스트 설정
        bubbleObject.SetActive(true);        // 그다음 보여주기
        StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        bubbleObject.SetActive(false);
        hideCoroutine = null;
    }

    public void HideSpeech()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (bubbleObject != null)
            bubbleObject.SetActive(false);

        if (bubbleText != null)
            bubbleText.text = "";
    }


    public void StartLoopingSpeech(string[] lines, float interval, float displayDuration)
    {
        if (loopCoroutine != null)
            StopCoroutine(loopCoroutine);

        loopCoroutine = StartCoroutine(LoopSpeech(lines, interval, displayDuration));
    }

    public void StopLoopingSpeech()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        HideSpeech(); // 혹시 말풍선이 켜져 있으면 끔
    }


    private IEnumerator LoopSpeech(string[] lines, float interval, float displayDuration)
    {
        Debug.Log("[SpeechBubble] LoopSpeech 시작됨");

        while (true)
        {
            string line = lines[Random.Range(0, lines.Length)];
            Debug.Log($"[SpeechBubble] 대사 출력: {line}");
            ShowSpeech(line, displayDuration);
            yield return new WaitForSeconds(interval);
        }
    }

    public void EnableOffsetFollow()
    {
        isForcedFollowOffset = true;
    }

    public void DisableOffsetFollow()
    {
        isForcedFollowOffset = false;
    }


}
