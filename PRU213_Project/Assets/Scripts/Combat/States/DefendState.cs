using UnityEngine;

public class DefendState : PlayerState
{
    public DefendState(Player player) : base(player) { }

    public override void Enter()
    {
        player.Rigidbody.velocity = Vector2.zero; 
        player.Animator.Play("Defend"); 
        player.Animator.SetBool("IsDefending", true);
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
        player.Animator.SetBool("IsDefending", false);
    }
}