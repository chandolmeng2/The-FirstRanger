using UnityEngine;

public class PatrolJournalInteractTarget : MonoBehaviour, IPatrolInteractable
{
    public string GetInteractionText()
    {
        return "순찰 일지 작성 <color=yellow>[E]</color>";
    }

    public void Interact()
    {
        if (!PatrolManager.Instance.IsAllPatrolComplete())
        {
            UIManager.Instance.ShowMessage("순찰을 마치고 와야 합니다.", 2f);
            return;
        }

        if (PatrolJournalUI.Instance.IsJournalCompleted())
        {
            UIManager.Instance.ShowMessage("순찰 일지 작성을 이미 완료했다. 돌아가자.", 2f);
            return;
        }

        // 순찰 완료 시 UI 열기
        PatrolJournalUI.Instance.Open();
    }
}

