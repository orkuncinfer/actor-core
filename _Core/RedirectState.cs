using UnityEngine;

public class RedirectState : MonoState
{
    [SerializeField] private MonoState _redirectTo;

    protected override void OnEnter()
    {
        base.OnEnter();
        if (_redirectTo == null)
        {
            return;
        }

        _redirectTo.CheckoutEnter(Owner);
    }

    protected override void OnExit()
    {
        base.OnExit();
        if (_redirectTo == null)
        {
            return;
        }

        _redirectTo.CheckoutExit();
    }
}