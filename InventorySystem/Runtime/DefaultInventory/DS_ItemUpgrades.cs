using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class DS_ItemUpgrades : Data
{
    [ShowInInspector]public Dictionary<string,int> UpgradeLevels = new Dictionary<string, int>();
    
    
    public void SetUpgradeLevel(string itemId, int level)
    {
        if (UpgradeLevels.ContainsKey(itemId))
        {
            UpgradeLevels[itemId] = level;
        }
        else
        {
            UpgradeLevels.Add(itemId, level);
        }
    }
    [Button]
    public void UpgradeItem(string itemId)
    {
        if (UpgradeLevels.ContainsKey(itemId))
        {
            UpgradeLevels[itemId]++;
        }
        else
        {
            UpgradeLevels.Add(itemId, 1);
        }
    }
    
    public int GetUpgradeLevel(string itemId)
    {
        if (UpgradeLevels.ContainsKey(itemId))
        {
            return UpgradeLevels[itemId];
        }
        return 0;
    }
    
}