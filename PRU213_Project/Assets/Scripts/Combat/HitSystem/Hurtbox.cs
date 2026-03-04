using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public Player owner;

    public void TakeHit(AttackData data, Vector2 hitDirection)
    {
        owner.TakeDamage(data, hitDirection);
    }
}