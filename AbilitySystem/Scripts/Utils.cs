using UnityEngine;
using UnityEngine.AI;


    public class Utils
    {
        public static Vector3 GetCenterOfCollider(Transform target)
        {
            Vector3 center;
            Collider collider = target.GetComponent<Collider>();
            switch (collider)
            {
                case CapsuleCollider capsuleCollider:
                    center = capsuleCollider.center;
                    break;
                case CharacterController characterController:
                    center = characterController.center;
                    break;
                default:
                    center = Vector3.zero;
                    Debug.LogWarning("Could not find center");
                    break;
            }

            return center;
        }
        
        public static float GetComponentHeight(GameObject target)
        {
            float height;
            if (target.TryGetComponent(out NavMeshAgent navMeshAgent))
            {
                height = navMeshAgent.height;
            }
            else if (target.TryGetComponent(out CharacterController characterController))
            {
                height = characterController.height;
            }
            else
            {
                height = 0f;
                Debug.LogWarning("Could not determine height!");
            }

            return height;
        }
        
        public static bool TryGetComponentInChildren<T>(GameObject gameObject, out T component) where T : Component
        {
            component = null; // Initialize the out parameter

            if (gameObject == null)
            {
                Debug.LogError("GameObject is null. Cannot search for components in children.");
                return false;
            }

            component = gameObject.GetComponentInChildren<T>();

            if (component != null)
            {
                // Component found
                Debug.Log($"Component of type {typeof(T)} found in children of {gameObject.name}.");
                return true;
            }
            else
            {
                // Component not found, handle gracefully
                Debug.Log($"Component of type {typeof(T)} not found in children of {gameObject.name}.");
                return false;
            }
        }
    }
