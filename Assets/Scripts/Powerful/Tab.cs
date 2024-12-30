using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Powerful
{
    [RequireComponent(typeof(RectTransform))]
    public class Tab : Selectable
    {
        [SerializeField] private int m_Index = 0;
        public int index => m_Index;

        [SerializeField] private TabGroup m_Group;
        public TabGroup group { get => m_Group; set => SetGroup(value); }
        
        [SerializeField] private GameObject[] m_Ons;
        [SerializeField] private GameObject[] m_Offs;
        
        [SerializeField] private bool m_IsOn = false;
        public bool isOn { get => m_IsOn; set => Set(value); }

        [System.Serializable] public class ValueChangedEvent : UnityEvent<bool> { }
        [FormerlySerializedAs("onValueChanged")] [SerializeField] private ValueChangedEvent m_OnValueChanged = new ValueChangedEvent();
        public ValueChangedEvent onValueChanged { get => m_OnValueChanged; set => m_OnValueChanged = value; }


        private Graphic[] m_OnGraphics;
        private Graphic[] m_OffGraphics;
        
        
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (m_Group != null)
                m_Group.Regist(this);
            
            PlayEffect(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (m_Group != null)
                m_Group.Unregist(this);
        }

        /// <summary>
        /// Assume the correct visual state.
        /// </summary>
        protected override void Start()
        {
            PlayEffect(true);
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            Set(value, false);
        }
        
        private void Set(bool value, bool sendCallback = true)
        {
            if (m_IsOn == value)
                return;
            
            m_IsOn = value;

            PlayEffect(true);
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Powerful.Tab.value", this);
                onValueChanged.Invoke(m_IsOn);
            }
        }

        /// <summary>
        /// Play the appropriate effect.
        /// </summary>
        private void PlayEffect(bool instant)
        {
            if (Application.isPlaying == false || m_OnGraphics == null)
            {
                var list = ListPool<Graphic>.Get();
                if (m_Ons != null)
                {
                    for (var i = 0; i < m_Ons.Length; i ++)
                    {
                        if (m_Ons[i] == null) continue;
                        var graphics = m_Ons[i].GetComponentsInChildren<Graphic>(true);
                        foreach (var graphic in graphics)
                            if (list.Contains(graphic) == false)
                                list.Add(graphic);
                    }
                }
                m_OnGraphics = list.ToArray();
                ListPool<Graphic>.Release(list);
            }
            if (Application.isPlaying == false || m_OffGraphics == null)
            {
                var list = ListPool<Graphic>.Get();
                if (m_Offs != null)
                {
                    for (var i = 0; i < m_Offs.Length; i ++)
                    {
                        if (m_Offs[i] == null) continue;
                        var graphics = m_Ons[i].GetComponentsInChildren<Graphic>(true);
                        foreach (var graphic in graphics)
                            if (list.Contains(graphic) == false)
                                list.Add(graphic);
                    } 
                }
                m_OffGraphics = list.ToArray();
                ListPool<Graphic>.Release(list);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                for (int i = 0; i < m_OnGraphics.Length; i ++)
                {
                    if (m_OnGraphics[i] == null) continue;
                    m_OnGraphics[i].canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
                }
                for (int i = 0; i < m_OffGraphics.Length; i ++)
                {
                    if (m_OffGraphics[i] == null) continue;
                    m_OffGraphics[i].canvasRenderer.SetAlpha(m_IsOn ? 0f : 1f);
                }
            }
            else
#endif
            {
                for (int i = 0; i < m_OnGraphics.Length; i ++)
                {
                    if (m_OnGraphics[i] == null) continue;
                    m_OnGraphics[i].CrossFadeAlpha(m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, true);
                }
                for (int i = 0; i < m_OffGraphics.Length; i ++)
                {
                    if (m_OffGraphics[i] == null) continue;
                    m_OffGraphics[i].CrossFadeAlpha(m_IsOn ? 0f : 1f, instant ? 0f : 0.1f, true);
                }
            }
        }


        private void SetGroup(TabGroup group)
        {
            if (m_Group != null)
                m_Group.Unregist(this);
            m_Group = group;
            if (m_Group != null)
                m_Group.Regist(this);
        }
        

        protected override void OnProcessPointerClick(BaseEventData eventData)
        {
            if (m_Group != null)
            {
                if (m_Group.valueChangeOnClick)
                    m_Group.index = index;
                
                m_Group.onClick.Invoke(index);
            }
            
            base.OnProcessPointerClick(eventData);
        }
        
    }
}
