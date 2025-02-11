using System;
using LevelSystem;
using UnityEngine;

[RequireComponent(typeof(ILevelable))]
public class PlayerAbilityController : AbilityController
{
    protected ILevelable _levelable;
    protected int _abilityPoints;
    public event Action<int> onAbilityPointsChanged;

    public int AbilityPoints
    {
        get => _abilityPoints;
        internal set
        {
            _abilityPoints = value;
            onAbilityPointsChanged?.Invoke(_abilityPoints);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AbilityPoints++;
        }
        if (Input.GetKey(KeyCode.F))
        {
            if (TestAbility)
            {
                AddAbilityIfNotHave(TestAbility);
                TryActiveAbilityWithDefinition(TestAbility);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _levelable = GetComponent<ILevelable>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _levelable.initialized += OnLevelableInitialized;
        _levelable.willUninitialize += UnregisterEvents;
        if (_levelable.isInitialized)
        {
            OnLevelableInitialized();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _levelable.initialized -= OnLevelableInitialized;
        _levelable.willUninitialize -= UnregisterEvents;
        if (_levelable.isInitialized)
        {
            UnregisterEvents();
        }
    }

    private void OnLevelableInitialized()
    {
        RegisterEvents();
    }

    private void UnregisterEvents()
    {
        _levelable.levelChanged -= OnLevelChanged;
    }

    private void RegisterEvents()
    {
        _levelable.levelChanged += OnLevelChanged;
    }

    private void OnLevelChanged()
    {
        AbilityPoints += 3;
    }

    public override object data
    {
        get
        {
            return new PlayerAbilityControllerData(base.data as AbilityControllerData)
            {
                AbilityPoints = _abilityPoints
            };
        }
        
    }

    public override void Load(object data)
    {
        base.Load(data);
        PlayerAbilityControllerData playerAbilityControllerData = (PlayerAbilityControllerData) data;
        AbilityPoints = playerAbilityControllerData.AbilityPoints;
        onAbilityPointsChanged?.Invoke(AbilityPoints);
    }
    [Serializable]
    protected class PlayerAbilityControllerData : AbilityControllerData
    {
        public int AbilityPoints;

        public PlayerAbilityControllerData(AbilityControllerData abilityControllerData)
        {
            Abilities = abilityControllerData.Abilities;
        }
    }
}