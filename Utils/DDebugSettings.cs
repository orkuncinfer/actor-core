using System;
using UnityEditor;
using UnityEngine;

public static class DDebugSettings
{
#if UNITY_EDITOR
    [SettingsProvider]
    public static SettingsProvider CreateMyCustomSettingsProvider()
    {
        var provider = new SettingsProvider("Preferences/DDebug", SettingsScope.User)
        {
            label = "DDebug Settings",
            guiHandler = (searchContext) =>
            {
                var newLoggingEnabled = EditorGUILayout.Toggle("Enable DDebug Logging", DDebug.enableLogging);
                if (newLoggingEnabled != DDebug.enableLogging)
                {
                    DDebug.enableLogging = newLoggingEnabled;
                    DDebug.SavePreferences();
                }

                foreach(EDebugType debugType in Enum.GetValues(typeof(EDebugType)))
                {
                    var newLoggingEnabledType = EditorGUILayout.Toggle("Enable " + debugType + " Logging", DDebug.enableLogging);
                    if (newLoggingEnabledType != DDebug.enableLogging)
                    {
                        
                    }
                }
            },

            // Optionally provide keywords to improve the searchability of your settings.
            keywords = new System.Collections.Generic.HashSet<string>(new[] { "logging", "debug", "DDebug" })
        };

        return provider;
    }
#endif
   
}