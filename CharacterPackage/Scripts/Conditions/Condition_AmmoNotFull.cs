using System;

[Serializable]
public class Condition_AmmoNotFull : StateCondition
{
    public override void Initialize(ActorBase owner)
    {
        base.Initialize(owner);
    }

    public override bool CheckCondition()
    {
        if (Owner.GetEquippedInstance().GetComponent<Gun>().BulletsInMagazine <
            Owner.GetEquippedInstance().GetComponent<Gun>().GetMaxMagazineSize())
        {
            return true;
        }

        return false;
    }
}