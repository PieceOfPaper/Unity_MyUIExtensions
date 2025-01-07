using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class RectTransformDimensionsChangeListener : UIBehaviour
    {
        public UnityEvent onChange = new UnityEvent();
        
        private RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (Application.isPlaying == false)
                    return GetComponent<RectTransform>();
                
                if (m_RectTransform == null)
                    m_RectTransform = transform as RectTransform;
                return m_RectTransform;
            }
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            onChange?.Invoke();
        }
        
    }
}
