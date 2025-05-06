using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PowerfulMVP
{
    public class OnClickCloseUI : MonoBehaviour, IPointerClickHandler
    {
        private Presenter m_Presenter;
        
        private void Start()
        {
            m_Presenter = GetComponentInParent<Presenter>();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => Execute();
        
        public void Execute()
        {
            if (Application.isPlaying == false)
                return;
            
            if (m_Presenter == null)
                return;
            
            m_Presenter.CallClose();
        }
    }
}
