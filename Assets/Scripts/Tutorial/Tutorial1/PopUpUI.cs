using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopUpUI : MonoBehaviour
{
    public CanvasGroup pointPanel;
    private bool isDone = false;
    // Start is called before the first frame update

    void Start()
    {
        pointPanel.alpha = 0;
    }
    void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("Player") && !isDone)
        {
            pointPanel.DOFade(1f, 1f).WaitForCompletion();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pointPanel.DOFade(0f, 1f).WaitForCompletion();
            isDone = true;
        }
    }
}
