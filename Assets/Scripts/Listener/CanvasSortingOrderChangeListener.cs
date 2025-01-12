using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class CanvasSortingOrderChangeListener : MonoBehaviour
    {
        public ChangeEvent onChange = new ChangeEvent();
        
        [System.Serializable]
        public class ChangeEvent : UnityEvent<int> { }
        
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


        private int m_PrevSortingOrder = 0;
        
        private void OnEnable()
        {
            m_PrevSortingOrder = canvas.sortingOrder;
        }

        private void LateUpdate()  => CheckSortingOrder();

        private void OnPreRender() => CheckSortingOrder();

        private void CheckSortingOrder()
        {
            if (m_PrevSortingOrder != canvas.sortingOrder)
            {
                onChange?.Invoke(canvas.sortingOrder);
                Canvas.ForceUpdateCanvases();
                m_PrevSortingOrder = canvas.sortingOrder;
            }
        }
    }
}
