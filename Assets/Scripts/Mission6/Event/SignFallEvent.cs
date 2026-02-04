using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class SignFallEvent : MonoBehaviour
{
    public SoundKey fallSoundKey = SoundKey.Mission6_Event_Sign;  // 사운드매니저의 사운드 키 지정
    public float fallForce = 3f;
    public Vector3 fallDirection = new Vector3(0, 0, 1);

    private bool hasFallen = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasFallen || !other.CompareTag("Player")) return;

        hasFallen = true;

        rb.isKinematic = false;
        rb.AddForce(fallDirection.normalized * fallForce, ForceMode.Impulse);

        // 사운드 매니저를 통해 사운드 재생
        SoundManager.Instance.Play(fallSoundKey);
    }
}
