using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float Sensitivity = 1f;
    
    
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    
    public InputActionReference LookAction;

    void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    private float add = 0;
    void Increase()
    {
        if (add < 5000)
        {
            _cinemachineTargetYaw += .1f;
            add += .1f;
        }
    }
    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        Vector2 lookInput = LookAction.action.ReadValue<Vector2>();
        if (lookInput.sqrMagnitude >= 0)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = 1; //IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += lookInput.x * deltaTimeMultiplier * Sensitivity * Time.deltaTime;
            _cinemachineTargetPitch += lookInput.y * deltaTimeMultiplier * Sensitivity * Time.deltaTime;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        transform.rotation = Quaternion.Euler(_cinemachineTargetPitch,
            _cinemachineTargetYaw, 0.0f);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void AddPitchYaw(float pitch, float yaw)
    {
        _cinemachineTargetPitch += pitch;
        _cinemachineTargetYaw += yaw;
    }

    public Vector2 GetLookInput()
    {
        return LookAction.action.ReadValue<Vector2>();
    }

    public Vector2 GetAddedPitchYawThisFrame()
    {
        Vector2 lookInput = LookAction.action.ReadValue<Vector2>();
        return new Vector2(lookInput.y * 1 * Sensitivity * Time.deltaTime, lookInput.x * 1 * Sensitivity * Time.deltaTime);
    }

    public float GetAddedPitchThisFrame()
    {
        Vector2 lookInput = LookAction.action.ReadValue<Vector2>();
        return lookInput.y * 1 * Sensitivity * Time.deltaTime;
    }

    public float GetAddedYawThisFrame()
    {
        Vector2 lookInput = LookAction.action.ReadValue<Vector2>();
        return lookInput.x * 1 * Sensitivity * Time.deltaTime;
    }
}