using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StatSystem
{
   [System.Serializable]
   public class Stat
   {
      [ShowInInspector]protected StatDefinition _definition;
      public StatDefinition Definition => _definition;
      
      protected int _value;
      public int Value => _value;
      public virtual int BaseValue => _definition.BaseValue;
      
      public event Action<int,int> onValueChangedWithArgs;
      public event Action onStatValueChanged;
      
      protected List<StatModifier> _modifiers = new List<StatModifier>();
      protected StatController _controller;
      
      
      public Stat(StatDefinition definition, StatController controller)
      {
         _definition = definition;
         _controller = controller;
      }

      public virtual void Initialize()
      {
         CalculateStatValue();
      }
   
      public void AddModifier(StatModifier modifier)
      {
         _modifiers.Add(modifier);
         CalculateStatValue();
      }
   
      public void RemoveModifierFromSource(object source)
      {
         foreach (var mod in _modifiers)
         {
            if(mod.Source == source)
            {
               //Debug.Log("Removing modifier : " + mod.Magnitude + mod.Source);
            }
         }
         int num = _modifiers.RemoveAll(modifier=> modifier.Source == source);
         if (num > 0)
         {
            CalculateStatValue();
         }
      }
      
      internal void CalculateStatValue()
      {
         float finalValue = BaseValue;

         if (_definition.Formula != null && _definition.Formula.RootNode != null)
         {
            finalValue += Mathf.RoundToInt(_definition.Formula.RootNode.CalculateValue(_controller.gameObject));
            //Debug.Log("stat is" + finalValue);
         }
         
         _modifiers.Sort((x, y) => x.Type.CompareTo(y.Type));
   
         for (int i = 0; i < _modifiers.Count; i++)
         {
            StatModifier modifier = _modifiers[i];
            if (modifier.Type == ModifierOperationType.Additive)
            {
               finalValue += modifier.Magnitude;
            }
            else if (modifier.Type == ModifierOperationType.Multiplicative)
            {
               finalValue *= modifier.Magnitude;
            }
         }
   
         if (_definition.Cap >= 0)
         {
            finalValue = Mathf.Min(finalValue, _definition.Cap);
         }
   
         if (_value != finalValue)
         {
            int oldValue = _value;
            _value = Mathf.RoundToInt(finalValue);
            onValueChangedWithArgs?.Invoke(oldValue,_value);
            onStatValueChanged?.Invoke();
         }
         _value = Mathf.RoundToInt(finalValue);
      }
   }
}

