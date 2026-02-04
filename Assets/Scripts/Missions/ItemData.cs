using UnityEngine;

public enum ItemType
{
    Codex,          // 싱글톤, 도감 전용 아이템
    Trash,          // 미션1 전용, 상호작용으로 삭제
    TrashPuzzle,    // 미션2 전용, 테트리스 퍼즐 실행
    LinePuzzle,     // 미션2 전용, 선 연결 퍼즐 ui
    SlidingPuzzle,  // 미션2 전용, 슬라이딩 퍼즐
    Racoon,         // 미션3 전용, 도주하는 너구리
    Misc            // 잔디, 트리거, 특수 오브젝트 등 포함 가능
}

public enum Rarity { Normal, Rare, Unique }

public enum CodexCategory { Nature, Animal, Outlaw }

[CreateAssetMenu(fileName = "NewItem", menuName = "NewItem/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;           // 이름
    [TextArea]
    public string description;        // 설명
    public Sprite icon;               // UI 아이콘
    public ItemType itemType;         // 아이템 분류

    [Header("쓰레기 퍼즐 UI 연결")]
    public int trashPuzzleIndex;

    [Header("선 연결 퍼즐 전용")]
    public int puzzleIndex; // 0: GamePanel1, 1: GamePanel2 등
    [Header("도감 카테고리(자연, 동물, 불법)")]
    public CodexCategory codexCategory;

    [Header("도감 희귀도 설정")]
    public Rarity rarity = Rarity.Normal;
}
