using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;

    public enum PlayerType { Player1, Player2 }
    [Header("Select Player")]
    public PlayerType playerType;

    public float MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CloseAttackPressed { get; private set; }
    public bool RangeAttackPressed { get; private set; }
    public bool UltimatePressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool DefendPressed { get; private set; }
    public bool SupportPressed { get; private set; }
    public bool Special1Pressed { get; private set; }
    public bool Special2Pressed { get; private set; }

    private void Awake() => controls = new PlayerControls();

    private void OnEnable()
    {
        BindControls();
    }

    private void BindControls()
    {
        if (playerType == PlayerType.Player1)
        {
            var map = controls.Fighting;
            map.Enable();

            map.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>().x;
            map.Move.canceled += ctx => MoveInput = 0f;
            map.Jump.started += ctx => JumpPressed = true;
            map.CloseAttack.started += ctx => CloseAttackPressed = true;
            map.RangeAttack.started += ctx => RangeAttackPressed = true;
            map.Ultimate.started += ctx => UltimatePressed = true;
            map.Dash.started += ctx => DashPressed = true;
            map.Support.started += ctx => SupportPressed = true;
            map.Defend.performed += ctx => DefendPressed = true;
            map.Defend.canceled += ctx => DefendPressed = false;
            map.Special1.started += ctx => Special1Pressed = true;
            map.Special2.started += ctx => Special2Pressed = true;
        }
        else
        {
            var map = controls.Player2;
            map.Enable();

            map.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>().x;
            map.Move.canceled += ctx => MoveInput = 0f;
            map.Jump.started += ctx => JumpPressed = true;
            map.CloseAttack.started += ctx => CloseAttackPressed = true;
            map.RangeAttack.started += ctx => RangeAttackPressed = true;
            map.Ultimate.started += ctx => UltimatePressed = true;
            map.Dash.started += ctx => DashPressed = true;
            map.Defend.performed += ctx => DefendPressed = true;
            map.Defend.canceled += ctx => DefendPressed = false;
            map.Special1.started += ctx => Special1Pressed = true;
            map.Special2.started += ctx => Special2Pressed = true;
        }
    }

    /// <summary>
    /// Khởi tạo PlayerInputHandler với playerType chỉ định
    /// Phương thức này phải được gọi TRƯỚC khi SetActive(true)
    /// </summary>
    public void Initialize(PlayerType type)
    {
        playerType = type;
        BindControls();
    }

    private void LateUpdate()
    {
        JumpPressed = false;
        CloseAttackPressed = false;
        RangeAttackPressed = false;
        UltimatePressed = false;
        DashPressed = false;
        SupportPressed = false;
        Special1Pressed = false;
        Special2Pressed = false;
    }

    private void OnDisable()
    {
        if (playerType == PlayerType.Player1) controls.Fighting.Disable();
        else controls.Player2.Disable();
    }

    public void DisableInput()
    {
        // Logic to disable all input
        MoveInput = 0;
        JumpPressed = false;
        CloseAttackPressed = false;
        RangeAttackPressed = false;
        UltimatePressed = false;
        DashPressed = false;
        DefendPressed = false;
        SupportPressed = false;
        Special1Pressed = false;
        Special2Pressed = false;
    }

    public void EnableInput()
    {
        // Logic to enable all input
        var map = controls.Fighting;
        map.Enable();
    }
}