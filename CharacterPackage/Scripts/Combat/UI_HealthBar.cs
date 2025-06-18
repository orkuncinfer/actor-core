using System.Collections;
using StatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages health bar UI with instant health fill updates and delayed damage fill animations.
/// Follows component-based design principles with configurable performance settings.
/// </summary>
public class UI_HealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _maxHealthText;
    [SerializeField] private Image _damageFill;
    [SerializeField] private Image _healthFill;

    [Header("Animation Configuration")]
    [SerializeField] private float _damageAnimationSpeed = 2.0f;
    [SerializeField] private float _damageAnimationDelay = 0.5f;
    [SerializeField] private AnimationCurve _damageAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Performance Settings")]
    [SerializeField] private bool _enableSmoothing = true;
    [SerializeField] private float _minimumFillDelta = 0.001f; // Prevents unnecessary micro-updates

    // State management
    private Actor _previousTarget;
    private Attribute _healthAttribute;
    
    // Animation state
    private Coroutine _damageAnimationCoroutine;
    private float _targetHealthFillAmount;
    private float _currentDamageFillAmount;
    private bool _isAnimating;

    // Performance optimization - cached values
    private float _lastHealthPercentage = -1f;
    private float _lastMaxHealth = -1f;

    #region Unity Lifecycle

    private void Awake()
    {
        ValidateComponents();
        InitializeDefaultValues();
    }

    private void OnDestroy()
    {
        CleanupTarget();
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Sets the target actor and initializes the health bar display.
    /// </summary>
    /// <param name="target">The actor to monitor</param>
    /// <param name="name">Display name for the health bar</param>
    public void SetTarget(Actor target, string name)
    {
        if (target == null)
        {
            Debug.LogWarning("UI_HealthBar: Attempted to set null target");
            return;
        }

        CleanupTarget();
        InitializeTarget(target, name);
    }

    /// <summary>
    /// Clears the current target and resets the health bar.
    /// </summary>
    public void ClearTarget()
    {
        CleanupTarget();
        ResetHealthBar();
    }

    /// <summary>
    /// Forces an immediate update of all UI elements (useful for initialization).
    /// </summary>
    public void ForceUpdate()
    {
        if (_healthAttribute == null) return;

        UpdateHealthDisplay();
        UpdateFillAmounts(true);
    }

    #endregion

    #region Target Management

    private void InitializeTarget(Actor target, string name)
    {
        if (_nameText != null) 
            _nameText.text = name;

        var gasService = target.GetService<Service_GAS>();
        if (gasService == null)
        {
            Debug.LogError($"UI_HealthBar: Target '{name}' does not have Service_GAS component");
            return;
        }

        _healthAttribute = gasService.StatController.GetStat("Health") as Attribute;
        if (_healthAttribute == null)
        {
            Debug.LogError($"UI_HealthBar: Target '{name}' does not have 'Health' attribute");
            return;
        }

        // Subscribe to health events
        _healthAttribute.onStatValueChanged += OnMaxHealthChanged;
        _healthAttribute.onCurrentValueChanged += OnCurrentHealthChanged;

        _previousTarget = target;

        // Initialize display
        InitializeHealthDisplay();
    }

    private void CleanupTarget()
    {
        if (_healthAttribute != null)
        {
            _healthAttribute.onStatValueChanged -= OnMaxHealthChanged;
            _healthAttribute.onCurrentValueChanged -= OnCurrentHealthChanged;
        }

        if (_damageAnimationCoroutine != null)
        {
            StopCoroutine(_damageAnimationCoroutine);
            _damageAnimationCoroutine = null;
        }

        _healthAttribute = null;
        _previousTarget = null;
        _isAnimating = false;
    }

    #endregion

    #region Health Event Handlers

    private void OnMaxHealthChanged()
    {
        if (_healthAttribute == null) return;

        float newMaxHealth = _healthAttribute.Value;
        
        // Performance optimization - avoid unnecessary updates
        if (Mathf.Approximately(newMaxHealth, _lastMaxHealth)) return;
        
        _lastMaxHealth = newMaxHealth;
        
        if (_maxHealthText != null)
            _maxHealthText.text = newMaxHealth.ToString("F0");

        // Recalculate fill amounts when max health changes
        UpdateFillAmounts(false);
    }

    private void OnCurrentHealthChanged()
    {
        if (_healthAttribute == null) return;

        UpdateHealthDisplay();
        UpdateFillAmounts(false);
    }

    #endregion

    #region UI Updates

    private void UpdateHealthDisplay()
    {
        if (_healthText != null)
            _healthText.text = _healthAttribute.CurrentValue.ToString("F0");
    }

    private void InitializeHealthDisplay()
    {
        UpdateHealthDisplay();
        
        if (_maxHealthText != null)
            _maxHealthText.text = _healthAttribute.Value.ToString("F0");

        // Initialize both fills to current health percentage
        float healthPercentage = GetHealthPercentage();
        SetFillAmount(_healthFill, healthPercentage);
        SetFillAmount(_damageFill, healthPercentage);
        
        _currentDamageFillAmount = healthPercentage;
        _targetHealthFillAmount = healthPercentage;
        _lastHealthPercentage = healthPercentage;
    }

    private void UpdateFillAmounts(bool forceImmediate = false)
    {
        float newHealthPercentage = GetHealthPercentage();
        
        // Performance optimization - avoid unnecessary updates
        if (!forceImmediate && Mathf.Abs(newHealthPercentage - _lastHealthPercentage) < _minimumFillDelta)
            return;

        _lastHealthPercentage = newHealthPercentage;
        _targetHealthFillAmount = newHealthPercentage;

        // Update health fill immediately
        SetFillAmount(_healthFill, newHealthPercentage);

        // Handle damage fill animation
        if (newHealthPercentage < _currentDamageFillAmount)
        {
            // Health decreased - animate damage fill down
            StartDamageAnimation(newHealthPercentage, forceImmediate);
        }
        else if (newHealthPercentage > _currentDamageFillAmount)
        {
            // Health increased - update damage fill immediately
            SetFillAmount(_damageFill, newHealthPercentage);
            _currentDamageFillAmount = newHealthPercentage;
            
            // Stop any ongoing animation
            if (_damageAnimationCoroutine != null)
            {
                StopCoroutine(_damageAnimationCoroutine);
                _damageAnimationCoroutine = null;
                _isAnimating = false;
            }
        }
    }

    #endregion

    #region Animation System

    private void StartDamageAnimation(float targetFillAmount, bool immediate = false)
    {
        // Stop existing animation
        if (_damageAnimationCoroutine != null)
        {
            StopCoroutine(_damageAnimationCoroutine);
        }

        if (immediate || !_enableSmoothing)
        {
            // Immediate update
            SetFillAmount(_damageFill, targetFillAmount);
            _currentDamageFillAmount = targetFillAmount;
            _isAnimating = false;
        }
        else
        {
            // Animated update
            _damageAnimationCoroutine = StartCoroutine(AnimateDamageFill(targetFillAmount));
        }
    }

    private IEnumerator AnimateDamageFill(float targetFillAmount)
    {
        _isAnimating = true;

        // Optional delay before starting animation
        if (_damageAnimationDelay > 0f)
        {
            yield return new WaitForSeconds(_damageAnimationDelay);
        }

        float startFillAmount = _currentDamageFillAmount;
        float fillDifference = startFillAmount - targetFillAmount;
        
        // Avoid division by zero and unnecessary animation
        if (fillDifference <= _minimumFillDelta)
        {
            SetFillAmount(_damageFill, targetFillAmount);
            _currentDamageFillAmount = targetFillAmount;
            _isAnimating = false;
            yield break;
        }

        float animationDuration = fillDifference / _damageAnimationSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Check if target changed during animation
            if (_targetHealthFillAmount != targetFillAmount)
            {
                // Health changed again - restart animation with new target
                _damageAnimationCoroutine = StartCoroutine(AnimateDamageFill(_targetHealthFillAmount));
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / animationDuration);
            
            // Apply animation curve for smooth easing
            float curveValue = _damageAnimationCurve.Evaluate(normalizedTime);
            float currentFillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, curveValue);
            
            SetFillAmount(_damageFill, currentFillAmount);
            _currentDamageFillAmount = currentFillAmount;

            yield return null; // Wait for next frame
        }

        // Ensure final value is exact
        SetFillAmount(_damageFill, targetFillAmount);
        _currentDamageFillAmount = targetFillAmount;
        _isAnimating = false;
        
        _damageAnimationCoroutine = null;
    }

    #endregion

    #region Utility Methods

    private float GetHealthPercentage()
    {
        if (_healthAttribute == null || _healthAttribute.Value <= 0f)
            return 0f;

        return Mathf.Clamp01((float)_healthAttribute.CurrentValue / _healthAttribute.Value);
    }

    private void SetFillAmount(Image fillImage, float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
        }
    }

    private void ResetHealthBar()
    {
        SetFillAmount(_healthFill, 0f);
        SetFillAmount(_damageFill, 0f);
        
        if (_nameText != null) _nameText.text = "";
        if (_healthText != null) _healthText.text = "0";
        if (_maxHealthText != null) _maxHealthText.text = "0";
        
        _currentDamageFillAmount = 0f;
        _targetHealthFillAmount = 0f;
        _lastHealthPercentage = -1f;
        _lastMaxHealth = -1f;
    }

    #endregion

    #region Validation

    private void ValidateComponents()
    {
        if (_healthFill == null)
            Debug.LogWarning("UI_HealthBar: Health fill image is not assigned", this);
        
        if (_damageFill == null)
            Debug.LogWarning("UI_HealthBar: Damage fill image is not assigned", this);
    }

    private void InitializeDefaultValues()
    {
        _currentDamageFillAmount = 0f;
        _targetHealthFillAmount = 0f;
        _lastHealthPercentage = -1f;
        _lastMaxHealth = -1f;
        _isAnimating = false;
    }

    #endregion

    #region Debug Information (Editor Only)

#if UNITY_EDITOR
    [Header("Debug Info (Runtime Only)")]
    [SerializeField, Tooltip("Current health percentage")]
    private float _debugHealthPercentage;
    [SerializeField, Tooltip("Current damage fill amount")]  
    private float _debugDamageFillAmount;
    [SerializeField, Tooltip("Is damage animation playing")]
    private bool _debugIsAnimating;

    private void Update()
    {
        // Update debug info in editor
        if (Application.isPlaying)
        {
            _debugHealthPercentage = GetHealthPercentage();
            _debugDamageFillAmount = _currentDamageFillAmount;
            _debugIsAnimating = _isAnimating;
        }
    }
#endif

    #endregion
}