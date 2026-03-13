using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 10;     // Lượng máu bị trừ
    public float damageCooldown = 1f;  // Thời gian chờ giữa 2 lần gây sát thương (nếu đứng lì trên bẫy)

    private float lastDamageTime;

    // Khi Player bắt đầu chạm vào bẫy
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyDamage(other.gameObject);
        }
    }

    // Khi Player đứng yên trên bẫy (ví dụ bẫy lửa, bẫy gai)
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > lastDamageTime + damageCooldown)
        {
            ApplyDamage(other.gameObject);
        }
    }

    private void ApplyDamage(GameObject playerObj)
    {
        // Giả sử Script trên Player của bạn tên là PlayerController
        // Bạn cần lấy ID của Player đó để báo cho GameEvents
        var playerCtrl = playerObj.GetComponent<Player>();

        if (playerCtrl != null)
        {
            int id = playerCtrl.playerID;

            // Gọi sự kiện gây sát thương đã có trong GameEvents.cs của bạn
            GameEvents.RaiseTakeDamage(id, damageAmount);

            lastDamageTime = Time.time;
            Debug.Log($"Bẫy đã kích hoạt! Player {id} mất {damageAmount} HP.");
        }
    }
}