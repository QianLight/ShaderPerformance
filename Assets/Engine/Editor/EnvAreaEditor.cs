using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [CustomEditor (typeof (EnvArea))]
    public class EnvAreaEditor : BaseEditor<EnvArea>
    {

        private SerializedProperty areaID;
        private SerializedProperty color;
        private SerializedProperty isActive;
        private SerializedProperty volumns;
        private SerializedProperty instances;
        private SerializedProperty effects;
        private SerializedProperty multiLayers;
        private SerializedProperty dirtyCameraFov;
        private SerializedProperty lookAtTarget;
        private SerializedProperty manualTrigger;
        private bool init = false;
        public void OnEnable ()
        {
            areaID = FindProperty (x => x.areaID);
            color = FindProperty (x => x.color);
            isActive = FindProperty (x => x.isActive);
            volumns = FindProperty (x => x.volumns);
            instances = FindProperty (x => x.instances);
            effects = FindProperty(x => x.effects);
            multiLayers = FindProperty(x => x.multiLayers);
            dirtyCameraFov = FindProperty(x => x.dirtyCameraFov);
            lookAtTarget = FindProperty(x => x.lookAtTarget);
            manualTrigger = FindProperty(x => x.manualTrigger);
            init = false;
        }

        private void OnDisable ()
        {
            init = false;
        }
        private void Refresh ()
        {
            EnvArea ea = target as EnvArea;
            if (!init && ea.profile != null)
            {
                EnvArea.BindEnvBlock (ea.profile.envBlock, ea.transform);
                init = true;
            }
        }

        public static void OnEnvBlockGUI (EnvAreaProfile profile, EnvBlock envBlock,
            IEnvContainer envContainer, bool lerpTime = true, EnvOpContext opContext = null)
        {
            EnvArea.OnEnvBlockGUI (profile, envBlock, envContainer, lerpTime, false, opContext);
        }

        private void EnvAreaGUI (IEnvContainer envContainer)
        {
            EnvArea ea = target as EnvArea;
            EditorGUILayout.ObjectField (ea.profile, typeof (EnvAreaProfile), false);
            if (ea.profile == null)
            {
                if (GUILayout.Button ("Create"))
                {
                    var scene = ea.gameObject.scene;
                    if (!string.IsNullOrEmpty (scene.path))
                    {
                        var scenePath = Path.GetDirectoryName (scene.path);
                        ea.CreatLoadProfile (scenePath);
                    }
                }
            }
            else
            {
                if (GUILayout.Button ("Save"))
                {
                    ea.OnSave (envContainer);
                    CommonAssets.SaveAsset (ea.profile);
                }
                EditorGUILayout.PropertyField (isActive);                
                EditorGUILayout.PropertyField (color);
                var profile = ea.profile;
                if (GUILayout.Button ("Add"))
                {
                    profile.areaList.Add (new EnvBox ()
                    {
                        center = new Vector3 (0, 5, 0),
                            size = new Vector3 (10, 10, 10),
                    });
                }
                int deleteIndex = -1;
                // Transform t = ea.transform;
                for (int i = 0; i < profile.areaList.Count; ++i)
                {
                    var box = profile.areaList[i];
                    EditorCommon.BeginGroup ("Box" + i.ToString ());
                    EditorGUI.BeginChangeCheck ();
                    box.center = EditorGUILayout.Vector3Field ("Center", box.center);
                    box.size = EditorGUILayout.Vector3Field ("Size", box.size);
                    box.rotY = EditorGUILayout.FloatField ("Rot", box.rotY);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        SceneView.RepaintAll ();
                    }

                    if (GUILayout.Button ("Delete"))
                    {
                        deleteIndex = i;
                    }
                    EditorCommon.EndGroup ();
                }
                if (deleteIndex >= 0)
                {
                    profile.areaList.RemoveAt (deleteIndex);
                }
                EditorGUILayout.PropertyField(areaID);
                EditorGUILayout.PropertyField(volumns);
                EditorGUILayout.PropertyField(instances);
                EditorGUILayout.PropertyField(effects);
                EditorGUILayout.PropertyField(multiLayers);
                EditorGUILayout.PropertyField(dirtyCameraFov);
                EditorGUILayout.PropertyField(lookAtTarget);
                EditorGUILayout.PropertyField(manualTrigger); 
            }

        }
        public override void OnInspectorGUI ()
        {
            Refresh ();
            EnvArea ea = target as EnvArea;
            serializedObject.Update ();
            EnvAreaGUI (ea);
            EditorUtilities.DrawSplitter ();
            if (ea.profile != null)
            {
                OnEnvBlockGUI (ea.profile, ea.profile.envBlock, ea);
            }

            serializedObject.ApplyModifiedProperties ();
        }

        public void OnSceneGUI ()
        {
            EnvArea ea = target as EnvArea;
            Transform t = ea.transform;
            if (ea.profile != null)
            {
                for (int i = 0; i < ea.profile.areaList.Count; ++i)
                {
                    var areaBox = ea.profile.areaList[i];

                    areaBox.boundsHandle.SetColor (ea.color);

                    Quaternion rot = Quaternion.Euler (0, areaBox.rotY, 0);
                    Vector3 worldPos = t.position + areaBox.center;

                    using (new Handles.DrawingScope (Matrix4x4.TRS (Vector3.zero, rot, Vector3.one)))
                    {
                        areaBox.boundsHandle.center = Handles.inverseMatrix * worldPos;
                        areaBox.boundsHandle.size = areaBox.size;
                        EditorGUI.BeginChangeCheck ();
                        areaBox.boundsHandle.DrawHandle ();
                        if (EditorGUI.EndChangeCheck ())
                        {
                            Undo.RecordObject (t, string.Format ("Modify {0}", ObjectNames.NicifyVariableName (t.name)));
                            areaBox.size = areaBox.boundsHandle.size;
                        }
                    }

                    if (Tools.current == Tool.Move)
                    {
                        EditorGUI.BeginChangeCheck ();
                        Vector3 pos = Handles.PositionHandle (worldPos, rot);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            areaBox.boundsHandle.center = pos;
                            areaBox.center = pos - t.position;
                        }
                    }
                    else if (Tools.current == Tool.Rotate)
                    {
                        EditorGUI.BeginChangeCheck ();
                        Quaternion newRot = Handles.RotationHandle (rot, worldPos);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            Vector3 euler = newRot.eulerAngles;
                            areaBox.rotY = euler.y;
                        }
                    }
                }
            }

        }
        private static ListObjectWrapper<ISceneObject> paramObjects;
        public static void InitEnvParam ()
        {
            if (EnvSetting.paramIndex.Count == 0)
            {
                ListObjectWrapper<ISceneObject>.Get (ref paramObjects);
                RenderingManager.InitEnvCreator ();
                for (int i = 0; i < (int) EnvSettingType.Num; ++i)
                {
                    var creator = RuntimeEnvModify.settingCreators[i];
                    if (creator != null)
                    {
                        RuntimeEnvModify rem = CFAllocator.Allocate<RuntimeEnvModify> ();
                        creator (out rem.runtime, out rem.modify);
                        rem.runtime.InitParamaters (paramObjects, rem.modify);
                    }
                }
            }

        }

        public static void PreSaveEnvBlock (EnvBlock envBlock)
        {
            envBlock.envStr.Clear();
            InitEnvParam ();
            for (int j = envBlock.envParams.Count - 1; j >=0; --j)
            {
                var param = envBlock.envParams[j];
                if (param.param != null)
                {
                    if (!EnvSetting.paramIndex.TryGetValue(param.hash, out var runtimeParam))
                    {
                        envBlock.envParams.RemoveAt(j);
                    }
                }
                else
                {
                    envBlock.envParams.RemoveAt(j);
                }
            }

            for (int j = 0; j < envBlock.envParams.Count; ++j)
            {
                var param = envBlock.envParams[j];
                if (param.param != null && param.valueMask != 0)
                {
                    if (EnvSetting.paramIndex.TryGetValue(param.hash, out var runtimeParam))
                    {
                        param.param.Serialize(param, envBlock);
                    }
                    else
                    {
                        DebugLog.AddErrorLog2 ("param not find  hash:{0}", param.hash.ToString ());
                    }
                }
            }
        }
        public static void SaveEnvBlock(System.IO.BinaryWriter bw, EnvBlock envBlock,
            List<LightingInfo> lights, bool needTimeLerp = true)
        {
            var envParams = new List<EnvParam> ();
            for (int j = 0; j < envBlock.envParams.Count; ++j)
            {
                var param = envBlock.envParams[j];
                if (param.valueMask != 0)
                {
                    envParams.Add (param);
                }
            }
            short paramCount = (short) envParams.Count;
            bw.Write (paramCount);
            for (int j = 0; j < paramCount; ++j)
            {
                var param = envParams[j];
                bw.Write ((byte) param.effectType);
                bw.Write (param.hash);
                // ParamOverride runtimePo;
                // if (!EnvSetting.paramIndex.TryGetValue (param.hash, out runtimePo))
                // {
                //     DebugLog.AddErrorLog ("null param");
                // }
                bw.Write (param.valueMask);
                bw.Write (param.dataOffset);
                bw.Write (param.resType);
            }
            bw.Write (envBlock.saveData.Count);
            for (int j = 0; j < envBlock.saveData.Count; ++j)
            {
                bw.Write (envBlock.saveData[j]);
            }

            int count = lights != null ? lights.Count : 0;
            bw.Write(count);
            if (count > 0)
            {
                for (int i = 0; i < lights.Count; ++i)
                {
                    var light = lights[i];
                    EditorCommon.WriteVector(bw, light.posCoverage);
                    EditorCommon.WriteVector(bw, light.color);
                    EditorCommon.WriteVector(bw, light.param);
                }
            }

            if (needTimeLerp)
            {
                bw.Write (envBlock.lerpTime);
                bw.Write (envBlock.lerpOutTime);
            }
        }
    }
}