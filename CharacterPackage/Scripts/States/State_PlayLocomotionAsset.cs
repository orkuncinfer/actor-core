using System.Collections;
using System.Collections.Generic;
using Animancer;
using ECM2;
using ECM2.Examples.Networking.FishNet;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class State_PlayLocomotionAsset : MonoState
{
    private enum AnimationState
    {
        Idle,
        Locomotion
    }

    [SerializeField] private DSGetter<Data_RefVar> _locomotionAsset;
    [SerializeField] private AvatarMask _avatarMask;
    [SerializeField] private int _layer;
    [SerializeField] private float _interpolationSpeed = 5f;
    [SerializeField] [ReadOnly] private Vector3 _velocityDebug;
    [SerializeField] [ReadOnly] private Vector2 _parameterDebug;
    [SerializeField] private float _velocityThreshold = 2f;

    private DS_MovingActor _movingActor;

    private RPG_PlayerController _networkRpgPlayerController;
    
    private CharacterMovement _character;
    private Rigidbody _rigidBody;
    public Vector2 MoveInput;
    private Data_Animancer _dataAnimancer;
    private LocomotionAsset _locomotionAssetData;
    private LinearMixerTransitionAsset _asset;
    private MixerTransition2DAsset _asset2D;
    private Camera _mainCam;

    // State management variables
    private AnimationState _currentState;
    private AnimationState _previousState;
    
    private Dictionary<object,AnimancerState> _cachedTransitions = new Dictionary<object, AnimancerState>();

    protected override void OnEnter()
    {
        base.OnEnter();

#if USING_FISHNET
        _networkRpgPlayerController = Owner.GetComponent<RPG_PlayerController>();
#endif
        
        _movingActor = Owner.GetData<DS_MovingActor>();
        _character = Owner.GetComponent<CharacterMovement>();
        _rigidBody= Owner.GetComponent<Rigidbody>();
        _dataAnimancer = Owner.GetData<Data_Animancer>();
        _locomotionAsset.GetData(Owner); 
        _dataAnimancer.AnimancerComponent.Layers[_layer].ApplyFootIK = true;
        _dataAnimancer.AnimancerComponent.Layers[_layer].ApplyAnimatorIK = true;
        
        _mainCam = Camera.main;
        _locomotionAssetData = _locomotionAsset.Data.Value as LocomotionAsset;
        _locomotionAsset.Data.onValueChanged += OnLocomotionAssetChanged;
        if(_locomotionAssetData == null) return;
        // Start with Idle state
        PlayIdle();
        ChangeState(AnimationState.Idle);
    }
    


    private void OnLocomotionAssetChanged(Object arg1, Object arg2)
    {
        _locomotionAssetData = _locomotionAsset.Data.Value as LocomotionAsset;
        if (_locomotionAssetData == null)
        {
            _dataAnimancer.AnimancerComponent.Layers[_layer].StartFade(0);
            return;
        }
        OnEnterState(_currentState);
    }

    void PlayLocomotionAsset()
    {
        if (_locomotionAssetData.Locomotion is LinearMixerTransitionAsset linear)
        {
            if(_cachedTransitions.ContainsKey(linear.GetTransition().Key))
            {
                _asset = linear;
                var tr = _cachedTransitions[linear.GetTransition().Key];
                if(_avatarMask) _dataAnimancer.AnimancerComponent.Layers[_layer].SetMask(_avatarMask);
                var state = _dataAnimancer.AnimancerComponent.Layers[_layer].Play(tr,_asset.FadeDuration,_asset.FadeMode);
                state.ApplyFootIK = true;
            }
            else
            {
                _asset = linear;
                var tr = linear.Transition.CreateState();
                if(_avatarMask) _dataAnimancer.AnimancerComponent.Layers[_layer].SetMask(_avatarMask);
                var state = _dataAnimancer.AnimancerComponent.Layers[_layer].Play(tr,_asset.FadeDuration,_asset.FadeMode);
                state.ApplyFootIK = true;
                _cachedTransitions.Add(linear.GetTransition().Key,tr);
            }
        }
        else if (_locomotionAssetData.Locomotion is MixerTransition2DAsset mixer)
        {
            if(_cachedTransitions.ContainsKey(mixer.GetTransition().Key))
            {
                _asset2D = mixer;
                var tr = _cachedTransitions[mixer.GetTransition().Key];
                if(_avatarMask) _dataAnimancer.AnimancerComponent.Layers[_layer].SetMask(_avatarMask);
                var state = _dataAnimancer.AnimancerComponent.Layers[_layer].Play(tr,_asset2D.FadeDuration,_asset2D.FadeMode);
                state.ApplyFootIK = true;
            }
            else
            {
                _asset2D = mixer;
                var tr = mixer.Transition.CreateState();
                if(_avatarMask) _dataAnimancer.AnimancerComponent.Layers[_layer].SetMask(_avatarMask);
                var state = _dataAnimancer.AnimancerComponent.Layers[_layer].Play(tr,_asset2D.FadeDuration,_asset2D.FadeMode);
                state.ApplyFootIK = true;
                _cachedTransitions.Add(mixer.GetTransition().Key,tr);
            }
       
            //Debug.Log($"played locomotion asset: {_asset2D.name} on layer {_layer}");
        }
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (_locomotionAssetData == null)
        {
            return;
        }

        // Get the world-space velocity
        Vector3 worldVelocity = Vector3.zero;

        if(_character != null)
            worldVelocity = _character.velocity;
        else
        {
            worldVelocity = _rigidBody.linearVelocity;
        }
        
        if (_networkRpgPlayerController)
        {
            //worldVelocity = _networkRpgPlayerController.LastSentVelocity;
        }
        
        // Convert world velocity to local velocity
        Vector3 localVelocity = Owner.transform.InverseTransformDirection(worldVelocity);

        // Convert local velocity to a 2D vector (X for sideways, Z for forward)
        Vector2 facingVelocity = new Vector2(localVelocity.x, localVelocity.z);

        _velocityDebug = facingVelocity; // Debugging visualization

        // Normalize movement input
        Vector2 inputMove = GetMovementInput();
        MoveInput = inputMove.normalized;

        float velocityMagnitude = worldVelocity.magnitude;

        // State changes based on movement
        if (velocityMagnitude < _velocityThreshold && MoveInput.magnitude < 0.1f)
        {
            ChangeState(AnimationState.Idle);
        }
        else
        {
            ChangeState(AnimationState.Locomotion);
        }

        // Locomotion parameter update when in locomotion state
        if (_currentState == AnimationState.Locomotion)
        {
            if (_dataAnimancer.AnimancerComponent.Layers[_layer].CurrentState is LinearMixerState linearMixerState)
            {
                linearMixerState.Parameter = Mathf.Lerp(linearMixerState.Parameter, velocityMagnitude,
                    Time.deltaTime * _interpolationSpeed);
                _parameterDebug = new Vector2(linearMixerState.Parameter, 0);
            }

            if (_asset2D != null &&
                _dataAnimancer.AnimancerComponent.Layers[_layer].CurrentState is CartesianMixerState mixerState2D)
            {
                mixerState2D.Parameter = Vector3.Lerp(mixerState2D.Parameter, facingVelocity, 
                    Time.deltaTime * _interpolationSpeed);
                _parameterDebug = mixerState2D.Parameter;
            }
        }
    }


    // Method to handle state transitions
    private void ChangeState(AnimationState newState)
    {
        if (_currentState == newState) return;

        OnExitState(_currentState); // Execute exit logic for the current state
        _previousState = _currentState;
        _currentState = newState;
        OnEnterState(_currentState); // Execute enter logic for the new state
    }

    // Called when entering a new state
    private void OnEnterState(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Idle:
                PlayIdle();
                break;
            case AnimationState.Locomotion:
                PlayLocomotionAsset();
                break;
        }
    }

    // Called when exiting the current state
    private void OnExitState(AnimationState state)
    {
        // Optionally, implement any logic you need when exiting a state
    }

    // Play the Idle animation
    private void PlayIdle()
    {
        if(_cachedTransitions.ContainsKey(_locomotionAssetData.Idle.Key))
        {
            var tr = _cachedTransitions[_locomotionAssetData.Idle.Key];
            if(_avatarMask) _dataAnimancer.AnimancerComponent.Layers[_layer].SetMask(_avatarMask);
            var state = _dataAnimancer.AnimancerComponent.Layers[_layer].Play(tr,_locomotionAssetData.Idle.FadeDuration,_locomotionAssetData.Idle.FadeMode);
            state.ApplyFootIK = true;
        }
        else
        {
            var tr = _locomotionAssetData.Idle.CreateState();
            if(_avatarMask) _dataAnimancer.AnimancerComponent.Layers[_layer].SetMask(_avatarMask);
            var state = _dataAnimancer.AnimancerComponent.Layers[_layer].Play(tr,_locomotionAssetData.Idle.FadeDuration,_locomotionAssetData.Idle.FadeMode);
            state.ApplyFootIK = true;
            _cachedTransitions.Add(_locomotionAssetData.Idle.Key,tr);
        }
    }

    Vector3 GetWorldSpaceMovement(Vector3 relativeMoveDirection)
    {
        Vector3 cameraForward = _mainCam.transform.forward;
        Vector3 cameraRight = _mainCam.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Matrix4x4 cameraRotationMatrix = new Matrix4x4(
            new Vector4(cameraRight.x, cameraRight.y, cameraRight.z, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(cameraForward.x, cameraForward.y, cameraForward.z, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 inverseCameraMatrix = cameraRotationMatrix.inverse;

        Vector3 worldSpaceDirection = inverseCameraMatrix.MultiplyPoint3x4(relativeMoveDirection);

        return worldSpaceDirection;
    }

    public virtual Vector2 GetMovementInput()
    {
        return _movingActor.MoveInput;
    }
}