using UnityEngine;

public class MNF_MobData : DataManifest
{
    [SerializeField] private Data_Mob _mobData;

    protected override Data[] InstallData()
    {
        return new Data[] { _mobData };
    }
}
[System.Serializable]
public class Data_Mob : Data
{
    public MobArea.MobGroup MobGroup;

    [SerializeField] private MobDefinition _mobDefinition;
    public MobDefinition MobDefinition
    {
        get => _mobDefinition;
        private set => _mobDefinition = value;
    }
}
