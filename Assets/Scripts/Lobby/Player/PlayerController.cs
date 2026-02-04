using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // �̵� �ӵ� ���� ����
    [SerializeField] private float walkSpeed = 3f;    // �ȱ� �ӵ�
    [SerializeField] private float runSpeed = 5f;     // �޸��� �ӵ�

    // ī�޶� ȸ�� ���� ����
    [SerializeField] private float lookSensitivity = 2f;       // ���콺 ȸ�� �ΰ���
    [SerializeField] private float cameraRotationLimit = 60f;  // ���Ʒ� ȸ�� ���� (�� ����)

    // �÷��̾ �ٶ󺸴� ī�޶�
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerModel;

    private float currentCameraRotationX = 0f; // ī�޶��� ���Ʒ� ȸ�� ���� ������
    private float currentSpeed;                // ���� �ӵ� (�ȱ� or �޸���)

    // �ʿ��� ������Ʈ 
    private Rigidbody rb;
    private Animator animator;
    [SerializeField] private ActionController actionController;

    public static bool IsJournalOpen = false;

    //���� ���� ����
    private bool isWalkingSoundPlaying = false;
    public static bool IsDialogueActive = false; //Ʃ����� �� ��ȭ �̺�Ʈ �� �÷��̾� �ȱ� �Ҹ� ���ܿ� ����
    [SerializeField] private bool isIndoor = false; // Inspector���� �����ϰų�, �� ���� �� �ڵ�� ����

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStaticVariables()
    {
        IsDialogueActive = false;
        IsJournalOpen = false;
    }

    void Start()
    {
        // 명시적으로 초기화
        IsDialogueActive = false;
        //컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();

        currentSpeed = walkSpeed; // ������ �ȴ� �ӵ�

        if (playerModel != null)
            animator = playerModel.GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ����, ����, �ִϸ��̼� ��, ���� Ȱ�� ���� �� ���� ����
        if (PuzzleUIManager.IsPuzzleActive ||
            BookCodex.codexActivated ||
            IsJournalOpen ||
            PlayerController.IsDialogueActive ||
            CodexScanController.IsScanning || //
            (actionController != null && actionController.IsTrashPuzzlePlaying))
        {
            Debug.Log("�̺�Ʈ�� ���� �̵� ����, �߼Ҹ� ���� �õ�");
            // �ȱ� ���� ���� ����
            if (isWalkingSoundPlaying)
            {
                isWalkingSoundPlaying = false;
                SoundManager.Instance.StopWalkingLoop();
            }
            return;
        }

        HandleMovement();        // �̵� ó��
        HandleCameraRotation();  // ���콺 ȸ�� ó��
        UpdateAnimation();       // �ִϸ��̼� ���� ������Ʈ
    }

    //�̵� ó�� �Լ� (WASD + Shift)
    private void HandleMovement()
    {
        // ���� �Է� �ޱ�
        float moveX = Input.GetAxisRaw("Horizontal"); // A, D
        float moveZ = Input.GetAxisRaw("Vertical");   // W, S

        // ���� ���� ���
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        // Shift Ű �Է� �� �޸���
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // ���� �̵� ó�� (rigidbody ��ġ ����)
        rb.MovePosition(transform.position + move * currentSpeed * Time.deltaTime);
    }

    //���콺 ȸ�� ó��
    private void HandleCameraRotation()
    {
        // ���콺 �̵��� ����
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity; // �¿�
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity; // ����

        // �÷��̾� ������Ʈ �¿� ȸ�� (Y�� ����)
        transform.Rotate(Vector3.up * mouseX);

        // ī�޶��� ���� ȸ�� ���� ����
        currentCameraRotationX -= mouseY;

        // ���Ʒ� ȸ�� ���� ���� (-cameraRotationLimit ~ +cameraRotationLimit)
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // ȸ�� ���� (ī�޶��� ���� X�ุ ȸ��)
        playerCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }

    //�ִϸ��̼� �Ķ���� ������Ʈ
    private void UpdateAnimation()
    {
        // �̵� Ű�� �Է� ���� (0: ����, 1: �ִ�)
        float speed = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).magnitude;

        // Shift Ű ���� �� �޸��� ��������
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // �ִϸ����� �Ķ���� ����
        animator.SetFloat("Speed", speed);        // �̵� ���� ����
        animator.SetBool("isRunning", isRunning); // �޸��� ���� ����

        HandleWalkSound(speed > 0, isRunning);
    }

    private void HandleWalkSound(bool isMoving, bool isRunning)
    {
        if (isMoving)
        {
            SoundKey key = isRunning
            ? SoundKey.PlayerRun
            : (isIndoor ? SoundKey.PlayerWalkIndoor : SoundKey.PlayerWalkOutdoor);

            var clip = SoundManager.Instance.GetClip(key);

            if (clip != null)
            {
                SoundManager.Instance.SwitchWalkingLoop(clip, 0.2f, isRunning, isIndoor); // isIndoor �Ķ���� �߰�
                isWalkingSoundPlaying = true;
            }
        }
        else
        {
            if (isWalkingSoundPlaying)
            {
                isWalkingSoundPlaying = false;
                SoundManager.Instance.StopWalkingLoop();
            }
        }
    }
}
