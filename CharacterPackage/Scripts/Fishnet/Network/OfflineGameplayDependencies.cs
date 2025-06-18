using FirstGearGames.FPSLand.Managers.Gameplay;
using UnityEngine;


public class OfflineGameplayDependencies : MonoBehaviour
{
    #region Serialized.

    /// <summary>
    /// 
    /// </summary>
    [Tooltip("SpawnManager component.")] [SerializeField]
    private SpawnManager _spawnManager;

    /// <summary>
    /// SpawnManager reference.
    /// </summary>
    public static SpawnManager SpawnManager
    {
        get { return _instance._spawnManager; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Tooltip("GameplayCanvases component.")] [SerializeField]

    #endregion

    #region Private.

    /// <summary>
    /// Singleton reference of this component.
    /// </summary>
    private static OfflineGameplayDependencies _instance;

    #endregion

    private void Awake()
    {
        _instance = this;
    }
}