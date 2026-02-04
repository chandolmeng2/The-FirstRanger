using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FPSInteractionSystem : MonoBehaviour
{
    public static FPSInteractionSystem instance;

    [Header("UI Raycasters")]
    [SerializeField] private GraphicRaycaster missionRaycaster;  // MissionBoard(Canvas)에 붙은 것
    [SerializeField] private GraphicRaycaster perkRaycaster;     // PerkBoard(Canvas)에 붙은 것

    public float interactionDistance = 3f;  // 상호작용할 수 있는 거리
    public LayerMask interactableLayer;     // 상호작용 가능한 오브젝트만 체크
    public Camera playerCamera;             // 플레이어 카메라
    public Camera boardCamera;              // 보드 상호작용용 카메라 

    public GameObject interactionTextUI;   // "E to Interact" 텍스트 UI
    public Image crosshairImage;           // 크로스헤어 점

    public Transform boardTransform;       // 보드의 위치
    public GameObject player;              // 플레이어 오브젝트
    private bool isInteracting = false;    // 상호작용 여부
    private bool isZoomingIn = false;      // 줌인 중 여부 체크

    private Vector3 originalPosition;      // 원래 카메라 위치
    private Quaternion originalRotation;   // 원래 카메라 회전
    private PlayerController playerController; // FirstPersonController 스크립트 참조

    // 미션 UI 구성요소들
    public GameObject[] missionButtons;   // 미션 버튼들
    public GameObject[] checkd;           // 미션 완료 표시
    public GameObject[] locked;           //  잠긴 미션

    // PauseMenuManager가 적용된 게임 오브젝트
    public GameObject pauseMenuManagerObject;  // 퍼즈 메뉴 관리 오브젝트

    public RectTransform expPanel; // 경험치 패널

    void Start()
    {
        boardCamera.GetComponent<BoardCameraController>().enabled = false; // 꺼줘야 안움직임
        playerController = player.GetComponent<PlayerController>(); // FirstPersonController 스크립트 참조
        expPanel.anchoredPosition = new Vector2(-550f, expPanel.anchoredPosition.y);
    }

    void Update()
    {
        CheckedMissions();
        Unlocked();
        
        // ESC 키로 상호작용 종료 (줌인 중이 아닐 때만 종료)
        if (isInteracting && !isZoomingIn && Input.GetKeyDown(KeyCode.Escape))
        {
            ExpUI.Instance.Show();
            EndInteraction();
            return;
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            expPanel.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutQuad);
        }
        else
        {
            expPanel.DOAnchorPosX(-550f, 0.5f).SetEase(Ease.OutQuad);



            // 화면 중앙에서 레이 쏘기
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // 중앙(0.5, 0.5)에서 레이 발사
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                // Raycast가 상호작용할 수 있는 오브젝트에 닿았을 때
                if (hit.collider.CompareTag("MissionBoard") && !isInteracting)
                {
                    interactionTextUI.SetActive(true);       // 텍스트 UI 활성화
                    crosshairImage.color = Color.green;     // 크로스헤어 색 변경

                    // E 키로 상호작용
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ExpUI.Instance.Hide();
                        Debug.Log("상호작용 대상: " + hit.collider.name);
                        StartInteraction();
                    }
                    return;
                }
                else if (hit.collider.CompareTag("PerkBoard") && !isInteracting)
                {
                    interactionTextUI.SetActive(true);
                    crosshairImage.color = Color.green;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Debug.Log("퍼크 보드 접근: " + hit.collider.name);
                        PerkBoardInteraction.Instance.TryInteract();
                    }
                    return;
                }
            }

            // Raycast가 아무 것도 감지하지 못하면
            interactionTextUI.SetActive(false); // 텍스트 UI 비활성화
            crosshairImage.color = Color.white;  // 크로스헤어 색 원상 복귀
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;

        // PauseMenuManager 오브젝트 비활성화 (상호작용 중일 때)
        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(false);
        }

        // 플레이어의 이동을 멈추기 (FirstPersonController 스크립트 비활성화)
        playerController.enabled = false;

        // 카메라의 원래 위치와 회전 저장
        originalPosition = playerCamera.transform.position;
        originalRotation = playerCamera.transform.rotation;

        // 크로스헤어 숨기기
        interactionTextUI.SetActive(false);
        crosshairImage.gameObject.SetActive(false);

        // 카메라를 보드의 정면 위치로 텔레포트 후 줌인 시작
        StartCoroutine(TeleportCameraToFrontOfBoard());
    }

    private System.Collections.IEnumerator TeleportCameraToFrontOfBoard()
    {
        // 보드의 정면을 기준으로 반대방향으로 카메라 이동
        Vector3 targetPosition = boardTransform.position + boardTransform.forward * 2f;  // 보드의 반대방향으로 2f 이동

        // 카메라를 목표 위치로 텔레포트
        playerCamera.transform.position = targetPosition;

        // 카메라가 보드를 정확히 바라보도록 설정
        playerCamera.transform.LookAt(boardTransform);

        // 줌인 시작
        yield return StartCoroutine(ZoomInToBoard());
    }

    private System.Collections.IEnumerator ZoomInToBoard()
    {
        isZoomingIn = true; // 줌인 시작

        // 보드로 줌인할 적정 거리 설정
        float zoomDistance = 1.15f;  // 보드로부터 몇 단위만큼 줌인할지 설정
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * zoomDistance;

        // 카메라가 목표 위치로 이동하는 동안
        while (Vector3.Distance(playerCamera.transform.position, targetPosition) > 0.1f)
        {
            // 카메라가 점진적으로 이동하도록 Lerp 사용
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, 1.5f * Time.deltaTime);
            yield return null;
        }
        isZoomingIn = false; // 줌인 완료
        PrintBoardCameraLocalPosition();
        // 줌인 완료 후, 미션 선택 UI 활성화
        ActivateMissionSelection();
    }
    private void ActivateMissionSelection()
    {
        boardCamera.transform.localPosition = new Vector3(-4.24f, -2.42f, -931.39f);
        playerCamera.enabled = false;
        boardCamera.enabled = true;
        boardCamera.GetComponent<BoardCameraController>().enabled = true;

        if (missionRaycaster) missionRaycaster.enabled = true;
        if (perkRaycaster) perkRaycaster.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EndInteraction()
    {
        // 상호작용이 끝날 때 PauseMenuManager 오브젝트 다시 활성화
        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(true);
        }

        // 인터랙션 종료 후, 카메라 원위치로 복귀
        boardCamera.transform.localPosition = new Vector3(-4.24f, -2.42f, -1082.73f);
        playerCamera.enabled = true;
        boardCamera.enabled = false;
        boardCamera.GetComponent<BoardCameraController>().enabled = false;
        playerCamera.transform.position = originalPosition;
        playerCamera.transform.rotation = originalRotation;

        if (missionRaycaster) missionRaycaster.enabled = false;

        // 플레이어의 이동을 다시 활성화
        playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;  // 마우스를 화면에 잠금
        Cursor.visible = false;

        // 크로스헤어 다시 활성화
        crosshairImage.gameObject.SetActive(true);

        // 상호작용 상태 종료
        isInteracting = false;
    }

    public void OnMission1ButtonClick()
    {

        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(true);
        }
        SceneTransitionManager.Instance.LoadScene("Mission1Scene");
    }

    public void OnMission2ButtonClick()
    {

        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(true);
        }
        SceneTransitionManager.Instance.LoadScene("Mission2Scene");
    }
    public void OnMission3ButtonClick()
    {

        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(true);
        }
        SceneTransitionManager.Instance.LoadScene("Mission3Scene");
    }
    public void OnMission4ButtonClick()
    {

        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(true);
        }
        SceneTransitionManager.Instance.LoadScene("Mission4Scene");
    }

    public void OnMission6ButtonClick()
    {

        if (pauseMenuManagerObject != null)
        {
            pauseMenuManagerObject.SetActive(true);
        }
        SceneTransitionManager.Instance.LoadScene("Mission6Scene");
    }

    void PrintBoardCameraLocalPosition()
    {
        Vector3 worldPosition = playerCamera.transform.position;
        Vector3 localPosition = boardCamera.transform.parent.InverseTransformPoint(worldPosition);

        Debug.Log("boardCamera localPosition = " + localPosition);
    }

    public void CheckedMissions()
    {
        for (int i = 0; i < missionButtons.Length; i++)
        {
            if (MissionManager.Instance.IsMissionClear(i + 1))
            {
                checkd[i].SetActive(true);
                GameObject button = missionButtons[i];
                Image image = button.GetComponent<Image>();
                image.color = new Color(118 / 255f, 118 / 255f, 118 / 255f);
            }
        }
    }

    public void Unlocked()
    {
        // 1,2 클리어 → 3,4 해제
        if (MissionManager.Instance.IsMissionClear(1) && MissionManager.Instance.IsMissionClear(2))
        {
            for (int i = 2; i < 4; i++) // 미션 3,4
            {
                locked[i].SetActive(false);
                GameObject button = missionButtons[i];
                Image image = button.GetComponent<Image>();

                // 미션 3,4가 클리어 됐으면 회색, 아니면 노란색
                if (MissionManager.Instance.IsMissionClear(i + 1))
                {
                    image.color = new Color(118 / 255f, 118 / 255f, 118 / 255f);
                    checkd[i].SetActive(true);
                }
                else
                {
                    image.color = new Color(240 / 255f, 255 / 255f, 0 / 255f);
                }

                missionButtons[i].GetComponent<Button>().enabled = true;
            }
        }

        // 3,4 클리어 → 6 해제
        if (MissionManager.Instance.IsMissionClear(3) && MissionManager.Instance.IsMissionClear(4))
        {
            int i = 4; // 미션6 버튼
            locked[i].SetActive(false);
            GameObject button = missionButtons[i];
            Image image = button.GetComponent<Image>();

            if (MissionManager.Instance.IsMissionClear(6))
            {
                image.color = new Color(118 / 255f, 118 / 255f, 118 / 255f);
                checkd[i].SetActive(true);
            }
            else
            {
                image.color = new Color(240 / 255f, 255 / 255f, 0 / 255f);
            }

            missionButtons[i].GetComponent<Button>().enabled = true;
        }
    }

}
