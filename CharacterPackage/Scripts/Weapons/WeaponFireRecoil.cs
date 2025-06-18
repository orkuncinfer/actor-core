using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponFireRecoil : MonoBehaviour
{
    [MinMaxSlider(0,4)]
    [SerializeField] private Vector2 _magnitudeRange;
    private Equippable _equippable;
    private GunFireComponent _gunFireComponent;
    
    private Recoil _recoil;
    private void OnEnable()
    {
        _equippable = GetComponent<Equippable>();
        _gunFireComponent = GetComponent<GunFireComponent>();
        _gunFireComponent.onFire += OnFire;
    }

    private void OnDisable()
    {
        _gunFireComponent.onFire -= OnFire;
    }

    private void OnFire(Gun target)
    {
        if (_equippable.IsEquipped)
        {
            if (_recoil == null)
            {
                _recoil = _equippable.Owner.GetComponentInChildren<Recoil>();
            }
            float randomMagnitude = UnityEngine.Random.Range(_magnitudeRange.x, _magnitudeRange.y);
            if(_recoil)_recoil.Fire(randomMagnitude);
        }
    }
}
