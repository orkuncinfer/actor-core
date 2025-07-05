using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Serialization;

public enum EEquipHand
{
    LeftHand,
    RightHand
}
public class EquipmentSetupIKTargets : MonoBehaviour
{
    [FormerlySerializedAs("_equipable")] [SerializeField] private Equippable _equippable;
    
    [SerializeField] private EEquipHand _equipHand;
    [SerializeField] private Transform _aimTransform;
    [SerializeField] private Transform _leftHandPivot;
    [SerializeField] private Transform _rightHandPivot;
    [SerializeField] private Transform _leftHandBendTarget;
    [SerializeField] private Vector3 _gunHoldOffset;

    [Header("RecoilSettings")] [SerializeField]
    private Vector3 _handRotationOffset;
    [SerializeField] private Vector3 _rotationRandom;
    [SerializeField] private Recoil.RecoilOffset[] _offsets;
    
    private FullBodyBipedIK ik;
    private AimIK _aimIK;
    private AimIKWeightHandler _aimIKWeightHandler;
    private Recoil _recoil;
    private Camera _camera;
    private void OnEnable()
    {
        _camera = Camera.main;
        _equippable.onEquipped += SetupIKTargets;
        _equippable.onUnequipped += OnUnequip;
    }


    private void OnDisable()
    {
        _equippable.onEquipped -= SetupIKTargets;
        _equippable.onUnequipped -= OnUnequip;
    }
    private void OnUnequip(ActorBase obj)
    {
        ik.solver.rightHandEffector.positionWeight = 0;
        
        ik.solver.leftHandEffector.positionWeight = 0;
        ik.solver.leftHandEffector.rotationWeight = 0;
        
        ik.solver.rightHandEffector.target = null;
        ik.solver.leftHandEffector.target = null;

        _aimIKWeightHandler.ik.solver.leftArmChain.bendConstraint.bendGoal = null;
        
        _aimIKWeightHandler.ReleaseRightHand(0.25f);
        _aimIKWeightHandler.ReleaseLeftHand(0.25f);
    }

    public void SetupIKTargets(ActorBase actor)
    {
        ik = actor.GetComponentInChildren<FullBodyBipedIK>();
        _aimIKWeightHandler = actor.GetComponentInChildren<AimIKWeightHandler>();
        //_aimIKWeightHandler.ToggleAiming(true);

        _aimIKWeightHandler.gunHoldOffset = _gunHoldOffset;
        
        if (_aimTransform)
        {
            _aimIK = actor.GetComponentInChildren<AimIK>();
            _aimIK.solver.transform = _aimTransform;
        }

        if (_aimIKWeightHandler)
        {
            _aimIKWeightHandler.LeftHandBendTarget = _leftHandBendTarget;
        }
        
        if (_leftHandPivot)
        {
            
            ik.solver.leftHandEffector.target = _leftHandPivot;
        }
        
        if (_rightHandPivot)
        {
            ik.solver.rightHandEffector.target = _rightHandPivot;
        }
        
        _aimIKWeightHandler.HoldLeftHand(0.25f);
        _aimIKWeightHandler.HoldRightHand(0.25f);

        if (ik.references.leftHand.TryGetComponent(out HandPoser handPoser))
        {
            handPoser.poseRoot = _leftHandPivot;
        }
        
        if (ik.references.rightHand.TryGetComponent(out HandPoser handPoserRight))
        {
            handPoserRight.poseRoot = _rightHandPivot;
        }

        _aimIKWeightHandler.ik.solver.leftArmChain.bendConstraint.bendGoal = _leftHandBendTarget;

        _recoil = actor.GetService<AimIKWeightHandler>().GetComponent<Recoil>();
        if(_recoil == null) return;
        _recoil.offsets = _offsets;
        _recoil.handRotationOffset = _handRotationOffset;
        _recoil.rotationRandom = _rotationRandom;
    }

    private void UpdateIK()
    {
        Vector3 lookDirection = _camera.transform.forward;
        Vector3 aimTarget = _camera.transform.position + (lookDirection * 10f);
        _aimIK.solver.IKPosition = aimTarget;
    }
}
