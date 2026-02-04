using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    public Tutorial3Manager t3m;
    public DialogueManager2 dm2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && t3m.isPhase1)
        {
            t3m.isTalking = true;
            dm2.goDialogue1 = true;
            t3m.StartCoroutine(t3m.Phase1());
        }
        if (other.CompareTag("Player") && t3m.isPhase2)
        {
            t3m.isTalking = true;
            dm2.goDialogue2 = true;
            t3m.StartCoroutine(t3m.Phase2());
        }
        if (other.CompareTag("Player") && t3m.isPhase3)
        {
            t3m.isTalking = true;
            dm2.goDialogue3 = true;
            t3m.StartCoroutine(t3m.Phase3());
        }
        if (other.CompareTag("Player") && t3m.tuto_end)
        {
            // �� ��ȯ ���� ���
            ExpManager.Instance.AddExp(-ExpManager.Instance.GetExp());
            SoundManager.Instance.Play(SoundKey.SceneTransition);
            SceneTransitionManager.Instance.LoadScene("LobbyScene");
        }
    }
}
