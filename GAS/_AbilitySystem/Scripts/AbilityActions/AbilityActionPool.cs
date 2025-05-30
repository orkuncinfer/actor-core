using System.Collections.Generic;
using UnityEngine;

public class AbilityActionPool<T> where T : AbilityAction, new()
{ 
    private static AbilityActionPool<T> _instance;
    public static AbilityActionPool<T> Shared 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = new AbilityActionPool<T>();
            }
                
            return _instance;
        } 
    }

    private Queue<T> _availableObjects = new Queue<T>();
    public int CountAll { get; private set; }
    public int CountActive => CountAll - CountInactive;
    public int CountInactive => _availableObjects.Count;

    public T Get()
    {
        if (_availableObjects.Count == 0)
        {
            CountAll++;
            //Debug.Log("created new object " + typeof(T) + " count " + _availableObjects.Count);
            return new T();
        }
        //Debug.Log("not created new object " + typeof(T) + " count " + _availableObjects.Count);
        return _availableObjects.Dequeue();
    }

    public void Release(T element)
    {
        element.Reset(); 
        _availableObjects.Enqueue(element);
    }
}