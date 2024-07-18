using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PanelInstanceView : MonoBehaviour
{
    [ReadOnly]public string PanelId;

    [ReadOnly]public GameObject PanelInstance;

    private CanvasGroup _canvasGroup;

    public UnityEvent onShowComplete;
    public UnityEvent onHideComplete;
    public event Action<PanelInstanceView> onHideCompleted;
    public event Action<PanelInstanceView> onShowCompleted;
    
    public bool FadeOutOnHide = true;
    public bool FadeInOnShow = true;

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
        while (_canvasGroup.alpha != 0 && FadeOutOnHide)
        {
            _canvasGroup.alpha -= Time.deltaTime * 5;
            yield return null;
        }
        gameObject.SetActive(false);
        _canvasGroup.alpha = 0;
        onHideCompleted?.Invoke(this);
        onHideComplete?.Invoke();
        PoolManager.ReleaseObject(this.gameObject);
    }
    
    IEnumerator ShowPanelSequence()
    {
        _canvasGroup.alpha = 0;
        while (_canvasGroup.alpha != 1 && FadeInOnShow)
        {
            _canvasGroup.alpha += Time.deltaTime * 5;
            yield return null;
        }
        _canvasGroup.alpha = 1;
        onShowCompleted?.Invoke(this);
        onShowComplete?.Invoke();
        Debug.Log("Show Complete");
    }
}