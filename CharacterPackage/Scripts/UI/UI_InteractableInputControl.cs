using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InteractableInputControl : MonoState
{
    private Interactable _interactable;
    public event Action<Interactable> onInteractableChanged;
    public Interactable Interactable
    {
        get => _interactable;
        set
        {
            if (_interactable != value)
            {
                _interactable = value;
                onInteractableChanged?.Invoke(_interactable);
            }
        }
    }

    private PanelActor _panelActor;

    public GameObject ViewToggle;
    public TextMeshProUGUI _interactButtonText;
    
    public GameObject EquipButtonToggle;
    public Image EquipButtonFill;

    protected override void OnEnter()
    {
        base.OnEnter();
        DecideShow();
        onInteractableChanged += OnInteractableChanged;
    }

    protected override void OnExit()
    {
        base.OnExit();
        onInteractableChanged -= OnInteractableChanged;
    }

    private void OnInteractableChanged(Interactable obj)
    {
        DecideShow();
        if (_interactable == null) return;
        
        EquipButtonToggle.SetActive(false);
        
        if(_interactable.transform.parent.TryGetComponent(out Collectible collectible))
        {
            _interactButtonText.text = "Collect";
        }
        else
        {
            _interactButtonText.text = "Interact";
        }
        
        if (_interactable.transform.parent.TryGetComponent(out Collectible itemDrop))
        {
            if(itemDrop.IsEquippable())
                EquipButtonToggle.SetActive(true);
        }
    }

    void DecideShow()
    {
        if (_interactable != null)
        {
            ViewToggle.SetActive(true);
        }
        else
        {
            ViewToggle.SetActive(false);
        }
    }
}
