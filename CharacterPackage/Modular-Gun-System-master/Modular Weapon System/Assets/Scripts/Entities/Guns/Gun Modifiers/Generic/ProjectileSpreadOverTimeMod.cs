using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Projectile Spread Over Time Modifier", menuName = "Gun Mods/Generic/Projectile Spread Over Time Modifier")]
public class ProjectileSpreadOverTimeMod : GunModifier
{
    [Tooltip("Projectile spread increase amount per shot as a percentage in decimal form before modifiers")]
    [SerializeField] private float spreadIncreasePerShot;
    [SerializeField] private float _cooldownSpeed = 1f;
    [SerializeField] float delayBeforeCooldown = 0.2f;

    private float _currentSpreadModifier;
    private float _cooldownTimer;
    private bool _isCoolingDown;
    float elapsedTimeSinceShot;

    private void OnFire(Gun target)
    {
        _currentSpreadModifier += spreadIncreasePerShot;
        _currentSpreadModifier = Mathf.Clamp(_currentSpreadModifier,0, 10);
        _cooldownTimer = 0f;
        _isCoolingDown = true;
        elapsedTimeSinceShot = 0f;
    }

    private void OnUpdate(Gun target)
    {
        if (elapsedTimeSinceShot > delayBeforeCooldown)
        {
            if (_currentSpreadModifier > 0)
            {
                _currentSpreadModifier -= _cooldownSpeed * Time.deltaTime;
                _currentSpreadModifier = Mathf.Max(0, _currentSpreadModifier); // Clamp to zero
            }
        }
        elapsedTimeSinceShot += Time.deltaTime;
    }


    public override void ApplyTo(Gun target)
    {
        target.GunData.SpreadRadius.AddMod(GetInstanceID(),GetSpreadValue);
        target.FireComponent.onFire += OnFire;
        target.onUpdate += OnUpdate;
        
        _currentSpreadModifier = 0f;
        _cooldownTimer = 0f;
        _isCoolingDown = false;
    }

    private float GetSpreadValue(float currentValue)
    {
        return currentValue + _currentSpreadModifier;
    }

    public override void RemoveFrom(Gun target)
    {
        target.FireComponent.onFire -= OnFire;
        target.onUpdate -= OnUpdate;
        target.GunData.SpreadRadius.RemoveMod(this.GetInstanceID());
    }
}