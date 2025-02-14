using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class GlobalDataDisplayer : PersistentSingleton<GlobalDataDisplayer>
{
    [ShowInInspector]private Dictionary<string, Data> _datasets = new Dictionary<string, Data>();

    public void FetchData(Dictionary<string, Data> dictionary)
    {
        _datasets = dictionary;
    }
}
