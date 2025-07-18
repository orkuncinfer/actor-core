using ECM2;
using StatSystem;
using UnityEngine;

public class AbilityAction_WarpToTarget : AbilityAction
{
    [Tooltip("Radius offset in XZ plane from the target's position.")]
    public float OffsetRadius;

    [Tooltip("If true, the target will instantly rotate to face the owner.")]
    public bool SnapTargetRotation;

    [Tooltip("If true, the owner will smoothly rotate to face the target during warp.")]
    public bool RotateToTarget;

    private Transform _targetTransform;
    private Attribute _targetHealth;
    private AbilityController _abilityController;

    private Character _targetCharacter;
    private Character _ownerCharacter;

    private Vector3 _startPosition;
    private Vector3 _targetFinalPosition;
    private float _elapsedTime;

    public override AbilityAction Clone()
    {
        base.Clone();
        AbilityAction_WarpToTarget clone = AbilityActionPool<AbilityAction_WarpToTarget>.Shared.Get();
        clone.EventName = EventName;
        clone.AnimWindow = AnimWindow;
        clone._hasTick = true;
        clone.OffsetRadius = OffsetRadius;
        clone.SnapTargetRotation = SnapTargetRotation;
        clone.RotateToTarget = RotateToTarget;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();

        _abilityController = Owner.GetService<Service_GAS>().AbilityController;
        if (_abilityController.Target == null)
        {
            RequestEndAbility();
            return;
        }

        _targetTransform = _abilityController.Target.transform;
        _targetCharacter = _abilityController.Target.GetComponent<Character>();
        _ownerCharacter = Owner.GetComponent<Character>();

        _startPosition = _ownerCharacter.transform.position;
        _elapsedTime = 0f;

        Vector3 toTargetXZ = (_targetTransform.position - _startPosition);
        toTargetXZ.y = 0;

        if (toTargetXZ.sqrMagnitude > 0.01f)
        {
            toTargetXZ.Normalize();
            _targetFinalPosition = _targetTransform.position - (toTargetXZ * OffsetRadius);

            if (SnapTargetRotation && _targetCharacter != null)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-toTargetXZ);
                _targetCharacter.SetRotation(lookRotation);
            }
        }
        else
        {
            _targetFinalPosition = _targetTransform.position;
        }

        _targetHealth = _abilityController.Target.GetComponent<Actor>()
            .GetService<Service_GAS>().StatController.GetAttribute("Health");
    }

    public override void OnTick(Actor owner)
    {
        base.OnTick(owner);
        if (_targetHealth == null || _targetHealth.CurrentValue <= 0)
        {
            RequestEndAbility();
            return;
        }

        _elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / TimeLength);

        Vector3 newPosition = Vector3.Lerp(_startPosition, _targetFinalPosition, progress);
        _ownerCharacter.SetPosition(newPosition);

        if (RotateToTarget && _targetTransform != null)
        {
            Vector3 direction = _targetTransform.position - _ownerCharacter.transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                Quaternion newRotation = Quaternion.Slerp(_ownerCharacter.transform.rotation, targetRotation, Time.deltaTime * 10f); // smooth factor
                _ownerCharacter.SetRotation(newRotation);
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_ownerCharacter != null)
        {
            _ownerCharacter.SetPosition(_targetFinalPosition);
        }

        AbilityActionPool<AbilityAction_WarpToTarget>.Shared.Release(this);
    }
}
