using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    public class CreateEntityPrefabWindow : EditorWindow
    {
        XReactEntranceWindow window;
        public virtual void OnEnable()
        {

        }

        public void Init(XReactEntranceWindow xReactEntranceWindow)
        {
            window = xReactEntranceWindow;
        }

        int dummy_id = 0;
        public void OnGUI()
        {
            using (new GUILayout.VerticalScope())
            {
                dummy_id = EditorGUILayout.IntField("DummyID", dummy_id);

                if (GUILayout.Button("Load"))
                {
                    if (dummy_id != 0)
                    {
                        window.ReactDataSet.ConfigData.Player = dummy_id;
                        window.IsHot = XReactDataHostBuilder.singleton.HotLoad(window.ReactDataSet);
                        if (window.IsHot)
                            Close();
                        var camera = GameObject.Find("Main Camera").gameObject;
                        if (XReactDataHostBuilder.hoster != null)
                        {
                            camera.transform.position = XReactDataHostBuilder.hoster.transform.position + new Vector3(0f, 1.5f, -5f);
                            XReactDataHostBuilder.hoster.transform.eulerAngles = new Vector3(0f, -180f, 0f);
                        }
                    }
                }
            }
        }
    }
}
