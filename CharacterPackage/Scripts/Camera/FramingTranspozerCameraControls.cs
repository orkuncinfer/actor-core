using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FramingTranspozerCameraControls : MonoBehaviour
{
    public InputActionAsset ActionAsset;
    
    [Header("Rotation Settings")]
    public float RotationSpeed = 5f;
    public float MinVerticalAngle = -30f;
    public float MaxVerticalAngle = 60f;
    public float InitialYaw = 30;
    public float InitialPitch = 0;
    
    [Header("Zoom Settings")]
    public float ZoomSpeed = 5f;
    public float MinZoomDistance = 3f;
    public float MaxZoomDistance = 20f;
    [Tooltip("Higher values make zooming smoother but slower to respond")]
    [Range(0.01f, 1f)]
    public float ZoomSmoothness = 0.1f;
    
    private InputAction _rotateAction;
    private InputAction _zoomAction;
    
    private CinemachineFramingTransposer _virtualCamera;
    
    private bool _lockedMouse;
    private Vector2 _storedCursorPos;
    
    private float _yaw;
    private float _pitch;
    
    private bool _performingRotate;
    
    // Smooth zooming fields
    private float _targetZoomDistance;
    private float _currentZoomVelocity;
    
    private void Start()
    {
        _yaw = InitialYaw;
        _pitch = InitialPitch;
        
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        
        _virtualCamera = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        transform.SetParent(null);
        
        _rotateAction = ActionAsset.FindAction("RotateCamera");
        _zoomAction = ActionAsset.FindAction("ZoomCamera");
        
        _zoomAction.performed += OnZoomPerformed;
        _rotateAction.performed += OnRotatePerformed;
        _rotateAction.Enable();
        _zoomAction.Enable();
        
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
        
        // Initialize target zoom distance to current camera distance
        _targetZoomDistance = _virtualCamera.m_CameraDistance;
    }
    
    private void OnDestroy()
    {
        _rotateAction.performed -= OnRotatePerformed;
        _zoomAction.performed -= OnZoomPerformed;
    }
    
    private void LateUpdate()
    {
        HandleMouseLock();
        HandleRotation();
        HandleSmoothZoom();
    }
    
    private void HandleMouseLock()
    {
        if (_lockedMouse)
        {
            Mouse.current.WarpCursorPosition(_storedCursorPos);
        }
    }
    
    private void HandleRotation()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = _rotateAction.ReadValue<Vector2>();
            _yaw += delta.x * RotationSpeed * Time.deltaTime;
            _pitch -= delta.y * RotationSpeed * Time.deltaTime;
            
            _pitch = Mathf.Clamp(_pitch, MinVerticalAngle, MaxVerticalAngle);
            
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
    
    private void HandleSmoothZoom()
    {
        // Smooth zoom using SmoothDamp for natural acceleration/deceleration
        float currentDistance = _virtualCamera.m_CameraDistance;
        _virtualCamera.m_CameraDistance = Mathf.SmoothDamp(
            currentDistance,
            _targetZoomDistance,
            ref _currentZoomVelocity,
            ZoomSmoothness,
            float.MaxValue,
            Time.deltaTime
        );
        
        // Ensure we stay within bounds
        _virtualCamera.m_CameraDistance = Mathf.Clamp(_virtualCamera.m_CameraDistance, MinZoomDistance, MaxZoomDistance);
        
        // Also clamp target to ensure we don't overshoot
        _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, MinZoomDistance, MaxZoomDistance);
    }
    
    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        if (!Mouse.current.rightButton.isPressed)
        {
            if (_lockedMouse)
            {
                _lockedMouse = false;
                Mouse.current.WarpCursorPosition(_storedCursorPos);
            }
            return;
        }
        
        if (!_lockedMouse)
        {
            _lockedMouse = true;
            _storedCursorPos = Mouse.current.position.value;
        }
    }
    
    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        // Get the scroll wheel delta
        float scrollDelta = context.ReadValue<Vector2>().y;
        
        // Apply zoom using the delta - negative value for zooming in, positive for zooming out
        _targetZoomDistance -= scrollDelta * ZoomSpeed * Time.deltaTime;
        
        // Clamp the target distance to stay within bounds
        _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, MinZoomDistance, MaxZoomDistance);
    }
}