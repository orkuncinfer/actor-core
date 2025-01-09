using UnityEngine;

public class RedirectMultiState : MonoState
{
    [SerializeField] private MonoState[] _redirectStates;

    protected override void OnEnter()
    {
        base.OnEnter();
        for (int i = 0; i < _redirectStates.Length; i++)
        {
            _redirectStates[i].CheckoutEnter(Owner);
        }
    }

    protected override void OnExit()
    {
        base.OnExit();
        for (int i = 0; i < _redirectStates.Length; i++)
        {
            _redirectStates[i].CheckoutExit();
        }
    }
}