using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UITabSystem
{
    public class TabGroupControl : MonoBehaviour
    {
        public RectTransform BodyParent;
        [SerializeField] private RectTransform _tabOverlay;
        [SerializeField] private RectTransform _tabButtons;
        public int DefaultTabIndex;
        public List<MenuTab> Tabs;
        
        private TabButton _activeButton;
        private TabButton _lastActiveButton;

        private bool _animating;

        private GameObject _currentTabContentInstance;
        private RectTransform _canvasRect;
        
        private Dictionary<TabButton,PanelActor> _panelActors = new Dictionary<TabButton, PanelActor>();
        
        public void OnTabClicked(TabButton obj)
        {
            if (_animating) return;
            if(obj == _activeButton) return;
            StartCoroutine(ChangeTab(obj));
        }
        [Button]
        public void SpawnTabs()
        {
            _canvasRect = FindFirstParentCanvas().GetComponent<RectTransform>();
            foreach (var tab in Tabs)
            {
                if(tab.TabContent == null) continue;
                GameObject tabInstance = Instantiate(tab.TabContent.gameObject, BodyParent);
                PanelActor panelActor = tabInstance.GetComponent<PanelActor>();
                panelActor.StartIfNot();
                _panelActors.Add(tab.TabButton, panelActor);
            }

            BodyParent.anchorMin = Vector2.zero;
            BodyParent.anchoredPosition = new Vector2(0, _tabButtons.sizeDelta.y /2);
            BodyParent.sizeDelta = new Vector2(BodyParent.sizeDelta.x, -_tabButtons.sizeDelta.y);
            OnTabClicked(Tabs[DefaultTabIndex].TabButton);

        }

        private void Start()
        {
            SpawnTabs();
        }

        public Canvas FindFirstParentCanvas()
        {
            Transform currentTransform = transform;
        
            while (currentTransform != null)
            {
                Canvas canvas = currentTransform.GetComponent<Canvas>();
                if (canvas != null)
                {
                    return canvas;
                }
                currentTransform = currentTransform.parent;
            }
        
            // Return null if no Canvas is found in the parent hierarchy
            return null;
        }

        private void Update()
        {
            if(_activeButton == null) return;
            
            _tabOverlay.localPosition = Vector3.Lerp(_tabOverlay.localPosition, _activeButton.transform.localPosition, Time.deltaTime * 10);
        }
        
        public void CloseCurrentTab()
        {
            PanelActor panelActor = _panelActors[_activeButton];
            panelActor.ClosePanel();
        }
        public void OpenCurrentTab()
        {
            if(_activeButton == null) return;
            PanelActor panelActor = _panelActors[_activeButton];
            panelActor.gameObject.SetActive(true);
            panelActor.OpenPanel();
        }

        IEnumerator ChangeTab(TabButton tabButton)
        {
            yield return null;
            _animating = true;
            _activeButton = tabButton;
            float time = 0.25f;
            
            GameObject newTabInstance = _panelActors[tabButton].gameObject;
            RectTransform newTabRectTransform = newTabInstance.GetComponent<RectTransform>();
            PanelActor panelActor = _panelActors[tabButton];
            panelActor.gameObject.SetActive(true);
            panelActor.StartIfNot();
            panelActor.OpenPanel();
            if (_lastActiveButton != null)
            {
                if (tabButton.transform.position.x > _lastActiveButton.transform.position.x)
                {
                    newTabRectTransform.localPosition = new Vector3(_canvasRect.sizeDelta.x, 0, 0);
                    if (_currentTabContentInstance != null)
                    {
                        _currentTabContentInstance.transform.DOLocalMove(new Vector3(-_canvasRect.sizeDelta.x, 0, 0), 0.4f);
                    }
                }
                else
                {
                    newTabRectTransform.localPosition = new Vector3(-_canvasRect.sizeDelta.x, 0, 0);
                    if (_currentTabContentInstance != null)
                    {
                        _currentTabContentInstance.transform.DOLocalMove(new Vector3(_canvasRect.sizeDelta.x, 0, 0), 0.4f);
                    }
                }
            }
           
            newTabRectTransform.DOLocalMove(Vector3.zero, 0.4f);
            
            if (_lastActiveButton != null)
            {
                DOTween.To(() => _lastActiveButton.LayoutElement.preferredWidth, x => _lastActiveButton.LayoutElement.preferredWidth = x, _lastActiveButton.DefaultPrefWidth, time);
                DOTween.To(() => _lastActiveButton.LayoutElement.minWidth, x => _lastActiveButton.LayoutElement.minWidth = x, _lastActiveButton.DefaultMinWidth, time);
                _lastActiveButton.IconRectTransform.DOScale(Vector3.one, time);
            }
            DOTween.To(() => _activeButton.LayoutElement.preferredWidth, x => _activeButton.LayoutElement.preferredWidth = x, 275, time);
            DOTween.To(() => _activeButton.LayoutElement.minWidth, x => _activeButton.LayoutElement.minWidth = x, 275, time);
            _activeButton.IconRectTransform.DOScale(Vector3.one * 1.2f, time);
            yield return new WaitForSeconds(0.4f);
            
            _lastActiveButton = tabButton;
            if(_currentTabContentInstance)_currentTabContentInstance.GetComponent<PanelActor>().ClosePanel();
            _currentTabContentInstance = newTabInstance;
            _animating = false;
        }
    }
    
}
[Serializable]
public class MenuTab
{
    public UITabSystem.TabButton TabButton;
    public GameObject TabContent;
}

