using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestContainer : MonoBehaviour
{
    [ExposedField("Health")]public int Health;
    public GenericKey gk;
    public GameplayTag gt;

    public BandoWare.GameplayTags.GameplayTag tag2;
}
