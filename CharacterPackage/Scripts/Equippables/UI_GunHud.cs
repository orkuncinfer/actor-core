using TMPro;
using UnityEngine;

public class UI_GunHud : UI_EquippableHud
{
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private TMP_Text _nameText;

    private Gun _lastGun;

    public override void SetEquippable(Equippable equippable)
    {
        base.SetEquippable(equippable);
        
        if (_lastGun != null)_lastGun.onBulletsInMagainzeChanged -= UpdateAmmo;
        
        if (equippable == null)
        {
            _nameText.text = "";
            _ammoText.text = "";
            return;
        }

        ItemBaseDefinition itemDefinition = DefaultPlayerInventory.Instance.GetItemDefinition(equippable.ItemData.ItemID);
        Gun gun = equippable.GetComponent<Gun>();
        GunData gunData = gun.GunData;

        if (itemDefinition == null)
        {
            Debug.Log("itemdef null for " + equippable.ItemData.ItemID);
        }
       

        gun.onBulletsInMagainzeChanged += UpdateAmmo;

        _nameText.text = itemDefinition.ItemName;
        _ammoText.text = gun.BulletsInMagazine +  "/" + gun.PlayerAmmoStorage.GetAmmoAmount(gunData.AmmoItemDefinition).ToString();
        _lastGun = gun;
    }

    private void UpdateAmmo(Gun arg1, int arg2)
    {
        _ammoText.text = arg1.BulletsInMagazine +  "/" + arg1.PlayerAmmoStorage.GetAmmoAmount(arg1.GunData.AmmoItemDefinition).ToString();
    }
}