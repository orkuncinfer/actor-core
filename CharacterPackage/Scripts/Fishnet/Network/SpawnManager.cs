using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay
{

    public class SpawnManager : MonoBehaviour
    {
        /// <summary>
        /// Parent object which hold spawn points.
        /// </summary>
        [Tooltip("Parent object which hold spawn points.")]
        [SerializeField]
        private Transform _spawnPointParent;

        #region Private.

        /// <summary>
        /// Found spawn points.
        /// </summary>
        public Transform SpawnPoint;
        #endregion

     

        /// <summary>
        /// Returns a random spawn point.
        /// </summary>
        /// <returns></returns>
        public Transform ReturnSpawnPoint()
        {
           
            //If here no valid spawn points found. Pick first one.
            return SpawnPoint;
        }

    }


}