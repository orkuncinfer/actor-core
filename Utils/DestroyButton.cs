using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestroyButton : MonoBehaviour
{
    public GameObject DestroyObject;

    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(OnDestroyButtonClicked);
    }

    private void OnDestroyButtonClicked()
    {
        Destroy(DestroyObject);
    }
}
