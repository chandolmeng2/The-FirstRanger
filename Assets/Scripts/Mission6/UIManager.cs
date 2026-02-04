using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Text messageText;
    private Coroutine currentRoutine;

    private void Awake() => Instance = this;

    public void ShowMessage(string message, float duration)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowRoutine(message, duration));
    }

    private IEnumerator ShowRoutine(string msg, float time)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        messageText.gameObject.SetActive(false);
    }
}

