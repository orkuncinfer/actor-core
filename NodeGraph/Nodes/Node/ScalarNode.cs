using System;
using UnityEngine;

namespace Core.Editor
{
    public class ScalarNode : CodeFunctionNode
    {
       [SerializeField] private float _value;
       public override float Value
       {
           get => _value;
           set
           {
               if (_value != value)
               {
                   _value = value;
                   OnValidateSelf();
               }
           }
       }

       public override float CalculateValue(GameObject source)
       {
           return _value;
       }

#if UNITY_EDITOR
       private void OnValidate()
       {
           OnValidateSelf();
       }
#endif
    }
}