using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public int sortingOrderPerDepth = 10000;
        public int sortingOrderPerUI = 100;

        [SerializeField] private DepthSettingData[] m_DepthSettings;
        public DepthSettingData[] depthSettings => m_DepthSettings;

        
        
        private Dictionary<int, DepthSettingData> m_DicDepthSettings = null;
        
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
        
        public DepthSettingData GetDepthSettingData(int depthID)
        {
            if (m_DicDepthSettings == null)
            {
                m_DicDepthSettings = new Dictionary<int, DepthSettingData>();
                if (m_DepthSettings != null)
                {
                    for (var i = 0; i < m_DepthSettings.Length; i ++)
                        m_DicDepthSettings[m_DepthSettings[i].depthID] = m_DepthSettings[i];
                }
            }

            return m_DicDepthSettings.ContainsKey(depthID) ? m_DicDepthSettings[depthID] : default;
        }

        
        
        private void OnValidate()
        {
            m_DicDepthSettings = null;
        }
        
    }
}
