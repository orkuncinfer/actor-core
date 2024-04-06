using System;
using System.Collections;
using System.Collections.Generic;
using StatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatPanelElement : MonoBehaviour
{
    [HideInInspector]public PlayerStatController StatController;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _levelUpButton;

    public PrimaryStat ThisStat;

    private void Start()
    {
        _nameText.text = ThisStat.Definition.name;
        _levelText.text = ThisStat.Value.ToString();
        OnStatPointChanged();
        ThisStat.onValueChanged += OnStatLeveled;
        StatController.onStatPointsChanged += OnStatPointChanged;
        _levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
    }

    private void OnStatPointChanged()
    {
        if (StatController.StatPoints > 0)
        {
            _levelUpButton.gameObject.SetActive(true);
        }
        else
        {
            _levelUpButton.gameObject.SetActive(false);
        }
    }

    private void OnStatLeveled()
    {
        _levelText.text = ThisStat.Value.ToString();
    }

    private void OnDisable()
    {
        _levelUpButton.onClick.RemoveListener(OnLevelUpButtonClicked);
    }

    private void OnLevelUpButtonClicked()
    {
        if(StatController.StatPoints <= 0) return;
        ThisStat.Add(1);
        StatController.StatPoints--;
    }
}
