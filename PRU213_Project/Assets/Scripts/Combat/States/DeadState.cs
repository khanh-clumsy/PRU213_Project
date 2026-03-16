using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Combat;

namespace Assets.Scripts.Combat.States
{
    public class DeadState : PlayerState
    {
        public DeadState(Player player) : base(player)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // Logic for entering DeadState
            player.Animator.Play("Dead");
            player.DisableAllActions();
        }

        public override void Update()
        {
            base.Update();
            // DeadState logic (e.g., prevent any input handling)
        }

        public override void Exit()
        {
            base.Exit();
            // Logic for exiting DeadState (if applicable)
        }
    }
}
