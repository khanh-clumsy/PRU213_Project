using UnityEngine;

public class DynamicCameraController : MonoBehaviour
{
    public Transform player1, player2;
    public SpriteRenderer backgroundSprite;

    [Header("Zoom Settings")]
    public float minSize = 3.5f;
    public float zoomPadding = 1.5f; // Khoảng trống giữa nhân vật và rìa màn hình
    public float zoomSpeed = 5f;

    [Header("Movement Settings")]
    public float smoothTime = 0.1f;

    private Camera cam;
    private Vector3 velocity;

    // Biến static để các Script Player có thể truy cập
    public static float CamMinX, CamMaxX;

    void Start() { cam = GetComponent<Camera>(); }

    void LateUpdate()
    {
        if (!player1 || !player2 || !backgroundSprite) return;

        HandleZoom();
        HandlePosition();

        // Cập nhật tọa độ biên màn hình hiện tại để chặn Player
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        CamMinX = transform.position.x - halfWidth;
        CamMaxX = transform.position.x + halfWidth;
    }

    void HandleZoom()
    {
        float distance = Vector2.Distance(player1.position, player2.position);

        // Công thức tính Size dựa trên khoảng cách và tỉ lệ màn hình (Aspect Ratio)
        // Đảm bảo nhân vật không bao giờ chạm sát mép camera nhờ zoomPadding
        float requiredSize = (distance / 2f) / cam.aspect + zoomPadding;

        // Giới hạn Size theo biên Map
        Bounds b = backgroundSprite.bounds;
        float maxAllowedSize = Mathf.Min(b.size.y / 2f, (b.size.x / 2f) / cam.aspect);

        float targetSize = Mathf.Clamp(requiredSize, minSize, maxAllowedSize);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    void HandlePosition()
    {
        Bounds b = backgroundSprite.bounds;
        float camH = cam.orthographicSize;
        float camW = camH * cam.aspect;

        Vector3 targetCenter = (player1.position + player2.position) / 2f;
        Vector3 smoothPos = Vector3.SmoothDamp(transform.position, new Vector3(targetCenter.x, targetCenter.y, -10f), ref velocity, smoothTime);

        // Chặn biên Camera trong Background
        float finalX = Mathf.Clamp(smoothPos.x, b.min.x + camW, b.max.x - camW);
        float finalY = Mathf.Clamp(smoothPos.y, b.min.y + camH, b.max.y - camH);

        transform.position = new Vector3(finalX, finalY, -10f);
    }
}