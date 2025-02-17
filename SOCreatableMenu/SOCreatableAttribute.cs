using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SOCreatableAttribute : Attribute
{
    public string Category { get; }
    
    public SOCreatableAttribute(string category = "Uncategorized")
    {
        Category = category;
    }
}