using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PatrolTrigger : MonoBehaviour
{
    [Header("순찰 반응 반경 (인스펙터에서 조절 가능)")]
    public float triggerRadius = 5f;

    private bool hasEntered = false;

    private void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;
    }

    private void OnValidate()
    {
        var col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = triggerRadius;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasEntered || !other.CompareTag("Player")) return;
        hasEntered = true;

        var currentStage = PatrolManager.Instance.GetCurrentStage();
        if (currentStage != null)
        {
            UIManager.Instance.ShowMessage(currentStage.enterMessage, 3f);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
#endif
}

