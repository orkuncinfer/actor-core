using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract  class Collectible : MonoBehaviour
{
    public abstract void Collect();
    public abstract bool IsEquippable();
    public abstract void Equip();
}
