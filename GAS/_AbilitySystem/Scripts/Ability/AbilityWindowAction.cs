using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
[Serializable]
public class AbilityWindowAction
{
    [SerializeReference][TypeFilter("GetFilteredTypeList")]
    public AbilityAction Action;
    public string EventName;
   
    
    public IEnumerable<Type> GetFilteredTypeList()
    {
        var baseType = typeof(AbilityAction);
        var q = baseType.Assembly.GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
        return q;
    }
}
