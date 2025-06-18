using TMPro;
using UnityEngine;

public class LevelBadge_UI : MonoState
{
    [SerializeField] private TextMeshProUGUI _levelText;
    private Service_GAS _gas;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gas = Owner.GetService<Service_GAS>();
        OnLevelChanged();
        _gas.LevelController.levelChanged += OnLevelChanged;
    }

    private void OnLevelChanged()
    {
        _levelText.text = _gas.LevelController.level.ToString();
    }

    protected override void OnExit()
    {
        base.OnExit();
        _gas.LevelController.levelChanged -= OnLevelChanged;
    }
}