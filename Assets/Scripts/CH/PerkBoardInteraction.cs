using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PerkBoardInteraction : MonoBehaviour
{
    public static PerkBoardInteraction Instance;

    [Header("참조 오브젝트")]
    public GameObject perkCanvas;        // Perk UI Canvas
    public GameObject player;            // FPS 플레이어
    public GameObject pauseMenuManager;  // PauseMenuManager
    public Camera playerCamera;          // FPS 카메라
    public Camera perkCamera;            // Perk 전용 카메라
    public Transform boardTransform;     // Perk 보드 Transform
    public Image crosshairImage;  // Crosshair UI

    [Header("UI Raycasters")]
    [SerializeField] private GraphicRaycaster missionRaycaster;  // MissionBoard(Canvas)
    [SerializeField] private GraphicRaycaster perkRaycaster;     // PerkBoard(Canvas)

    private PlayerController playerController;
    private bool isOpen = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public bool IsOpen => isOpen;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        if (perkCanvas != null)
            perkCanvas.SetActive(false);

        if (perkCamera != null)
        {
            perkCamera.enabled = false;
            var ctrl = perkCamera.GetComponent<PerkCameraController>();
            if (ctrl != null) ctrl.enabled = false;
        }
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePerkBoard();
        }
    }

    public void TryInteract()
    {
        if (!isOpen)
            StartPerkInteraction();
    }

    private void StartPerkInteraction()
    {
        isOpen = true;

        if (pauseMenuManager != null)
            pauseMenuManager.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        originalPosition = playerCamera.transform.position;
        originalRotation = playerCamera.transform.rotation;

        if (crosshairImage != null) crosshairImage.gameObject.SetActive(false);

        // 아직 playerCamera는 켜둔 상태
        StartCoroutine(TeleportCameraToFrontOfPerkBoard());
    }

    private IEnumerator TeleportCameraToFrontOfPerkBoard()
    {
        Vector3 targetPosition = boardTransform.position + boardTransform.forward * 2f;

        playerCamera.transform.position = targetPosition;
        playerCamera.transform.LookAt(boardTransform);

        yield return StartCoroutine(ZoomInToPerkBoard());
    }

    private IEnumerator ZoomInToPerkBoard()
    {
        float zoomDistance = 1.15f;
        Vector3 dir = (boardTransform.position - playerCamera.transform.position).normalized;

        // 여기에 Y축(위쪽) 방향을 더함
        Vector3 moveDir = (dir + Vector3.up * 2f).normalized; // 0.3f는 위쪽 비율, 값 조정 가능

        Vector3 targetPosition = playerCamera.transform.position + moveDir * zoomDistance;

        while (Vector3.Distance(playerCamera.transform.position, targetPosition) > 0.1f)
        {
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPosition,
                1.5f * Time.deltaTime
            );
            yield return null;
        }

        ActivatePerkSelection();
    }

    private void ActivatePerkSelection()
    {
        perkCamera.transform.localPosition = new Vector3(0f, 0f, -355f);

        // 여기서 전환: playerCamera 끄고 perkCamera 켬
        playerCamera.enabled = false;
        perkCamera.enabled = true;
        perkCamera.GetComponent<PerkCameraController>().enabled = true;

        if (perkRaycaster) perkRaycaster.enabled = true;
        if (missionRaycaster) missionRaycaster.enabled = false;

        if (perkCanvas != null)
            perkCanvas.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void ClosePerkBoard()
    {
        isOpen = false;

        if (perkRaycaster) perkRaycaster.enabled = false;

        if (pauseMenuManager != null)
            pauseMenuManager.SetActive(true);

        if (perkCanvas != null)
            perkCanvas.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        // MissionBoard처럼: playerCamera 먼저 켜고 perkCamera 끔
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.transform.position = originalPosition;
            playerCamera.transform.rotation = originalRotation;
        }

        if (perkCamera != null)
        {
            var ctrl = perkCamera.GetComponent<PerkCameraController>();
            if (ctrl != null) ctrl.enabled = false;
            perkCamera.enabled = false;
        }

        if (crosshairImage != null) crosshairImage.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
