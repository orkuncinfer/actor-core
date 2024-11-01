using System;

public static class LastStaticUpdater
{
    public static event Action onUpdate;
    public static event Action onLateUpdate;
    public static event Action onFixedUpdate;
    
    public static void Update()
    {
        onUpdate?.Invoke();
    }
    
    public static void LateUpdate()
    {
        onLateUpdate?.Invoke();
    }
    
    public static void FixedUpdate()
    {
        onFixedUpdate?.Invoke();
    }
}