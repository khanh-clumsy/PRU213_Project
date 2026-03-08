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
            player.StateMachine.ChangeState(player.JumpState);
        }
        if (player.Input.CloseAttackPressed)
        {
            player.PerformAttack(player.lightAttackData);
        }
        if (player.Input.RangeAttackPressed)
        {
            player.PerformStrongAttack();
            return;
        }
    }
}