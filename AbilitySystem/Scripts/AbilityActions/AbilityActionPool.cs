using System.Collections.Generic;

public class AbilityActionPool<T> where T : AbilityAction, new()
{ 
    private static AbilityActionPool<T> _instance;
    public static AbilityActionPool<T> Shared 
    { 
        get 
        {
            if (_instance == null)
                _instance = new AbilityActionPool<T>();
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
            return new T();
        }
        DDebug.Log("returned an element from pool");
        return _availableObjects.Dequeue();
    }

    public void Release(T element)
    {
        element.Reset();  // Assuming Reset is a method to clean up the object for reuse
        _availableObjects.Enqueue(element);
    }
}