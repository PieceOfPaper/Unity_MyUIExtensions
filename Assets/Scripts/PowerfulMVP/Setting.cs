using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PowerfulMVP
{
    [System.Serializable]
    public struct DepthSettingData
    {
        public int depthID;
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
        [Tooltip("Depth당 SortingOrder 증가량")] public int sortingOrderPerDepth = 100;
        [Tooltip("UI당 SortingOrder 증가량")] public int sortingOrderPerUI = 10;
        
        
        [Header("Depth Settings")]
        [SerializeField] private DepthSettingData[] m_DepthSettings;
        public DepthSettingData[] depthSettings => m_DepthSettings;

        

        
        
        private Dictionary<int, DepthSettingData> m_DicDepthSettings = null;

        private void CacheDepthSettings()
        {
            if (m_DicDepthSettings == null) m_DicDepthSettings = new Dictionary<int, DepthSettingData>();
            m_DicDepthSettings.Clear();
            
            if (m_DepthSettings != null)
            {
                for (var i = 0; i < m_DepthSettings.Length; i ++)
                    m_DicDepthSettings[m_DepthSettings[i].depthID] = m_DepthSettings[i];
            }
        }
        
        public int GetDepthIndex(int depthID)
        {
            if (m_DepthSettings == null)
                return -1;

            for (var i = 0; i < m_DepthSettings.Length; i ++)
            {
                if (m_DepthSettings[i].depthID == depthID)
                    return i;
            }
            return -1;
        }

        public int GetDepthIndexLength() => m_DepthSettings?.Length ?? -1;
        
        public DepthSettingData GetDepthSettingData(int depthID)
        {
            if (m_DicDepthSettings == null)
                CacheDepthSettings();

            return m_DicDepthSettings.ContainsKey(depthID) ? m_DicDepthSettings[depthID] : default;
        }

        
        
        private void OnValidate()
        {
            m_DicDepthSettings = null;
        }
        
    }
}
