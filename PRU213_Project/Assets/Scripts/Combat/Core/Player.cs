using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Combat.States;

public class Player : MonoBehaviour
{
    public int playerID;

    [Header("Health")]
    public int maxHP = 100;
    public int attackDamage = 10;

    [SerializeField]
    private int currentHP;
    public int CurrentHP => currentHP;
    public int maxMana = 100;

    [SerializeField]
    private int currentMana = 0;
    public int CurrentMana => currentMana;

    // Property để set currentHP và currentMana từ GameManager khi apply stats
    public int SetCurrentHP { set => currentHP = value; }
    public int SetCurrentMana { set => currentMana = value; }

    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.2f;
    public float lastDashTime = -999f;

   

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
    public DashState DashState;
    public DefendState DefendState { get; private set; }

    public DeadState DeadState { get; private set; }

    public AttackData lightAttackData;
    public AttackData guardBreakAttackData;

    public bool IsLocked { get; set; } = false;

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

        DeadState = new DeadState(this);

        // Existing states
        IdleState = new IdleState(this);
        LightAttackState = new AttackState(this, lightAttackData);
        RunState = new RunState(this);
        JumpState = new JumpState(this);
        DefendState = new DefendState(this);

        currentHP = maxHP;
    }

    private void Start()
    {
        StateMachine.ChangeState(IdleState);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(playerID, this);
        }
    }

    private void OnEnable()
    {
        // Subscribe to damage events to react appropriately
        GameEvents.OnTakeDamage += HandleTakeDamage;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        GameEvents.OnTakeDamage -= HandleTakeDamage;
    }

    private void Update()
    {
        StateMachine.Update();
        
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

    public void ModifyMaxHP(int amount)
    {
        maxHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP); // Đảm bảo currentHP không vượt maxHP mới
        GameEvents.RaiseHealthChanged(playerID, currentHP);
    }

    public void ModifyCurrentMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
    }

    public void ModifyAttackDamage(int amount)
    {
        attackDamage += amount;
    }

    public void ResetHealth()
    {
        currentHP = maxHP;
        GameEvents.RaiseHealthChanged(playerID, currentHP);
    }
    public void TakeDamage(AttackData data, Vector2 direction)
    {
        int finalDamage = data.damage;

        if (StateMachine.CurrentState is DefendState)
        {
            if (data.isGuardBreak) // Check flag trong AttackData
            {
                Debug.Log("Bị phá thủ!");
                // Ép chuyển sang HurtState và nhận full sát thương
                StateMachine.ChangeState(new HurtState(this, data.hitstunFrames * 2, direction * data.knockbackForce));
                currentHP -= finalDamage;
                return;
            }

            finalDamage = Mathf.RoundToInt(data.damage * 0.3f);
        }

        currentHP -= finalDamage;

        GameEvents.RaiseHealthChanged(playerID, currentHP);

        //if (currentHP <= 0)
        //{
        //    currentHP = 0;
        //    GameEvents.RaisePlayerDied(playerID);
        //    return;
        //}

        if (!(StateMachine.CurrentState is DefendState))
        {
            Vector2 knockback = direction * data.knockbackForce;
            StateMachine.ChangeState(new HurtState(this, data.hitstunFrames, knockback));
        }

        if (currentHP <= 0)
        {
            currentHP = 0;
            StateMachine.ChangeState(DeadState);
            GameEvents.RaisePlayerDied(playerID);
            return;
        }
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

    // ==========================================
    // TRAP & COLLISION HANDLING
    // ==========================================

    /// <summary>
    /// Handle when player enters a trap trigger
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            // Trap will handle damage through event system
            // Player just needs to know a trap was hit
            Debug.Log($"Player {playerID} touched a trap!");
        }
    }

    /// <summary>
    /// Handle when player stays in a trap trigger
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        // Trap handles repeated damage through OnTriggerStay2D
        // No additional logic needed here
    }

    /// <summary>
    /// Handles damage events from traps or other sources
    /// Changes state to HurtState when damaged
    /// </summary>
    private void HandleTakeDamage(int playerID, int damageAmount)
    {
        // Only process if this damage is for this player
        if (playerID != this.playerID)
            return;

        // Apply damage
        currentHP -= damageAmount;

        // Clamp to 0
        if (currentHP < 0)
            currentHP = 0;

        // Update UI
        GameEvents.RaiseHealthChanged(playerID, currentHP);

        // Change to HurtState if not already in it
        if (!(StateMachine.CurrentState is HurtState))
        {
            // Use a default knockback direction (away from trap)
            Vector2 knockbackDirection = (transform.position - new Vector3(0, 0)).normalized;
            StateMachine.ChangeState(new HurtState(this, 15, knockbackDirection * 5f));
        }

        // Check if player died
        if (currentHP <= 0)
        {
            Debug.Log($"Player {playerID} died!");
            GameEvents.RaisePlayerDied(playerID);
        }

        Debug.Log($"Player {playerID} took {damageAmount} damage. HP: {currentHP}/{maxHP}");
    }

    public void DisableAllActions()
    {
        // Logic to disable all player actions
        Input.DisableInput();
        Movement.StopMovement();
    }

    public void EnableAllActions()
    {
        // Logic to enable all player actions
        Input.EnableInput();
        Movement.ResumeMovement();
    }
}