using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    // [AddComponentMenu("Layout/Aspect Ratio Contains Scaler", 142)]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class AspectRatioContainsScaler : UIBehaviour, ILayoutSelfController
    {
        [SerializeField] private float m_ScaleMin = 0.001f;
        public float scaleMin { get { return m_ScaleMin; } set { if (SetPropertyUtility_SetStruct(ref m_ScaleMin, value)) SetDirty(); } }
        [SerializeField] private float m_ScaleMax = 10f;
        public float scaleMax { get { return m_ScaleMax; } set { if (SetPropertyUtility_SetStruct(ref m_ScaleMax, value)) SetDirty(); } }
        [Range(0f, 1f)] [SerializeField] private float m_ScaleRatio = 1f;
        public float scaleRatio { get { return m_ScaleRatio; } set { if (SetPropertyUtility_SetStruct(ref m_ScaleRatio, value)) SetDirty(); } }
        [SerializeField] private bool m_ContainsHorizontal = true;
        public bool containsHorizontal { get { return m_ContainsHorizontal; } set { if (SetPropertyUtility_SetStruct(ref m_ContainsHorizontal, value)) SetDirty(); } }
        [SerializeField] private bool m_ContainsVertical = true;
        public bool containsVertical { get { return m_ContainsVertical; } set { if (SetPropertyUtility_SetStruct(ref m_ContainsVertical, value)) SetDirty(); } }

        [System.NonSerialized]
        private RectTransform m_Rect;

        // This "delayed" mechanism is required for case 1014834.
        private bool m_DelayedSetDirty = false;

        private RectTransform m_ParentRectTransform;
        private Vector2 m_PrevParentSize = Vector2.zero;

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

        protected AspectRatioContainsScaler() {}

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ParentRectTransform = rectTransform.parent == null ? null : rectTransform.parent as RectTransform;
            SetDirty();
        }

        protected override void Start()
        {
            base.Start();
            //Disable the component if the aspect mode is not valid or the object state/setup is not supported with AspectRatio setup.
            if (!IsComponentValidOnObject() || m_ParentRectTransform == null)
                this.enabled = false;
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            m_ParentRectTransform = rectTransform.parent == null ? null : rectTransform.parent as RectTransform;
            SetDirty();
        }

        /// <summary>
        /// Update the rect based on the delayed dirty.
        /// Got around issue of calling onValidate from OnEnable function.
        /// </summary>
        protected virtual void Update()
        {
            if (m_PrevParentSize != GetParentSize())
            {
                m_PrevParentSize = GetParentSize();
                m_DelayedSetDirty = true;
            }
            
            if (m_DelayedSetDirty)
            {
                m_DelayedSetDirty = false;
                SetDirty();
            }
        }

        /// <summary>
        /// Function called when this RectTransform or parent RectTransform has changed dimensions.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        private void UpdateRect()
        {
            if (!IsActive() || !IsComponentValidOnObject())
                return;

            m_Tracker.Clear();

            if (m_ContainsHorizontal == false && m_ContainsVertical == false)
                return;
            
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors);
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Pivot);
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.AnchoredPosition);
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Scale);

            rectTransform.anchorMin = Vector2.one * 0.5f;
            rectTransform.anchorMax = Vector2.one * 0.5f;
            rectTransform.pivot = Vector2.one * 0.5f;
            rectTransform.anchoredPosition = Vector2.one * 0.5f;

            var parentSize = GetParentSize();
            var scale = m_ScaleMax;

            if (m_ContainsHorizontal == true)
            {
                scale = Mathf.Min(scale, parentSize.x / rectTransform.rect.width);
            }
            if (m_ContainsVertical == true)
            {
                scale = Mathf.Min(scale, parentSize.y / rectTransform.rect.height);
            }

            rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, Mathf.Max(scale, m_ScaleMin), m_ScaleRatio);
        }

        private Vector2 GetParentSize()
        {
            return !m_ParentRectTransform ? Vector2.zero : m_ParentRectTransform.rect.size;
        }

        /// <summary>
        /// Method called by the layout system. Has no effect
        /// </summary>
        public virtual void SetLayoutHorizontal() {}

        /// <summary>
        /// Method called by the layout system. Has no effect
        /// </summary>
        public virtual void SetLayoutVertical() {}

        /// <summary>
        /// Mark the AspectRatioFitter as dirty.
        /// </summary>
        protected void SetDirty()
        {
            UpdateRect();
        }

        public bool IsComponentValidOnObject()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas && canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            {
                return false;
            }
            return true;
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
