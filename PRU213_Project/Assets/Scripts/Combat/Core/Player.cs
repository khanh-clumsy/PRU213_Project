using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerID;

    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;

    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public Hitbox Hitbox { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerInputHandler Input { get; private set; }

    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerState IdleState;
    public RunState RunState;
    public JumpState JumpState;

    [Header("Combo")]
    public List<AttackData> lightComboSequence;

    private Queue<bool> inputBuffer = new Queue<bool>();
    private const int MAX_BUFFER_SIZE = 3;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Hitbox = GetComponentInChildren<Hitbox>();
        Movement = GetComponent<PlayerMovement>();
        Input = GetComponent<PlayerInputHandler>();

        Hitbox.owner = this;

        StateMachine = new PlayerStateMachine();

        IdleState = new IdleState(this);
        RunState = new RunState(this);
        JumpState = new JumpState(this);

        currentHP = maxHP;
    }

    private void Start()
    {
        StateMachine.ChangeState(IdleState);
    }

    private void Update()
    {
        StateMachine.Update();
    }

   
   

    public void TakeDamage(AttackData data, Vector2 direction)
    {
        Debug.Log($"[Player] {name} TakeDamage. HP: {currentHP}");

        currentHP -= data.damage;

        GameEvents.RaiseHealthChanged(playerID, currentHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            GameEvents.RaisePlayerDied(playerID);
        }

        Vector2 knockback = direction * data.knockbackForce;

        StateMachine.ChangeState(
            new HurtState(this, data.hitstunFrames, knockback)
        );
    }

   

    public bool IsAttackState()
    {
        return StateMachine.CurrentState is AttackState;
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }
}