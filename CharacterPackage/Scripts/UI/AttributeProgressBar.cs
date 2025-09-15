using System;
using System.Collections;
using Sirenix.OdinInspector;
using StatSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Attribute = StatSystem.Attribute;

public class AttributeProgressBar : MonoState
{
    [SerializeField] private Image _energyFill;
    [SerializeField] private float _fillDuration = 0.5f;

    [SerializeField] private string _statName = "Health";

    private Transform m_MainCamera;

    private Attribute _attribute;
    private Stat _maxValueStat;

    private Service_GAS _gas;

    private void LateUpdate()
    {
        transform.LookAt(transform.position + m_MainCamera.forward);
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        _gas = Owner.GetService<Service_GAS>();

        _attribute = _gas.StatController.GetAttribute(_statName);

        _maxValueStat = _attribute.TryGetMaxValueStat();

        _attribute.onCurrentValueChanged += OnCurrentValueChanged;

        _maxValueStat = _attribute.TryGetMaxValueStat();

        UpdateHealthBarInstant();
    }

    private void OnCurrentValueChanged()
    {
        if (!gameObject.activeInHierarchy) return;
        UpdateHealthBar();
    }

    protected override void OnExit()
    {
        base.OnExit();
        _attribute.onCurrentValueChanged -= OnCurrentValueChanged;
    }


    private void OnCurrentChange()
    {
        if (!gameObject.activeInHierarchy) return;
        UpdateHealthBar();
    }

    private void Awake()
    {
        m_MainCamera = Camera.main.transform;
    }

    void UpdateHealthBar()
    {
        StartCoroutine(UpdateHealthFill(GetProgress()));
    }

    private float GetProgress()
    {
        float maxValue;

        if (_maxValueStat != null)
        {
            maxValue = _maxValueStat.Value;
        }
        else
        {
            maxValue = _attribute.BaseValue;
        }

        return _attribute.CurrentValue / maxValue;
    }

    void UpdateHealthBarInstant()
    {
        _energyFill.fillAmount = GetProgress();
    }

    IEnumerator UpdateHealthFill(float newFillAmount)
    {
        float time = 0f;
        float startFill = _energyFill.fillAmount;

        while (time < _fillDuration)
        {
            _energyFill.fillAmount = Mathf.Lerp(startFill, newFillAmount, time / _fillDuration);
            time += Time.deltaTime;
            yield return null;
        }

        _energyFill.fillAmount = newFillAmount;
    }
}