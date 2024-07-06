using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class DataList : MonoBehaviour
{
    [HideLabel][HideIf("_changeNameToggle")]
    [DisplayAsString(false, 20, TextAlignment.Center, true)]
    [InlineButton("A", SdfIconType.Pencil, "")]
    public string Description ;

    [ShowIf("_changeNameToggle")]  [InlineButton("A", SdfIconType.Check, "")][HorizontalGroup("ChangeName")]
    public string ChangeName;
    
    private bool _changeNameToggle;
    
    [FormerlySerializedAs("Datas")] 
    [SerializeReference]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)][Searchable]
    public List<Data> Datas = new List<Data>();
    
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
        Debug.Log("1");
        for (int i = 0; i < Datas.Count; i++)
        {
            Debug.Log("11");
            if ( Datas[i].IsGlobal)
            {
                Debug.Log("111");
                string key= "";
                if (Datas[i].UseKey)
                {
                    key = Datas[i].DataKey.ID;
                }
                Debug.Log("1111");
                GlobalData.LoadData(key,Datas[i]);
            }
        }
    }
}

