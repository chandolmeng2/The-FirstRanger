using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 1.5f; // 높일수록 더 빠름

    public Transform playerCamera;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // 마우스 Y 움직임으로 상하 회전
        if (Mathf.Abs(mouseY) > 0.01f)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 마우스 X 움직임으로 좌우 회전
        if (Mathf.Abs(mouseX) > 0.01f)
        {
            transform.Rotate(Vector3.up * mouseX); // 좌우 회전만 적용
        }
    }



    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * horizontal + transform.forward * vertical).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}


