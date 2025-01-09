using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class GlobalDataDisplayer : PersistentMonoSingleton<GlobalDataDisplayer>
{
    [ShowInInspector]private Dictionary<string, Data> _datasets = new Dictionary<string, Data>();

    public void FetchData(Dictionary<string, Data> dictionary)
    {
        _datasets = dictionary;
    }
}
