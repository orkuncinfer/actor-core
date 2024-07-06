using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class DataSingle : MonoBehaviour
{
    [HideLabel][HideIf("_changeNameToggle")]
    [DisplayAsString(false, 20, TextAlignment.Center, true)]
    [InlineButton("A", SdfIconType.Pencil, "")]
    public string Description ;

    [ShowIf("_changeNameToggle")]  [InlineButton("A", SdfIconType.Check, "")][HorizontalGroup("ChangeName")]
    public string ChangeName;
    
    private bool _changeNameToggle;
    
    [SerializeReference]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = false)]
    public Data Data = new Data();
    
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

    private void OnEnable()
    {
        if ( Data.IsGlobal)
        {
            string key= "";
            if (Data.UseKey)
            {
                key = Data.DataKey.ID + Data.GetType();
            }
            else
            {
                key = Data.GetType().ToString();
            }
            GlobalData.LoadData(key,Data);
        }
    }
}