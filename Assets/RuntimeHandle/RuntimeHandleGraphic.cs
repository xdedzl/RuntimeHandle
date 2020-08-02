using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xdedzl.RuntimeHandle
{
    public class RuntimeHandleGraphic : MonoBehaviour
    {

        #region Singleton

        private static RuntimeHandleGraphic _instance;
        public static RuntimeHandleGraphic Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 先在场景中找寻这个单例
                    _instance = (RuntimeHandleGraphic)FindObjectOfType(typeof(RuntimeHandleGraphic));

                    if (FindObjectsOfType(typeof(RuntimeHandleGraphic)).Length > 1)
                    {
                        if (Debug.isDebugBuild)
                        {
                            Debug.LogWarning("[Singleton] " + typeof(RuntimeHandleGraphic) +
                                                    " - there should never be more than 1 singleton!");
                        }

                        return _instance;
                    }

                    // 场景中找不到就创建
                    if (_instance == null)
                    {
                        var singleton = Camera.main.gameObject;
                        _instance = singleton.AddComponent<RuntimeHandleGraphic>();
                    }
                }

                return _instance;
            }
        }

        #endregion

        private List<RuntimeHandle> m_handles = new List<RuntimeHandle>();

        public void AddHandle(RuntimeHandle runtimeHandle)
        {
            m_handles.Add(runtimeHandle);
        }

        public void RemoveHadle(RuntimeHandle runtimeHandle)
        {
            m_handles.Remove(runtimeHandle);
        }

        private void OnPostRender()
        {
            foreach (var item in m_handles)
            {
                item.DrawHandle();
            }
        }
    }
}
