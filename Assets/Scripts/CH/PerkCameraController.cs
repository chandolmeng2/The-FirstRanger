using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkCameraController : MonoBehaviour
{
    public float moveSpeed = 0.02f; // ���콺 �����ӿ� ���� �̵� �ӵ�
    public float smoothTime = 0.07f; // Lerp ���� �ӵ�
    public Vector2 xLimit = new Vector2(-0.2f, 0.2f); // X�� �̵� ����
    public Vector2 yLimit = new Vector2(-0.2f, 0.2f); // Y�� �̵� ����

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
        /*Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

        // ���콺 �������� ������� ��ǥ ��ġ ���
        float moveX = mouseDelta.x * moveSpeed * Time.deltaTime;
        float moveY = mouseDelta.y * moveSpeed * Time.deltaTime;

        targetPosition += new Vector3(moveX, moveY, 0);

        // ��� ����
        targetPosition.x = Mathf.Clamp(targetPosition.x, startPosition.x + xLimit.x, startPosition.x + xLimit.y);
        targetPosition.y = Mathf.Clamp(targetPosition.y, startPosition.y + yLimit.x, startPosition.y + yLimit.y);

        // ���� ��ġ�� ��ǥ ��ġ�� Lerp
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime);

        lastMousePosition = Input.mousePosition;*/
    }
}

