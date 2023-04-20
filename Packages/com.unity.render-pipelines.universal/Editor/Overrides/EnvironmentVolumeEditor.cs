using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(EnvironmentVolume))]
    sealed class EnvironmentVolumeEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_OverrideCamera;
        SerializedDataParameter m_CameraFOV;
        SerializedDataParameter m_CameraNear;

        SerializedDataParameter m_CameraFar;

        // SerializedDataParameter m_CameraUseSkybox;
        SerializedDataParameter m_CameraClearFlagsParam;
        SerializedDataParameter m_CameraCloseAA;
        SerializedDataParameter m_ShadowColor;
        SerializedDataParameter m_OverrideShadowDistance;
        SerializedDataParameter m_ShadowDistance;
        SerializedDataParameter m_DepthBias;
        SerializedDataParameter m_NormalBias;

        SerializedDataParameter m_LightObject;

        SerializedDataParameter m_ControlCount;
        SerializedDataParameter m_ControlObjects;
        StringParameter m_LightObjectSource;

        SerializedDataParameter m_TerrainBlend;

        SerializedProperty m_CameraCull;
        SerializedProperty m_QulityDistanceScale;
        SerializedProperty m_CullLod0;
        SerializedProperty m_CullLod1;
        SerializedProperty m_CullLod2;

        SerializedDataParameter CloudShadowTex;
        SerializedDataParameter EnableCloudShadow;
        SerializedDataParameter CloudShadowIntensity;
        SerializedDataParameter CloudShadowSpeedX;
        SerializedDataParameter CloudShadowSpeedY;
        SerializedDataParameter CloudShadowScale;

        Transform m_LightObjectTransform;
        List<Transform> transList = new List<Transform>();
        EnvironmentVolume targetObj = null;
        PropertyFetcher<EnvironmentVolume> o = null;

        public override void OnEnable()
        {
            transList.Clear();

            targetObj = (EnvironmentVolume) serializedObject.targetObject;

            o = new PropertyFetcher<EnvironmentVolume>(serializedObject);
            m_OverrideCamera = Unpack(o.Find(x => x.OverrideCamera));
            m_CameraFOV = Unpack(o.Find(x => x.CameraFOV));
            m_CameraNear = Unpack(o.Find(x => x.CameraNear));
            m_CameraFar = Unpack(o.Find(x => x.CameraFar));
            // m_CameraUseSkybox = Unpack(o.Find(x => x.CameraUseSkybox));
            m_CameraClearFlagsParam = Unpack(o.Find(x => x.CameraClearFlagsParam));
            m_CameraCloseAA = Unpack(o.Find(x => x.CameraCloseAA));

            m_ShadowColor = Unpack(o.Find(x => x.ShadowColor));
            m_OverrideShadowDistance = Unpack(o.Find(x => x.OverrideShadowDistance));
            m_ShadowDistance = Unpack(o.Find(x => x.ShadowDistance));
            m_DepthBias = Unpack(o.Find(x => x.DepthBias));
            m_NormalBias = Unpack(o.Find(x => x.NormalBias));

            m_LightObject = Unpack(o.Find(x => x.Lightbject));

            m_ControlCount = Unpack(o.Find(x => x.ControlCount));
            m_ControlObjects = Unpack(o.Find(x => x.ControlObjects));

            m_TerrainBlend = Unpack(o.Find(x => x.TerrainFeature));
            CloudShadowTex = Unpack(o.Find(x => x.CloudShadowTex));
            EnableCloudShadow = Unpack(o.Find(x => x.EnableCloudShadow));
            CloudShadowIntensity = Unpack(o.Find(x => x.CloudShadowIntensity));
            CloudShadowSpeedX = Unpack(o.Find(x => x.CloudShadowSpeedX));
            CloudShadowSpeedY = Unpack(o.Find(x => x.CloudShadowSpeedY));
            CloudShadowScale = Unpack(o.Find(x => x.CloudShadowScale));

            m_CameraCull = serializedObject.FindProperty("m_CameraCull");
            m_QulityDistanceScale = serializedObject.FindProperty("m_QulityDistanceScale");
            m_CullLod0 = serializedObject.FindProperty("m_CullLod0");
            m_CullLod1 = serializedObject.FindProperty("m_CullLod1");
            m_CullLod2 = serializedObject.FindProperty("m_CullLod2");

            m_LightObjectSource = targetObj.Lightbject;
            UnityEngine.Transform obj = m_LightObjectSource.GetObjectByName();
            if (obj != null)
            {
                m_LightObjectTransform = obj;
            }

            getParameter(transList, targetObj.ControlObjects);
        }
        
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Scene:");
                GUILayout.Label(scene.name);
                GUILayout.EndHorizontal();
            }

            PropertyField(m_OverrideCamera);
            PropertyField(m_CameraFOV);
            PropertyField(m_CameraNear);
            PropertyField(m_CameraFar);
            // PropertyField(m_CameraUseSkybox);
            PropertyField(m_CameraClearFlagsParam);
            PropertyField(m_CameraCloseAA);
            PropertyField(m_TerrainBlend);


            EditorGUILayout.Space();
            GUILayout.Label("Scene:");
            EditorGUILayout.PropertyField(m_CameraCull, new GUIContent("Set Camera Cull"));
            EditorGUI.indentLevel += 2;
            EditorGUI.BeginDisabledGroup(!m_CameraCull.boolValue);
            EditorGUILayout.PropertyField(m_QulityDistanceScale,
                new GUIContent("Distance Scale", "By Mat level:w->Ultra&High; z->Medium; y->Low; x->VeryLow"));
            EditorGUILayout.PropertyField(m_CullLod0, new GUIContent("Cull LOD0"));
            EditorGUILayout.PropertyField(m_CullLod1, new GUIContent("Cull LOD1"));
            EditorGUILayout.PropertyField(m_CullLod2, new GUIContent("Cull LOD2"));
            if (GUILayout.Button("Set Camera"))
            {
                targetObj.SetCamera();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel -= 2;

            PropertyField(m_ShadowColor);
            PropertyField(m_OverrideShadowDistance);
            PropertyField(m_ShadowDistance);
            PropertyField(m_DepthBias);
            PropertyField(m_NormalBias);

            viewObject("Light Object.", ref m_LightObjectTransform, ref m_LightObject);

            if (m_ControlCount.value.intValue < 0)
                m_ControlCount.value.intValue = 0;
            PropertyField(m_ControlCount);

            if (m_ControlCount.overrideState.boolValue && targetObj.ControlObjects != null)
            {
                if (m_ControlCount.value.intValue != targetObj.ControlObjects.Length)
                {
                    List<StringParameter> paras = new List<StringParameter>();
                    paras.AddRange(targetObj.ControlObjects);

                    if (paras.Count > m_ControlCount.value.intValue)
                    {
                        paras.RemoveRange(m_ControlCount.value.intValue, paras.Count - m_ControlCount.value.intValue);
                    }
                    else
                    {
                        int count = m_ControlCount.value.intValue - paras.Count;
                        for (int i = 0; i < count; i++)
                        {
                            paras.Add(new StringParameter(null));
                        }
                    }

                    targetObj.ControlObjects = paras.ToArray();
                    getParameter(transList, targetObj.ControlObjects);
                }


                int refresh = viewParameter(transList, targetObj.ControlObjects);
                if (refresh > 0)
                {
                    EditorUtility.SetDirty(target);
                    GUI.changed = true;
                }
            }

            PropertyField(EnableCloudShadow);
            if (EnableCloudShadow.value.boolValue)
            {
                PropertyField(CloudShadowTex);
                PropertyField(CloudShadowIntensity);
                PropertyField(CloudShadowSpeedX);
                PropertyField(CloudShadowSpeedY);
                PropertyField(CloudShadowScale);
                if (UnityEngine.GUILayout.Button("刷新云投影"))
                {
                    if (VolumeManager.instance != null)
                    {
                        targetObj.SetCloudShadow(true);
                    }
                }
            }
            else
            {
                targetObj.SetCloudShadow(false);
            }

            bool canUpdate = targetObj.GUIUpdate();
            if (canUpdate)
            {
                if (UnityEngine.GUILayout.Button("显示其他"))
                {
                    if (VolumeManager.instance != null)
                    {
                        setVolume(true, false);
                    }
                }

                if (UnityEngine.GUILayout.Button("显示所有"))
                {
                    if (VolumeManager.instance != null)
                    {
                        setVolume(true, true);
                    }
                }

                if (UnityEngine.GUILayout.Button("隐藏其他"))
                {
                    if (VolumeManager.instance != null)
                    {
                        setVolume(false, false);
                    }
                }

                if (UnityEngine.GUILayout.Button("隐藏所有"))
                {
                    if (VolumeManager.instance != null)
                    {
                        setVolume(false, true);
                    }
                }
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        void getParameter(List<Transform> transList, StringParameter[] paras)
        {
            if (paras != null)
            {
                if (paras.Length != transList.Count)
                {
                    transList.Clear();
                    for (int i = 0; i < paras.Length; i++)
                    {
                        StringParameter para = paras[i];
                        Transform t = para.GetObjectByName();
                        transList.Add(t);
                    }
                }
            }
        }

        int viewParameter(List<Transform> transList, StringParameter[] paras)
        {
            int refresh = 0;
            if (paras != null && paras.Length > 0)
            {
                for (int i = 0; i < paras.Length; i++)
                {
                    StringParameter para = paras[i];
                    refresh += viewParameter("Control Object " + i, ref transList, i, ref para);
                }
            }

            return refresh;
        }

        int viewParameter(string lab, ref List<Transform> transList, int i, ref StringParameter para)
        {
            int refresh = 0;
            Transform obj = transList[i];
            Transform lastObj = obj;
            EditorGUILayout.LabelField(lab, EditorStyles.miniLabel);
            obj = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Transform), true) as UnityEngine.Transform;

            if (lastObj != obj)
            {
                if (obj != null)
                {
                    para.value = para.GetRootName(obj);
                }
                else
                {
                    para.SetValue("");
                }

                refresh = 1;
            }

            transList[i] = obj;
            para.value = EditorGUILayout.TextField(lab, para.value);
            return refresh;
        }

        void viewObject(string lab, ref Transform obj, ref SerializedDataParameter para)
        {
            EditorGUILayout.LabelField(lab, EditorStyles.miniLabel);
            Transform lastObj = obj;
            obj = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Transform), true) as UnityEngine.Transform;
            if (lastObj != obj)
            {
                if (obj != null)
                {
                    para.value.stringValue = m_LightObjectSource.GetRootName(obj);
                }
                else
                {
                    para.value.stringValue = "";
                }
            }

            PropertyField(para);
        }

        void setVolume(bool enable, bool contant)
        {
            Volume[] volumes = VolumeManager.instance.GetVolumes(UnityEngine.LayerMask.GetMask("Default"));
            if (target != null)
            {
                foreach (Volume v in volumes)
                {
                    if (v.enabled && (v.gameObject.transform != EnvironmentVolume.LastVolumeRoot || contant))
                    {
                        foreach (var component in v.profileRef.components)
                        {
                            if (!component.active)
                                continue;

                            if (component.name == "EnvironmentVolume")
                            {
                                EnvironmentVolume tmpEv = component as EnvironmentVolume;
                                tmpEv.SetControlObjects(enable);
                            }
                        }
                    }
                }
            }
        }


        public override void OnDisable()
        {
        }
    }
}