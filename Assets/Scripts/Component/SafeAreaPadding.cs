using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeAreaPadding : UIBehaviour, ILayoutSelfController
    {
        [SerializeField][Range(0f, 1f)] private float m_Top = 1f;
        [SerializeField][Range(0f, 1f)] private float m_Bottom = 1f;
        [SerializeField][Range(0f, 1f)] private float m_Left = 1f;
        [SerializeField][Range(0f, 1f)] private float m_Right = 1f;
        
        public float top { get => m_Top; set { if (SetPropertyUtility_SetStruct(ref m_Top, value)) SetDirty(); } }
        public float bottom { get => m_Bottom; set { if (SetPropertyUtility_SetStruct(ref m_Bottom, value)) SetDirty(); } }
        public float left { get => m_Left; set { if (SetPropertyUtility_SetStruct(ref m_Left, value)) SetDirty(); } }
        public float right { get => m_Right; set { if (SetPropertyUtility_SetStruct(ref m_Right, value)) SetDirty(); } }
        
        

        // This "delayed" mechanism is required for case 1014834.
        private bool m_DelayedSetDirty = false;

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        // field is never assigned warning
        #pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
        #pragma warning restore 649

        
        private RectTransform m_RootCanvasRectTransform;
        private RectTransformDimensionsChangeListener m_RootCanvasRectChangeListener;

        private Vector2 m_PrevRootCanvasSize;
        private Rect m_PrevSafeArea;
        
        
        protected SafeAreaPadding() {}

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateRootCanvas();
            SetDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            if (m_RootCanvasRectChangeListener != null)
                m_RootCanvasRectChangeListener.onChange.RemoveListener(OnRectTransformDimensionsChange);

            m_RootCanvasRectTransform = null;
            m_RootCanvasRectChangeListener = null;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            
            UpdateRootCanvas();
            SetDirty();
        }

        protected virtual void Update()
        {
            if (Application.isPlaying == false)
            {
                if (m_RootCanvasRectTransform != null)
                {
                    if (m_PrevSafeArea != Screen.safeArea || m_PrevRootCanvasSize != m_RootCanvasRectTransform.sizeDelta)
                    {
                        m_DelayedSetDirty = true;
                    }
                }
            }
            
            if (m_DelayedSetDirty)
            {
                m_DelayedSetDirty = false;
                SetDirty();
            }
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        private void UpdateRootCanvas()
        {
            if (m_RootCanvasRectChangeListener != null)
                m_RootCanvasRectChangeListener.onChange.RemoveListener(OnRectTransformDimensionsChange);
            m_RootCanvasRectChangeListener = null;
            
            var canvas = GetComponentInParent<Canvas>();
            var rootCanvas = canvas == null ? null : (canvas.isRootCanvas ? canvas : canvas.rootCanvas);
            m_RootCanvasRectTransform = rootCanvas == null ? null : rootCanvas.transform as RectTransform;

            if (Application.isPlaying == true)
            {
                if (m_RootCanvasRectTransform != null)
                {
                    m_RootCanvasRectChangeListener = m_RootCanvasRectTransform.GetComponent<RectTransformDimensionsChangeListener>();
                    if (m_RootCanvasRectChangeListener == null)
                        m_RootCanvasRectChangeListener = m_RootCanvasRectTransform.gameObject.AddComponent<RectTransformDimensionsChangeListener>();
                
                    m_RootCanvasRectChangeListener.onChange.AddListener(OnRectTransformDimensionsChange);
                }
            }
        }
        
        private void UpdateRect()
        {
            if (!IsActive())
                return;

            if (m_RootCanvasRectTransform == null)
                return;

            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors);
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.AnchoredPosition);
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Pivot);
;
            var scale = m_RootCanvasRectTransform.rect.width / Screen.width;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var safeArea = Screen.safeArea;
            safeArea.xMin *= m_Left;
            safeArea.yMin *= m_Bottom;
            safeArea.xMax = screenSize.x - (screenSize.x - safeArea.xMax) * m_Right;
            safeArea.yMax = screenSize.y - (screenSize.y - safeArea.yMax) * m_Top;
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = Vector2.one * 0.5f;
            rectTransform.anchoredPosition = (safeArea.center - screenSize * 0.5f) * scale;
            rectTransform.sizeDelta = (safeArea.size - screenSize) * scale;
            
            m_PrevSafeArea = Screen.safeArea;
            m_PrevRootCanvasSize = m_RootCanvasRectTransform.sizeDelta;
        }

        public virtual void SetLayoutHorizontal() {}

        public virtual void SetLayoutVertical() {}

        protected void SetDirty()
        {
            UpdateRect();
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_DelayedSetDirty = true;
        }

    #endif
        
        public static bool SetPropertyUtility_SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}
