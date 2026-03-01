using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState CurrentSate { get; private set; }

    public void ChangeState(PlayerState newState)
    {
        if (CurrentSate == newState) return;
        CurrentSate = newState;
    }
}
