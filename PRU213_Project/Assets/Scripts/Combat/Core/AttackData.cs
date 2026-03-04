using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    public string attackName; // Tên Animation trong Animator
    public int startupFrames = 5;
    public int activeFrames = 3;
    public int recoveryFrames = 10;
    public int damage = 10;
    public float knockbackForce = 5f;
    public int hitstunFrames = 20;
}