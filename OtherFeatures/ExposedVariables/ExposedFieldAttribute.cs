using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class ExposedFieldAttribute : Attribute
{
   public string DisplayName;
   
   public ExposedFieldAttribute(string displayName)
   {
      DisplayName = displayName;
   }
}
