using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class UI_StatPanel : MonoBehaviour
{
    [SerializeField] private DataGetter<Data_Player> _playerData;
    [SerializeField] private GameObject _statElementPrefab;

    [SerializeField] private GameObject _statElementsContainer;

  
    private void Start()
    {
        _playerData.GetData();
        if (_playerData.Data.StatController.PrimaryStatList.Count > 0)
        {
            GenerateAbilityElements();
        }
    }

    [Button]
    private void GenerateAbilityElements()
    {
        for (int i = 0; i < _playerData.Data.StatController.PrimaryStatList.Count; i++)
        {
            GameObject instance = Instantiate(_statElementPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(_statElementsContainer.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            UI_StatPanelElement abilityElement = instance.GetComponent<UI_StatPanelElement>();
            abilityElement.ThisStat = _playerData.Data.StatController.PrimaryStatList[i];
            abilityElement.StatController = _playerData.Data.StatController;
        }
    }
}