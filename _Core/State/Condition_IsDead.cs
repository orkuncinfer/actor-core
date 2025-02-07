using System;

[Serializable]
public class Condition_IsDead : StateCondition
{
    private Data_Living _livingData;

    public override void Initialize(ActorBase owner)
    {
        base.Initialize(owner);
        _livingData = Owner.GetData<Data_Living>();
    }

    public override bool CheckCondition()
    {
        AddWatchingValue( "ShouldDieTrigger:"+ _livingData.ShouldDieTrigger.ToString());
        if(_livingData.ShouldDieTrigger)
        {
            _livingData.ShouldDieTrigger = false;
            return true;
        }

        return false;
    }
}