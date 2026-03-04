using UnityEngine;

public class JumpState : PlayerState
{
    public JumpState(Player player) : base(player) { }

    private bool hasLeftGround = false;
    private float landingTimer = 0f;
    private const float LANDING_STABLE_TIME = 0.05f;

    public override void Enter()
    {
        player.Animator.Play("Jump");
        player.Movement.ApplyJumpForce(); // Thực hiện nhảy vật lý
        hasLeftGround = false;
        landingTimer = 0f;
    }

    public override void Update()
    {
        // Cho phép di chuyển trên không
        player.Movement.Move(player.Input.MoveInput);

        if (!hasLeftGround)
        {
            // Chỉ bắt đầu đợi tiếp đất sau khi đã thực sự rời khỏi mặt đất
            if (!player.IsGrounded())
                hasLeftGround = true;
            return;
        }

        if (player.IsGrounded())
        {
            landingTimer += Time.deltaTime;
            if (landingTimer >= LANDING_STABLE_TIME)
            {
                player.StateMachine.ChangeState(player.IdleState);
            }
        }
        else
        {
            landingTimer = 0f;
        }
    }
}