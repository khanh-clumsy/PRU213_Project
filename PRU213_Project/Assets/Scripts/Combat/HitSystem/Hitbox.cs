using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player owner;

    public Vector2 size = new Vector2(2.0f, 0.6f);
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
                owner.AddMana(20);
                Vector2 dir =
                    (hurtbox.transform.position - transform.position).normalized;

                hurtbox.TakeHit(attack, dir);

                Debug.Log($"Hit confirmed: {owner.name} → {hurtbox.owner.name}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Vẽ hitbox khi chế độ dev (lúc bao giờ cũng thấy)
        Vector2 center = (Vector2)transform.position + offset;

        Gizmos.color = Color.red;
        DrawBoxGizmo(center, size);
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ chi tiết hơn khi chọn object này
        Vector2 center = (Vector2)transform.position + offset;

        Gizmos.color = Color.green;
        DrawBoxGizmo(center, size);

        // Vẽ điểm tâm của hitbox
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.1f);

        // Vẽ vị trí gốc của character
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }

    private void DrawBoxGizmo(Vector2 center, Vector2 size)
    {
        Vector2 halfSize = size / 2f;

        // 4 góc của hình chữ nhật
        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);

        // Vẽ 4 cạnh của hình chữ nhật
        Gizmos.DrawLine(topLeft, topRight);      // Trên
        Gizmos.DrawLine(topRight, bottomRight);  // Phải
        Gizmos.DrawLine(bottomRight, bottomLeft);// Dưới
        Gizmos.DrawLine(bottomLeft, topLeft);    // Trái
    }


}