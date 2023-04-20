#if UNITY_EDITOR
using System.Collections.Generic;
using CFClient;
using CFUtilPoolLib;
using FMODUnity;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
namespace CFEngine
{
    public partial class EnvironmentExtra
    {
        private StudioListener listener;
        private XFmodBus m_fmodbus;

        [System.NonSerialized]
        public int hideLayer = 0;
        public void InitExternal ()
        {
            TimelineAsset.getAnimEnv = AnimEnv.GetAnimEnv;

        }
        public void UpdateExternal ()
        {
            if (listener == null)
            {
                this.gameObject.TryGetComponent (out listener);
                if (listener == null)
                {
                    listener = this.gameObject.AddComponent<StudioListener> ();
                }
            }
            if (listener != null)
            {
                FMODUnity.RuntimeManager.StudioSystem.update ();
            }

            if (m_fmodbus == null)
            {
                this.gameObject.TryGetComponent(out m_fmodbus);
                if (m_fmodbus == null)
                {
                    m_fmodbus = this.gameObject.AddComponent<XFmodBus>();
                }
            }

            if (AudioObject.add == null)
            {
                AudioObject.add = XFmod.AddXFmodComponent;
            }

            TimelineAsset.getAnimEnv = AnimEnv.GetAnimEnv;
            RTimeline.singleton.Update (Time.deltaTime);
        }

        public static void InitEditorData()
        {
            var prefabDiy = LoadMgr.singleton.LoadAssetImmediate<PrefabDIY>(
                "EditorAssetRes/Config/PrefabDIY", ".prefab", LoadMgr.EditorRes);
            if (prefabDiy != null)
            {
                var pc = PrefabConfig.singleton;
                for (int i = 0; i < prefabDiy.prefabs.Count; ++i)
                {
                    var prefab = prefabDiy.prefabs[i];
                    if (prefab.go != null)
                        pc.AddParts(prefab.go.name);
                }
            }
            LoadMgr.singleton.DestroyImmediate();
        }
    }
}
#endif