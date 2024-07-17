using UnityEngine;
using UnityEngine.Serialization;

public class PanelActor : ActorBase
{
    [SerializeField] private MonoState _openedState;
    [SerializeField] private Transform _viewTransform;

    protected override void OnActorStart()
    {
        base.OnActorStart();
        _viewTransform.gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        _viewTransform.gameObject.SetActive(true);
        if (_openedState)
        {
            _openedState.CheckoutEnter(this);
        }
    }

    public void ClosePanel()
    {
        _viewTransform.gameObject.SetActive(false);
        if (_openedState)
        {
            _openedState.CheckoutExit();
        }
    }

    protected override void OnActorStop()
    {
        base.OnActorStop();
        if (_openedState)
        {
            if (_openedState.IsRunning)
            {
                _openedState.CheckoutExit();
            }
        }

        PoolManager.ReleaseObject(gameObject);
    }
}