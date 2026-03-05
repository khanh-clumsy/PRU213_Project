using UnityEngine;

public class AttackState : PlayerState
{
    private AttackData data;
    private int frameCounter;
    private float frameTimer;
    private const float FRAME_TIME = 1f / 60f;

    public AttackState(Player player, AttackData data) : base(player)
    {
        this.data = data;
    }

    public override void Enter()
    {
        frameCounter = 0;
        frameTimer = 0f;
        player.Animator.Play(data.attackName);
        player.Movement.Stop(); 
    }

    public override void Update()
    {
        frameTimer += Time.deltaTime;
        while (frameTimer >= FRAME_TIME)
        {
            frameTimer -= FRAME_TIME;
            ProcessFrame();
        }
    }

    private void ProcessFrame()
    {
        frameCounter++;
        Debug.Log($"<color=cyan>[AttackState]</color> {player.name} đang thực hiện {data.attackName}. Frame: {frameCounter}/{data.startupFrames + data.activeFrames + data.recoveryFrames}");

        // Kích hoạt Hitbox ở Active Frames
        if (frameCounter == data.startupFrames)
            player.Hitbox.EnableHitbox(data);

        // Tắt Hitbox sau khi hết Active Frames
        if (frameCounter == data.startupFrames + data.activeFrames)
            player.Hitbox.DisableHitbox();

        // Kết thúc trạng thái sau khi hết Recovery Frames
        if (frameCounter >= data.startupFrames + data.activeFrames + data.recoveryFrames)
            player.StateMachine.ChangeState(player.IdleState);
    }
}