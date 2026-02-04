using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    public GameObject player;
    public Animator npcAnimator;
    public SequenceManager manager;
    public GameObject trigger1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npcAnimator.SetBool("isMeet", true);
            trigger1.SetActive(false);
            manager.isTriggered = true;
        }
    }
}
