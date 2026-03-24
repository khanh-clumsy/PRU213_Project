using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private AttackData data;
    private Player owner;
    private Vector2 direction;
    private float speed = 10f;
    public bool isDestroyed = false;

    [Header("Clash Effect")]
    public GameObject clashEffectPrefab; // (Tuỳ chọn) Hiệu ứng nổ khi 2 đạn chạm nhau
    public float clashRadius = 0.6f;     // Bán kính phát hiện va chạm giữa 2 projectile

    // ── Static list để track TẤT CẢ projectile đang sống trên sân ──
    private static readonly List<Projectile> activeProjectiles = new List<Projectile>();

    private void OnEnable()
    {
        activeProjectiles.Add(this);
    }

    private void OnDisable()
    {
        activeProjectiles.Remove(this);
    }

    public void Setup(Player owner, AttackData data, Vector2 direction, float speed)
    {
        this.owner = owner;
        this.data = data;
        this.direction = direction;
        this.speed = speed;

        // TỰ ĐỘNG set Layer theo owner
        if (owner.playerID == 1)
        {
            gameObject.layer = LayerMask.NameToLayer("Hitbox1");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Hitbox2");
        }

        // Hủy sau 3 giây để tránh rác bộ nhớ
        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        if (isDestroyed) return;

        transform.Translate(direction * speed * Time.deltaTime);
        CheckProjectileClash();
    }

    private void CheckProjectileClash()
    {
        foreach (Projectile other in activeProjectiles)
        {
            // Bỏ qua chính mình, đạn đã hủy, hoặc đạn cùng chủ
            if (other == this || other.isDestroyed || other.owner == owner) continue;

            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist <= clashRadius)
            {
                Debug.Log($"<color=yellow>[Projectile Clash]</color> {owner?.name} vs {other.owner?.name} — Triệt tiêu nhau tại dist={dist:F2}!");

                // Spawn hiệu ứng nổ ở điểm giữa 2 đạn (nếu có prefab)
                if (clashEffectPrefab != null)
                {
                    Vector3 midPoint = (transform.position + other.transform.position) / 2f;
                    Instantiate(clashEffectPrefab, midPoint, Quaternion.identity);
                }

                // Hủy cả 2
                other.SelfDestroy();
                SelfDestroy();
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        // Va chạm với Hurtbox (đánh người)
        if (collision.TryGetComponent(out Hurtbox hurtbox))
        {
            // Không tự bắn trúng mình
            if (hurtbox.owner == owner) return;

            if (owner != null)
            {
                owner.AddMana(5);
            }

            hurtbox.TakeHit(data, direction);
            Debug.Log($"{owner?.name}'s projectile hit {hurtbox.owner.name}");
            SelfDestroy();
        }
    }

    public void SelfDestroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        Destroy(gameObject);
    }
}