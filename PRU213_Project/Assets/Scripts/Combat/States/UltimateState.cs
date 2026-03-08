using UnityEngine;

public class UltimateState : AttackState
{
    public UltimateState(Player player, AttackData data) : base(player, data) { }

    public override void Enter()
    {
        
        if (player.CurrentMana < 100) 
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        player.UseMana(100);
        base.Enter(); 

        Time.timeScale = 0.05f;
        Debug.Log("<color=red>!!! ULTIMATE CUTSCENE !!!</color>");
        player.StartCoroutine(RestoreTimeAfterDelay(0.5f));
    }

    private System.Collections.IEnumerator RestoreTimeAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
    }
}