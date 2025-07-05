using UnityEngine;
using UnityEngine.InputSystem;

public class State_PlayerMoveInput : MonoState
{
    [SerializeField] private InputActionAsset _inputAsset;
    [SerializeField] private bool _normalizeInput;
    protected InputAction movementInputAction { get; set; }
    private DS_MovingActor _movingActor;
    private Camera _camera;

    protected override void OnEnter()
    {
        base.OnEnter();
        _movingActor = Owner.GetData<DS_MovingActor>();
        _camera = Camera.main;
        
        SetupPlayerInput();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!_movingActor.BlockMoveInput)
        {
            Vector2 readValue = movementInputAction.ReadValue<Vector2>();
            if (_normalizeInput) readValue = readValue.normalized;
            _movingActor.MoveInput = readValue;

        }
        
        _movingActor.LookDirection = _camera.transform.forward;
    }
    protected virtual void SetupPlayerInput()
    {
        if (_inputAsset == null)
            return;
        movementInputAction = _inputAsset.FindAction("Movement");
        movementInputAction.Enable();
    }
}