using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FlashlightFlickerTrigger : MonoBehaviour
{
    public float flickerDuration = 3f;       // ±ôºýÀÌ´Â ÃÑ ½Ã°£
    public float flickerInterval = 0.1f;     // ±ôºýÀÌ´Â °£°Ý
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;

        var flashlight = FindObjectOfType<FlashlightController>();
        if (flashlight != null)
        {
            flashlight.StartFlicker(flickerDuration, flickerInterval);
        }

        Destroy(gameObject);
    }
}
