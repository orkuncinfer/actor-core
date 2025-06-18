using UnityEngine;

public class MNF_Combatable : DataManifest
{
    [SerializeField] private Data_Combatable _combatable; 
    protected override Data[] InstallData()
    {
        return new Data[] { _combatable };
    }
}