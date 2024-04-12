using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "DataKey",menuName = "Keys/Data Key")]
public class DataKey : ScriptableObject
{
    [SerializeField]private string _id;
    public string ID
    {
        get => _id;
        set => _id = value;
    }

    [Button(ButtonSizes.Gigantic)]
    void MatchName()
    {
        ID = name;
    }
}