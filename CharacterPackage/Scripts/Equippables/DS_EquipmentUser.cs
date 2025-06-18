using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
[System.Serializable]
public class DS_EquipmentUser : Data
{
    [SerializeField][HideInPlayMode]
    private GameObject _equipmentPrefab;
    public event Action<GameObject,GameObject> onEquipmentPrefabChanged;
    [ShowInInspector][HideInEditorMode]
    public GameObject EquipmentPrefab
    {
        get => _equipmentPrefab;
        set
        {
            GameObject oldValue = _equipmentPrefab;
            bool isChanged = _equipmentPrefab != value;
            _equipmentPrefab = value;
            if (isChanged)
            {
                onEquipmentPrefabChanged?.Invoke(oldValue,value);
                EquipmentPrefabChanged(oldValue,value);
            }
        }
    }
    
    public event Action<GameObject> onEquipmentInstanceChanged;
    [SerializeField][ReadOnly] private GameObject _equipmentInstance;
    public GameObject EquipmentInstance
    {
        get => _equipmentInstance;
        set
        {
            if (_equipmentInstance != value)
            {
                _equipmentInstance = value;
                onEquipmentInstanceChanged?.Invoke(_equipmentInstance);
            }
        }
    }

    public event Action<Equippable> onEquipmentDropped; 

    [SerializeField] private GameObject _itemToEquip;
    public GameObject ItemToEquip
    {
        get => _itemToEquip;
        set => _itemToEquip = value;
    }
    
    [SerializeField] private string _socketName;

    public string SocketName
    {
        get => _socketName;
        set => _socketName = value;
    }

    public float LerpSpeed;

    public List<Equippable> _equippedInstances; // stays here unless it is dropped

    public Dictionary<int, Equippable> _equippedSlots = new Dictionary<int, Equippable>();
    public event Action onEquippedListChanged;

    private bool _equippedAsWorldInstance;//??

    protected override void OnActorStarted()
    {
        base.OnActorStarted();
        EquipCurrentPrefab();
    }

    private void EquipmentPrefabChanged(GameObject oldPrefab, GameObject newPrefab)
    {
        if (_equippedAsWorldInstance)
        {
            _equippedAsWorldInstance = false;
            return;
        }
        ReleaseInstance();
        EquipCurrentPrefab();
    }
    
    
    private void EquipCurrentPrefab()
    {
        if(EquipmentPrefab == null) return;
        ReleaseInstance();
        if (_equipmentPrefab.gameObject.activeInHierarchy) // if exist in world
        {
            EquipmentInstance = EquipmentPrefab;
        }
        else
        {
            EquipmentInstance = PoolManager.SpawnObject(EquipmentPrefab, Vector3.zero, Quaternion.identity);
        }
        
        if (EquipmentInstance.TryGetComponent(out Equippable weapon))
        {
            weapon.Owner = OwnerActor as Actor;
            weapon.OnEquip(weapon.Owner);
            if(_equippedInstances.Contains(weapon) == false)_equippedInstances.Add(weapon);
            onEquippedListChanged?.Invoke();
        }
        
        
        SetInstanceTransform();
    }
    public void UnequipCurrent(bool destroyInstance = true)
    {
        if (_equipmentInstance != null)
        {
            if (EquipmentInstance.TryGetComponent(out Equippable weapon))
            {
                weapon.OnUnequip(OwnerActor);
            }
            EquipmentInstance = null;
            if(destroyInstance)
                ReleaseInstance();
        }
    }
    [Button]
    public void DropCurrent()
    {
        Debug.Log("tried to drop");
        if (_equipmentInstance != null)
        {
            if (EquipmentInstance.TryGetComponent(out Equippable weapon))
            {
                Debug.Log("tried to drop1");
                onEquipmentDropped?.Invoke(weapon);
                weapon.OnUnequip(OwnerActor);
                PoolManager.ReleaseObject(weapon.gameObject);
                _equippedInstances.Remove(weapon);
                onEquippedListChanged?.Invoke();
            }
            EquipmentInstance = null;
        }
    }
    public void DropEquippable(Equippable equippable)
    {
        equippable.OnUnequip(OwnerActor);
        _equippedInstances.Remove(equippable);
        onEquippedListChanged?.Invoke();
        if (_equipmentInstance != null)
        {
            if (_equipmentInstance.TryGetComponent(out Equippable equippable2))
            {
                if (equippable == equippable2)
                {
                    _equipmentInstance = null;
                }
            }
        }
    }
    public void EquipWorldInstance(GameObject equipmentInstance,string socketName = "")
    {
        _equippedAsWorldInstance = true;
        ReleaseInstance();
        
        EquipmentPrefab = equipmentInstance;
        EquipmentInstance = equipmentInstance;
        if(socketName != "") SocketName = socketName;
        
        if (EquipmentInstance.TryGetComponent(out Equippable weapon))
        {
            weapon.Owner = OwnerActor as Actor;
            weapon.OnEquip(weapon.Owner);
            if(_equippedInstances.Contains(weapon) == false)_equippedInstances.Add(weapon);
            onEquippedListChanged?.Invoke();
        }
        
        SetInstanceTransform();
        equipmentInstance.SetActive(true);
    }

