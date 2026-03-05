using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public Player owner;

    public void TakeHit(AttackData data, Vector2 hitDirection)
    {
        // Thêm Log này
        Debug.Log($"<color=yellow>[Hurtbox]</color> {owner.name} nhận lệnh TakeHit");
        owner.TakeDamage(data, hitDirection);
    }
}