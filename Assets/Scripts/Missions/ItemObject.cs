using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemData itemData;   // 연결된 ScriptableObject

    public string GetInteractionText()
    {
        switch (itemData.itemType)
        {
            case ItemType.Trash: return "쓰레기 수거하기 <color=yellow>(E)</color>";
            case ItemType.TrashPuzzle: return "쓰레기 정리하기 <color=yellow>(E)</color>";
            case ItemType.LinePuzzle: return "점검하기 <color=yellow>(E)</color>";
            case ItemType.SlidingPuzzle: return "점검하기 <color=yellow>(E)</color>";
            case ItemType.Misc: return "잔디 헤치기 <color=yellow>(E)</color>";
            case ItemType.Racoon: return "야생 너구리 구조 <color=yellow>(E)</color>"; // 미션 3 전용
            default: return " ";
        }
    }

    public void Interact()
    {
        switch (itemData.itemType)
        {
            case ItemType.Trash:
                Debug.Log(itemData.itemName + " 수거됨");
                Mission1Manager manager = FindObjectOfType<Mission1Manager>();
                if (manager != null)
                {
                    manager.AddTrashCount();
                }
                Destroy(gameObject);
                break;

            case ItemType.TrashPuzzle:
                Destroy(gameObject);
                break;
            case ItemType.Codex:
                //Codex.instance.RegisterToCodex(itemData);
                //Destroy(gameObject);
                break;
            case ItemType.LinePuzzle:
            case ItemType.SlidingPuzzle: // 동일 처리
                PuzzleUIManager uiManager = FindObjectOfType<PuzzleUIManager>();
                if (uiManager != null)
                {
                    uiManager.ShowPuzzle(itemData.puzzleIndex);
                    uiManager.SetTargetPuzzleItem(this.gameObject);
                }
                break;

            case ItemType.Racoon: // 너구리 상호작용
                RacoonAgentWithQTE racoon = GetComponent<RacoonAgentWithQTE>();
                if (racoon != null)
                {
                    racoon.TriggerQTE(); // QTE 발동
                }
                break;

            case ItemType.Misc:
                break;
        }
    }
}
