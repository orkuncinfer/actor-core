using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
public enum InitializableStateFlags
{
    Nothing =0,
    Initialized =1<<0,
}
public abstract class MonoInitializable : MonoBehaviour
{
    [HideInInspector]public bool IsInitialized;
    [ShowInInspector][HorizontalGroup("Status")][DisplayAsString(FontSize = 14)][PropertyOrder(-1000)][HideLabel][GUIColor("GetColorForProperty")]
    public virtual InitializableStateFlags Flags
    {
        get
        {
            InitializableStateFlags flags = InitializableStateFlags.Nothing;
            if (IsInitialized)
            {
                flags |= InitializableStateFlags.Initialized;
            }
            return flags;
        }
    }

    public abstract void Initialize();
    protected void SetInitialized(bool value)
    {
        IsInitialized = value;
    }
    private Color GetColorForProperty()
    {
        return IsInitialized ? new Color(0.35f,.83f,.29f,255) : new Color(1f,.29f,.29f,255);
    }
}