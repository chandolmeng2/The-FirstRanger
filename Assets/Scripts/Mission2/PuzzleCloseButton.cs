using UnityEngine;

public class PuzzleCloseButton : MonoBehaviour
{
    public void ClosePuzzle()
    {
        if (PuzzleUIManager.IsPuzzleActive && PuzzleUIManager.IsLocked)
            return;

        SlidingManager sm = FindObjectOfType<SlidingManager>();
        if (sm != null && sm.IsTransitioning())
            return;

        // 버튼 클릭 효과음 추가
        SoundManager.Instance.Play(SoundKey.Mission2_UIClick_Button);

        PuzzleUIManager ui = FindObjectOfType<PuzzleUIManager>();
        if (ui != null)
            ui.HideAll(withFadeIn: true);
    }
}

