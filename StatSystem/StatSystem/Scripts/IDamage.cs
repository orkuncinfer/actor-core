using UnityEngine;


public interface IDamage
{ 
    bool IsCriticalHit { get; }
    int Magnitude { get; }
    GameObject Instigator { get; }
    object Source { get; }
}
