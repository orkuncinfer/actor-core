using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class DataList : MonoBehaviour
{
    [HideLabel][HideIf("_changeNameToggle")]
    [DisplayAsString(false, 20, TextAlignment.Center, true)]
    [InlineButton("A", SdfIconType.Pencil, "")]
    public string Description ;

    [ShowIf("_changeNameToggle")]  [InlineButton("A", SdfIconType.Check, "")]
    public string ChangeName;

    private bool _changeNameToggle;
    
    
    [SerializeReference][TypeFilter("GetFilteredTypeList")] [ListDrawerSettings(ShowFoldout = true)]
    public List<Data> AbilityActions = new List<Data>();
    
    public IEnumerable<Type> GetFilteredTypeList()
    {
        var baseType = typeof(Data);
        var q = baseType.Assembly.GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
        return q;
    }
    
    private void A()
    {
        _changeNameToggle = !_changeNameToggle;
        Description = ChangeName;
    }
}

