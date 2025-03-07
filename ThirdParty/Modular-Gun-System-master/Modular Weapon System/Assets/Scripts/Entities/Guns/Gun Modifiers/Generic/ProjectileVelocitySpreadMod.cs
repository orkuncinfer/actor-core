using ECM2;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Velocity Spread Modifier", menuName = "Gun Mods/Generic/Projectile Velocity Spread Modifier")]
public class ProjectileVelocitySpreadMod : GunModifier
{
    [Tooltip("Projectile spread increase amount per shot as a percentage in decimal form before modifiers")]
    [SerializeField] private float _multiplier;

    private float _currentSpreadModifier;

    private Character _character;
    

    private void OnUpdate(Gun target)
    {
        if (_character == null)
        {
            return;
        }
        Vector3 velocity = _character.GetVelocity();
        _currentSpreadModifier = velocity.magnitude * _multiplier;
    }


    public override void ApplyTo(Gun target)
    {
        target.GetComponent<Equippable>().onEquipped += OnEquipped;
        target.GunData.SpreadRadius.AddMod(GetInstanceID(),GetSpreadValue);
        target.onUpdate += OnUpdate;
    }

    private float GetSpreadValue(float currentValue)
    {
        return currentValue + _currentSpreadModifier;
    }

    public override void RemoveFrom(Gun target)
    {
        target.GetComponent<Equippable>().onEquipped -= OnEquipped;
        target.onUpdate -= OnUpdate;
        target.GunData.SpreadRadius.RemoveMod(this.GetInstanceID());
    }
    private void OnEquipped(ActorBase obj)
    {
        _character = obj.GetData<Data_Character>().Character;
    }
}