using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MNF_Character : DataManifest
{
    [SerializeField] private DS_MovingActor _movingActor;
    [SerializeField] private Data_Living _livingData;
    [SerializeField] private Data_Character _characterData;
    
    protected override Data[] InstallData()
    {
        return new Data[] { _movingActor, _livingData, _characterData};
    }
}
