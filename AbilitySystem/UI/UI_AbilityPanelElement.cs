using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_AbilityPanelElement : MonoBehaviour
{
    public PlayerAbilityController AbilityController;
    //[SerializeField] private DataGetter<PlayerData> _playerData;
    public AbilityDefinition AbilityDefinition;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _abilityDescriptionText;

    [SerializeField] private Button _levelUpButton;
    private void Start()
    {
        _nameText.text = AbilityDefinition.Title;
        _levelText.text = AbilityController.Abilities[AbilityDefinition.name].level.ToString();
        _abilityDescriptionText.text = AbilityController.Abilities[AbilityDefinition.name].ToString();
        OnAbilityPointsChange(AbilityController.AbilityPoints);
        AbilityController.onAbilityPointsChanged += OnAbilityPointsChange;
        _levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);

        AbilityController.Abilities[AbilityDefinition.name].levelChanged += OnAbilityLevelChanged;
    }

    private void OnAbilityLevelChanged()
    {
        _levelText.text = AbilityController.Abilities[AbilityDefinition.name].level.ToString();
    }

    private void OnLevelUpButtonClicked()
    {
        if (AbilityController.AbilityPoints > 0 && AbilityController.Abilities[AbilityDefinition.name].level< AbilityDefinition.MaxLevel)
        {
            AbilityController.AbilityPoints--;
            AbilityController.Abilities[AbilityDefinition.name].level++;
        }
    }

    private void OnAbilityPointsChange(int newValue)
    {
        if (newValue > 0)
        {
            _levelUpButton.gameObject.SetActive(true);
        }
        else
        {
            _levelUpButton.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        AbilityController.onAbilityPointsChanged -= OnAbilityPointsChange;
        _levelUpButton.onClick.RemoveListener(OnLevelUpButtonClicked);
    }
}
