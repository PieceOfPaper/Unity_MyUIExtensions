using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace PowerfulUI
{
    [RequireComponent(typeof(RectTransform))]
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] private int m_Index = 0;
        public int index { get => m_Index; set => Set(value); }

        [SerializeField] private bool m_ValueChangeOnClick = false;
        public bool valueChangeOnClick => m_ValueChangeOnClick;
        
        [Serializable] public class ClickEvent : UnityEvent<int> { }
        [FormerlySerializedAs("onClick")] [SerializeField] private ClickEvent m_OnClick = new ClickEvent();
        public ClickEvent onClick { get => m_OnClick; set => m_OnClick = value; }
        
        [Serializable] public class ValueChangedEvent : UnityEvent<int> { }
        [FormerlySerializedAs("onValueChanged")] [SerializeField] private ValueChangedEvent m_OnValueChanged = new ValueChangedEvent();
        public ValueChangedEvent onValueChanged { get => m_OnValueChanged; set => m_OnValueChanged = value; }
        
        private List<Tab> m_Tabs = new List<Tab>();

        private void OnEnable()
        {
            ApplyTabs();
        }

        public void Regist(Tab tab)
        {
            if (tab == null)
                return;
            
            if (m_Tabs.Contains(tab))
                return;
            
            m_Tabs.Add(tab);
            tab.isOn = tab.index == m_Index;
        }

        public void Unregist(Tab tab)
        {
            if (tab == null)
                return;

            m_Tabs.Remove(tab);
        }

        
        public void SetIsOnWithoutNotify(int value)
        {
            Set(value, false);
        }

        private void Set(int value, bool sendCallback = true)
        {
            if (m_Index == value)
                return;

            m_Index = value;

            if (isActiveAndEnabled == true)
                ApplyTabs();
            
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Powerful.TabGroup.value", this);
                onValueChanged.Invoke(m_Index);
            }
        }

        private void ApplyTabs()
        {
            for (int i = 0; i < m_Tabs.Count; i ++)
            {
                if (m_Tabs[i] == null) continue;
                m_Tabs[i].isOn = m_Tabs[i].index == m_Index;
            }
        }
    }
}
