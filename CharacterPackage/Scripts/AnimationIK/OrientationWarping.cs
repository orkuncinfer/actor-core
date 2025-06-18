using UnityEngine;

public class OrientationWarping : MonoBehaviour
{
    [Header("Bone Transforms")]
    public Transform Hips;
    public Transform Spine1;
    public Transform Spine2;
    public Transform Spine3;

    [Header("Spine Rotation Weights")]
    [Range(0f, 1f)] public float Spine1Weight = 0.3f;
    [Range(0f, 1f)] public float Spine2Weight = 0.5f;
    [Range(0f, 1f)] public float Spine3Weight = 0.7f;

    [Header("Rotation Settings")]
    public float LocomotionAngle; // Target hip rotation in degrees
    public float RotationSpeed = 5f; // Speed of interpolation
    public bool isHipsAdditive = true;
    public bool isChestAdditive = false; // Toggle additive rotation

    private float _currentHipsAngle = 0f;

    void LateUpdate()
    {
        if (Hips == null)
            return;

        // Step 1: Smoothly interpolate hips rotation
        _currentHipsAngle = Mathf.Lerp(_currentHipsAngle, LocomotionAngle, Time.deltaTime * RotationSpeed);
        Quaternion hipsRotation = Quaternion.Euler(0f, _currentHipsAngle, 0f);

        if (isHipsAdditive)
        {
            // Apply additive rotation
            Hips.rotation *= hipsRotation;
        }
        else
        {
            // Override rotation
            Hips.rotation = hipsRotation;
        }

        // Step 2: Apply spine rotation (additive or override)
        RotateSpine(Spine1, Spine1Weight);
        RotateSpine(Spine2, Spine2Weight);
        RotateSpine(Spine3, Spine3Weight);
    }

    void RotateSpine(Transform spineBone, float weight)
    {
        if (spineBone == null) return;

        // Calculate the blended counter-rotation angle
        float counterRotationAngle = Mathf.Lerp(_currentHipsAngle, 0f, weight);
        Quaternion counterRotation = Quaternion.Euler(0f, -counterRotationAngle, 0f);

        if (isChestAdditive)
        {
            // Apply rotation additively
            spineBone.rotation *= counterRotation;
        }
        else
        {
            // Override rotation to maintain facing forward
            spineBone.rotation = counterRotation * Hips.rotation;
        }
    }
}
