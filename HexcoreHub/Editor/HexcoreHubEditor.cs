using System;
using System.Collections;
using System.Collections.Generic;
using SaveSystem.Scripts.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Timeline;

public class HexcoreHubEditor : OdinMenuEditorWindow
{
    [MenuItem("Hexcore/HexcoreHub")]
    private static void OpenWindow()
    {
        GetWindow<HexcoreHubEditor>("HexcoreHub").Show();
    }
	
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        tree.Add("Create", new TextureUtilityEditor());
        tree.Add("Scenes", new SceneAssetsViewer());
        tree.Add("Save", new SaveWindow());
        tree.Add("Build", new BuildWindow());
        return tree;
    }
}

public class SaveWindow
{
    [Button(ButtonSizes.Large)]
    public void DeleteSaveData()
    {
        string[] guids = AssetDatabase.FindAssets("t:SaveData");
        List<SaveData> saveDataAssets = new List<SaveData>();
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SaveData saveData = AssetDatabase.LoadAssetAtPath<SaveData>(assetPath);
            if (saveData != null)
            {
                saveDataAssets.Add(saveData);
            }
        }
        foreach (SaveData saveData in saveDataAssets)
        {
            saveData.DeleteSave();
            EditorUtility.SetDirty(saveData);
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Save Data Deletion",
            saveDataAssets.Count + " SaveData assets were processed and their saves were deleted.",
            "OK");
    }
}

public class BuildWindow
{
    public ItemListDefinition AllItemsList;
    [Button(ButtonSizes.Large)]
    public void MagicButton()
    {
        AllItemsList.FindAllItems();
    }
}

public class TextureUtilityEditor
{
    public string FileName;
    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void GenericKey()
    {
        string fileName = "GK_";
        if (!string.IsNullOrEmpty(FileName)) fileName = FileName;
        string path = "Assets/_Project/GenericKeys/" + fileName + ".asset";
        
        CreateGenericKey(path);
    }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void DataKey()
    {
        string fileName = "DK_";
        if (!string.IsNullOrEmpty(FileName)) fileName = FileName;
        string path = "Assets/_Project/GenericKeys/DataKeys/" + fileName + ".asset";
        
        CreateGenericKey(path);
    }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void ItemKey()
    {
        string fileName = "IK_";
        if (!string.IsNullOrEmpty(FileName)) fileName = FileName;
        string path = "Assets/_Project/GenericKeys/ItemKeys/" + fileName + ".asset";
        
        CreateGenericKey(path);
    }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void ActorTag()
    {
        string fileName = "AT_";
        if (!string.IsNullOrEmpty(FileName)) fileName = FileName;
        string path = "Assets/_Project/GenericKeys/ActorTags/" + fileName + ".asset";
        
        CreateGenericKey(path);
    }

    private void CreateGenericKey(string path)
    {
        if (string.IsNullOrEmpty(path)) {
            Debug.LogError("Asset path is empty.");
            return;
        }

        GenericKey asset = ScriptableObject.CreateInstance<GenericKey>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        asset.ID = asset.name;

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}

public class SceneAssetsViewer
{
    [ShowInInspector]
    public List<SceneItem> Scenes;

    public SceneAssetsViewer()
    {
        Scenes = new List<SceneItem>();
        LoadScenesInBuildSettings();
    }

    private void LoadScenesInBuildSettings()
    {
        Scenes.Clear();
        var scenesInBuild = new HashSet<string>();

        // Collect all scenes that are in the build settings
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (buildScene.enabled)
            {
                scenesInBuild.Add(buildScene.path);
            }
        }

        // Add only those scenes to the list
        foreach (string scenePath in scenesInBuild)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null)
            {
                var sceneItem = new SceneItem();
                sceneItem.SceneAsset = sceneAsset;
                Scenes.Add(sceneItem);
            }
        }
    }

    [Button(ButtonSizes.Medium), GUIColor(0.7f, 0.7f, 1f)]
    private void RefreshScenes()
    {
        LoadScenesInBuildSettings();
    }

    
    
    [Serializable]
    public class SceneItem
    {
        public SceneAsset SceneAsset;
        
        [Button(ButtonSizes.Small), GUIColor(0.3f, 0.8f, 0.3f)]
        public void Show()
        {
            if (SceneAsset != null)
            {
                string scenePath = AssetDatabase.GetAssetPath(SceneAsset);
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
            }
        }
    }
}


