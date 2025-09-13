using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class UI_AbilityPanel : MonoBehaviour
{
    [FormerlySerializedAs("_playerData")] [SerializeField] private DSGetter<Data_Player> _playerDS;
    [SerializeField] private GameObject _abilityElementPrefab;

    [SerializeField] private GameObject _abilityElementsContainer;
    private void Start()
    {
        _playerDS.GetData();
        if (_playerDS.Data.AbilityController.Abilities.Count > 0)
        {
            GenerateAbilityElements();
        }
    }

    private void OnAbilityControllerInitialized()
    {
        if (_playerDS.Data.AbilityController.Abilities.Count > 0)
        {
            GenerateAbilityElements();
        }

        _playerDS.Data.AbilityController.onInitialized += OnAbilityControllerInitialized;
    }

    [Button]
    private void GenerateAbilityElements()
    {
        for (int i = 0; i < _playerDS.Data.AbilityController.AbilityDefinitions.Count; i++)
        {
            AbilityDefinition abilityDefinition = _playerDS.Data.AbilityController.AbilityDefinitions[i];
            GameObject instance = Instantiate(_abilityElementPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(_abilityElementsContainer.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            UI_AbilityPanelElement abilityElement = instance.GetComponent<UI_AbilityPanelElement>();
            abilityElement.AbilityController = _playerDS.Data.AbilityController;
            abilityElement.AbilityDefinition = _playerDS.Data.AbilityController.AbilityDefinitions[i];
        }
    }
}
