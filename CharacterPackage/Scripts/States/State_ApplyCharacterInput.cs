using System.Collections;
using System.Collections.Generic;
using ECM2;
using UnityEngine;
using UnityEngine.Serialization;

public class State_ApplyCharacterInput : MonoState
{
    private Character _character;

    public bool BlockInput;

    [SerializeField][Tooltip("Uncheck if not player")] private bool _isCameraRelative = true;
    [SerializeField] private bool _orientToLookDirection = true;

    [SerializeField] private float _orientationSpeed = 10f;

    private DS_MovingActor _movingActor;

    private Camera _mainCam;
    
    protected override void OnEnter()
    {
        base.OnEnter();
        _movingActor = Owner.GetData<DS_MovingActor>();
        _character = Owner.GetData<Data_Character>().Character;
        
        _mainCam = Camera.main;
        StaticUpdater.onUpdate += OnStaticUpdate;
    }

    protected override void OnExit()
    {
        base.OnExit();
        _character.SetMovementDirection(Vector3.zero);
        StaticUpdater.onUpdate -= OnStaticUpdate;
    }

    private void OnStaticUpdate()
    {
        Vector2 inputMove = GetMovementInput();
        
        Vector3 movementDirection = Vector3.zero;
        if (_isCameraRelative)
        {
            Vector3 cameraForward = _mainCam.transform.forward;
            Vector3 cameraRight = _mainCam.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction relative to the camera's orientation
            movementDirection = cameraForward * inputMove.y + cameraRight * inputMove.x;
        }
        else
        {
            movementDirection = Vector3.zero;
            movementDirection += Vector3.right * inputMove.x;
            movementDirection += Vector3.forward * inputMove.y;
        }

        if (_orientToLookDirection)
        {
            _character.RotateTowards(_movingActor.LookDirection, Time.fixedDeltaTime * _orientationSpeed);
        }

        // Obtain the camera's forward and right vectors, but ignore the y component to keep movement horizontal.
        
        _character.SetMovementDirection(movementDirection);
    }

    public virtual Vector2 GetMovementInput()
    {
        if (BlockInput) return Vector2.zero;

        return _movingActor.MoveInput;
    }
}