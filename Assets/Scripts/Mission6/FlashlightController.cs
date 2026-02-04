using UnityEngine;
using System.Collections;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private Light flashlight;

    private bool isOn = false;
    private bool isLocked = false; // 외부 이벤트 중일 때 조작 잠금

    private Coroutine flickerRoutine;

    private void Start()
    {
        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();

        flashlight.enabled = isOn;
    }

    private void Update()
    {
        if (isLocked) return;

        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;
            flashlight.enabled = isOn;

            SoundManager.Instance.Play(SoundKey.Flashlight);
        }
    }

    // 외부에서 호출하는 깜빡이기 이벤트
    public void StartFlicker(float duration, float interval = 0.1f)
    {
        if (flickerRoutine != null) StopCoroutine(flickerRoutine);
        flickerRoutine = StartCoroutine(FlickerRoutine(duration, interval));
    }

    private IEnumerator FlickerRoutine(float duration, float baseInterval)
    {
        isLocked = true;
        float elapsed = 0f;
        bool blackoutTriggered = false;

        while (elapsed < duration)
        {
            // 중간 암흑
            if (!blackoutTriggered && elapsed >= duration * 0.4f && Random.value < 0.5f)
            {
                blackoutTriggered = true;

                flashlight.enabled = false;
                yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
                elapsed += 2f;
                continue;
            }

            // 깜빡임
            flashlight.enabled = !flashlight.enabled;

            if (Random.value < 0.85f)
                SoundManager.Instance.Play(SoundKey.Flashlight);

            float randomInterval = Random.Range(baseInterval * 0.5f, baseInterval * 2.5f);
            elapsed += randomInterval;
            yield return new WaitForSeconds(randomInterval);
        }

        flashlight.enabled = isOn;
        isLocked = false;

        // 늑대 울음 사운드 재생 (0.5초 뒤)
        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.Play(SoundKey.Mission6_Event_Wolf);
    }

}
