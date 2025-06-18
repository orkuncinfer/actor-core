using UnityEngine;

public class UI_EquippableHud : MonoBehaviour
{
    public Equippable Equippable;

    public virtual void SetEquippable(Equippable equippable)
    {
        Equippable = equippable;
    }
    public virtual void ResetEquippable(Equippable equippable)
    {
        Equippable = equippable;
    }
}