using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDropManager : MonoBehaviour
{
    public MonsterListDefinition MonsterListDefinition;
    public float MonsterListFetchFrequency = 10f;
    
    private void Start()
    {
        StartCoroutine(FetchMonsterList());
    }
    
    private IEnumerator FetchMonsterList()
    {
        while (true)
        {
            MonsterListDefinition.FetchData();
            yield return new WaitForSeconds(MonsterListFetchFrequency);
        }
    }
    [Button]
    public void KilledMonster(string monsterID)
    {
        MonsterData monsterData = MonsterListDefinition.MonsterDataDictionary[monsterID];

        foreach (ItemDropData possibleDrop in monsterData.PossibleDrops)
        {
            float dropChance = possibleDrop.DropChance; // 0.2f
            if (Random.Range(0f, 1f) <= dropChance)
            {
                Debug.Log("Item dropped: " + possibleDrop.ItemId);

                ItemDefinition item = InventoryUtils.FindItemWithId(possibleDrop.ItemId);
                if (item is UniqueItemDefinition uniqueItemDefinition)
                {
                    Debug.Log("item is unique");
                }
            }
            else
            {
               
            }
        }
    }
}

[Serializable]
public class GeneratedItemResult
{
    public string itemId;
    public string uniqueItemId;
    public List
        <ItemModifierResult> modifiers;
    public int rarity;
}

[Serializable]
public class ItemModifierResult
{
    public string modifierId;
    public int rndValue;
}
