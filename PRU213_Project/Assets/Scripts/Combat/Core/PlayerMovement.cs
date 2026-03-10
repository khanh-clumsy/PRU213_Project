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

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Player = GetComponent<Player>();
    }
    public void ModifyMoveSpeed(int amount)
    {
        moveSpeed += amount;
    }
    private void Update()
    {
        HandleFlip();
    }

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