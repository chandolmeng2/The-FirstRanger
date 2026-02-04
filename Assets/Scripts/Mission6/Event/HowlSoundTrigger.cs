using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HowlSoundTrigger : MonoBehaviour
{
    public SoundKey soundKey = SoundKey.Mission6_Event_Howl;
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed || !other.CompareTag("Player")) return;

        hasPlayed = true;
        SoundManager.Instance.Play(soundKey);
    }
}