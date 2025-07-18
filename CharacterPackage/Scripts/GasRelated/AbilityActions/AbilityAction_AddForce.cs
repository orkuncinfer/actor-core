using ECM2;
using UnityEngine;

public class AbilityAction_AddForce : AbilityAction
{
    private ActiveAbility _ability;

    [SerializeField, Tooltip("Direction relative to character's orientation (e.g., forward = (0,0,1))")]
    private Vector3 _direction;

    [SerializeField, Tooltip("Magnitude of the force to apply")]
    private float _force;

    [SerializeField] private float _yForce;

    private Character _character;

    public override AbilityAction Clone()
    {
        AbilityAction_AddForce clone = AbilityActionPool<AbilityAction_AddForce>.Shared.Get();
        clone.EventName = EventName;
        clone._direction = _direction;
        clone._force = _force;
        clone._yForce =  _yForce;
        return clone;
    }

    public override void OnStart()
    {
        base.OnStart();

        _character = Owner.GetData<Data_Character>().Character;

        // Transform local direction to world space based on character's orientation
        Vector3 worldDirection = _character.transform.TransformDirection(_direction.normalized);

        // Apply force in the direction relative to character's orientation
        Vector3 forceVector = new Vector3(worldDirection.x * _force, worldDirection.y * _yForce, worldDirection.z * _force);
        _character.PauseGroundConstraint();
        _character.LaunchCharacter(forceVector);


    }

    public override void OnExit()
    {
        base.OnExit();
        AbilityActionPool<AbilityAction_AddForce>.Shared.Release(this);
    }
}