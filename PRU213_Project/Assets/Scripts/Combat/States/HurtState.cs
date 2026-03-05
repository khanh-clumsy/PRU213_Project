using UnityEngine;

public class HurtState : PlayerState
{
    private int stunFrames;
    private int counter;
    private Vector2 knockback;
    private float frameTimer;
    private const float FRAME_TIME = 1f / 60f;

    public HurtState(Player player, int stunFrames, Vector2 knockback) : base(player)
    {
        this.stunFrames = stunFrames;
        this.knockback = knockback;
    }

    public override void Enter()
    {
        Debug.Log($"<color=blue>[HurtState]</color> {player.name} đã vào HurtState. Frames choáng: {stunFrames}");
        counter = 0;
        frameTimer = 0f;
        player.Rigidbody.velocity = knockback;
        player.Animator.Play("Hurt");
    }

    public override void Update()
    {
        frameTimer += Time.deltaTime;
        while (frameTimer >= FRAME_TIME)
        {
            frameTimer -= FRAME_TIME;
            counter++;

            if (counter >= stunFrames)
            {
                player.StateMachine.ChangeState(player.IdleState);
            }
        }
    }
}