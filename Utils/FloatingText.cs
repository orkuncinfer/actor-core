using System;
using DG.Tweening;
using UnityEngine;

namespace Core
{
    public class FloatingText : MonoBehaviour
    {
        public event Action<FloatingText> finished;
        public float time = 0.5f;
        private Transform m_MainCamera;
        private TextMesh m_TextMesh;
        
        

        private void Awake()
        {
            m_TextMesh = GetComponent<TextMesh>();
            m_MainCamera = Camera.main.transform;
        }

        private void OnEnable()
        {
            transform.localScale = Vector3.one;
        }

        public void Animate()
        {
            transform.DOMove(transform.position + new Vector3(0.3f,0.5f,0), time).OnKill((OnFinish));
            transform.DOScale(Vector3.zero, time).SetEase(Ease.InExpo);
        }

        private void OnFinish()
        {
            finished?.Invoke(this);
            PoolManager.ReleaseObject(gameObject);
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + m_MainCamera.forward);
        }

        public void Set(string value, Color color)
        {
            m_TextMesh.text = value;
            m_TextMesh.color = color;
        }
    }
}