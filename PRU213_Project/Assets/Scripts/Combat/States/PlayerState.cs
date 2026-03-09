using UnityEngine;

public abstract class PlayerState
{
    protected Player player;

    public PlayerState(Player player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    protected void HandleGlobalInput()
    {
        //// Ưu tiên các đòn đánh đặc biệt (Modifier W)
        //if (player.Input.TeleportAttackPressed)
        //{
        //    // teleportAttackData nên được gán trong Player hoặc CombatController
        //    stateMachine.ChangeState(new AttackState(player, player.teleportAttackData));
        //    return;
        //}

        if (player.Input.Special1Pressed)
        {
            player.StateMachine.ChangeState(new AttackState(player, player.guardBreakAttackData));
            return;
        }

        //Kiểm tra Dash
        if (player.Input.DashPressed && Time.time >= player.lastDashTime + player.dashCooldown)
        {
            player.lastDashTime = Time.time;
            player.StateMachine.ChangeState(new DashState(player));
            return;
        }

        //// Kiểm tra Defend
        if (player.Input.DefendPressed && player.IsGrounded())
        {
            Debug.Log("Defend button pressed and player is grounded. Changing to DefendState.");
            player.StateMachine.ChangeState(player.DefendState);
        }
    }

}