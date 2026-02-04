using UnityEngine;

public abstract class MissionEvent : MonoBehaviour
{
    protected TimedMissionController _mission;
    public bool IsResolved { get; protected set; }

    public virtual void Initialize(TimedMissionController mission) { _mission = mission; }
    public abstract void Begin();

    protected virtual void Resolve()
    {
        if (IsResolved) return;
        IsResolved = true;
        _mission.EventResolved(this);
        Destroy(gameObject, 0.1f); // 필요 없으면 제거
    }

    public virtual void Abort()
    {
        Destroy(gameObject);
    }
}
