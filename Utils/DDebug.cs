using UnityEngine;

public static class DDebug
{
    public static bool enableLogging = true;

    // Load and save the enableLogging state
    static DDebug()
    {
        enableLogging = PlayerPrefs.GetInt("EnableDDebugLogging", 1) == 1;
    }

    public static void Log(object message)
    {
        if (enableLogging)
            Debug.Log(message);
    }

    public static void LogError(object message)
    {
        if (enableLogging)
            Debug.LogError(message);
    }

    public static void LogWarning(object message)
    {
        if (enableLogging)
            Debug.LogWarning(message);
    }

    public static void LogException(System.Exception exception)
    {
        if (enableLogging)
            Debug.LogException(exception);
    }

    // Saves the logging preference state
    public static void SavePreferences()
    {
        PlayerPrefs.SetInt("EnableDDebugLogging", enableLogging ? 1 : 0);
        PlayerPrefs.Save();
    }
}