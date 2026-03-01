using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControls controls;

    // Các biến này phải TRÙNG TÊN với biến mà PlayerMovement gọi
    public float MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CloseAttackPressed { get; private set; }
    public bool RangeAttackPressed { get; private set; }
    public bool UltimatePressed { get; private set; }
    public bool DashPressed { get; private set; }
    public bool DefendPressed { get; private set; }
    public bool SupportPressed { get; private set; }

    private void Awake() => controls = new PlayerControls();

    private void OnEnable()
    {
        controls.Fighting.Enable();
        controls.Fighting.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>().x;
        controls.Fighting.Move.canceled += ctx => MoveInput = 0f;

        controls.Fighting.Jump.started += ctx => JumpPressed = true;
        controls.Fighting.CloseAttack.started += ctx => CloseAttackPressed = true;
        controls.Fighting.RangeAttack.started += ctx => RangeAttackPressed = true;
        controls.Fighting.Ultimate.started += ctx => UltimatePressed = true;
        controls.Fighting.Dash.started += ctx => DashPressed = true;
        controls.Fighting.Support.started += ctx => SupportPressed = true;

        controls.Fighting.Defend.performed += ctx => DefendPressed = true;
        controls.Fighting.Defend.canceled += ctx => DefendPressed = false;
    }

    private void LateUpdate()
    {
        JumpPressed = false;
        CloseAttackPressed = false;
        RangeAttackPressed = false;
        UltimatePressed = false;
        DashPressed = false;
        SupportPressed = false;
    }

    private void OnDisable() => controls.Fighting.Disable();
}