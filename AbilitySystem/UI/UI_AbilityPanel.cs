using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class UI_AbilityPanel : MonoBehaviour
{
    [SerializeField] private DataGetter<Data_Player> _playerData;
    [SerializeField] private GameObject _abilityElementPrefab;

    [SerializeField] private GameObject _abilityElementsContainer;
    private void Start()
    {
        _playerData.GetData();
        if (_playerData.Data.AbilityController.Abilities.Count > 0)
        {
            GenerateAbilityElements();
        }
    }

    private void OnAbilityControllerInitialized()
    {
        if (_playerData.Data.AbilityController.Abilities.Count > 0)
        {
            GenerateAbilityElements();
        }

        _playerData.Data.AbilityController.onInitialized += OnAbilityControllerInitialized;
    }

    [Button]
    private void GenerateAbilityElements()
    {
        for (int i = 0; i < _playerData.Data.AbilityController.AbilityDefinitions.Count; i++)
        {
            AbilityDefinition abilityDefinition = _playerData.Data.AbilityController.AbilityDefinitions[i];
            GameObject instance = Instantiate(_abilityElementPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(_abilityElementsContainer.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            UI_AbilityPanelElement abilityElement = instance.GetComponent<UI_AbilityPanelElement>();
            abilityElement.AbilityController = _playerData.Data.AbilityController;
            abilityElement.AbilityDefinition = _playerData.Data.AbilityController.AbilityDefinitions[i];
        }
    }
}
