using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Powerful
{
    // [AddComponentMenu("UI/Powerful/Selectable", 35)]
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Selectable : UnityEngine.UI.Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonClickedEvent : UnityEvent {}

        // Event delegates triggered on click.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();
        public ButtonClickedEvent onClick { get => m_OnClick; set => m_OnClick = value; }

        [SerializeField] private bool m_OnSubmitToClick = false;
        [SerializeField] private bool m_EnableEventOnDisabled = false;

        
        
        protected Selectable() { }
        

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
                OnProcessPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
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
            
            if (m_EnableEventOnDisabled == true || (IsActive() && IsInteractable()))
                OnProcessPointerExit(eventData);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (!IsActive() || !IsInteractable())
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
    }
}
