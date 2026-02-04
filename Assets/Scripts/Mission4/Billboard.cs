using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera; // 직접 연결 가능

    void LateUpdate()
    {
        if (targetCamera == null || !targetCamera.enabled)
        {
            // 현재 활성화된 카메라를 자동으로 찾기
            Camera[] cameras = Camera.allCameras;
            foreach (var cam in cameras)
            {
                if (cam.enabled)
                {
                    targetCamera = cam;
                    break;
                }
            }

            // 그래도 못 찾으면 패스
            if (targetCamera == null) return;
        }

        // 말풍선을 카메라가 보는 방향으로 회전
        transform.forward = targetCamera.transform.forward;
    }
}
