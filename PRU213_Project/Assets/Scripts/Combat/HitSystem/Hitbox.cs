using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player owner;
    public AttackData currentAttack;

    private bool active;

    public void EnableHitbox(AttackData data)
    {
        currentAttack = data;
        active = true;
    }

    public void DisableHitbox()
    {
        active = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        if (other.TryGetComponent(out Hurtbox hurtbox))
        {
            if (hurtbox.owner == owner) return;

            Vector2 dir = (hurtbox.transform.position - transform.position).normalized;
            hurtbox.TakeHit(currentAttack, dir);

            active = false; // đánh trúng 1 lần
        }
    }
}