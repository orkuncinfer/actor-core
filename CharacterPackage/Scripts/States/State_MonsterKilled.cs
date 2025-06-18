using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_MonsterKilled : MonoState
{
    private Data_Monster _monsterData;
    [SerializeField] private EventField<MobKilledEventArgs> _onMobKilled;
    protected override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("dead11");
        _monsterData = Owner.GetData<Data_Monster>();
        _onMobKilled.Raise(new MobKilledEventArgs()
        {
            MobId = "_monsterData.Definition.MonsterID",
            Position = Owner.transform.position+Utils.GetCenterOfCollider(Owner.transform)
        });
/*        Events.onExampleEvent.Invoke(new MobKilledEventArgs()
        {
            MobId = _monsterData.Definition.MonsterID,
            Position = Owner.transform.position+Utils.GetCenterOfCollider(Owner.transform)
        });*/
    }
}
public struct MobKilledEventArgs
{
    public string MobId;
    public Vector3 Position;
    
}
