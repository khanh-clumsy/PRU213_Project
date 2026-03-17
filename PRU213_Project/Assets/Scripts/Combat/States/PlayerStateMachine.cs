using Assets.Scripts.Combat.States;

public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void ChangeState(PlayerState newState)
    {
        // Guard: Không cho phép thoát khỏi DeadState
        if (CurrentState is DeadState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}