using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicItemDisplayer : MonoBehaviour
{
    public ItemBaseDefinition ItemDefinition;
    public Image ItemIcon;
    public TextMeshProUGUI ItemNameText;
    public TextMeshProUGUI ItemDescriptionText;
    public TextMeshProUGUI ItemCountText;

    public int ItemCount;
    private void OnEnable()
    {
        if(ItemDefinition == null) return;
        ItemIcon.sprite = ItemDefinition.Icon;
        ItemNameText.text = ItemDefinition.ItemName;
        ItemDescriptionText.text = ItemDefinition.Description;
    }

    public void DisplayItem(ItemBaseDefinition definition)
    {
        ItemDefinition = definition;
        if(ItemDefinition.Icon)ItemIcon.sprite = ItemDefinition.Icon;
        ItemNameText.text = ItemDefinition.ItemName;
        ItemDescriptionText.text = ItemDefinition.Description;
    }
    public void SetItemCount(int count)
    {
        ItemCount = count;
        if(ItemCountText) ItemCountText.text = ItemCount.ToString();
    }
    public void SetName(string name)
    {
        ItemNameText.text = name;
    }
}
