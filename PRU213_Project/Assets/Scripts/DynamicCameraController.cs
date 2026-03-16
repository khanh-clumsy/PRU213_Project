using UnityEngine;

public class DynamicCameraController : MonoBehaviour
{
    [Header("Targets")]
    public Transform player1;
    public Transform player2;

    [Header("Camera Zoom Settings")]
    public float minSize = 5f;
    public float padding = 2f;
    public float smoothSpeed = 5f;

    [Header("Map Bounds Settings")]
    public SpriteRenderer mapBackground;
    [Tooltip("Khoảng lùi viền an toàn (0.1 -> 0.5) để giấu nét cắt của ảnh nền")]
    public float edgeBuffer = 0.2f;

    private Camera cam;
    private float maxSize;

    // --- Biến mới cho Tường Vô Hình ---
    private BoxCollider2D leftWall;
    private BoxCollider2D rightWall;
    private float wallThickness = 2f; // Độ dày của tường (để nhân vật không xuyên qua được)

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (mapBackground != null)
        {
            // QUAN TRỌNG: Trừ hao edgeBuffer ngay ở MaxSize để chừa không gian cho viền an toàn
            maxSize = (mapBackground.bounds.size.y / 2f) - edgeBuffer;
        }
        else
        {
            maxSize = 10f;
        }

        SetupCameraWalls(); // Khởi tạo tường vô hình
    }

    private void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        ZoomCamera();
        MoveAndClampCamera();

        // Cập nhật vị trí bức tường theo viền Camera mỗi frame
        UpdateCameraWalls();
    }

    private void ZoomCamera()
    {
        float distanceX = Mathf.Abs(player1.position.x - player2.position.x);
        float distanceY = Mathf.Abs(player1.position.y - player2.position.y);

        float sizeX = (distanceX * 0.5f + padding) / cam.aspect;
        float sizeY = distanceY * 0.5f + padding;

        float targetSize = Mathf.Max(sizeX, sizeY);
        targetSize = Mathf.Clamp(targetSize, minSize, maxSize);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * smoothSpeed);
    }

    private void MoveAndClampCamera()
    {
        Vector3 targetPosition = (player1.position + player2.position) / 2f;
        targetPosition.z = transform.position.z;

        // Di chuyển mượt mà
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // Khóa viền tuyệt đối
        if (mapBackground != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float minX = mapBackground.bounds.min.x + camWidth + edgeBuffer;
            float maxX = mapBackground.bounds.max.x - camWidth - edgeBuffer;
            float minY = mapBackground.bounds.min.y + camHeight + edgeBuffer;
            float maxY = mapBackground.bounds.max.y - camHeight - edgeBuffer;

            // BẢO HIỂM 100%: Xử lý lỗi min > max khi Camera zoom đạt đỉnh
            if (minX > maxX)
            {
                // Khóa cứng trục X ở chính giữa Map
                minX = mapBackground.bounds.center.x;
                maxX = mapBackground.bounds.center.x;
            }
            if (minY > maxY)
            {
                // Khóa cứng trục Y ở chính giữa Map
                minY = mapBackground.bounds.center.y;
                maxY = mapBackground.bounds.center.y;
            }

            // Ép vị trí thực tế
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }
    // ==========================================
    // LOGIC TƯỜNG VÔ HÌNH (INVISIBLE WALLS)
    // ==========================================
    private void SetupCameraWalls()
    {
        // 1. Thêm Rigidbody2D (Kinematic) để xử lý va chạm mượt mà khi Camera di chuyển
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; // Rất quan trọng: Kinematic giúp tường đẩy được nhân vật mà không bị rớt do trọng lực
        }

        // 2. Tạo 2 cái BoxCollider2D
        leftWall = gameObject.AddComponent<BoxCollider2D>();
        rightWall = gameObject.AddComponent<BoxCollider2D>();
    }

    private void UpdateCameraWalls()
    {
        // Tính toán chiều cao và chiều rộng thực tế của Camera
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // Cập nhật Size cho tường (Chiều cao tường x2 để đảm bảo chặn kín kể cả khi nhân vật nhảy)
        leftWall.size = new Vector2(wallThickness, camHeight * 2f);
        rightWall.size = new Vector2(wallThickness, camHeight * 2f);

        // Cập nhật Vị trí (Offset) cho tường sao cho nằm chính xác ở 2 mép màn hình
        leftWall.offset = new Vector2(-camWidth / 2f - wallThickness / 2f, 0f);
        rightWall.offset = new Vector2(camWidth / 2f + wallThickness / 2f, 0f);
    }
}