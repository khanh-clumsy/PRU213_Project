using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player owner;
    public AttackData currentAttack;

    private bool active;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void EnableHitbox(AttackData data)
    {
        currentAttack = data;
        gameObject.SetActive(true);
    }

    public void DisableHitbox()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"<color=yellow>[Hitbox]</color> {owner.name} đã kích hoạt hitbox và va chạm với {other.name} {active}");

        if (other.TryGetComponent(out Hurtbox hurtbox))
        {
            if (hurtbox.owner == owner) return;

            // Thêm Log này
            Debug.Log($"<color=red>[Hitbox]</color> {owner.name} đã đánh trúng {hurtbox.owner.name}");

            Vector2 dir = (hurtbox.transform.position - transform.position).normalized;
            hurtbox.TakeHit(currentAttack, dir);

            active = false;
        }
    }
}