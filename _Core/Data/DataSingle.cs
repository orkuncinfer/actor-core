using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class DataSingle : MonoBehaviour
{
    [HideLabel][HideIf("_changeNameToggle")]
    [DisplayAsString(false, 20, TextAlignment.Center, true)]
    [InlineButton("A", SdfIconType.Pencil, "")]
    public string Description ;

    [ShowIf("_changeNameToggle")]  [InlineButton("A", SdfIconType.Check, "")][HorizontalGroup("ChangeName")]
    public string ChangeName;
    
    private bool _changeNameToggle;
    
    [SerializeReference][TypeFilter("GetFilteredTypeList")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = false)]
    public Data Data = new Data();

    public bool showData;
    
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
                key = Data.DataKey + Data.GetType();
            }
            else
            {
                key = Data.GetType().ToString();
            }
            GlobalData.InstallData(key,Data);
        }
    }
}