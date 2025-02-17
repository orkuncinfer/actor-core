using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

#if UNITY_EDITOR
using Sirenix.OdinInspector; // Requires Odin Inspector
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class BodyColliderBuilder : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private float _armRadius;
    [SerializeField] private float _upperLegRadius;
    [SerializeField] private float _lowerLegRadius;
    [SerializeField]private List<Collider> _colliders = new List<Collider>();

    private Dictionary<HumanBodyBones, (HumanBodyBones, ColliderType)> _bonePairs = new Dictionary<HumanBodyBones, (HumanBodyBones, ColliderType)>
    {
        { HumanBodyBones.Hips, (HumanBodyBones.Spine, ColliderType.Box) },
        { HumanBodyBones.Spine, (HumanBodyBones.Chest, ColliderType.Box) },
        { HumanBodyBones.Chest, (HumanBodyBones.Head, ColliderType.Box) },
        { HumanBodyBones.Head, (HumanBodyBones.Head, ColliderType.Sphere) },

        { HumanBodyBones.LeftUpperLeg, (HumanBodyBones.LeftLowerLeg, ColliderType.Capsule) },
        { HumanBodyBones.LeftLowerLeg, (HumanBodyBones.LeftFoot, ColliderType.Capsule) },
        { HumanBodyBones.RightUpperLeg, (HumanBodyBones.RightLowerLeg, ColliderType.Capsule) },
        { HumanBodyBones.RightLowerLeg, (HumanBodyBones.RightFoot, ColliderType.Capsule) },

        { HumanBodyBones.LeftUpperArm, (HumanBodyBones.LeftLowerArm, ColliderType.Capsule) },
        { HumanBodyBones.LeftLowerArm, (HumanBodyBones.LeftHand, ColliderType.Capsule) },
        { HumanBodyBones.RightUpperArm, (HumanBodyBones.RightLowerArm, ColliderType.Capsule) },
        { HumanBodyBones.RightLowerArm, (HumanBodyBones.RightHand, ColliderType.Capsule) },
    };

    private enum ColliderType { Capsule, Box, Sphere }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (!_animator.isHuman)
        {
            Debug.LogError("BodyColliderBuilder requires a humanoid Animator!");
        }
    }

    [Button("Build Colliders")]
    public void Build()
    {
        _animator = GetComponent<Animator>();
        Remove(); // Ensure old colliders are removed before adding new ones
        
        foreach (var bonePair in _bonePairs)
        {
            Transform startBone = _animator.GetBoneTransform(bonePair.Key);
            Transform endBone = _animator.GetBoneTransform(bonePair.Value.Item1);
            ColliderType colliderType = bonePair.Value.Item2;
            if (bonePair.Key == HumanBodyBones.Head)
            {
                colliderType = ColliderType.Capsule;
            }

            if (bonePair.Key == HumanBodyBones.Chest)
            {
                endBone = _animator.GetBoneTransform(HumanBodyBones.Neck);
            }

            if (startBone == null || endBone == null) continue;

            Collider newCollider = null;
            switch (colliderType)
            {
                case ColliderType.Capsule:
                    startBone.gameObject.AddComponent<HumanBodyBoneTag>().Bone = bonePair.Key;
                    bool negativeCenter = bonePair.Key == HumanBodyBones.RightUpperArm ||
                                          bonePair.Key == HumanBodyBones.RightLowerArm
                                          || bonePair.Key == HumanBodyBones.RightUpperLeg ||
                                          bonePair.Key == HumanBodyBones.RightLowerLeg;
                    float capsuleRadius = -1;
                    if (bonePair.Key == HumanBodyBones.RightUpperLeg ||( bonePair.Key == HumanBodyBones.LeftUpperLeg))
                    {
                        capsuleRadius = _upperLegRadius;
                    }
                    if (bonePair.Key == HumanBodyBones.RightLowerLeg || (bonePair.Key == HumanBodyBones.LeftLowerLeg))
                    {
                        capsuleRadius = _lowerLegRadius;
                    }
                    if (bonePair.Key.ToString().Contains("Arm"))
                    {
                        capsuleRadius = _armRadius;
                    }
                    
                    newCollider = CreateCapsuleCollider(startBone, endBone, negativeCenter,capsuleRadius);
                    break;
                case ColliderType.Box:
                    startBone.gameObject.AddComponent<HumanBodyBoneTag>().Bone = bonePair.Key;
                    newCollider = CreateBoxCollider(startBone, endBone);
                    break;
                case ColliderType.Sphere:
                    startBone.gameObject.AddComponent<HumanBodyBoneTag>().Bone = bonePair.Key;
                    newCollider = CreateSphereCollider(startBone);
                    break;
            }

            if (newCollider != null)
            {
                _colliders.Add(newCollider);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this); // Ensure changes are saved in the editor
#endif
    }

    [Button("Remove Colliders")]
    public void Remove()
    {
        foreach (var col in _colliders)
        {
            if (col != null)
            {
                if (col.TryGetComponent(out Rigidbody rigidbody))
                {
                    DestroyImmediate(rigidbody);
                }
                if (col.TryGetComponent(out HumanBodyBoneTag boneTag))
                {
                    DestroyImmediate(boneTag);
                }
                DestroyImmediate(col);
            }
        }
        _colliders.Clear();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [Button("Enforce T-Pose")]
    public void TPose()
    {
        _animator = GetComponent<Animator>();
        Debug.Log(_animator.avatar.humanDescription.upperArmTwist);
        if (_animator == null || !_animator.isHuman)
        {
            Debug.LogError("Animator is either missing or not a humanoid!");
            return;
        }

        SetBoneRotation(HumanBodyBones.Hips, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.Spine, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.Chest, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.UpperChest, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.Neck, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.Head, Quaternion.identity);

        // Arms (T-Pose)
        SetBoneRotation(HumanBodyBones.LeftUpperArm, Quaternion.Euler(0, 0, 90));
        SetBoneRotation(HumanBodyBones.LeftLowerArm, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.LeftHand, Quaternion.identity);

        SetBoneRotation(HumanBodyBones.RightUpperArm, Quaternion.Euler(0, 0, 90));
        SetBoneRotation(HumanBodyBones.RightLowerArm, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.RightHand, Quaternion.identity);

        // Legs (Straight Down)
        SetBoneRotation(HumanBodyBones.LeftUpperLeg, Quaternion.Euler(180, 0, 0));
        SetBoneRotation(HumanBodyBones.LeftLowerLeg, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.LeftFoot, Quaternion.identity);

        SetBoneRotation(HumanBodyBones.RightUpperLeg, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.RightLowerLeg, Quaternion.identity);
        SetBoneRotation(HumanBodyBones.RightFoot, Quaternion.identity);

        Debug.Log("T-Pose Enforced!");

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [Button]
    public void MakeAllIsTrigger()
    {
        foreach (var collider in _colliders)
        {
            collider.isTrigger = true;
        }
    }
    [Button]
    public void SetLayerToBone()
    {
        foreach (var collider in _colliders)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("HumanBone");
        }
    }
    [Button]
    public void SetLayerToDefault()
    {
        foreach (var collider in _colliders)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    [Button]
    public void AddRigidbodies()
    {
        foreach (var collider in _colliders)
        {
            collider.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void SetBoneRotation(HumanBodyBones bone, Quaternion rotation)
    {
        Transform boneTransform = _animator.GetBoneTransform(bone);
        if (boneTransform != null)
        {
            boneTransform.localRotation = rotation;
        }
    }

    private CapsuleCollider CreateCapsuleCollider(Transform startBone, Transform endBone, bool negativeCenter = false, float capsuleRadius = -1)
    {
        CapsuleCollider collider = startBone.gameObject.AddComponent<CapsuleCollider>();
        Vector3 direction = endBone.position - startBone.position;
        float length = direction.magnitude;
        float radius = length * 0.15f;
        if (capsuleRadius != -1) radius = capsuleRadius;
        collider.center = Vector3.up * (length / 2);
        if (negativeCenter) collider.center *= -1;
        collider.height = length;
        collider.radius = radius;
        collider.direction = 1;
        collider.isTrigger = true;
        
        return collider;
    }

    private BoxCollider CreateBoxCollider(Transform startBone, Transform endBone)
    {
        BoxCollider collider = startBone.gameObject.AddComponent<BoxCollider>();
        Vector3 direction = endBone.position - startBone.position;
        float length = direction.magnitude;
        Vector3 size = new Vector3(length * 0.6f, length, length * 0.4f);

        collider.center = Vector3.up * (length / 2);
        collider.size = size;
        collider.isTrigger = true;
        return collider;
    }

    private SphereCollider CreateSphereCollider(Transform bone)
    {
        SphereCollider collider = bone.gameObject.AddComponent<SphereCollider>();
        float radius = Vector3.Distance(bone.position, _animator.GetBoneTransform(HumanBodyBones.Neck).position) * 0.8f;
        collider.radius = radius;
        collider.isTrigger = true;
        return collider;
    }
}
