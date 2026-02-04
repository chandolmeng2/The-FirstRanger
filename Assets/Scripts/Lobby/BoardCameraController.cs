using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCameraController : MonoBehaviour
{
    public float moveSpeed = 0.02f; // 마우스 움직임에 따른 이동 속도
    public float smoothTime = 0.07f; // Lerp 보간 속도
    public Vector2 xLimit = new Vector2(-0.2f, 0.2f); // X축 이동 제한
    public Vector2 yLimit = new Vector2(-0.2f, 0.2f); // Y축 이동 제한

    private Vector3 startPosition;
    private Vector3 lastMousePosition;
    private Vector3 targetPosition;

    private void Start()
    {
        startPosition = transform.position;
        lastMousePosition = Input.mousePosition;
        targetPosition = transform.position;
    }

    void Update()
    {
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

        // 마우스 움직임을 기반으로 목표 위치 계산
        float moveX = -mouseDelta.x * moveSpeed * Time.deltaTime;
        float moveY = mouseDelta.y * moveSpeed * Time.deltaTime;

        targetPosition += new Vector3(moveX, moveY, 0);

        // 경계 제한
        targetPosition.x = Mathf.Clamp(targetPosition.x, startPosition.x + xLimit.x, startPosition.x + xLimit.y);
        targetPosition.y = Mathf.Clamp(targetPosition.y, startPosition.y + yLimit.x, startPosition.y + yLimit.y);

        // 현재 위치를 목표 위치로 Lerp
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime);

        lastMousePosition = Input.mousePosition;
    }
}

