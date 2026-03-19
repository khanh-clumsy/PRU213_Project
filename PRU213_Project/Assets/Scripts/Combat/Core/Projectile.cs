using UnityEngine;

public class Projectile : MonoBehaviour
{
    private AttackData data;
    private Player owner;
    private Vector2 direction;
    private float speed = 10f;

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
            Debug.Log($"[Projectile] Player 1 projectile - Layer set to: Hitbox");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Hitbox2");
            Debug.Log($"[Projectile] Player 2 projectile - Layer set to: Hitbox2");
        }

        // Hủy sau 3 giây để tránh rác bộ nhớ
        Destroy(gameObject, 3f);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Hurtbox hurtbox))
        {
            // Không tự bắn trúng mình
            if (hurtbox.owner == owner) return;
            owner.AddMana(5);
            // Gây sát thương dựa trên AttackData
            hurtbox.TakeHit(data, direction);

            // Hiệu ứng và biến mất
            Debug.Log($"{owner.name}'s projectile hit {hurtbox.owner.name}");
            Destroy(gameObject);
        }
    }
}