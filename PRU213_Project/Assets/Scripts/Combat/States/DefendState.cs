using UnityEngine;

public class DefendState : PlayerState
{
    public DefendState(Player player) : base(player) { }

    public override void Enter()
    {
        player.Rigidbody.velocity = Vector2.zero; 
        player.Animator.Play("Defend"); 
    }

    public override void Update()
    {
        if (!player.Input.DefendPressed)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
    }
}