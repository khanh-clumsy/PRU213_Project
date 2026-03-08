using UnityEngine;

public class DashState : PlayerState
{
    private float timer;
    private Vector2 dashDirection;

    public DashState(Player player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Enter Dash mode");
        timer = player.dashDuration;

        float facingDir = player.transform.localScale.x > 0 ? 1f : -1f;
        Vector2 dashDirection = new Vector2(facingDir, 0);

        player.Rigidbody.velocity = Vector2.zero;
        player.Rigidbody.velocity = dashDirection * player.dashForce;

        player.Animator.Play("Dash");
        player.Rigidbody.gravityScale = 0;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        player.Rigidbody.gravityScale = 3f; 
        player.Rigidbody.velocity = Vector2.zero;
    }
}