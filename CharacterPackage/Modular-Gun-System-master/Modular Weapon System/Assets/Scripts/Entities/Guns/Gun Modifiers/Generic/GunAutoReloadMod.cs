using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Auto Reload Modifier", menuName = "Gun Mods/Generic/Auto Reload Modifier")]
public class GunAutoReloadMod : GunModifier
{
 

    [SerializeField] private AbilityDefinition _reloadAbility;

   

    void ResetTransform(){
       
    }

    public override void ApplyTo(Gun target){
        target.FireComponent.onFire += OnFire;
    }

    private void OnFire(Gun target)
    {
        if (target.BulletsInMagazine <= 0)
        {
            target.GetComponent<Equippable>().Owner.GetService<Service_GAS>().AbilityController.TryActiveAbilityWithDefinition(_reloadAbility);
        }
    }

    public override void RemoveFrom(Gun target){
        target.FireComponent.onFire -= OnFire;
    }
}