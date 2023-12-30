using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericKey",menuName = "Keys/Generic Key")]
public class GenericKey : ScriptableObject
{
    [SerializeField]private string _id;
    public string ID
    {
        get => _id;
        set => _id = value;
    }
}