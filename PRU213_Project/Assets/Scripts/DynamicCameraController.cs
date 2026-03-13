using UnityEngine;

public class DynamicCameraController : MonoBehaviour
{
    [Header("Targets")]
    public Transform player1;
    public Transform player2;

    [Header("Size & Zoom")]
    public float minSize = 4.5f;      // Mức Zoom gần nhất (khi 2 người đứng sát nhau)
    public float maxSize = 6.5f;      // Mức Zoom xa nhất (không nên để quá lớn sẽ lộ biên)
    public float zoomSpeed = 5f;

    [Header("Map Boundaries")]
    // Bạn kéo Camera đến mép Trái/Phải/Trên/Dưới của Map rồi điền tọa độ vào đây
    public float minX = -5f;
    public float maxX = 5f;
    public float minY = -2f;
    public float maxY = 2f;

    [Header("Smoothing")]
    public float smoothTime = 0.15f;
    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Thiết lập size ban đầu dựa trên ảnh bạn gửi
        cam.orthographicSize = minSize;
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        HandleCameraMovement();
        HandleCameraZoom();
    }

    void HandleCameraMovement()
    {
        // 1. Tìm điểm chính giữa 2 người chơi
        Vector3 centerPoint = (player1.position + player2.position) / 2f;

        // 2. Giới hạn (Clamp) tâm không được vượt quá biên của Map
        float clampedX = Mathf.Clamp(centerPoint.x, minX, maxX);
        float clampedY = Mathf.Clamp(centerPoint.y, minY, maxY);

        Vector3 targetPosition = new Vector3(clampedX, clampedY, -10f);

        // 3. Di chuyển mượt mà
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void HandleCameraZoom()
    {
        // Tính khoảng cách giữa 2 nhân vật
        float distance = Vector2.Distance(player1.position, player2.position);

        // Chuyển đổi khoảng cách thành Orthographic Size
        // Khoảng cách càng lớn (distance), targetSize càng tiến gần maxSize
        float targetSize = Mathf.Lerp(minSize, maxSize, distance / 10f);

        // Áp dụng zoom mượt mà
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }
}