    public void RegisterWithoutEquip(GameObject equipmentInstance,string socketName = "")
    {
        if (equipmentInstance.TryGetComponent(out Equippable weapon))
        {
            weapon.Owner = OwnerActor as Actor;
            _equippedInstances.Add(weapon);
            onEquippedListChanged?.Invoke();
        }
    }
    
    private void SetInstanceTransform()
    {
        if (EquipmentInstance == null) return;
        EquipmentInstance.transform.SetParent(OwnerActor.GetSocket(SocketName),true);
        if(LerpSpeed > 0)
        {
            EquipmentInstance.transform.DOLocalRotate(Vector3.zero, LerpSpeed);
            EquipmentInstance.transform.DOLocalMove(Vector3.zero, LerpSpeed).OnComplete(() =>
            {
                EquipmentInstance.transform.SetParent(OwnerActor.GetSocket(SocketName));
                EquipmentInstance.transform.localPosition = Vector3.zero;
                EquipmentInstance.transform.localEulerAngles = Vector3.zero;
                EquipmentInstance.transform.localScale = Vector3.one;
                LerpSpeed = 0;
            });
            return;
        }
        
        EquipmentInstance.transform.localPosition = Vector3.zero;
        EquipmentInstance.transform.localEulerAngles = Vector3.zero;
        EquipmentInstance.transform.localScale = Vector3.one;
    }

    [Button]
    public void EquipToSlot(int index, GameObject equippable)
    {
        
        if (_equippedSlots.ContainsKey(index))
        {
            if (_equippedSlots[index] != null)
            {
                _equippedSlots[index].OnUnequip(OwnerActor);
                PoolManager.ReleaseObject(_equippedSlots[index].gameObject);
            }
            _equippedSlots.Remove(index);
            
        }
        if(equippable == null) return;
        GameObject equippableInstance = PoolManager.SpawnObject(equippable, Vector3.zero, Quaternion.identity);
        if (equippableInstance.TryGetComponent(out Equippable weapon))
        {
            weapon.Owner = OwnerActor as Actor;
            weapon.OnEquip(weapon.Owner);
            _equippedSlots.Add(index,weapon);
            onEquippedListChanged?.Invoke();
        }

        EquipmentInstance = _equippedSlots[index].gameObject;
        SocketName = weapon.EquipSocketName;
        SetInstanceTransform();
    }

    private void ReleaseInstance()
    {
        if (EquipmentInstance != null)
        {
            PoolManager.ReleaseObject(EquipmentInstance);
            EquipmentInstance = null;
        }
    }
}
