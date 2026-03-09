using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    private Player player;
    private PlayerInputHandler input;

    [Header("Light Combo")]
    public List<AttackData> lightCombo;

    [Header("Other Attacks")]
    public AttackData rangeAttack;
    public AttackData special1;
    public AttackData special2;
    public AttackData ultimate;

    private Queue<AttackData> inputBuffer = new Queue<AttackData>();
    private const int BUFFER_SIZE = 3;

    private void Awake()
    {
        player = GetComponent<Player>();
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        ReadInput();
        TryExecuteAttack();
    }

    void ReadInput()
    {
        if (input.Special1Pressed)
        {
            Debug.Log("Buffering Special 1 Attack");
            BufferAttack(special1);
        }

        else if (input.Special2Pressed)
        {
            Debug.Log("Buffering Special 2 Attack");
            BufferAttack(special2);
        }
        else if (input.CloseAttackPressed)
        {
            BufferAttack(lightCombo[0]);
        }

        else if (input.RangeAttackPressed)
        {
            BufferAttack(rangeAttack);
        }


        else if (input.UltimatePressed )
        {
            Debug.Log("Buffering Ultimate Attack");
            BufferAttack(ultimate);
        }
    }

    void BufferAttack(AttackData attack)
    {
        if (attack == null) return;

        if (inputBuffer.Count >= BUFFER_SIZE)
            return;

        inputBuffer.Enqueue(attack);
    }

    void TryExecuteAttack()
    {
        if (player.StateMachine.CurrentState is AttackState ||
            player.StateMachine.CurrentState is HurtState ||
            player.StateMachine.CurrentState is DefendState)
            return;

        if (inputBuffer.Count == 0)
            return;

        AttackData attack = inputBuffer.Peek(); 

        
        if (attack.type == AttackType.Ultimate && player.CurrentMana < 100)
        {
            inputBuffer.Clear(); 
            return;
        }

        attack = inputBuffer.Dequeue(); 

        if (attack.type == AttackType.Ultimate)
            player.StateMachine.ChangeState(new UltimateState(player, attack));
        else
            player.StateMachine.ChangeState(new AttackState(player, attack));
    }
 


    public AttackData GetBufferedAttack()
    {
        if (inputBuffer.Count == 0)
            return null;

        return inputBuffer.Dequeue();
    }

    public void ClearBuffer()
    {
        inputBuffer.Clear();
    }
}