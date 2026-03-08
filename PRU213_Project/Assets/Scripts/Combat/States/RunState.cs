using UnityEngine;

public class RunState : PlayerState
{
    public RunState(Player player) : base(player) { }

    private float noMoveTimer = 0f;
    private const float NO_MOVE_THRESHOLD = 0.12f;

    public override void Enter()
    {
        base.Enter();
        noMoveTimer = 0f;
        player.Animator.Play("Run");
    }

    public override void Update()
    {
        if (player.Input == null) return;

        float mv = player.Input.MoveInput;

        if (Mathf.Abs(mv) > 0.01f)
        {
            player.Movement.Move(mv);
            noMoveTimer = 0f;
        }
        else
        {
            player.Movement.Stop();
            noMoveTimer += Time.deltaTime;
            if (noMoveTimer >= NO_MOVE_THRESHOLD)
            {
                player.StateMachine.ChangeState(player.IdleState);
            }
        }

        if (player.Input.JumpPressed && player.IsGrounded())
        {
            player.StateMachine.ChangeState(player.JumpState);
        }
        //if (player.Input.CloseAttackPressed)
        //{
        //    player.PerformAttack(player.lightAttackData);
        //}
    }
}