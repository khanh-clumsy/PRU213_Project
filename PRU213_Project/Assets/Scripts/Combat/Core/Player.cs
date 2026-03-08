using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerID;

    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;
    public int maxMana = 100;
    private int currentMana = 0;
    public int CurrentMana => currentMana;

    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0f;
    private float lastDashTime = -999f;

    public DashState DashState;


    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public Hitbox Hitbox { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerInputHandler Input { get; private set; }

    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerState IdleState;

    public AttackState LightAttackState;
    public AttackState StrongAttackState;

    public RunState RunState;
    public JumpState JumpState;

    public AttackData lightAttackData;
    public AttackData strongAttackData;

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
        LightAttackState = new AttackState(this, lightAttackData);
        StrongAttackState = new AttackState(this, strongAttackData);
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
        CheckDashInput();
    }
    void CheckDashInput()
    {
        if (Input.DashPressed && Time.time >= lastDashTime + dashCooldown)
        {
            if (StateMachine.CurrentState is not AttackState && StateMachine.CurrentState is not HurtState)
            {
                lastDashTime = Time.time;
                StateMachine.ChangeState(new DashState(this));
            }
        }
    }
    public void AddMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        Debug.Log($"Mana hiện tại của {name}: {currentMana}");
    }

    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false;
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