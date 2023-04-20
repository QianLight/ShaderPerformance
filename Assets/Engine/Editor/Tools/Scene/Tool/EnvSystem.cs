using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class EnvSystem : SceneResProcess
    {
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
        }
        ////////////////////////////PreSerialize////////////////////////////

        ////////////////////////////Serialize////////////////////////////
        private static void SetAreaMask<T>(Transform trans, uint mask) where T : MonoBehaviour
        {
            if (trans != null)
            {
                if (trans.TryGetComponent<T>(out var areaObject))
                {
                    if (areaObject is IMatObject)
                    {
                        (areaObject as IMatObject).SetAreaMask(mask);
                    }
                }
                for (int i = 0; i < trans.childCount; ++i)
                {
                    SetAreaMask<T>(trans.GetChild(i), mask);
                }
            }
        }

        private static void SetAreaMask<T>(List<Transform> root, uint mask) where T : MonoBehaviour
        {
            for (int i = 0; i < root.Count; ++i)
            {
                var v = root[i];
                SetAreaMask<T>(v, mask);
            }
        }
        protected static void PreSerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<EnvArea> (out var env))
            {
                SceneAssets.SortSceneObjectName (env, "EnvArea", ssContext.objIDMap);
                if (env.profile != null)
                {
                    EnvObject eo = new EnvObject ();
                    eo.envName = env.profile.name;
                    eo.areaID = env.areaID;
                    eo.color = env.color;
                    eo.pos = trans.position;
                    uint mask = 0xffffffff;
                    if (env.areaID >= 0)
                    {
                        mask = (uint) (1 << env.areaID);
                    }
                    eo.flag.SetFlag(env.profile.envBlock.flag.flag);
                    SetAreaMask<MeshRenderObject>(env.volumns, mask);
                    SetAreaMask<InstanceObject>(env.instances, mask);
                    SetAreaMask<SFXWrapper>(env.effects, mask);
                    SetAreaMask<MultiLayer>(env.multiLayers, mask);
                    eo.profile = env.profile;
                    //eo.flag.SetFlag(EnvBlock.Flag_DirtyCameraFov,env.dirtyCameraFov); 
                    ssContext.sd.envObjects.Add (eo);
                }
            }
            else
            {
                EnumFolder (trans, ssContext, ssContext.preSerialize);
            }
        }
        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            var ssc = bsc as SceneSaveContext;
            Ambient.shColorDebug.Clear();
            int envIndex = 0;
            for (int i = 0; i < ssc.sd.envObjects.Count; ++i)
            {
                var eo = ssc.sd.envObjects[i];
                // string path = string.Format ("{0}/{1}.asset",
                //     ssc.sceneContext.configDir,
                //     eo.envName);
                EnvAreaProfile eap = eo.profile;
                if (eap != null)
                {                    
                    eap.envBlock.saveData.Clear ();
                    int dataLength = eap.areaList.Count * 8 + 1;
                    eap.envBlock.saveData.Add (dataLength);
                    for (int j = 0; j < eap.areaList.Count; ++j)
                    {
                        var envBox = eap.areaList[j];
                        Vector3 worldPos = envBox.center + eo.pos;
                        float halfY = envBox.size.y * 0.5f;
                        float yMin = worldPos.y - halfY;
                        float yMax = worldPos.y + halfY;
                        float cosA = Mathf.Cos (envBox.rotY * Mathf.Deg2Rad);
                        float sinA = Mathf.Sin (envBox.rotY * Mathf.Deg2Rad);

                        eap.envBlock.saveData.Add (worldPos.x);
                        eap.envBlock.saveData.Add (worldPos.z);
                        eap.envBlock.saveData.Add (cosA);
                        eap.envBlock.saveData.Add (sinA);
                        eap.envBlock.saveData.Add (envBox.size.x * 0.5f);
                        eap.envBlock.saveData.Add (envBox.size.z * 0.5f);
                        eap.envBlock.saveData.Add (yMin);
                        eap.envBlock.saveData.Add (yMax);
                    }
                    EnvBlock.envObjIndex = envIndex++;
                    EnvAreaEditor.PreSaveEnvBlock (eap.envBlock);
                    for (int j = 0; j < eap.envBlock.envStr.Count; ++j)
                    {
                        var str = eap.envBlock.envStr[j];
                        ssc.AddResName (str);
                    }
                }
            }
        }
        ////////////////////////////Save////////////////////////////
        private static void SaveEnvData (BinaryWriter bw, SceneSaveContext ssc, EnvProfile profile)
        {
            var settings = profile.GetProfileSettings ();
            byte settingCount = (byte) settings.Count;
            bw.Write (settingCount);
            for (int j = 0; j < settingCount; ++j)
            {
                var setting = settings[j];
                byte settingType = (byte) setting.GetEnvType ();
                bw.Write (settingType);
                setting.Save (bw);
            }
        }

        public static void SaveHead (BinaryWriter bw, SceneSaveContext ssc)
        {
            byte globalEnv = 0;
            EnvProfile globalEnvProfile = null;
            string profilePath = string.Format ("{0}/{1}_Profiles.asset",
                ssc.sceneContext.configDir,
                ssc.sceneContext.name);
            if (File.Exists (profilePath))
            {
                globalEnvProfile = AssetDatabase.LoadAssetAtPath<EnvProfile> (profilePath);
                if (globalEnvProfile != null && globalEnvProfile.settings.Count > 0)
                {
                    globalEnv = 1;
                }
            }

            bw.Write (globalEnv);

            if (globalEnv == 1)
            {
                SaveEnvData (bw, ssc, globalEnvProfile);
            }
            byte envCount = 0;
            for (int i = 0; i < ssc.sd.envObjects.Count; ++i)
            {
                var envArea = ssc.sd.envObjects[i];
                if (envArea.profile != null)
                {
                    envCount++;
                }
            }
            bw.Write (envCount);
            for (int i = 0; i < ssc.sd.envObjects.Count; ++i)
            {
                var envArea = ssc.sd.envObjects[i];
                EnvAreaProfile eap = envArea.profile;
                if (eap != null)
                {
                    bw.Write(envArea.flag.flag);

                    uint mask = 0xffffffff;
                    if (envArea.areaID >= 0)
                    {
                        mask = (uint)(1 << envArea.areaID);
                    }
                    bw.Write(mask);
                    EditorCommon.WriteVector(bw, eap.envBlock.shadowPos);
                    bw.Write(eap.envBlock.envStr.Count);
                    for (int j = 0; j < eap.envBlock.envStr.Count; ++j)
                    {
                        var str = eap.envBlock.envStr[j];
                        ssc.SaveStringIndex(bw, str);
                    }
                    EnvAreaEditor.SaveEnvBlock(bw, eap.envBlock, eap.lights);
                } 
            }
            DebugLog.DebugStream (bw, "EnvHead");
        }
    }
}