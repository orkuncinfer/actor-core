using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MNF_GameMode : DataManifest
{
    [SerializeField] private DS_GameModeRuntime _gameModeRuntime;
    [SerializeField] private DS_GameModePersistent _gameModePersistent;
    
    protected override Data[] InstallData()
    {
        return new Data[] { _gameModePersistent , _gameModeRuntime};
    }
}
