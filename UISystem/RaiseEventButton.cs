using UnityEngine;
using UnityEngine.EventSystems;

public class RaiseEventButton : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private EventSignal _event;
    public void OnPointerUp(PointerEventData eventData)
    {
        _event.Raise();
    }
}