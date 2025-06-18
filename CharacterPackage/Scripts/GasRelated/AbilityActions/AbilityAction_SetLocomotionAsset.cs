using RootMotion.FinalIK;
using UnityEngine;

public class AbilityAction_SetLocomotionAsset : AbilityAction
{
    [SerializeField] private DSGetter<Data_RefVar> _locomotionAsset;
    [SerializeField] private Object _locomotionAssetData;
    public override AbilityAction Clone()
    {
        AbilityAction_SetLocomotionAsset clone = AbilityActionPool<AbilityAction_SetLocomotionAsset>.Shared.Get();
        clone._locomotionAssetData = _locomotionAssetData;
        clone._locomotionAsset = _locomotionAsset;
        return clone;
    }

    public override void Reset()
    {
        base.Reset();

    }

    public override void OnStart()
    {
        base.OnStart();
        _locomotionAsset.GetData(Owner);
        _locomotionAsset.Data.Value = _locomotionAssetData;
    }

    public override void OnExit()
    {
        base.OnExit();
        _locomotionAsset.Data.Value = null;
    }
}