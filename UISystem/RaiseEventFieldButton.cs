using UnityEngine;
using UnityEngine.EventSystems;

public class RaiseEventFieldButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private EventField _event;

    public void OnPointerClick(PointerEventData eventData)
    {
        _event.Raise();
    }
}