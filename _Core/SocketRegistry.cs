using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class SocketRegistry : MonoBehaviour
{
    public List<SocketData> Sockets;

    public List<string> Slots;

    public Dictionary<string, Transform> SocketDictionary = new Dictionary<string, Transform>();
    [ShowInInspector] public Dictionary<string, Transform> SlotDictionary = new Dictionary<string, Transform>();

    private void Awake()
    {
        //ReInstantiateSockets();

        for (int i = 0; i < Sockets.Count; i++)
        {
            SocketDictionary.Add(Sockets[i].Key, Sockets[i].Transform);
        }

        for (int i = 0; i < Slots.Count; i++)
        {
            SlotDictionary.Add(Slots[i], null);
        }
    }


    public Transform GetSocket(string key)
    {
        if (SocketDictionary.TryGetValue(key, out Transform socket))
        {
            return socket;
        }
        else
        {
            Debug.LogError($"{key} , could not found in the socket registry in : {gameObject.name}");
        }

        return null;
    }

    [Button]
    public void FindTransformsInRig()
    {
        foreach (SocketData socket in Sockets)
        {
            Debug.Log("searching for " + socket.Key);
            if (socket.Transform == null)
            {
                if (GetChildGameObject(gameObject, socket.Key) == null) continue;
                socket.Transform = GetChildGameObject(gameObject, socket.Key).transform;
            }
        }
    }

    public GameObject GetChildGameObject(GameObject fromGameObject, string withName)
    {
        var allKids = fromGameObject.GetComponentsInChildren<Transform>();
        var kid = allKids.FirstOrDefault(k => k.gameObject.name == withName);
        if (kid == null) return null;
        return kid.gameObject;
    }

    #region Editor-Only Features

#if UNITY_EDITOR

    // Comprehensive bone name mapping dictionary for various naming conventions
     private static readonly Dictionary<string, List<string>> _boneNameVariations = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
    {
        // Spine and Core
        { "hips", new List<string> { "hips", "pelvis", "root", "hip", "waist", "bip001", "bip001 pelvis" } },
        { "spine", new List<string> { "spine", "spine1", "spine_01", "spine_1", "chest", "torso", "bip001 spine", "bip001 spine1" } },
        { "spine1", new List<string> { "spine1", "spine2", "spine_02", "spine_2", "chest_upper", "bip001 spine1", "bip001 spine2" } },
        { "spine2", new List<string> { "spine2", "spine3", "spine_03", "spine_3", "chest_top", "bip001 spine2", "bip001 spine3" } },
        { "neck", new List<string> { "neck", "neck_01", "neck_1", "neck1", "bip001 neck" } },
        { "head", new List<string> { "head", "head_01", "head_1", "head1", "bip001 head", "bip001 headnub" } },
        
        // Left Arm
        { "left_shoulder", new List<string> { "left_shoulder", "shoulder_l", "leftshoulder", "l_shoulder", "shoulder.l", "clavicle_l", "left_clavicle", "l_clavicle", "bip001 l clavicle" } },
        { "left_arm", new List<string> { "left_arm", "arm_l", "leftarm", "l_arm", "arm.l", "upper_arm_l", "left_upper_arm", "l_upper_arm", "upperarm_l", "bip001 l upperarm" } },
        { "left_forearm", new List<string> { "left_forearm", "forearm_l", "leftforearm", "l_forearm", "forearm.l", "lower_arm_l", "left_lower_arm", "l_lower_arm", "lowerarm_l", "bip001 l forearm" } },
        { "left_hand", new List<string> { "left_hand", "hand_l", "lefthand", "l_hand", "hand.l", "bip001 l hand" } },
        
        // Right Arm
        { "right_shoulder", new List<string> { "right_shoulder", "shoulder_r", "rightshoulder", "r_shoulder", "shoulder.r", "clavicle_r", "right_clavicle", "r_clavicle", "bip001 r clavicle" } },
        { "right_arm", new List<string> { "right_arm", "arm_r", "rightarm", "r_arm", "arm.r", "upper_arm_r", "right_upper_arm", "r_upper_arm", "upperarm_r", "bip001 r upperarm" } },
        { "right_forearm", new List<string> { "right_forearm", "forearm_r", "rightforearm", "r_forearm", "forearm.r", "lower_arm_r", "right_lower_arm", "r_lower_arm", "lowerarm_r", "bip001 r forearm" } },
        { "right_hand", new List<string> { "right_hand", "hand_r", "righthand", "r_hand", "hand.r", "bip001 r hand" } },
        
        // Left Leg
        { "left_thigh", new List<string> { "left_thigh", "thigh_l", "leftthigh", "l_thigh", "thigh.l", "upper_leg_l", "left_upper_leg", "l_upper_leg", "upperleg_l", "left_leg", "bip001 l thigh" } },
        { "left_shin", new List<string> { "left_shin", "shin_l", "leftshin", "l_shin", "shin.l", "lower_leg_l", "left_lower_leg", "l_lower_leg", "lowerleg_l", "left_calf", "calf_l", "bip001 l calf" } },
        { "left_foot", new List<string> { "left_foot", "foot_l", "leftfoot", "l_foot", "foot.l", "bip001 l foot" } },
        { "left_toe", new List<string> { "left_toe", "toe_l", "lefttoe", "l_toe", "toe.l", "toes_l", "left_toes", "bip001 l toe0", "bip001 l toenub" } },
        
        // Right Leg
        { "right_thigh", new List<string> { "right_thigh", "thigh_r", "rightthigh", "r_thigh", "thigh.r", "upper_leg_r", "right_upper_leg", "r_upper_leg", "upperleg_r", "right_leg", "bip001 r thigh" } },
        { "right_shin", new List<string> { "right_shin", "shin_r", "rightshin", "r_shin", "shin.r", "lower_leg_r", "right_lower_leg", "r_lower_leg", "lowerleg_r", "right_calf", "calf_r", "bip001 r calf" } },
        { "right_foot", new List<string> { "right_foot", "foot_r", "rightfoot", "r_foot", "foot.r", "bip001 r foot" } },
        { "right_toe", new List<string> { "right_toe", "toe_r", "righttoe", "r_toe", "toe.r", "toes_r", "right_toes", "bip001 r toe0", "bip001 r toenub" } },
        
        // Fingers (Left)
        { "left_thumb", new List<string> { "left_thumb", "thumb_l", "l_thumb", "thumb.l", "thumb_01_l", "thumb_1_l", "bip001 l finger0", "bip001 l finger0nub" } },
        { "left_index", new List<string> { "left_index", "index_l", "l_index", "index.l", "index_01_l", "index_1_l", "bip001 l finger1", "bip001 l finger10", "bip001 l finger1nub" } },
        { "left_middle", new List<string> { "left_middle", "middle_l", "l_middle", "middle.l", "middle_01_l", "middle_1_l", "bip001 l finger2", "bip001 l finger20", "bip001 l finger2nub" } },
        { "left_ring", new List<string> { "left_ring", "ring_l", "l_ring", "ring.l", "ring_01_l", "ring_1_l", "bip001 l finger3", "bip001 l finger30", "bip001 l finger3nub" } },
        { "left_pinky", new List<string> { "left_pinky", "pinky_l", "l_pinky", "pinky.l", "pinky_01_l", "pinky_1_l", "little_l", "left_little", "bip001 l finger4", "bip001 l finger40", "bip001 l finger4nub" } },
        
        // Fingers (Right)
        { "right_thumb", new List<string> { "right_thumb", "thumb_r", "r_thumb", "thumb.r", "thumb_01_r", "thumb_1_r", "bip001 r finger0", "bip001 r finger0nub" } },
        { "right_index", new List<string> { "right_index", "index_r", "r_index", "index.r", "index_01_r", "index_1_r", "bip001 r finger1", "bip001 r finger10", "bip001 r finger1nub" } },
        { "right_middle", new List<string> { "right_middle", "middle_r", "r_middle", "middle.r", "middle_01_r", "middle_1_r", "bip001 r finger2", "bip001 r finger20", "bip001 r finger2nub" } },
        { "right_ring", new List<string> { "right_ring", "ring_r", "r_ring", "ring.r", "ring_01_r", "ring_1_r", "bip001 r finger3", "bip001 r finger30", "bip001 r finger3nub" } },
        { "right_pinky", new List<string> { "right_pinky", "pinky_r", "r_pinky", "pinky.r", "pinky_01_r", "pinky_1_r", "little_r", "right_little", "bip001 r finger4", "bip001 r finger40", "bip001 r finger4nub" } },
        
        // Additional Mixamo-specific bones
        { "footsteps", new List<string> { "footsteps", "bip001 footsteps" } },
        { "props", new List<string> { "props", "bip001 props" } },
        { "weapon", new List<string> { "weapon", "weapon_hand", "weapon_r", "weapon_l", "bip001 prop1", "bip001 prop2" } }
    };

    /// <summary>
    /// Editor-only: Migrates socket transforms from current hierarchy to a target root's hierarchy
    /// by intelligently matching bone names across different naming conventions.
    /// </summary>
    /// <param name="targetRoot">The root transform of the target humanoid rig</param>
    [Button("Migrate Sockets to Target Root")]
    public void MigrateSockets(Transform targetRoot)
    {
        if (targetRoot == null)
        {
            Debug.LogError($"[SocketRegistry] MigrateSockets: Target root is null!");
            return;
        }

        // Record undo for editor operation
        Undo.RecordObject(this, "Migrate Sockets");

        // Record all socket transforms for undo
        foreach (var socket in Sockets)
        {
            if (socket.Transform != null)
            {
                Undo.RecordObject(socket.Transform, "Migrate Socket Transform");
            }
        }

        // Cache all transforms in the target hierarchy for efficient searching
        Dictionary<string, Transform> targetBoneCache = BuildBoneCache(targetRoot);

        int successfulMigrations = 0;
        int failedMigrations = 0;
        List<string> migrationReport = new List<string>();

        foreach (SocketData socketData in Sockets)
        {
            if (socketData.Transform == null)
            {
                Debug.LogWarning($"[SocketRegistry] Socket '{socketData.Key}' has null transform, skipping.");
                continue;
            }

            Transform originalParent = socketData.Transform.parent;
            if (originalParent == null)
            {
                Debug.LogWarning($"[SocketRegistry] Socket '{socketData.Key}' has no parent, skipping.");
                continue;
            }

            // Store original local position and rotation for maintaining offset
            Vector3 localPosition = socketData.Transform.localPosition;
            Quaternion localRotation = socketData.Transform.localRotation;
            Vector3 localScale = socketData.Transform.localScale;

            // Find matching bone in target hierarchy
            Transform targetBone = FindMatchingBone(originalParent.name, targetBoneCache);

            if (targetBone != null)
            {
                // Reparent the socket
                socketData.Transform.SetParent(targetBone, false);

                // Restore local transform values to maintain offset
                socketData.Transform.localPosition = localPosition;
                socketData.Transform.localRotation = localRotation;
                socketData.Transform.localScale = localScale;

                successfulMigrations++;
                migrationReport.Add($"✓ '{socketData.Key}': {originalParent.name} → {targetBone.name}");

                Debug.Log(
                    $"[SocketRegistry] Successfully migrated socket '{socketData.Key}' from '{originalParent.name}' to '{targetBone.name}'");
            }
            else
            {
                failedMigrations++;
                migrationReport.Add($"✗ '{socketData.Key}': No match found for '{originalParent.name}'");

                Debug.LogWarning(
                    $"[SocketRegistry] Failed to find matching bone for socket '{socketData.Key}' (parent: '{originalParent.name}')");
            }
        }

        // Mark as dirty for saving
        EditorUtility.SetDirty(this);

        // Log migration summary
        Debug.Log($"[SocketRegistry] Migration Complete!\n" +
                  $"Successful: {successfulMigrations}\n" +
                  $"Failed: {failedMigrations}\n" +
                  $"Details:\n{string.Join("\n", migrationReport)}");
    }

    /// <summary>
    /// Editor-only: Preview migration without applying changes
    /// </summary>
    [Button("Preview Socket Migration")]
    public void PreviewMigration(Transform targetRoot)
    {
        if (targetRoot == null)
        {
            Debug.LogError($"[SocketRegistry] PreviewMigration: Target root is null!");
            return;
        }

        Dictionary<string, Transform> targetBoneCache = BuildBoneCache(targetRoot);
        List<string> previewReport = new List<string>();

        foreach (SocketData socketData in Sockets)
        {
            if (socketData.Transform == null || socketData.Transform.parent == null)
                continue;

            Transform originalParent = socketData.Transform.parent;
            Transform targetBone = FindMatchingBone(originalParent.name, targetBoneCache);

            if (targetBone != null)
            {
                previewReport.Add($"✓ '{socketData.Key}': {originalParent.name} → {targetBone.name}");
            }
            else
            {
                previewReport.Add($"✗ '{socketData.Key}': No match found for '{originalParent.name}'");
            }
        }

        Debug.Log($"[SocketRegistry] Migration Preview:\n{string.Join("\n", previewReport)}");
    }

    /// <summary>
    /// Editor-only: Validates current socket setup
    /// </summary>
    [Button("Validate Socket Setup")]
    public void ValidateSocketSetup()
    {
        List<string> issues = new List<string>();

        // Check for null transforms
        foreach (var socket in Sockets)
        {
            if (socket.Transform == null)
            {
                issues.Add($"Socket '{socket.Key}' has null transform");
            }
            else if (socket.Transform.parent == null)
            {
                issues.Add($"Socket '{socket.Key}' has no parent");
            }
        }

        // Check for duplicate keys
        var duplicateKeys = Sockets
            .GroupBy(s => s.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var key in duplicateKeys)
        {
            issues.Add($"Duplicate socket key found: '{key}'");
        }

        if (issues.Count == 0)
        {
            Debug.Log("[SocketRegistry] Socket setup validation passed!");
        }
        else
        {
            Debug.LogWarning(
                $"[SocketRegistry] Socket setup validation found {issues.Count} issues:\n{string.Join("\n", issues)}");
        }
    }

    /// <summary>
    /// Editor-only: Builds a cache of all transforms in the hierarchy with normalized names as keys
    /// </summary>
    private Dictionary<string, Transform> BuildBoneCache(Transform root)
    {
        Dictionary<string, Transform> cache = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        Transform[] allTransforms = root.GetComponentsInChildren<Transform>();

        foreach (Transform t in allTransforms)
        {
            string normalizedName = NormalizeBoneName(t.name);

            // Handle potential duplicates by keeping the first occurrence
            if (!cache.ContainsKey(normalizedName))
            {
                cache[normalizedName] = t;
            }

            // Also add the original name
            if (!cache.ContainsKey(t.name))
            {
                cache[t.name] = t;
            }
        }

        return cache;
    }

    /// <summary>
    /// Editor-only: Finds a matching bone in the target hierarchy using intelligent name matching
    /// </summary>
    private Transform FindMatchingBone(string sourceBoneName, Dictionary<string, Transform> targetBoneCache)
    {
        // First, try direct match
        if (targetBoneCache.TryGetValue(sourceBoneName, out Transform directMatch))
        {
            return directMatch;
        }

        // Normalize the source bone name
        string normalizedSource = NormalizeBoneName(sourceBoneName);
        if (targetBoneCache.TryGetValue(normalizedSource, out Transform normalizedMatch))
        {
            return normalizedMatch;
        }

        // Check against known bone variations
        foreach (var boneMapping in _boneNameVariations)
        {
            // Check if source name matches any variation
            bool sourceMatches = boneMapping.Value.Any(variation =>
                string.Equals(sourceBoneName, variation, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedSource, NormalizeBoneName(variation), StringComparison.OrdinalIgnoreCase));

            if (sourceMatches)
            {
                // Try to find any of the variations in the target
                foreach (string variation in boneMapping.Value)
                {
                    if (targetBoneCache.TryGetValue(variation, out Transform variantMatch))
                    {
                        return variantMatch;
                    }

                    // Also try normalized version
                    string normalizedVariation = NormalizeBoneName(variation);
                    if (targetBoneCache.TryGetValue(normalizedVariation, out Transform normalizedVariantMatch))
                    {
                        return normalizedVariantMatch;
                    }
                }
            }
        }

        // Fallback: Try fuzzy matching for numbered bones
        return TryFuzzyNumberedBoneMatch(sourceBoneName, targetBoneCache);
    }

    /// <summary>
    /// Editor-only: Normalizes bone names by removing common separators and standardizing format
    /// </summary>
    private string NormalizeBoneName(string boneName)
    {
        // Remove common separators and convert to lowercase
        return boneName
            .Replace("_", "")
            .Replace("-", "")
            .Replace(".", "")
            .Replace(" ", "")
            .ToLower();
    }

    /// <summary>
    /// Editor-only: Attempts to match numbered bones with different numbering schemes
    /// </summary>
    private Transform TryFuzzyNumberedBoneMatch(string sourceBoneName, Dictionary<string, Transform> targetBoneCache)
    {
        // Extract base name and number from source
        var sourceMatch = System.Text.RegularExpressions.Regex.Match(sourceBoneName, @"(.+?)[\s_\-.]?(\d+)$");
        if (!sourceMatch.Success)
            return null;

        string baseName = sourceMatch.Groups[1].Value;
        string number = sourceMatch.Groups[2].Value;

        // Try different number formats
        string[] numberFormats = new[]
        {
            number,
            number.PadLeft(2, '0'), // 1 -> 01
            $"0{number}", // 1 -> 01 (alternative)
        };

        string[] separators = new[] { "_", "-", ".", " ", "" };

        foreach (string sep in separators)
        {
            foreach (string numFormat in numberFormats)
            {
                string candidate = $"{baseName}{sep}{numFormat}";
                if (targetBoneCache.TryGetValue(candidate, out Transform match))
                {
                    return match;
                }
            }
        }

        return null;
    }

#endif

    #endregion
}

[System.Serializable]
public class SocketData
{
    public string Key;
    [HorizontalGroup()] public Transform Transform;
}