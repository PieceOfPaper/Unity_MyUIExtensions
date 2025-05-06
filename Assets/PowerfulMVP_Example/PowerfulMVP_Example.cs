using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerfulMVP.Example
{
    public class ExampleUIFactory : UIFactoryBase
    {
        public override IEnumerator LoadPrefab(string ui_name, Action<GameObject> callback)
        {
            var request = Resources.LoadAsync<GameObject>($"UIPrefabs/{ui_name}");
            yield return request;
            callback?.Invoke(request.asset == null ? null : request.asset as GameObject);
        }
    }
    
    public class PowerfulMVP_Example : MonoBehaviour
    {
        [SerializeField] private Setting m_Setting;
        
        // Start is called before the first frame update
        void Start()
        {
            var uiMgrObj = new GameObject("UIManager");
            var uiMgr = uiMgrObj.AddComponent<UIManager>();
            uiMgr.Initialize(m_Setting, new ExampleUIFactory());
            
            uiMgr.Open<MainMenu>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
