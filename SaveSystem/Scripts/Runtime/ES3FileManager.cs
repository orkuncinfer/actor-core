using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem.Scripts.Runtime
{
    /// <summary>
    /// File manager implementation using Easy Save 3 for robust cross-platform data persistence.
    /// Provides improved performance, automatic serialization, and built-in encryption support.
    /// </summary>
    public class ES3FileManager
    {
        private const string DefaultFileExtension = ".es3";
        
        
        public static void SaveToBinaryFile(string path, Dictionary<string, object> data, ES3Settings settings = null)
        {
            try
            {
                string filePath = EnsureFileExtension(path);
                
                // Use provided settings or create default ones
                ES3Settings saveSettings = settings ?? CreateDefaultSettings();
                
                // Clear existing file to ensure clean save
                if (ES3.FileExists(filePath))
                {
                    ES3.DeleteFile(filePath, saveSettings);
                }
                
                // Save each key-value pair individually for better error handling
                foreach (KeyValuePair<string, object> kvp in data)
                {
                    ES3.Save(kvp.Key, kvp.Value, filePath, saveSettings);
                }
                
                Debug.Log($"Successfully saved {data.Count} entries to {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save file at {path}: {e.Message}\nStack Trace: {e.StackTrace}");
            }
        }
        
        public static void LoadFromBinaryFile(string path, out Dictionary<string, object> data, ES3Settings settings = null)
        {
            data = new Dictionary<string, object>();
            
            try
            {
                string filePath = EnsureFileExtension(path);
                
                // Use provided settings or create default ones
                ES3Settings loadSettings = settings ?? CreateDefaultSettings();
                
                // Check if file exists
                if (!ES3.FileExists(filePath))
                {
                    Debug.LogWarning($"File does not exist at {filePath}. Returning empty dictionary.");
                    return;
                }
                
                // Get all keys from the file
                string[] keys = ES3.GetKeys(filePath, loadSettings);
                
                if (keys.Length == 0)
                {
                    Debug.LogWarning($"File at {filePath} exists but contains no data.");
                    return;
                }
                
                // Load each key-value pair
                foreach (string key in keys)
                {
                    try
                    {
                        object value = ES3.Load<object>(key, filePath, loadSettings);
                        data[key] = value;
                    }
                    catch (Exception keyException)
                    {
                        Debug.LogWarning($"Failed to load key '{key}' from {filePath}: {keyException.Message}");
                    }
                }
                
                Debug.Log($"Successfully loaded {data.Count} entries from {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load file at {path}: {e.Message}\nStack Trace: {e.StackTrace}");
                data = new Dictionary<string, object>();
            }
        }
        
        public static T LoadValue<T>(string key, string path, T defaultValue = default(T), ES3Settings settings = null)
        {
            try
            {
                string filePath = EnsureFileExtension(path);
                ES3Settings loadSettings = settings ?? CreateDefaultSettings();
                
                if (!ES3.FileExists(filePath))
                {
                    Debug.LogWarning($"File does not exist at {filePath}. Returning default value for key '{key}'.");
                    return defaultValue;
                }
                
                return ES3.Load<T>(key, filePath, defaultValue, loadSettings);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load key '{key}' from {path}: {e.Message}. Returning default value.");
                return defaultValue;
            }
        }
        
        public static void SaveValue<T>(string key, T value, string path, ES3Settings settings = null)
        {
            try
            {
                string filePath = EnsureFileExtension(path);
                ES3Settings saveSettings = settings ?? CreateDefaultSettings();
                
                ES3.Save<T>(key, value, filePath, saveSettings);
                Debug.Log($"Successfully saved key '{key}' to {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save key '{key}' to {path}: {e.Message}");
            }
        }
        
        public static bool FileExists(string path, ES3Settings settings = null)
        {
            string filePath = EnsureFileExtension(path);
            ES3Settings checkSettings = settings ?? CreateDefaultSettings();
            return ES3.FileExists(filePath, checkSettings);
        }
        
        public static bool DeleteFile(string path, ES3Settings settings = null)
        {
            try
            {
                string filePath = EnsureFileExtension(path);
                ES3Settings deleteSettings = settings ?? CreateDefaultSettings();
                
                if (!ES3.FileExists(filePath, deleteSettings))
                {
                    Debug.LogWarning($"Cannot delete file at {filePath}: File does not exist.");
                    return false;
                }
                
                ES3.DeleteFile(filePath, deleteSettings);
                Debug.Log($"Successfully deleted file at {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete file at {path}: {e.Message}");
                return false;
            }
        }
        
        private static ES3Settings CreateDefaultSettings()
        {
            return new ES3Settings
            {
                location = ES3.Location.File,
                directory = ES3.Directory.PersistentDataPath,
                encryptionType = ES3.EncryptionType.None, // Change to AES for encryption
                compressionType = ES3.CompressionType.None, // Enable if file size is a concern
                format = ES3.Format.JSON // JSON is more readable for debugging
            };
        }
        
        private static string EnsureFileExtension(string path)
        {
            return path.EndsWith(DefaultFileExtension) ? path : path + DefaultFileExtension;
        }
        
        public static string[] GetKeys(string path, ES3Settings settings = null)
        {
            try
            {
                string filePath = EnsureFileExtension(path);
                ES3Settings loadSettings = settings ?? CreateDefaultSettings();
                
                if (!ES3.FileExists(filePath, loadSettings))
                {
                    Debug.LogWarning($"File does not exist at {filePath}. Returning empty key array.");
                    return new string[0];
                }
                
                return ES3.GetKeys(filePath, loadSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get keys from {path}: {e.Message}");
                return new string[0];
            }
        }
    }
}