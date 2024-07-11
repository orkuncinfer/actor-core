using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[SOCreatable]
[CreateAssetMenu(fileName = "New Mission", menuName = "Mission System")]
public class MissionDefinition : ItemBaseDefinition
{
    [SerializeReference] [TypeFilter("GetFilteredTypeList")][ListDrawerSettings(ShowFoldout = true)]
    public List<GameCondition> Conditions = new List<GameCondition>();
    
    public float[,] Numbers = new float[10, 10];

    public bool IsCompleted(ActorBase actor)
    {
        bool isCompleted = true;
        for (int i = 0; i < Conditions.Count; i++)
        {
            if (!Conditions[i].IsConditionMet(actor))
            {
                isCompleted = false;
                break;
            }
        }

        return isCompleted;
    }
    
    public IEnumerable<Type> GetFilteredTypeList()
    {
        var baseType = typeof(GameCondition);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var derivedTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Debug.LogWarning($"Could not load types from assembly: {assembly.FullName}, Exception: {ex.Message}");
                types = ex.Types.Where(t => t != null).ToArray();
            }

            derivedTypes.AddRange(types.Where(t => t != baseType && baseType.IsAssignableFrom(t)));
        }

        return derivedTypes;
    }
}
