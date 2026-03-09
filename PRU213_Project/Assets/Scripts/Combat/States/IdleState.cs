using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerMovement playerMovement, Player player) : base(player) { }

    // Backwards-compatible constructor for Player-only usage
    public IdleState(Player player) : base(player) { }

    public override void Enter()
    {
        player.Animator.Play("Idle");
    }
    public override void Update()
    {
        if (player.Input == null) return;

        if (Mathf.Abs(player.Input.MoveInput) > 0.01f)
        {
            player.StateMachine.ChangeState(player.RunState);
        }

        else if (player.Input.JumpPressed && player.IsGrounded())
        {
            Debug.Log("Jump input detected in IdleState");
            player.StateMachine.ChangeState(player.JumpState);
        }
        
    }
}