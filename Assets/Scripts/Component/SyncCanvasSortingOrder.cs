using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class SyncCanvasSortingOrder : UIBehaviour
    {
        [SerializeField] private bool m_SyncRoot = false;
        public bool SyncRoot
        {
            get => m_SyncRoot;
            set
            {
                if (m_SyncRoot == value)
                    return;

                m_SyncRoot = value;
                UpdateTargetCanvas();
                UpdateSortingOrder();
            }
        }
        [SerializeField] private int m_AddSortingOrder = 0;
        public int addSortingOrder
        {
            get => m_AddSortingOrder;
            set
            {
                if (m_AddSortingOrder == value)
                    return;
                
                m_AddSortingOrder = value;
                UpdateSortingOrder();
            }
        }
        [SerializeField] private Renderer[] m_TargetRenderers;
        
        private Canvas m_Canvas;
        private Canvas m_TargetCanvas;
        private CanvasSortingOrderChangeListener m_TargetCanvasSortingOrderListener;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Canvas = GetComponent<Canvas>();
            UpdateTargetCanvas();
            UpdateSortingOrder();
        }

        protected override void OnDisable()
        {
            base.OnEnable();
            if (m_TargetCanvasSortingOrderListener != null)
                m_TargetCanvasSortingOrderListener.onChange.RemoveListener(OnChangeRootCanvasSortingOrder);
            m_TargetCanvas = null;
            m_TargetCanvasSortingOrderListener = null;
        }
        
#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (Application.isPlaying == false)
                UpdateSortingOrder();
        }
#endif

        private void OnChangeRootCanvasSortingOrder(int sortingOrder)
        {
            UpdateSortingOrder();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            UpdateTargetCanvas();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateSortingOrder();
        }


        private void UpdateTargetCanvas()
        {
            if (m_TargetCanvasSortingOrderListener != null)
                m_TargetCanvasSortingOrderListener.onChange.RemoveListener(OnChangeRootCanvasSortingOrder);

            m_TargetCanvas = null;
            m_TargetCanvasSortingOrderListener = null;

            if (m_SyncRoot == true)
            {
                if (m_Canvas != null)
                {
                    m_TargetCanvas = m_Canvas.rootCanvas;
                }
                else
                {
                    var parentCanvas = transform.GetComponentInParent<Canvas>();
                    if (parentCanvas != null && parentCanvas.isRootCanvas == false)
                    {
                        m_TargetCanvas = parentCanvas.rootCanvas;
                    }
                }
            }
            else
            {
                if (transform.parent != null)
                {
                    m_TargetCanvas = transform.parent.GetComponentInParent<Canvas>();
                }
            }
            
            if (Application.isPlaying == true && m_TargetCanvas != null)
            {
                m_TargetCanvasSortingOrderListener = m_TargetCanvas.GetComponent<CanvasSortingOrderChangeListener>();
                if (m_TargetCanvasSortingOrderListener == null)
                    m_TargetCanvasSortingOrderListener = m_TargetCanvas.gameObject.AddComponent<CanvasSortingOrderChangeListener>();
                m_TargetCanvasSortingOrderListener.onChange.AddListener(OnChangeRootCanvasSortingOrder);
            }
        }

        private void UpdateSortingOrder()
        {
            if (Application.isPlaying == false)
                m_Canvas = GetComponent<Canvas>();
            
            var sortingOrder = (m_TargetCanvas == null ? 0 : m_TargetCanvas.sortingOrder) + m_AddSortingOrder;
            if (m_Canvas != null) m_Canvas.sortingOrder = sortingOrder;
            if (m_TargetRenderers != null)
            {
                for (var i = 0; i < m_TargetRenderers.Length; i ++)
                {
                    if (m_TargetRenderers[i] == null) continue;
                    m_TargetRenderers[i].sortingOrder = sortingOrder;
                }
            }
        }
        
        
#if UNITY_EDITOR
        [ContextMenu("Regist Renderers")]
        private void ContextMenu_RegistRenderers()
        {
            var list = new List<Renderer>(m_TargetRenderers);
            for (var i = 0; i < list.Count; i ++)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    i --;
                }
            }
            
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                if (list.Contains(renderer) == true) continue;

                var script = renderer.GetComponentInParent<SyncCanvasSortingOrder>();
                if (script != this) continue;
                
                list.Add(renderer);
            }
            m_TargetRenderers = list.ToArray();
        }
#endif

    }
}