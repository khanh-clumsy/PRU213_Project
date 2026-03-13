using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int playerID; // 1 cho Player 1, 2 cho Player 2
    public bool isFacingRight; // Tích vào nếu muốn nhân vật quay mặt sang phải

    // Vẽ Icon và hướng nhìn trong cửa sổ Scene (không hiện trong Game)
    private void OnDrawGizmos()
    {
        // Vẽ vòng tròn đại diện cho vị trí Spawn
        Gizmos.color = (playerID == 1) ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Vẽ mũi tên chỉ hướng mặt của nhân vật
        Gizmos.color = Color.yellow;
        Vector3 direction = isFacingRight ? Vector3.right : Vector3.left;
        Vector3 arrowTip = transform.position + direction * 1.0f;

        Gizmos.DrawLine(transform.position, arrowTip);
        // Vẽ thêm 2 nét gạch cho giống hình mũi tên
        Gizmos.DrawLine(arrowTip, arrowTip + (Quaternion.Euler(0, 0, 150) * direction) * 0.3f);
        Gizmos.DrawLine(arrowTip, arrowTip + (Quaternion.Euler(0, 0, -150) * direction) * 0.3f);
    }
}