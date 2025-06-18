using UnityEngine;

public class State_GasTargetHealthBarHandler : MonoState
{
    private AbilityController _abilityController;

    [SerializeField] private PanelActor _mobInfoPanel;

    private PanelActor _panelInstance;
    protected override void OnEnter()
    {
        base.OnEnter();
        _abilityController = Owner.GetService<Service_GAS>().AbilityController;

        _abilityController.OnTargetChanged += OnTargetChanged;
    }

    protected override void OnExit()
    {
        base.OnExit();
        _abilityController.OnTargetChanged -= OnTargetChanged;
    }

    private void OnTargetChanged(GameObject oldTarget, GameObject newTarget)
    {
        if (newTarget != null)
        {
            GameObject panelInstance = null;
            if (_panelInstance != null)
            {
                panelInstance = CanvasManager.Instance.ShowAdditive(_panelInstance);
            }
            else
            {
                panelInstance = CanvasManager.Instance.ShowAdditive(_mobInfoPanel);
            }
   
            _panelInstance = panelInstance.GetComponent<PanelActor>();
            
            
            Actor targetActor = newTarget.GetComponent<Actor>();
            Data_Mob mob = targetActor.GetData<Data_Mob>();
            if(mob == null) return;
            string mobName = mob.MobDefinition.ItemName;
            
            panelInstance.GetComponent<UI_HealthBar>().SetTarget(targetActor,mobName);
        }
        else
        {
            if (_panelInstance != null)
            {
                CanvasManager.Instance.HidePanel(_panelInstance.PanelId);
            }
        }
    }
}
