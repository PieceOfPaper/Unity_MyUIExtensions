using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerfulMVP
{
    public interface IUIManagerPresenterSetter
    {
        void Initialize(string ui_name, UIManager manager, Presenter.Context context);
        void Open();
        void Close();
        bool EscapeKey();
    }
    
    public abstract class Presenter : MonoBehaviour, IUIManagerPresenterSetter
    {
        public struct Setting
        {
            public int depthID;
        }
        public abstract Setting setting { get; }
        
        public abstract class Context
        {
        
        }

        private string m_UIName;
        public string ui_name => m_UIName;
        
        protected UIManager m_Manager;
        protected Context m_Context;

        private OpenHandler m_OpenHandler;
        public OpenHandler openHandler => m_OpenHandler;

        public OpenHandler.State openState
        {
            get
            {
                if (m_OpenHandler != null)
                    return m_OpenHandler.state;

                return gameObject.activeSelf ? OpenHandler.State.Opened : OpenHandler.State.Closed;
            }
        }

        private Canvas m_Canvas;
        public Canvas canvas => m_Canvas;

        private RectTransform m_RectTransform;
        public RectTransform rectTransform => m_RectTransform;

        private bool m_IsInitialized = false;
        public bool isInitialized => m_IsInitialized;
        
        
        void IUIManagerPresenterSetter.Initialize(string ui_name, UIManager manager, Context context)
        {
            m_UIName = ui_name;
            m_Manager = manager;
            m_Context = context;

            m_OpenHandler = GetComponent<OpenHandler>();
            
            m_Canvas = GetComponent<Canvas>();
            if (m_Canvas == null)
            {
                m_Canvas = gameObject.AddComponent<Canvas>();
            }
            
            m_RectTransform = GetComponent<RectTransform>();
            if (m_RectTransform == null)
            {
                m_RectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            OnInitialize(context);
            m_IsInitialized = true;
        }
        
        void IUIManagerPresenterSetter.Open()
        {
            if (m_OpenHandler != null) m_OpenHandler.Open();
            else gameObject.SetActive(true);
            
            OnOpen();
        }
        
        void IUIManagerPresenterSetter.Close()
        {
            if (m_OpenHandler != null) m_OpenHandler.Close();
            else gameObject.SetActive(false);
            
            OnClose();
        }

        bool IUIManagerPresenterSetter.EscapeKey() => OnEscapeKey();
        
        protected virtual void OnInitialize(Context context) { }
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }

        protected virtual bool OnEscapeKey()
        {
            switch (openState)
            {
                case OpenHandler.State.Closed:
                case OpenHandler.State.Closing:
                    m_Manager.Close(m_UIName);
                    return true;
            }
            
            return false;
        }
    }
    
    public abstract class PresenterTemplate<T, CT> : Presenter
        where T : Presenter
        where CT : Presenter.Context
    {
        public CT myContext => m_Context == null ? null : m_Context as CT;

        protected override void OnInitialize(Context context)
        {
            base.OnInitialize(context);

            if (context == null)
            {
                m_Context = m_Manager.GetPresenterContext<CT>(ui_name);
            }
        }
    }

}
