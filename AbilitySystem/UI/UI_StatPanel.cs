using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;
using UnityEngine.Serialization;

public class UI_StatPanel : MonoBehaviour
{
    [SerializeField] private GameObject _statElementPrefab;

    [SerializeField] private GameObject _statElementsContainer;
    [SerializeField] private StatController _statController;
  
    private void Start()
    {
        if (_statController.PrimaryStatList.Count > 0)
        {
            GenerateAbilityElements();
        }
    }

    [Button]
    private void GenerateAbilityElements()
    {
        for (int i = 0; i < _statController.PrimaryStatList.Count; i++)
        {
            GameObject instance = Instantiate(_statElementPrefab, Vector3.zero, Quaternion.identity);
            instance.transform.SetParent(_statElementsContainer.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            UI_StatPanelElement abilityElement = instance.GetComponent<UI_StatPanelElement>();
            abilityElement.ThisStat = _statController.PrimaryStatList[i];
            abilityElement.StatController = (PlayerStatController)_statController;
        }
    }
}