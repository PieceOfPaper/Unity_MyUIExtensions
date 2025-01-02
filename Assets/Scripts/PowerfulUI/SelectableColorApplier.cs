using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PowerfulUI
{
    [ExecuteAlways]
    public class SelectableColorApplier : MonoBehaviour, ISelectableTransitionApplier
    {
        // Colors used for a color tint-based transition.
        [FormerlySerializedAs("colors")]
        [SerializeField]
        private ColorBlock m_Colors = ColorBlock.defaultColorBlock;
        
        // Graphic that will be colored.
        [FormerlySerializedAs("highlightGraphic")]
        [FormerlySerializedAs("m_HighlightGraphic")]
        [SerializeField]
        private Graphic m_TargetGraphic;
        
        
        public ColorBlock        colors            { get { return m_Colors; } set { if (UIUtil.SetStruct(ref m_Colors, value))            OnSetProperty(); } }

        public Graphic           targetGraphic     { get { return m_TargetGraphic; } set { if (UIUtil.SetClass(ref m_TargetGraphic, value))     OnSetProperty(); } }


        private Selectable m_Selectable;
        private int m_LastState = 0;

        private void OnEnable()
        {
            m_Selectable = GetComponentInParent<Selectable>();
            if (m_Selectable != null) m_Selectable.RegistTransitionApplier(this);
        }

        private void OnDisable()
        {
            if (m_Selectable != null) m_Selectable.UnregistTransitionApplier(this);
        }
        
        private void OnSetProperty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DoStateTransition(m_LastState, true);
            else
#endif
                DoStateTransition(m_LastState, false);
        }
        
        public void DoStateTransition(int state, bool instant)
        {
            m_LastState = state;
            if (!gameObject.activeInHierarchy)
                return;

            Color tintColor;

            switch ((SelectionState)state)
            {
                case SelectionState.Normal:
                    tintColor = m_Colors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    tintColor = m_Colors.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    tintColor = m_Colors.pressedColor;
                    break;
                case SelectionState.Selected:
                    tintColor = m_Colors.selectedColor;
                    break;
                case SelectionState.Disabled:
                    tintColor = m_Colors.disabledColor;
                    break;
                default:
                    tintColor = Color.black;
                    break;
            }

            StartColorTween(tintColor * m_Colors.colorMultiplier, instant);
        }
        
        /// <summary>
        /// An enumeration of selected states of objects
        /// </summary>
        protected enum SelectionState
        {
            /// <summary>
            /// The UI object can be selected.
            /// </summary>
            Normal,

            /// <summary>
            /// The UI object is highlighted.
            /// </summary>
            Highlighted,

            /// <summary>
            /// The UI object is pressed.
            /// </summary>
            Pressed,

            /// <summary>
            /// The UI object is selected
            /// </summary>
            Selected,

            /// <summary>
            /// The UI object cannot be selected.
            /// </summary>
            Disabled,
        }
        
        void StartColorTween(Color targetColor, bool instant)
        {
            if (m_TargetGraphic == null)
                return;

            // m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);

            if (instant || Application.isPlaying == false)
            {
                m_TargetGraphic.color = targetColor;
            }
            else
            {
                StopCoroutine("DoColorTween");
                StartCoroutine(DoColorTween(targetColor));
            }
        }

        IEnumerator DoColorTween(Color targetColor)
        {
            var startColor = m_TargetGraphic.color;
            var endColor = targetColor;
            var time = 0f;
            while (time < m_Colors.fadeDuration)
            {
                m_TargetGraphic.color = Color.Lerp(startColor, endColor, time / m_Colors.fadeDuration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
            m_TargetGraphic.color = endColor;
        }

        private void OnValidate()
        {
            DoStateTransition(m_LastState, true);
        }
    }

}
