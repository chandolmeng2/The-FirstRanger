using UnityEngine;

public class PatrolInteractTarget : MonoBehaviour, IPatrolInteractable
{
    [TextArea] public string interactionMessage = "불법취사의 흔적이 발견됐다. 다음 지역으로 이동하자.";

    public string GetInteractionText()
    {
        return "<color=yellow>[E]</color> 확인하기";
    }

    public void Interact()
    {
        var stage = PatrolManager.Instance.GetCurrentStage();
        if (stage != null)
        {
            UIManager.Instance.ShowMessage(stage.interactMessage, 3f);
            PatrolManager.Instance.CompleteCurrentStage();
        }
    }
}

