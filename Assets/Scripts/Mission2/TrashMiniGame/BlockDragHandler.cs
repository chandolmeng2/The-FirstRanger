using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BlockData blockData;
    public GameObject ghostPrefab;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool isPlaced = false;

    private GameObject ghostPreview;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
        originalScale = rectTransform.localScale;
        originalParent = transform.parent;

        rectTransform.localScale = Vector3.one;

        ghostPreview = Instantiate(ghostPrefab, TrashPuzzleGrid.Instance.blockRoot);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced || ghostPreview == null) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            TrashPuzzleGrid.Instance.gridArea,
            eventData.position,
            null,
            out localPoint))
        {
            List<Vector2Int> offsets = blockData.GetOffsets();
            RectTransform ghostRect = ghostPreview.GetComponent<RectTransform>();
            TrashPuzzleGrid.Instance.UpdateGhost(ghostRect, offsets, localPoint);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (ghostPreview != null)
            Destroy(ghostPreview);
        Vector2 localPoint;
        bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            TrashPuzzleGrid.Instance.gridArea,
            eventData.position,
            null,
            out localPoint);

        SoundManager.Instance.Play(SoundKey.Mission2_Puzzle2_LineConnect); // 사운드

        if (!result)
        {
            Debug.LogWarning("로컬 포인트 변환 실패!");
            rectTransform.anchoredPosition = originalPosition;
            
            return;
        }

        List<Vector2Int> offsets = blockData.GetOffsets();

        if (TrashPuzzleGrid.Instance.TryPlaceBlock(offsets, rectTransform, localPoint))
        {
            rectTransform.SetParent(TrashPuzzleGrid.Instance.blockRoot, false);
            isPlaced = true;
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
            rectTransform.localRotation = originalRotation;
            rectTransform.localScale = originalScale;
        }
    }

    public void ResetToInitialState()
    {
        isPlaced = false;
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
    }

    public bool IsPlaced()
    {
        return isPlaced;
    }
}
