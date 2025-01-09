using System.Collections.Generic;

public static class ActorIDManager
{
    private static Dictionary<string, int> _nameCounts = new Dictionary<string, int>();

    public static string GetUniqueID(string baseName)
    {
        if (!_nameCounts.ContainsKey(baseName))
        {
            _nameCounts[baseName] = 1;
        }
        else
        {
            _nameCounts[baseName]++;
        }

        return baseName + "_" + _nameCounts[baseName].ToString();
    }
}