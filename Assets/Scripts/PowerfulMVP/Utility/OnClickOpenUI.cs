using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace PowerfulMVP
{
    public class OnClickOpenUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private string targetUIName;

        private Presenter m_Presenter = null;
        
        private string m_CachedUIName = null;
        private System.Type m_CachedUIType = null;

        private void Start()
        {
            m_Presenter = GetComponentInParent<Presenter>();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => Execute();

        public bool Cache()
        {
            if (m_Presenter == null)
                m_Presenter = GetComponentInParent<Presenter>();

            m_CachedUIType = null;
            if (string.IsNullOrWhiteSpace(targetUIName) == false)
            {
                //일단 깡으로 찾는다.
                m_CachedUIType = GetType().Assembly.GetType(targetUIName);
            
                //못찾았다면 네임스페이스 넣고 다시 찾아본다.
                if (m_CachedUIType == null)
                    m_CachedUIType = GetType().Assembly.GetType($"{m_Presenter.GetType().Namespace}.{targetUIName}");
            
                //Presenter를 상속받는 타입이 아니면 다시 되돌린다.
                if (m_CachedUIType != null && m_CachedUIType.IsSubclassOf(typeof(Presenter)) == false)
                    m_CachedUIType = null;
            }
            
            m_CachedUIName = targetUIName;

            return m_CachedUIType != null;
        }

        public void Execute()
        {
            if (m_Presenter == null || m_Presenter.manager == null)
                return;
            
            if (string.IsNullOrWhiteSpace(m_CachedUIName) || m_CachedUIName == targetUIName)
            {
                if (Cache() == false)
                    return;
            }

            if (m_CachedUIType == null)
                return;
            
            m_Presenter.manager.Open(m_CachedUIType);
        }
    }
}
