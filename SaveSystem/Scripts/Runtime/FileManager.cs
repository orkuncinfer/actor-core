using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SaveSystem.Scripts.Runtime
{
    public class FileManager
    {
        public static void SaveToBinaryFile(string path, Dictionary<string, object> data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // Normalize the path
            string normalizedPath = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(normalizedPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            FileStream file = File.Open(normalizedPath, FileMode.Create);
            try
            {
                formatter.Serialize(file, data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to save file at {normalizedPath}: {e.Message}");
            }
            finally
            {
                file.Close();
            }
        }
        
       
       public static void LoadFromBinaryFile(string path, out Dictionary<string, object> data)
       {
           BinaryFormatter formatter = new BinaryFormatter();
           // Normalize path to ensure consistency in file separators.
           string normalizedPath = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

           // Check if the file exists before attempting to open it.
           if (!File.Exists(normalizedPath))
           {
               Debug.LogWarning($"File does not exist at {normalizedPath}");
               data = new Dictionary<string, object>();
               return;
           }

           FileStream file = File.Open(normalizedPath, FileMode.Open);

           try
           {
               data = formatter.Deserialize(file) as Dictionary<string, object>;
           }
           catch (Exception e)
           {
               Debug.LogWarning($"Failed to load file at {normalizedPath}: {e.Message}");
               data = new Dictionary<string, object>();
           }
           finally
           {
               file.Close();
           }
       }
    }
}