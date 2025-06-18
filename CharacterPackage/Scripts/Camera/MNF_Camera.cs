using UnityEngine;

public class MNF_Camera : DataManifest
{
    [SerializeField] private Data_Camera _camera;

    protected override Data[] InstallData()
    {
        return new Data[] { _camera };
    }
}
