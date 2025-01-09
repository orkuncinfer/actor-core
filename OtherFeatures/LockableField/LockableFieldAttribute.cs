using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LockableFieldAttribute : PropertyAttribute
{
    public string LockStateFieldName { get; private set; }

    public LockableFieldAttribute(string lockStateFieldName)
    {
        LockStateFieldName = lockStateFieldName;
    }
}