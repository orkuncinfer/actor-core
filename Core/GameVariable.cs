using System;
using UnityEngine;
[Serializable]
public class GameVariable<T> : ScriptableObject
{
    [SerializeField]
    private T value;

    // Define a C# event with Action<T>
    public event Action<T> OnChange;

    public T Value
    {
        get => value;
        set
        {
            if (!Equals(this.value, value))
            {
                this.value = value;
                OnChange?.Invoke(this.value);
            }
        }
    }

    public void SetValue(T newValue)
    {
        if (!Equals(value, newValue))
        {
            value = newValue;
            OnChange?.Invoke(value);
        }
    }

    public T GetValue()
    {
        return value;
    }
}
