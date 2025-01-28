using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerfulMVP
{
    public class UIManager : MonoBehaviour
    {
        private Setting m_Setting;
        public Setting setting => m_Setting;
        

        private readonly Dictionary<int, Transform> m_DepthParents = new Dictionary<int, Transform>();
        
        private readonly Dictionary<string, Presenter> m_Presenters = new Dictionary<string, Presenter>();
        private readonly Dictionary<string, Presenter.Setting> m_PresenterSettings = new Dictionary<string, Presenter.Setting>();
        private readonly Dictionary<string, Presenter.Context> m_PresenterContexts = new Dictionary<string, Presenter.Context>();
        
        private readonly Dictionary<int, List<string>> m_EnabledPresenters = new Dictionary<int, List<string>>();
        private readonly Dictionary<string, Coroutine> m_OpeningPresenters = new Dictionary<string, Coroutine>();
        private readonly Dictionary<string, Coroutine> m_ClosingPresenters = new Dictionary<string, Coroutine>();


        private void Awake()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ProcessEscapeKey();
        }
        

        public void Initialize()
        {
            m_Setting = Resources.Load<Setting>("PowerfulMVPSetting");
            if (m_Setting == null) m_Setting = ScriptableObject.CreateInstance<Setting>();

            if (setting.depthSettings != null)
            {
                for (var i = 0; i < setting.depthSettings.Length; i ++)
                {
                    var depthSetting = setting.depthSettings[i];
                
                    var depthObj = new GameObject(string.IsNullOrWhiteSpace(depthSetting.name) ? depthSetting.depthID.ToString() : depthSetting.name);
                    var depthTransform = depthObj.transform;
                    depthTransform.SetParent(transform);
                    depthTransform.localPosition = Vector3.zero;
                    depthTransform.localRotation = Quaternion.identity;
                    depthTransform.localScale = Vector3.one;
                    depthTransform.SetAsLastSibling();
                    m_DepthParents[depthSetting.depthID] = depthTransform;
                }
            }
        }
        

        public void Open<T>() where T : Presenter => Open(typeof(T));

        public void Open(System.Type type) => Open(type == null ? null : type.Name);

        public void Open(string ui_name)
        {
            if (string.IsNullOrWhiteSpace(ui_name))
            {
                Debug.LogError("UI Name NULL");
                return;
            }

            if (m_OpeningPresenters.ContainsKey(ui_name))
            {
                Debug.LogError("이미 열리는 중.");
                return;
            }


            if (m_ClosingPresenters.ContainsKey(ui_name))
            {
                StopCoroutine(m_ClosingPresenters[ui_name]);
                m_ClosingPresenters.Remove(ui_name);
            }

            m_OpeningPresenters[ui_name] = StartCoroutine(OpenRoutine(ui_name));
        }
        
        
        public void Close<T>() where T : Presenter => Close(typeof(T));

        public void Close(System.Type type) => Close(type == null ? null : type.Name);

        public void Close(string ui_name)
        {
            if (string.IsNullOrWhiteSpace(ui_name))
            {
                Debug.LogError("UI Name NULL");
                return;
            }

            if (m_ClosingPresenters.ContainsKey(ui_name))
            {
                Debug.LogError("이미 닫히는 중.");
                return;
            }

            if (m_OpeningPresenters.ContainsKey(ui_name))
            {
                StopCoroutine(m_OpeningPresenters[ui_name]);
                m_OpeningPresenters.Remove(ui_name);
            }

            m_ClosingPresenters[ui_name] = StartCoroutine(CloseRoutine(ui_name));
        }


        public T GetPresenter<T>() where T : Presenter
        {
            var presenter = GetPresenter(typeof(T));
            return presenter == null ? null : (T)presenter;
        }

        public Presenter GetPresenter(System.Type type) => GetPresenter(type.Name);

        public Presenter GetPresenter(string ui_name)
        {
            if (m_Presenters.ContainsKey(ui_name) == false)
                return null;

            return m_Presenters[ui_name];
        }
        
        public T GetPresenterContext<T>(string ui_name) where T : Presenter.Context => (T)GetPresenterContext(ui_name, typeof(T));
        
        public Presenter.Context GetPresenterContext(string ui_name, System.Type type)
        {
            var context = GetPresenterContext(ui_name);
            if (context != null) return context;

            context = System.Activator.CreateInstance(type) as Presenter.Context;
            m_PresenterContexts[ui_name] = context;
            return context;
        }
        
        public Presenter.Context GetPresenterContext(string ui_name)
        {
            if (m_PresenterContexts.ContainsKey(ui_name) == false)
                return null;

            return m_PresenterContexts[ui_name];
        }
        
        private IEnumerator LoadRoutine(string ui_name)
        {
            GameObject prefab = null;
            
            var request = Resources.LoadAsync<GameObject>($"UI/{ui_name}");
            yield return request;
            prefab = request.asset == null ? null : request.asset as GameObject;
            
            if (prefab == null)
            {
                //TODO - 강제종료
                Debug.LogError("프리팹 로드 실패!");
                yield break;
            }

            Presenter.Setting presenterSetting = default;
            if (m_PresenterSettings.ContainsKey(ui_name) == true)
            {
                presenterSetting = m_PresenterSettings[ui_name];
            }
            else
            {
                var prefabPresenter = prefab.GetComponent<Presenter>();
                if (prefabPresenter != null)
                {
                    presenterSetting = prefabPresenter.setting;
                    m_PresenterSettings[ui_name] = presenterSetting;
                }
            }
            
            var obj = Instantiate(prefab, transform);
            var presenter = obj.GetComponent<Presenter>();
            if (presenter == null)
            {
                //TODO - 강제종료
                yield break;
            }
                
            var presenterSetter = (IUIManagerPresenterSetter)presenter;
            presenterSetter.Initialize(ui_name, this, GetPresenterContext(ui_name));

            var parent = m_DepthParents.ContainsKey(presenterSetting.depthID) ? m_DepthParents[presenterSetting.depthID] : transform;
            presenter.rectTransform.SetParent(parent);
            presenter.rectTransform.anchoredPosition = Vector2.zero;
            presenter.rectTransform.localRotation = Quaternion.identity;
            
            presenter.gameObject.SetActive(false);
            
            m_Presenters[ui_name] = presenter;
        }
        
        private IEnumerator OpenRoutine(string ui_name)
        {
            if (m_Presenters.ContainsKey(ui_name) == false || m_Presenters[ui_name] == null)
                yield return StartCoroutine(LoadRoutine(ui_name));
            
            var presenter = m_Presenters.ContainsKey(ui_name) ? m_Presenters[ui_name] : null;
            if (presenter == null)
            {
                m_OpeningPresenters.Remove(ui_name);
                yield break;
            }
            
            var presenterSetter = (IUIManagerPresenterSetter)presenter;
            var presenterSetting = m_PresenterSettings.ContainsKey(ui_name) ? m_PresenterSettings[ui_name] : default;
            
            if (m_EnabledPresenters.ContainsKey(presenterSetting.depthID) == false)
                m_EnabledPresenters.Add(presenterSetting.depthID, new List<string>());
            m_EnabledPresenters[presenterSetting.depthID].Remove(ui_name);
            m_EnabledPresenters[presenterSetting.depthID].Add(ui_name);
            
            UpdateSortingOrders(presenterSetting.depthID);

            presenter.rectTransform.SetAsLastSibling();
            presenterSetter.Open();
            while (presenter.openState == OpenHandler.State.Opening) yield return null;
            
            UpdateSortingOrders(presenterSetting.depthID);
            
            m_OpeningPresenters.Remove(ui_name);
        }

        private IEnumerator CloseRoutine(string ui_name)
        {
            var presenter = m_Presenters.ContainsKey(ui_name) ? m_Presenters[ui_name] : null;
            if (presenter == null)
            {
                m_ClosingPresenters.Remove(ui_name);
                yield break;
            }
            
            var presenterSetter = (IUIManagerPresenterSetter)presenter;
            var presenterSetting = m_PresenterSettings.ContainsKey(ui_name) ? m_PresenterSettings[ui_name] : default;
            presenterSetter.Close();
            while (presenter != null && presenter.openState == OpenHandler.State.Closing) yield return null;

            if (m_EnabledPresenters.ContainsKey(presenterSetting.depthID) == true)
                m_EnabledPresenters[presenterSetting.depthID].Remove(ui_name);
            UpdateSortingOrders(presenterSetting.depthID);
            
            if (presenter != null)
                Destroy(presenter.gameObject);
            
            m_ClosingPresenters.Remove(ui_name);
        }
        
        private void UpdateSortingOrders(int depthID)
        {
            if (m_EnabledPresenters.ContainsKey(depthID) == false)
                return;

            var depthIndex = setting.GetDepthIndex(depthID);
            for (var i = 0; i < m_EnabledPresenters[depthID].Count; i ++)
            {
                var ui_name = m_EnabledPresenters[depthID][i];
                if (string.IsNullOrWhiteSpace(ui_name)) continue;
                if (m_Presenters.ContainsKey(ui_name) == false) continue;

                var presenter = m_Presenters[ui_name];
                if (presenter == null) continue;

                presenter.canvas.sortingOrder = (depthIndex + 1) * setting.sortingOrderPerDepth + (i + 1) * setting.sortingOrderPerUI;
            }
        }

        private void ProcessEscapeKey()
        {
            var proceedEscapeKey = false;
            for (var i = setting.depthSettings.Length - 1; i >= 0; i --)
            {
                var depthSetting = setting.depthSettings[i];
                if (m_EnabledPresenters.ContainsKey(depthSetting.depthID) == false) continue;

                for (var j = m_EnabledPresenters[depthSetting.depthID].Count - 1; j >= 0; j --)
                {
                    var ui_name = m_EnabledPresenters[depthSetting.depthID][j];
                    if (string.IsNullOrWhiteSpace(ui_name) == true) continue;

                    var presenter = GetPresenter(ui_name);
                    if (presenter == null) continue;

                    var presenterSetter = (IUIManagerPresenterSetter)presenter;
                    if (presenterSetter.EscapeKey())
                    {
                        proceedEscapeKey = true;
                        break;
                    }
                }

                if (proceedEscapeKey == true)
                    break;
            }
        }

    }
}
