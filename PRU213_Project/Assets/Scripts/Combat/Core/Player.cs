using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerID;
    public int maxHP = 100;
    public int attackDamage = 10;   
    private int currentHP;

    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public Hitbox Hitbox { get; private set; }
    public PlayerMovement Movement { get; private set; } 

    public PlayerStateMachine StateMachine { get; private set; }

    public PlayerState IdleState;
    public AttackState LightAttackState;

    public PlayerInputHandler Input { get; private set; }
    public RunState RunState;
    public JumpState JumpState;

    public AttackData lightAttackData;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Hitbox = GetComponentInChildren<Hitbox>();
        Movement = GetComponent<PlayerMovement>(); 

        Hitbox.owner = this;
        StateMachine = new PlayerStateMachine();
        Input = GetComponent<PlayerInputHandler>();

        IdleState = new IdleState(this);
        LightAttackState = new AttackState(this, lightAttackData);
        RunState = new RunState(this);
        JumpState = new JumpState(this);

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

    private void Update()
    {
        StateMachine.Update();
    }

    public void PerformAttack(AttackData data)
    {
        // Không cho phép tấn công khi đang bị choáng (HurtState) hoặc đang tấn công
        if (StateMachine.CurrentState is AttackState || StateMachine.CurrentState is HurtState)
            return;

        StateMachine.ChangeState(LightAttackState);
    }

    public void TakeDamage(AttackData data, Vector2 direction)
    {
        Debug.Log($"<color=orange>[Player]</color> {name} đang xử lý TakeDamage. Máu hiện tại: {currentHP}");
        currentHP -= data.damage;
        GameEvents.RaiseHealthChanged(playerID, currentHP); // Cập nhật lên UI

        if (currentHP <= 0)
        {
            currentHP = 0;
            GameEvents.RaisePlayerDied(playerID);
        }

        Vector2 knockback = direction * data.knockbackForce;
        // Chuyển sang trạng thái bị đòn
        StateMachine.ChangeState(new HurtState(this, data.hitstunFrames, knockback));
    }
    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}