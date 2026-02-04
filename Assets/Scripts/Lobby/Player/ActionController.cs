using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ActionController : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerController playerController;

    [Header("UI 요소")]
    [SerializeField] private Text actionText;
    [SerializeField] private RectTransform canvasRootTransform;
    [SerializeField] private GameObject persuadeUIPanel;

    [Header("불법 NPC 설득용")]
    [SerializeField] private Camera illegalDialogueCamera;
    [SerializeField, Range(-5f, 5f)] private float cameraSideOffset = 2.5f;
    [SerializeField, Range(1f, 5f)] private float cameraHeight = 3f;
    [SerializeField, Range(-5f, 5f)] private float cameraDistance = 0f;
    [SerializeField, Range(-45f, 45f)] private float playerRotationOffsetY = 0f;

    private ItemObject currentItem = null;
    private IllegalNPC currentTargetNPC = null;
    private Vector3 savedLookDirection;

    public bool IsTrashPuzzlePlaying { get; private set; } = false;

    //드랍쓰레기,바위쓰레기 기믹, 나무흔들기
    private TrashDropTrigger currentTrigger = null;
    private RockRevealTrigger currentRockTrigger = null;
    private TreeShakeTrigger currentTreeTrigger = null;

    private IPatrolInteractable currentPatrolInteractable = null;


    void Start()
    {
        playerCamera.enabled = true;
        illegalDialogueCamera.enabled = false;
    }

    void Update()
    {
        if (FadeController.IsFading ||
            PuzzleUIManager.IsPuzzleActive ||
            IsTrashPuzzlePlaying ||
            BookCodex.codexActivated ||
            CodexScanController.IsScanning ||
            MissionPanel.IsOpen)
        {
            actionText.gameObject.SetActive(false);
            return;
        }

        if (PatrolJournalUI.Instance != null && PatrolJournalUI.Instance.IsOpen())
            return;

        CheckAction();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAction();
        }
    }

    private void CheckAction()
    {
        actionText.gameObject.SetActive(false);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {           
            if (hit.collider.TryGetComponent(out ItemObject item))
            {
                currentItem = item;
                actionText.text = item.GetInteractionText();
                actionText.gameObject.SetActive(true);
                return;
            }

            if (hit.collider.TryGetComponent(out IllegalNPC npc) && npc.isActive)
            {
                currentTargetNPC = npc;
                currentItem = null;
                actionText.text = "단속하기 <color=yellow>[E)</color>";
                actionText.gameObject.SetActive(true);
                return;
            }

            if (hit.collider.TryGetComponent(out TrashDropTrigger dropTrigger))
            {
                actionText.text = dropTrigger.GetInteractionText();
                currentTrigger = dropTrigger;
                actionText.gameObject.SetActive(true);
            }
            else
            {
                currentTrigger = null;
            }

            if (hit.collider.TryGetComponent(out RockRevealTrigger rockTrigger))
            {
                currentRockTrigger = rockTrigger;
                actionText.text = rockTrigger.GetInteractionText();
                actionText.gameObject.SetActive(true);
                return;
            }
            else
                currentRockTrigger = null;

            if (hit.collider.TryGetComponent(out TreeShakeTrigger treeTrigger))
            {
                currentTreeTrigger = treeTrigger;
                actionText.text = treeTrigger.GetInteractionText();
                actionText.gameObject.SetActive(true);
                return;
            }
            else
                currentTreeTrigger = null;

            if (hit.collider.TryGetComponent(out IPatrolInteractable patrolInteractable))
            {
                currentPatrolInteractable = patrolInteractable;
                actionText.text = patrolInteractable.GetInteractionText();
                actionText.gameObject.SetActive(true);
                return;
            }
            else
            {
                currentPatrolInteractable = null;
            }
        }

        currentItem = null;
        currentTargetNPC = null;
    }

    private void TryAction()
    {
        if (currentItem != null)
        {
            switch (currentItem.itemData.itemType)
            {
                case ItemType.Trash:
                    SoundManager.Instance.Play(SoundKey.Mission1_Pickup_Trash); // 쓰레기 줍는 사운드 추가
                    currentItem.Interact();
                    break;

                case ItemType.Racoon:
                    currentItem.Interact();
                    break;

                case ItemType.TrashPuzzle:
                    StartPuzzleImmediate(currentItem.gameObject);
                    break;

                case ItemType.LinePuzzle:
                case ItemType.SlidingPuzzle:
                    currentItem.Interact();
                    break;

                default:
                    Debug.Log("해당 아이템은 직접 상호작용 대상이 아닙니다.");
                    break;
            }

            actionText.gameObject.SetActive(false);
            currentItem = null;
        }
        else if (currentTargetNPC != null)
        {
            savedLookDirection = playerCamera.transform.forward;
            savedLookDirection.y = 0f;
            StartCoroutine(BeginPersuasionSequence(currentTargetNPC));
        }
        else if (currentTrigger != null)
        {
            currentTrigger.Interact();
        }
        else if (currentRockTrigger != null)
        {
            currentRockTrigger.Interact();
            actionText.gameObject.SetActive(false);
            currentRockTrigger = null;
        }
        else if (currentTreeTrigger != null)
        {
            currentTreeTrigger.Interact();
            actionText.gameObject.SetActive(false);
            currentTreeTrigger = null;
        }

        else if (currentPatrolInteractable != null)
        {
            currentPatrolInteractable.Interact();
            actionText.gameObject.SetActive(false);
            currentPatrolInteractable = null;
        }
    }

    private void StartPuzzleImmediate(GameObject itemToDestroy)
    {
        StartCoroutine(FadeAndStartPuzzle(itemToDestroy));
    }

    private IEnumerator FadeAndStartPuzzle(GameObject itemToDestroy)
    {
        yield return FadeController.Instance.FadeIn(1.0f);

        IsTrashPuzzlePlaying = true;
        playerController.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // ?? itemToDestroy에서 ItemObject를 가져와서 itemData를 추출
        var itemObject = itemToDestroy.GetComponent<ItemObject>();
        if (itemObject == null || itemObject.itemData == null)
        {
            Debug.LogError("[FadeAndStartPuzzle] itemObject or itemData is NULL");
            yield break;
        }

        int puzzleIndex = itemObject.itemData.trashPuzzleIndex;
        Debug.Log($"[ActionController] puzzleIndex: {puzzleIndex}");

        GameObject panel = TrashPuzzleManager.Instance.GetPanelByIndex(puzzleIndex);
        TrashPuzzleGameController controller = TrashPuzzleManager.Instance.GetControllerByIndex(puzzleIndex);

        Debug.Log($"[ActionController] panel: {(panel == null ? "NULL" : panel.name)}");
        Debug.Log($"[ActionController] controller: {(controller == null ? "NULL" : controller.name)}");

        if (controller != null && panel != null)
        {
            controller.targetPuzzleItem = itemToDestroy;
            controller.onGameEnd += () => { StartCoroutine(EndTrashSequence()); };
            panel.SetActive(true);
        }
        else
        {
            Debug.LogError("[ActionController] Controller or Panel is NULL");
        }

        yield return new WaitForSeconds(0.3f);
        yield return FadeController.Instance.FadeOut(1.0f);
    }


    private IEnumerator EndTrashSequence()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"[EndTrashSequence] currentItem: {(currentItem == null ? "NULL" : currentItem.name)}");

        if (currentItem != null)
        {
            currentItem.Interact();
            currentItem = null;
        }

        yield return null;

        IsTrashPuzzlePlaying = false;
        playerController.enabled = true;
    }

    public void ForceCancelPuzzle()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        IsTrashPuzzlePlaying = false;
        playerController.enabled = true;
    }

    public void StartPuzzle()
    {
        IsTrashPuzzlePlaying = true;
        playerController.enabled = false;
    }

    public void EndPuzzle()
    {
        IsTrashPuzzlePlaying = false;
        playerController.enabled = true;
    }

    private IEnumerator BeginPersuasionSequence(IllegalNPC npc)
    {
        playerController.enabled = false;

        yield return FadeController.Instance.FadeIn(1.5f);

        if (savedLookDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(savedLookDirection);
            lookRotation *= Quaternion.Euler(0f, playerRotationOffsetY, 0f);
            playerRoot.rotation = lookRotation;
        }

        playerRoot.position = npc.transform.position - savedLookDirection.normalized * 2.5f;
        playerCamera.enabled = false;
        illegalDialogueCamera.enabled = true;

        npc.GetComponent<IllegalNPCAI>()?.EnterDialogueIdle();

        Vector3 offset =
            playerRoot.right * cameraSideOffset +
            Vector3.up * cameraHeight +
            -playerRoot.forward * cameraDistance;

        illegalDialogueCamera.transform.position = playerRoot.position + offset;

        Vector3 midpoint = (playerRoot.position + npc.transform.position) / 2f + Vector3.up * 1.5f;
        illegalDialogueCamera.transform.LookAt(midpoint);

        IllegalInteractionManager.Instance.BeginInteraction(npc);

        yield return new WaitForSeconds(1.0f);
        yield return FadeController.Instance.FadeOut(2f);

        persuadeUIPanel.SetActive(true);
        currentTargetNPC = null;
        actionText.gameObject.SetActive(false);
    }
}
