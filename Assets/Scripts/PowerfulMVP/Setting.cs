using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PowerfulMVP
{
    [System.Serializable]
    public struct DepthGroupSettingData
    {
        public int depthGroupID;
        public string name;
    }
    
    public class Setting : ScriptableObject
    {
        [Header("Root Settings")]
        [SerializeField] private GameObject m_CameraPrefab;
        public GameObject cameraPrefab => m_CameraPrefab;
        [SerializeField] private GameObject m_EventSystemPrefab;
        public GameObject EventSystemPrefab => m_EventSystemPrefab;
        
        
        [Header("Default Canvas Settings")]
        [SerializeField] private Canvas m_DefaultCanvas;
        public Canvas defaultCanvas => m_DefaultCanvas;
        [SerializeField] private CanvasScaler m_DefaultCanvasScaler;
        public CanvasScaler defaultCanvasScaler => m_DefaultCanvasScaler;
        [SerializeField] private GraphicRaycaster m_DefaultGraphicRaycaster;
        public GraphicRaycaster defaultGraphicRaycaster => m_DefaultGraphicRaycaster;
        
        
        [Header("Sorting Orders")]
        [Tooltip("DepthGroup당 SortingOrder 증가량")] public int sortingOrderPerDepthGroup = 100;
        [Tooltip("UI당 SortingOrder 증가량")] public int sortingOrderPerUI = 10;
        
        
        [Header("Depth Group Settings")]
        [SerializeField] private DepthGroupSettingData[] m_DepthGroupSettings;
        public DepthGroupSettingData[] depthGroupSettings => m_DepthGroupSettings;

        

        
        
        private Dictionary<int, DepthGroupSettingData> m_DicDepthGroupSettings = null;

        private void CacheDepthGroupSettings()
        {
            if (m_DicDepthGroupSettings == null) m_DicDepthGroupSettings = new Dictionary<int, DepthGroupSettingData>();
            m_DicDepthGroupSettings.Clear();
            
            if (m_DepthGroupSettings != null)
            {
                for (var i = 0; i < m_DepthGroupSettings.Length; i ++)
                    m_DicDepthGroupSettings[m_DepthGroupSettings[i].depthGroupID] = m_DepthGroupSettings[i];
            }
        }
        
        public int GetDepthGroupIndex(int depthID)
        {
            if (m_DepthGroupSettings == null)
                return -1;

            for (var i = 0; i < m_DepthGroupSettings.Length; i ++)
            {
                if (m_DepthGroupSettings[i].depthGroupID == depthID)
                    return i;
            }
            return -1;
        }

        public int GetDepthGroupIndexLength() => m_DepthGroupSettings?.Length ?? -1;
        
        public DepthGroupSettingData GetDepthGroupSettingData(int depthID)
        {
            if (m_DicDepthGroupSettings == null)
                CacheDepthGroupSettings();

            return m_DicDepthGroupSettings.ContainsKey(depthID) ? m_DicDepthGroupSettings[depthID] : default;
        }

        
        
        private void OnValidate()
        {
            m_DicDepthGroupSettings = null;
        }
        
    }
}
