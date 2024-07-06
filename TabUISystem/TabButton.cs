using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UITabSystem
{
    public class TabButton : MonoBehaviour, IPointerClickHandler
    {
        public LayoutElement LayoutElement;
        public RectTransform IconRectTransform;
        public float DefaultMinWidth;
        public float DefaultPrefWidth;
        
        private TabGroupControl _tabGroupControl;
        private void Awake()
        {
            //LayoutElement = GetComponent<LayoutElement>();
            //IconRectTransform = GetComponent<RectTransform>();
            DefaultMinWidth = LayoutElement.minWidth;
            DefaultPrefWidth = LayoutElement.preferredWidth;
            _tabGroupControl = GetComponentInParent<TabGroupControl>();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            _tabGroupControl.OnTabClicked(this);
        }
    }
}