using UnityEngine;

public class MNF_EquipmentUser : DataManifest
{
    [SerializeField] private DS_EquipmentUser _equipmentUser;

    protected override Data[] InstallData()
    {
        return new Data[] { _equipmentUser };
    }
}
