using UnityEngine;

public class AttackState : PlayerState
{
    private AttackData data;
    private int frameCounter;
    private float frameTimer;

    private const float FRAME_TIME = 1f / 60f;

    private int comboIndex;

    private CombatController combat;

    public AttackState(Player player, AttackData data, int index = 0) : base(player)
    {
        this.data = data;
        this.comboIndex = index;
        combat = player.GetComponent<CombatController>();
    }

    public override void Enter()
    {
        frameCounter = 0;
        frameTimer = 0f;

        player.Animator.speed = data.animationSpeed;
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

        // reset hitbox khi bắt đầu active
        if (frameCounter == data.startupFrames)
        {
            player.Hitbox.ResetHitbox();
        }

        // active frame → check hit
        if (frameCounter >= data.startupFrames &&
            frameCounter < data.startupFrames + data.activeFrames)
        {
            player.Hitbox.CheckHit(data);
        }

        // kết thúc recovery
        if (frameCounter >= data.startupFrames + data.activeFrames + data.recoveryFrames)
        {
            AttackData nextAttack = combat.GetBufferedAttack();

            if (nextAttack != null && comboIndex < combat.lightCombo.Count - 1)
            {
                int nextIndex = comboIndex + 1;

                player.StateMachine.ChangeState(
                    new AttackState(
                        player,
                        combat.lightCombo[nextIndex],
                        nextIndex
                    ));
            }
            else
            {
                combat.ClearBuffer();
                player.StateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void Exit()
    {
        player.Animator.speed = 1f;
    }
}