using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace PowerfulUI
{
    [AddComponentMenu("UI/Powerful/Powerful.Selectable", 35)]
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Selectable : UnityEngine.UI.Selectable, IPointerClickHandler, ISubmitHandler
    {
        [FormerlySerializedAs("onClick")] [SerializeField] private UnityEvent m_OnClick = new UnityEvent();
        public UnityEvent onClick { get => m_OnClick; set => m_OnClick = value; }

        // Options
        [SerializeField] private bool m_OnSubmitToClick = false;
        public bool onSubmitToClick { get => m_OnSubmitToClick; set => m_OnSubmitToClick = value; }
        [SerializeField] private bool m_EnableEventOnDisabled = false;
        public bool enableEventOnDisabled { get => m_EnableEventOnDisabled; set => m_EnableEventOnDisabled = value; }

        // Long Press
        [SerializeField] private bool m_EnableLongPress = false;
        public bool enableLongPress { get => m_EnableLongPress; set { if (SetPropertyUtility_SetStruct( ref m_EnableLongPress, value)) ResetLongPressState(); } }
        [FormerlySerializedAs("onBeginLongPress")] [SerializeField] private UnityEvent m_OnBeginLongPress = new UnityEvent();
        public UnityEvent onBeginLongPress { get => m_OnBeginLongPress; set => m_OnBeginLongPress = value; }
        [FormerlySerializedAs("onEndLongPress")] [SerializeField] private UnityEvent m_OnEndLongPress = new UnityEvent();
        public UnityEvent onEndLongPress { get => m_OnEndLongPress; set => m_OnEndLongPress = value; }


        private SelectableTransitionApplier[] m_CachedTransitionApplier;
        private SelectableColorApplier[] m_CachedColorApplier;
        private int m_LastTransitionState;
        private List<ISelectableTransitionApplier> m_TransitionAppliers = new List<ISelectableTransitionApplier>();


        private const float LONGPRESS_READY_TIME = 1.0f; //TODO - EventSystem 확장으로 빼면 좋겠다.
        public enum LongPressState
        {
            None,
            Ready,
            Begun,
            Ended,
        }
        private LongPressState m_LongPressState = 0;
        public LongPressState longPressState => m_LongPressState;
        
        private int m_LongPressPointerID = 0;
        private float m_LongPressStartTime = 0f;
        private Vector2 m_LongPressStartPoint = Vector2.zero;

        public float LongPressTime
        {
            get
            {
                switch (m_LongPressState)
                {
                    case LongPressState.Ready:
                        return Time.realtimeSinceStartup - m_LongPressStartTime;
                    case LongPressState.Begun:
                    case LongPressState.Ended:
                        return Time.realtimeSinceStartup - m_LongPressStartTime - LONGPRESS_READY_TIME;
                    default:
                        return 0f;
                }
            }
        }
        
        
        protected Selectable() { }

        protected virtual void Update()
        {
            if (Application.isPlaying == true)
            {
                if (m_EnableLongPress == true)
                {
                    //드래그 체크
                    if (m_LongPressState == LongPressState.Ready || m_LongPressState == LongPressState.Begun)
                    {
                        var currentPointerPosition = UIUtil.GetPoinsterPosition(m_LongPressPointerID);
                        var deltaSqr = (currentPointerPosition - m_LongPressStartPoint).sqrMagnitude;
                        var dragThreshold = EventSystem.current == null ? 0 : EventSystem.current.pixelDragThreshold;
                        if (deltaSqr >= (dragThreshold * dragThreshold))
                            EndLongPress();
                    }
                    
                    if (m_LongPressState == LongPressState.Ready && (Time.realtimeSinceStartup - m_LongPressStartTime) >= LONGPRESS_READY_TIME)
                    {
                        m_LongPressState = LongPressState.Begun;
                        OnProcessBeginLongPress();
                    }
                }
            }
        }


        public void RegistTransitionApplier(ISelectableTransitionApplier transitionApplier)
        {
            if (transitionApplier == null)
                return;
            
            m_TransitionAppliers.Add(transitionApplier);
            transitionApplier.DoStateTransition(m_LastTransitionState, true);
        }

        public void UnregistTransitionApplier(ISelectableTransitionApplier transitionApplier)
        {
            if (transitionApplier == null)
                return;

            m_TransitionAppliers.Remove(transitionApplier);
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            m_LastTransitionState = (int)state;
            for (var i = 0; i < m_TransitionAppliers.Count; i ++)
            {
                if (m_TransitionAppliers[i] == null) continue;
                m_TransitionAppliers[i].DoStateTransition(m_LastTransitionState, instant);
            }
        }


        protected virtual void ResetLongPressState()
        {
            EndLongPress();
            
            m_LongPressPointerID = 0;
            m_LongPressStartTime = 0f;
            m_LongPressStartPoint = Vector2.zero;
        }

        protected virtual void EndLongPress()
        {
            if (m_LongPressState == LongPressState.Begun)
            {
                m_LongPressState = LongPressState.Ended;
                OnProcessEndLongPress();
            }
            else
            {
                m_LongPressState = LongPressState.None;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (m_EnableLongPress == true)
            {
                EndLongPress();
                
                m_LongPressState = LongPressState.Ready;
                m_LongPressPointerID = eventData.pointerId;
                m_LongPressStartTime = Time.realtimeSinceStartup;
                m_LongPressStartPoint = eventData.position;
                OnProcessReadyLongPress();
            }
            
            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
                OnProcessPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (m_EnableLongPress == true && m_LongPressPointerID == eventData.pointerId)
                EndLongPress();
            
            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
                OnProcessPointerUp(eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
                OnProcessPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (m_EnableLongPress == true && m_LongPressPointerID == eventData.pointerId)
                EndLongPress();
            
            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
                OnProcessPointerExit(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (!IsActive() || !IsInteractable())
                return;

            if (m_LongPressState >= LongPressState.Begun)
                return;

            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
            {
                UISystemProfilerApi.AddMarker("Button.onClick", this);
                m_OnClick.Invoke();
                OnProcessPointerClick(eventData);
            }
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (m_OnSubmitToClick == false)
                return;
            
            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            m_OnClick.Invoke();
            OnProcessPointerClick(eventData);

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        
        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        protected virtual void OnProcessPointerDown(PointerEventData eventData) { }

        protected virtual void OnProcessPointerUp(PointerEventData eventData) { }

        protected virtual void OnProcessPointerEnter(PointerEventData eventData) { }

        protected virtual void OnProcessPointerExit(PointerEventData eventData) { }

        protected virtual void OnProcessPointerClick(BaseEventData eventData) { }

        protected virtual void OnProcessReadyLongPress() { }

        protected virtual void OnProcessBeginLongPress() { }

        protected virtual void OnProcessEndLongPress() { }



        public static bool SetPropertyUtility_SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}
