using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class StateTransition
{
    public MonoState ToState;
    
    [BoxGroup("General", ShowLabel = false)]
    [TypeFilter("GetFilteredTypeList")] [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference]public List<StateCondition> Conditions = new List<StateCondition>();

    public void Initialize(ActorBase owner)
    {
        Conditions.ForEach(x => x.Initialize(owner));
    }
    
    public IEnumerable<Type> GetFilteredTypeList()
    {
        var baseType = typeof(StateCondition);
        var q = baseType.Assembly.GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
        
        /*var dom = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var domain in dom)
        {
            q = q.Concat(domain.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => baseType.IsAssignableFrom(x) && x != baseType));
        }*/
       
        return q;
    }
}