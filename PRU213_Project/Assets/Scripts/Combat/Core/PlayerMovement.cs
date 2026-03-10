using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D RB { get; private set; }
    public Player Player { get; private set; }
    public PlayerInputHandler Input => Player.Input;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 16f;

    [Header("Double Jump Settings")]
    public int maxJumpCount = 2;
    private int remainingJumps;

    [Header("Coyote Time Settings")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer Settings")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    private bool wasGrounded;

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Player = GetComponent<Player>();
    }

    private void Update()
    {
        if (Player.IsLocked) return;
        HandleFlip();
        HandleCoyoteTime();
        HandleJumpBuffer();
    }

    // --- Coyote Time ---
    private void HandleCoyoteTime()
    {
        bool isGrounded = Player.IsGrounded();

        if (wasGrounded && !isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime; 
            remainingJumps = maxJumpCount;  
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        wasGrounded = isGrounded;
    }

    private void HandleJumpBuffer()
    {
        if (Input == null) return;

        if (Input.JumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && CanJump())
        {
            ExecuteJump();
            jumpBufferCounter = 0f;
        }
    }

    private bool CanJump()
    {
        if (coyoteTimeCounter > 0f) return true;
        if (remainingJumps > 0) return true;
        return false;
    }

    private void ExecuteJump()
    {
        bool isCoyoteJump = coyoteTimeCounter > 0f && !Player.IsGrounded();

        ApplyJumpForce();

        if (isCoyoteJump)
        {
            remainingJumps = maxJumpCount - 1;
        }
        else
        {
            remainingJumps--;
        }

        coyoteTimeCounter = 0f; 
    }

    // --- Movement ---
    public void Move(float input)
    {
        RB.velocity = new Vector2(input * moveSpeed, RB.velocity.y);
    }

    public void Stop()
    {
        RB.velocity = new Vector2(0, RB.velocity.y);
    }

    public void ApplyJumpForce()
    {
        RB.velocity = new Vector2(RB.velocity.x, jumpForce);
    }

    private void HandleFlip()
    {
        if (Input == null) return;
        if (Input.MoveInput > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (Input.MoveInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnDrawGizmos()
    {
        if (Player != null && Player.groundCheck != null)
        {
            Gizmos.color = Player.IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(Player.groundCheck.position, Player.groundCheckRadius);
        }
    }
}