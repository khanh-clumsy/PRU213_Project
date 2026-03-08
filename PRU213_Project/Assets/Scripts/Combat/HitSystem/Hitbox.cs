using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player owner;

    public Vector2 size = new Vector2(1.2f, 0.6f);
    public Vector2 offset;

    private HashSet<Hurtbox> hitTargets = new HashSet<Hurtbox>();

    public void ResetHitbox()
    {
        hitTargets.Clear();
    }

    public void CheckHit(AttackData attack)
    {
        Vector2 center = (Vector2)transform.position + offset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            size,
            0f
        );

        foreach (var col in hits)
        {
            if (col.TryGetComponent(out Hurtbox hurtbox))
            {
                if (hurtbox.owner == owner) continue;

                if (hitTargets.Contains(hurtbox)) continue;

                hitTargets.Add(hurtbox);
                owner.AddMana(10);
                Vector2 dir =
                    (hurtbox.transform.position - transform.position).normalized;

                hurtbox.TakeHit(attack, dir);

                Debug.Log($"Hit confirmed: {owner.name} → {hurtbox.owner.name}");
            }
        }
    }


}