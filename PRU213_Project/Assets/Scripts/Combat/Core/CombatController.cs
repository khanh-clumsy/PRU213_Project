using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Combat.States;

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

    private float chargeTime = 0f;
    private bool isChargedAttackReady = false;

    private void Awake()
    {
        player = GetComponent<Player>();
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        ReadInput();
        HandleCharging();
        TryExecuteAttack();
    }

    private void HandleCharging()
    {
        // Bỏ qua nếu nhân vật đang bị khoá hoặc đã chết
        if (player.IsLocked || player.StateMachine.CurrentState is DeadState)
        {
            chargeTime = 0f;
            isChargedAttackReady = false;
            return;
        }

        if (input.isCloseAttackHeld)
        {
            chargeTime += Time.deltaTime;
            
            // Nếu giữ đủ 2 giây và chưa bị kích hoạt
            if (chargeTime >= 2.0f && !isChargedAttackReady)
            {
                isChargedAttackReady = true;
                ExecuteChargedAttack();
            }
        }
        else
        {
            // Reset khi nhả nút
            chargeTime = 0f;
            isChargedAttackReady = false;
        }
    }

    private void ExecuteChargedAttack()
    {
        if (player.guardBreakAttackData == null)
        {
            Debug.LogWarning($"<color=orange>[Charged Attack]</color> Player {player.playerID} thiếu guardBreakAttackData (Strong Attack)!");
            return;
        }

        // Tăng gấp đôi sát thương & set GuardBreak bằng cách clone dữ liệu tạm thời
        AttackData chargedData = ScriptableObject.Instantiate(player.guardBreakAttackData);
        chargedData.damage *= 2;
        chargedData.isGuardBreak = true;

        Debug.Log($"<color=red>[Charged Attack]</color> Player {player.playerID} tung đòn Charged Attack! x2 Damage & Guard Break!");

        // Ép xoá buffer và chuyển ngay sang trạng thái đánh tụ lực
        inputBuffer.Clear();
        player.StateMachine.ChangeState(new AttackState(player, chargedData));
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