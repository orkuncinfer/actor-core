using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class TestContainer : MonoBehaviour
{
   public int Health;
    public GenericKey gk;
    public GameplayTag gt;
    
    public GameplayTagContainer tagContainer;
    
    [FormerlySerializedAs("tagContainer2")] public GameplayTagContainer _tagContainer;
    

    [Button]
    public void Test1()
    {
        Debug.Log(tagContainer.HasTag(gt));
    }
    
    [Button]
    public void Test2()
    {
        Debug.Log(tagContainer.HasAnyExact(_tagContainer));
    }
    
    [Button]
    public void Remove2From1()
    {
        tagContainer.RemoveTags(_tagContainer);
    }
    [Button]
    public void Add2To1()
    {
        tagContainer.AddTags(_tagContainer);
    }
}
