using System.Collections;
using System.Collections.Generic;
using StatSystem;
using UnityEngine;
using UnityEngine.UIElements;

public class StatCollectionEditor : ScriptableObjectCollectionEditor<StatDefinition>
{
    public new class UxmlFactory : UxmlFactory<StatCollectionEditor,UxmlTraits>
    {
        
    }
}
