using System;
using System.Collections.Generic;
using System.Text;
using ECM2;
using StatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Attribute = StatSystem.Attribute;

public class FishnetDebugs : MonoBehaviour
{
    [SerializeField] private Transform _root;

    [SerializeField] private GameObject _holder;
    [SerializeField] private TMP_Text _velocityText;
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _attributesText;
    [SerializeField] private TMP_Text _effectsText;
    [SerializeField] private TMP_Text _gameplayTags;

    private ActorStateMachine[] _stateMachines;
    private GenericStateMachine[] _genericStateMachines;

     private CharacterMovement _movement;
    private StatController _statController;
    private GameplayEffectController _effectController;
    private Actor _actor;

    private readonly StringBuilder _statesBuilder = new StringBuilder(256);
    private readonly StringBuilder _attributesBuilder = new StringBuilder(768);
    private readonly StringBuilder _effectsBuilder = new StringBuilder(256);
    private readonly StringBuilder _gameplayTagsBuilder = new StringBuilder(256);

    private static readonly string ColorHeader = "#FFA500"; // Orange
    private static readonly string ColorKey = "#00FFFF";    // Cyan
    private static readonly string ColorValue = "#90EE90";  // Light Green

    private void Start()
    {
        _actor = _root.GetComponent<Actor>();
        _stateMachines = _root.GetComponentsInChildren<ActorStateMachine>(true);
        _genericStateMachines = _root.GetComponentsInChildren<GenericStateMachine>(true);
        _statController = _root.GetComponentInChildren<StatController>();
        _effectController = _root.GetComponentInChildren<GameplayEffectController>();
        if (_root.TryGetComponent(out CharacterMovement mvm))
        {
            _movement = mvm;
        }
    }

    private void Update()
    {
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.fKey.wasPressedThisFrame)
        {
            _holder.SetActive(!_holder.activeSelf);
        }
       
        
        if(_movement)
            _velocityText.text = $"<color={ColorHeader}>Velocity</color> : <color={ColorValue}>{_movement.velocity}</color>";

        UpdateStates();
        UpdateAttributes();
        UpdateEffects();
        UpdateGameplayTags();
    }

    private void UpdateGameplayTags()
    {
        var gameplayTags = _actor.GameplayTags.GetTags();
        
        _gameplayTagsBuilder.Clear();
        _gameplayTagsBuilder.AppendLine($"<color={ColorHeader}>Gameplay Tags</color> :");
        foreach (var gtag in gameplayTags)
        {
            _gameplayTagsBuilder.Append("<color=").Append(ColorKey).Append(">")
                                .Append(gtag.FullTag).Append("</color>")
                                .AppendLine();
        }
        _gameplayTags.text = _gameplayTagsBuilder.ToString();
    }

    private void UpdateStates()
    {
        _statesBuilder.Clear();

        for (int i = 0; i < _genericStateMachines.Length; i++)
        {
            var sm = _genericStateMachines[i];
            if (sm.CurrentState == null) continue;

            _statesBuilder.Append("<color=").Append(ColorKey).Append(">")
                          .Append(sm.Description).Append("</color> : <color=")
                          .Append(ColorValue).Append(">")
                          .Append(sm.CurrentState.gameObject.name)
                          .AppendLine("</color>");
        }

        for (int i = 0; i < _stateMachines.Length; i++)
        {
            var sm = _stateMachines[i];
            if (sm.CurrentState == null) continue;

            _statesBuilder.Append("<color=").Append(ColorKey).Append(">")
                          .Append(sm.GetType().Name).Append("</color> : <color=")
                          .Append(ColorValue).Append(">")
                          .Append(sm.CurrentState.gameObject.name)
                          .AppendLine("</color>");
        }

        _stateText.text = _statesBuilder.ToString();
    }

    private void UpdateAttributes()
    {
        _attributesBuilder.Clear();

        // === Attributes ===
        _attributesBuilder.AppendLine($"<color={ColorHeader}>Attributes</color>:");
        foreach (var kvp in _statController.Stats)
        {
            if (kvp.Value is Attribute attr)
            {
                _attributesBuilder.Append("<color=").Append(ColorKey).Append(">")
                                  .Append(kvp.Key).Append("</color> : <color=")
                                  .Append(ColorValue).Append(">")
                                  .Append(attr.CurrentValue.ToString("F2")).AppendLine("</color>");
            }
        }

        // === Primary Stats ===
        _attributesBuilder.AppendLine($"\n<color={ColorHeader}>Primary Stats</color>:");
        foreach (var kvp in _statController.Stats)
        {
            if (kvp.Value is PrimaryStat ps)
            {
                _attributesBuilder.Append("<color=").Append(ColorKey).Append(">")
                                  .Append(kvp.Key).Append("</color> : <color=")
                                  .Append(ColorValue).Append(">")
                                  .Append(ps.Value.ToString("F2")).AppendLine("</color>");
            }
        }

        // === Other Stats ===
        _attributesBuilder.AppendLine($"\n<color={ColorHeader}>Stats</color>:");
        foreach (var kvp in _statController.Stats)
        {
            if (kvp.Value is Attribute || kvp.Value is PrimaryStat) continue;

            _attributesBuilder.Append("<color=").Append(ColorKey).Append(">")
                              .Append(kvp.Key).Append("</color> : <color=")
                              .Append(ColorValue).Append(">")
                              .Append(kvp.Value.Value.ToString("F2")).AppendLine("</color>");
        }

        _attributesText.text = _attributesBuilder.ToString();
    }

    private void UpdateEffects()
    {
        _effectsBuilder.Clear();
        _effectsBuilder.AppendLine($"<color={ColorHeader}>Effects</color> :");

        foreach (var effect in _effectController.ActiveEffects)
        {
            _effectsBuilder.Append("<color=").Append(ColorKey).Append(">")
                           .Append(effect.Definition.name).Append("</color>")
                           .Append(" - <color=").Append(ColorValue).Append(">")
                           .Append(effect.RemainingDuration.ToString("F1"))
                           .AppendLine("s</color>");
        }

        _effectsText.text = _effectsBuilder.ToString();
    }
}
