using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EffectTypeAttribute : Attribute
{
    public readonly Type type;

    public EffectTypeAttribute(Type type)
    {
        this.type = type;
    }
}
