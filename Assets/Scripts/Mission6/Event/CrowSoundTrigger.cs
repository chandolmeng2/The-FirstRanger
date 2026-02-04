using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CrowSoundTrigger : MonoBehaviour
{
    public SoundKey soundKey = SoundKey.Mission6_Event_Crow;  // 사운드매니저에서 등록된 사운드 키
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed || !other.CompareTag("Player")) return;

        hasPlayed = true;
        SoundManager.Instance.Play(soundKey);
    }
}

