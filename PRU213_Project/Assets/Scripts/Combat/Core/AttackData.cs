using UnityEngine;
public enum AttackType { Melee, Projectile, Ultimate }

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    public AttackType type;
    public bool isTeleport;
    public bool isGuardBreak;
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public string attackName; // Tên Animation trong Animator
    public float animationSpeed = 1f; 
    public int startupFrames = 5;
    public int activeFrames = 3;
    public int recoveryFrames = 10;
    public int damage = 10;
    public float knockbackForce = 5f;
    public int hitstunFrames = 20;
}