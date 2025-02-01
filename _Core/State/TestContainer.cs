using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestContainer : MonoBehaviour
{
    [ExposedField("Health")]public int Health;
    public GenericKey gk;
    public GameplayTag gt;
    
    public GameplayTagContainer2 tagContainer;
    
    public GameplayTagContainer2 tagContainer2;

    public BandoWare.GameplayTags.GameplayTag tag2;

    [Button]
    public void Test1()
    {
        Debug.Log(tagContainer.HasTag(gt));
    }
    
    [Button]
    public void Test2()
    {
        Debug.Log(tagContainer.HasAnyExact(tagContainer2));
    }
    
    [Button]
    public void Remove2From1()
    {
        tagContainer.RemoveTags(tagContainer2);
    }
    [Button]
    public void Add2To1()
    {
        tagContainer.AddTags(tagContainer2);
    }
}
