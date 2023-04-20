using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class SceneSFXCreator:MonoBehaviour
    {
        public List<SceneSFXLoadData> data;
        private bool setToReInit = false;
        // private List<SFX> runtimeSFXs;
#if UNITY_EDITOR
        public bool Add(GameObject sfx, bool isHide, SceneSFXData extra = null)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(sfx);
            if (prefab is null) return false;
            if (data == null) data = new List<SceneSFXLoadData>();
            PrefabUtility.UnpackPrefabInstance(sfx, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            while (sfx.transform.childCount > 0)
            {
                DestroyImmediate(sfx.transform.GetChild(0).gameObject);
            }
            // GameObject newTrans = new GameObject(sfx.name);
            // newTrans.transform.parent = sfx.transform.parent;
            // newTrans.transform.localPosition = Vector3.zero;
            // newTrans.transform.localRotation =        
            sfx.SetActive(isHide);
            data.Add(new SceneSFXLoadData()
            {
                name = prefab.name,
                trans = sfx.transform,
                pos = sfx.transform.localPosition,
                rot = sfx.transform.localRotation,
                scale = sfx.transform.lossyScale,
                delay = extra == null ? 0 : extra.delay
                // active = !isHide
            });
            sfx.transform.localPosition = Vector3.zero;
            sfx.transform.localRotation = Quaternion.identity;
            sfx.transform.localScale = Vector3.one;
            var c = sfx.GetComponents<Component>();
            for (int i = 0; i < c.Length; i++)
            {
                if (!(c[i] is Transform)) DestroyImmediate(c[i]);
            }
            return true;
        }
#endif
        public void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!setToReInit)
            {
                InitSceneSFX();
                SceneSFXManager.ReInit -= InitSceneSFX;
                SceneSFXManager.ReInit += InitSceneSFX;
                setToReInit = true;
            }
        }
        
        public void InitSceneSFX()
        {
            if (data != null)
            {
                Transform parent = this.transform;
                // runtimeSFXs = new List<SFX>();
                for (var index = 0; index < data.Count; index++)
                {
                    var d = data[index];
                    if (d.trans == null)
                    {
                        DebugLog.AddErrorLog2("场景特效配置丢失，重新保存场景以解决问题");
                        return;
                    }
                    SFX sfx = SFXMgr.singleton.Create(d.name, 0/*SFXMgr.Flag_DontDestroy*/, ulong.MaxValue, LoadFinish, true);
                    if (sfx != null && data[index].trans.gameObject.activeSelf)
                    {
                        sfx.Play();
                    }
                    // runtimeSFXs.Add(sfx);

                    void LoadFinish(SFX s)
                    {
                        s.flag.SetFlag(SFX.Flag_Follow, true);
                        s.SetParent(d.trans);
                        s.SetPos(ref d.pos);
                        s.SetRot(ref d.rot);
                        s.SetScale(ref d.scale);
                    }
                }
            }
        }
        //                SceneSFXManager.ReInit();


        public void OnDisable()
        {
            SceneSFXManager.ReInit -= InitSceneSFX;
            data.Clear();
        }
    }

    [Serializable]
    public struct SceneSFXLoadData
    {
        public string name;
        public Transform trans;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;

        public float delay;
        // public bool active;
    }
}