using UnityEngine;

public class SetParentNull : MonoBehaviour
{
    void Start()
    {
        transform.SetParent(null);
    }
}
