using System;
using UnityEngine;

public class Weapon_CrosshairStarter : MonoBehaviour
{
    [SerializeField] private PanelActor _crossHairPanel;

    private Equippable _equippable;

    private PanelActor _panelInstance;
    private bool _isPlayer;

    private CrosshairController _crosshairController;
    private GunFireComponent _fireComponent;
    private void Awake()
    {
        _equippable = GetComponent<Equippable>();
        _fireComponent = GetComponent<GunFireComponent>();
    }

    private void OnEnable()
    {
        _equippable.onEquipped += OnEquipped;
        _equippable.onUnequipped += OnUnequipped;
    }

    private void OnDisable()
    {
        _equippable.onEquipped -= OnEquipped;
        _equippable.onUnequipped -= OnUnequipped;
    }

    private void OnUnequipped(ActorBase obj)
    {
        if(!_isPlayer) return;
        if(!_panelInstance) return;
        PoolManager.ReleaseObject(_panelInstance.gameObject);
        _panelInstance = null;
        _crosshairController = null;
    }

    private void OnEquipped(ActorBase obj)
    {
        if(_panelInstance != null) return;
        if (_equippable.Owner.ContainsTag("Player"))
        {
            _isPlayer = true;
        }
        
        if(!_isPlayer) return;
        
        GameObject panelInstance = CanvasManager.Instance.ShowAdditive(_crossHairPanel);
        _panelInstance = panelInstance.GetComponent<PanelActor>();
        _crosshairController = panelInstance.GetComponentInChildren<CrosshairController>();
    }

    private void Update()
    {
        if (_crosshairController)
        {
            _crosshairController.SetSpread(_fireComponent.GetSpreadValue());
        }
    }
}
