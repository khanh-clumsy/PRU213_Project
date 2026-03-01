using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerInputHandler input;
    public PlayerStateMachine stateMachine;
    public Transform groundCheck;
    private Rigidbody2D rb;

    [Header("Settings")]
    public float moveSpeed = 10f;
    public float jumpForce = 16f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private bool canMove = false;
    private bool facingRight = true;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void OnEnable()
    {
        GameEvents.OnMatchStarted += () => canMove = true;
        GameEvents.OnMatchEnded += (id) => { canMove = false; rb.velocity = Vector2.zero; };
    }

    void Update()
    {
        if (!canMove) return;


    }
    void CheckGround() => isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

    void HandleFlip()
    {
        if (input.MoveInput > 0 && !facingRight) Flip();
        else if (input.MoveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    void HandleState()
    {
        if (!isGrounded)
            stateMachine.ChangeState(rb.velocity.y > 0 ? PlayerState.Jump : PlayerState.Fall);
        else
            stateMachine.ChangeState(Mathf.Abs(input.MoveInput) > 0.1f ? PlayerState.Run : PlayerState.Idle);
    }
}
