using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataGetter
{
    public static T GetData<T>(string key = "", Actor owner = null) where T : Data
    {
        if (owner != null)
        {
            return owner.GetData<T>(key);
        }
        return GlobalData.GetData<T>(key);
    }
}
