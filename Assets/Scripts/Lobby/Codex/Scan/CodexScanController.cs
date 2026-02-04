using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CodexScanController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject scanBgUI;
    [SerializeField] private Image scanGauge;
    [SerializeField] private RectTransform guideImageTransform; // F키 안내 이미지
    [SerializeField] private GameObject codexCompletePanel;
    [SerializeField] private Image whiteFlash;

    [Header("설정")]
    [SerializeField] private float scanDistance = 10f;
    [SerializeField] private float fillSpeed = 0.5f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("씬 제한 설정")]
    [SerializeField] private string[] disabledScenes = { "Mission6Scene", "Mission6" }; // 비활성화할 씬 이름들
    [SerializeField] private bool enableScanSystem = true; // Inspector에서 직접 on/off 가능

    private Camera playerCamera;
    private PlayerController playerController;

    public static bool IsScanning { get; private set; }
    private bool isScanning = false;
    private float currentFill = 0f;
    private ItemObject targetItem = null;
    private bool isReadyToRegister = false;

    private Tween guidePulseTween;

    private Quaternion initialRotation;
    private float initialX, initialY;
    private float rotationX = 0f;
    private float rotationY = 0f;

    // 오직 Tutorial3을 위한 변수임
    public bool isRegistered = false;

    void Awake()
    {
        playerCamera = Camera.main;
        playerController = FindObjectOfType<PlayerController>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 현재 씬 체크
        CheckCurrentScene();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerCamera = Camera.main;
        playerController = FindObjectOfType<PlayerController>();

        // 씬 로드 시마다 체크
        CheckCurrentScene();
    }

    // 현재 씬이 비활성화 목록에 있는지 체크
    void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 비활성화 씬 목록에 현재 씬이 있는지 확인
        foreach (string sceneName in disabledScenes)
        {
            if (currentSceneName.Contains(sceneName))
            {
                enableScanSystem = false;

                // 이미 스캔 중이었다면 강제 종료
                if (isScanning)
                {
                    ForceStopScanning();
                }
                return;
            }
        }

        // 비활성화 목록에 없다면 활성화
        enableScanSystem = true;
    }

    // 스캔 가능 여부 체크 메서드
    bool CanStartScanning()
    {
        // 시스템이 비활성화되어 있으면 false
        if (!enableScanSystem)
            return false;

        // 다른 UI가 활성화되어 있으면 false
        if (BookCodex.codexActivated || PuzzleUIManager.IsPuzzleActive || QTEManager.IsQTEActive)
            return false;

        return true;
    }

    void Update()
    {
        // 스캔 시스템이 비활성화되어 있으면 Update 로직을 실행하지 않음
        if (!enableScanSystem)
            return;

        if (Input.GetKeyDown(KeyCode.F) && CanStartScanning())
        {
            if (!isScanning)
            {
                isScanning = true;
                IsScanning = true;

                // 스캔 모드 시작 시 걷기 소리 즉시 중지
                SoundManager.Instance.StopWalkingLoop(0.1f);

                CanvasGroup scanGroup = scanBgUI.GetComponent<CanvasGroup>();
                if (scanGroup != null)
                {
                    scanBgUI.SetActive(true);
                    scanGroup.alpha = 0f;
                    scanGroup.DOFade(1f, 0.6f);
                }
                else
                {
                    scanBgUI.SetActive(true);
                }

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (playerController != null) playerController.enabled = false;

                initialRotation = playerCamera.transform.localRotation;
                Vector3 euler = initialRotation.eulerAngles;
                initialX = NormalizeAngle(euler.x);
                initialY = NormalizeAngle(euler.y);
                rotationX = 0f;
                rotationY = 0f;
            }
            else
            {
                if (isReadyToRegister && targetItem != null)
                {
                    //Codex.instance.RegisterToCodex(targetItem.itemData);
                    BookCodex.instance.RegisterToCodex(targetItem.itemData);
                    BookCodex.instance.RefreshPage();
                    isRegistered = true;

                    // 도감 등록 순간 찰칵 소리 재생
                    SoundManager.Instance.Play(SoundKey.CameraSound);

                    Destroy(targetItem.gameObject);
                    StartCoroutine(PlayRegistrationEffectWithDelay());
                    return; // 등록 후 스캔 종료는 잠시 지연
                }

                StopScanning();
            }
        }

        if (isScanning)
        {
            RotateCameraByMouse();
            HandleScan();
        }
    }

    // 스캔 중지 로직을 별도 메서드로 분리
    void StopScanning()
    {
        isScanning = false;
        IsScanning = false;

        CanvasGroup scanGroup = scanBgUI.GetComponent<CanvasGroup>();
        if (scanGroup != null)
        {
            scanGroup.DOFade(0f, 0.6f).OnComplete(() => scanBgUI.SetActive(false));
        }
        else
        {
            scanBgUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.enabled = true;

        scanGauge.fillAmount = 0f;
        scanGauge.color = Color.white;
        currentFill = 0f;
        isReadyToRegister = false;
        targetItem = null;

        if (guidePulseTween != null)
        {
            guidePulseTween.Kill();
            guidePulseTween = null;
            guideImageTransform.localScale = Vector3.one;
        }
    }

    // 강제 스캔 중지 (씬 전환 시 사용)
    void ForceStopScanning()
    {
        isScanning = false;
        IsScanning = false;

        if (scanBgUI != null)
            scanBgUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;

        if (scanGauge != null)
        {
            scanGauge.fillAmount = 0f;
            scanGauge.color = Color.white;
        }

        currentFill = 0f;
        isReadyToRegister = false;
        targetItem = null;

        if (guidePulseTween != null)
        {
            guidePulseTween.Kill();
            guidePulseTween = null;
            if (guideImageTransform != null)
                guideImageTransform.localScale = Vector3.one;
        }
    }

    void HandleScan()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * scanDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, scanDistance))
        {
            ItemObject item = hit.collider.GetComponent<ItemObject>();

            if (item != null && item.itemData.itemType == ItemType.Codex)
            {
                if (targetItem != item)
                {
                    targetItem = item;
                    currentFill = 0f;
                    isReadyToRegister = false;
                    scanGauge.color = Color.white;
                }

                currentFill += Time.deltaTime * fillSpeed;
                scanGauge.fillAmount = currentFill;

                if (currentFill >= 1f)
                {
                    currentFill = 1f;
                    isReadyToRegister = true;
                    scanGauge.color = Color.yellow;

                    if (guidePulseTween == null)
                    {
                        guidePulseTween = guideImageTransform.DOScale(1.1f, 0.5f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine);
                    }
                }
                return;
            }
        }

        targetItem = null;
        currentFill = 0f;
        scanGauge.fillAmount = 0f;
        isReadyToRegister = false;
        scanGauge.color = Color.white;

        if (guidePulseTween != null)
        {
            guidePulseTween.Kill();
            guidePulseTween = null;
            guideImageTransform.localScale = Vector3.one;
        }
    }

    void RotateCameraByMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -15f, 15f);
        rotationY = Mathf.Clamp(rotationY, -15f, 15f);

        float finalX = initialX + rotationX;
        float finalY = initialY + rotationY;

        playerCamera.transform.localRotation = Quaternion.Euler(finalX, finalY, 0f);
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    IEnumerator PlayRegistrationEffectWithDelay()
    {
        codexCompletePanel.SetActive(true);
        CanvasGroup cg = codexCompletePanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.6f);
        }

        whiteFlash.color = new Color(1f, 1f, 1f, 0f);
        whiteFlash.DOFade(0.6f, 0.1f).OnComplete(() =>
        {
            whiteFlash.DOFade(0f, 0.5f);
        });

        yield return new WaitForSeconds(1.5f);

        if (cg != null)
        {
            cg.DOFade(0f, 0.6f).OnComplete(() => codexCompletePanel.SetActive(false));
        }
        else
        {
            codexCompletePanel.SetActive(false);
        }

        // 스캔 종료 처리 - StopScanning 메서드 재사용
        StopScanning();
    }

    // 외부에서 스캔 시스템을 활성화/비활성화할 수 있는 public 메서드
    public void SetScanSystemEnabled(bool enabled)
    {
        enableScanSystem = enabled;

        if (!enabled && isScanning)
        {
            ForceStopScanning();
        }
    }

    // 현재 스캔 시스템 활성화 상태를 반환
    public bool IsScanSystemEnabled()
    {
        return enableScanSystem;
    }
}