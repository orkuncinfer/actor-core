using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class PanelInstanceModel : MonoBehaviour
{
    [ReadOnly]public string PanelId;

    [ReadOnly]public GameObject PanelInstance;

    private CanvasGroup _canvasGroup;

    public event Action<PanelInstanceModel> onHideCompleted;
    public event Action<PanelInstanceModel> onShowCompleted;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    public void HidePanel()
    {
        StartCoroutine(HidePanelSequence());
    }

    public void ShowPanel()
    {
        StartCoroutine(ShowPanelSequence());
    }

    IEnumerator HidePanelSequence()
    {
        while (_canvasGroup.alpha != 0)
        {
            _canvasGroup.alpha -= Time.deltaTime * 5;
            yield return null;
        }
        gameObject.SetActive(false);
        onHideCompleted?.Invoke(this);
    }
    
    IEnumerator ShowPanelSequence()
    {
        _canvasGroup.alpha = 0;
        while (_canvasGroup.alpha != 1)
        {
            _canvasGroup.alpha += Time.deltaTime * 5;
            yield return null;
        }
        onShowCompleted?.Invoke(this);
    }
}