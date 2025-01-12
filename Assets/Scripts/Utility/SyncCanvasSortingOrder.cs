using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Canvas))]
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
        
        private Canvas m_Canvas;
        public Canvas canvas
        {
            get
            {
                if (Application.isPlaying == false)
                    return GetComponent<Canvas>();

                if (m_Canvas == null)
                    m_Canvas = GetComponent<Canvas>();
                return m_Canvas;
            }
        }

        private Canvas m_TargetCanvas;
        private CanvasSortingOrderChangeListener m_TargetCanvasSortingOrderListener;

        protected override void OnEnable()
        {
            base.OnEnable();
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
                if (canvas.isRootCanvas == false)
                {
                    m_TargetCanvas = canvas.rootCanvas;
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
            canvas.sortingOrder = (m_TargetCanvas == null ? 0 : m_TargetCanvas.sortingOrder) + m_AddSortingOrder;
        }

    }
}