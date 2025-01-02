using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PowerfulUI
{
    [ExecuteAlways]
    public class SelectableTransitionApplier : MonoBehaviour, ISelectableTransitionApplier
    {
        // Type of the transition that occurs when the button state changes.
        [FormerlySerializedAs("transition")]
        [SerializeField]
        private UnityEngine.UI.Selectable.Transition m_Transition = UnityEngine.UI.Selectable.Transition.ColorTint;

        // Colors used for a color tint-based transition.
        [FormerlySerializedAs("colors")]
        [SerializeField]
        private ColorBlock m_Colors = ColorBlock.defaultColorBlock;

        // Sprites used for a Image swap-based transition.
        [FormerlySerializedAs("spriteState")]
        [SerializeField]
        private SpriteState m_SpriteState;

        [FormerlySerializedAs("animationTriggers")]
        [SerializeField]
        private AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

        // Graphic that will be colored.
        [FormerlySerializedAs("highlightGraphic")]
        [FormerlySerializedAs("m_HighlightGraphic")]
        [SerializeField]
        private Graphic m_TargetGraphic;
        
        
        
        public UnityEngine.UI.Selectable.Transition        transition        { get { return m_Transition; } set { if (SetPropertyUtility_SetStruct(ref m_Transition, value))        OnSetProperty(); } }

        public ColorBlock        colors            { get { return m_Colors; } set { if (SetPropertyUtility_SetStruct(ref m_Colors, value))            OnSetProperty(); } }

        public SpriteState       spriteState       { get { return m_SpriteState; } set { if (SetPropertyUtility_SetStruct(ref m_SpriteState, value))       OnSetProperty(); } }

        public AnimationTriggers animationTriggers { get { return m_AnimationTriggers; } set { if (SetPropertyUtility_SetClass(ref m_AnimationTriggers, value)) OnSetProperty(); } }

        public Graphic           targetGraphic     { get { return m_TargetGraphic; } set { if (SetPropertyUtility_SetClass(ref m_TargetGraphic, value))     OnSetProperty(); } }

        public Image image
        {
            get { return m_TargetGraphic as Image; }
            set { m_TargetGraphic = value; }
        }
        
#if PACKAGE_ANIMATION
        public Animator animator
        {
            get { return GetComponent<Animator>(); }
        }
#endif

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
            Sprite transitionSprite;
            string triggerName;

            switch ((SelectionState)state)
            {
                case SelectionState.Normal:
                    tintColor = m_Colors.normalColor;
                    transitionSprite = null;
                    triggerName = m_AnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = m_Colors.highlightedColor;
                    transitionSprite = m_SpriteState.highlightedSprite;
                    triggerName = m_AnimationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = m_Colors.pressedColor;
                    transitionSprite = m_SpriteState.pressedSprite;
                    triggerName = m_AnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = m_Colors.selectedColor;
                    transitionSprite = m_SpriteState.selectedSprite;
                    triggerName = m_AnimationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = m_Colors.disabledColor;
                    transitionSprite = m_SpriteState.disabledSprite;
                    triggerName = m_AnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            switch (m_Transition)
            {
                case UnityEngine.UI.Selectable.Transition.ColorTint:
                    StartColorTween(tintColor * m_Colors.colorMultiplier, instant);
                    break;
                case UnityEngine.UI.Selectable.Transition.SpriteSwap:
                    DoSpriteSwap(transitionSprite);
                    break;
                case UnityEngine.UI.Selectable.Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }
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

            m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);
        }

        void DoSpriteSwap(Sprite newSprite)
        {
            if (image == null)
                return;

            image.overrideSprite = newSprite;
        }

        void TriggerAnimation(string triggername)
        {
#if PACKAGE_ANIMATION
            if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
                return;

            animator.ResetTrigger(m_AnimationTriggers.normalTrigger);
            animator.ResetTrigger(m_AnimationTriggers.highlightedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.pressedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.selectedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.disabledTrigger);

            animator.SetTrigger(triggername);
#endif
        }
        
        private void OnValidate()
        {
            DoStateTransition(m_LastState, true);
        }
        
        public static bool SetPropertyUtility_SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }
        
        public static bool SetPropertyUtility_SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }

}
