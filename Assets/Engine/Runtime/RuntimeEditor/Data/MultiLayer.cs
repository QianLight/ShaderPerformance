using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = System.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif

using   UsingTheirs.ShaderHotSwap;
using UnityEngine.Rendering.Universal.Internal;
namespace CFEngine
{
    public enum MultiLayerQuality
    {
        Test = 1,
        Low=2,
        Normal = 3,
        High = 4,
        VeryHigh = 5
    }
    [Serializable]
    public class CloudSetting
    {
        public MultiLayerQuality m_MultiLayerQuality;
        
        [Range(1,30)]
        public int m_LayerCount;
        public float m_CloudAlpha;

        public CloudSetting(MultiLayerQuality q, int n, float a)
        {
            m_MultiLayerQuality = q;
            m_LayerCount = n;
            m_CloudAlpha = a;
        }
        
    }

    [ExecuteInEditMode]
    public class MultiLayer : MonoBehaviour

#if UNITY_EDITOR
        , IMatObject
#endif

    {
        public MultiLayerQuality quality = MultiLayerQuality.Normal;

        [SerializeField] public CloudSetting[] cloudSetting;

        private CloudSetting m_CloudSettingData;

        public uint areaMask = 0xffffffff;
        public bool isPlaying = false;
        public bool directDraw = false;


        public bool bSplitMesh;

        public Mesh m;
        
        private Mesh m_RenderMesh;
        
        private MeshRenderer mr;
        private MeshFilter mf;

        public bool safeMode = false;
        public Material safeModeMaterial;
        private MaterialPropertyBlock safeModeProperty;
        private Matrix4x4[] matricies;


        public Material mat;
        private MaterialPropertyBlock mpb;

        private ComputeBuffer argBuffer;

        //private ComputeBuffer paramBuffer;
        private Transform m_Trans;
        public static int _Param2 = Shader.PropertyToID("_Param2");
        public static int _CloudAlpha = Shader.PropertyToID("_CloudAlpha");
        public static int _CloudLength = Shader.PropertyToID("_CloudLength");

        private void Start()
        {
            HandlerSwapShaders.HandlerSwapShadersAction += HotSwapShader;
            DepthOnlyPass.customDrawDepth += DrawDepth;
            
            if (Object.Equals(cloudSetting, null))
                InitDefaultCloudSetting();

            SetQuality(quality);
            SetRenderMesh(m);

            switch (GameQualitySetting.MatLevel)
            {
                case RenderQualityLevel.Ultra:
                    SetQuality(MultiLayerQuality.VeryHigh);
                    break;
                case RenderQualityLevel.High:
                    SetQuality(MultiLayerQuality.High);
                    break;
                case RenderQualityLevel.Medium:
                    SetQuality(MultiLayerQuality.Normal);
                    break;
                case RenderQualityLevel.Low:
                    SetQuality(MultiLayerQuality.Low);
                    break;
                case RenderQualityLevel.VeryLow:
                    SetQuality(MultiLayerQuality.Test);
                    break;
            }

            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                SetQuality(MultiLayerQuality.VeryHigh);
#endif
            }

            Refresh();

            if (EngineUtility.GetMainCamera())
            {
                StartSplitMesh();
            }
        }

        public void HotSwapShader(Shader shader, string shaderName)
        {
            if (shaderName != mat.shader.name) return;
            mat.shader = shader;

            Debug.Log("MultiLayer HotSwapShader!");
        }

        public void SetRenderMesh(Mesh m1)
        {
            m_RenderMesh = m1;
        }

        private void SetQuality(MultiLayerQuality q)
        {
            if(q<MultiLayerQuality.Test||q>MultiLayerQuality.VeryHigh) return;
            quality = q;
            quality = MultiLayerQuality.VeryHigh;
            for (int i = 0; i < cloudSetting.Length; i++)
            {
                if (cloudSetting[i].m_MultiLayerQuality == q)
                {
                    m_CloudSettingData = cloudSetting[i];
                    break;
                }
            }

            if (m_CloudSettingData == null)
            {
                InitDefaultCloudSetting();
                SetQuality(q);
            }
        }

        private void InitDefaultCloudSetting()
        {
            cloudSetting = new[]
            {
                new CloudSetting(MultiLayerQuality.Test, 2, 1.8f),
                new CloudSetting(MultiLayerQuality.Low, 2, 1.8f),
                new CloudSetting(MultiLayerQuality.Normal, 3, 1.0f),
                new CloudSetting(MultiLayerQuality.High, 4, 0.8f),
                new CloudSetting(MultiLayerQuality.VeryHigh, 5, 0.7f)
            };
        }

        public MultiLayerCulling m_MultiLayerCulling;

        public void StartSplitMesh()
        {
            if (!bSplitMesh) return;
            
            hasInitCulling = true;
            DestroySplitMesh();
            m_MultiLayerCulling = new MultiLayerCulling(this.gameObject, this);

            if (!Application.isPlaying)
            {
                m_MultiLayerCulling.ClearJob();
            }
            
            
        }

        private bool hasInitCulling = false;

        private void InitByCamera()//游戏里面 场景初始化没有相机的 等相机初始化之后执行
        {
            if (hasInitCulling) return;
            if (!EngineUtility.GetMainCamera()) return;
            StartSplitMesh();
        }

        private void UpdateCullingData()
        {
            if (!bSplitMesh) return;
            InitByCamera();
            
            if (m_MultiLayerCulling == null) return;
            m_MultiLayerCulling.UpdateData();
            
        }
        
        private void DestroySplitMesh()
        {
            if (!bSplitMesh) return;

            if (m_MultiLayerCulling == null) return;
            m_MultiLayerCulling.OnDestory();
            m_MultiLayerCulling = null;
            m_RenderMesh = null;
        }


        private void OnEnable()
        {
            // #if UNITY_EDITOR
            //             quality = MultiLayerQuality.VeryHigh;
            // #endif
        }

        public void OnDrawGizmos()
        {
            if (!bSplitMesh) return;
            if (m_MultiLayerCulling == null) return;
            m_MultiLayerCulling.OnDrawGizmos();
        }

        private void OnDestroy()
        {
            DestroySplitMesh();
            HandlerSwapShaders.HandlerSwapShadersAction -= HotSwapShader;
            DepthOnlyPass.customDrawDepth -= DrawDepth;
        }


        private void Update()
        {
            Application.targetFrameRate = 1000;
            
            UpdateLastState();
            UpdateCullingData();
            UpdateShow();
        }
        
        private MultiLayerQuality m_LastMultiLayerQuality = MultiLayerQuality.Test;
        private void UpdateLastState()
        {
            if (m_LastMultiLayerQuality != quality)
            {
                m_LastMultiLayerQuality = quality;
                SetQuality(quality);
            }
        }

        private bool CheckIsVisiable()
        {
            if (!isPlaying || m_RenderMesh==null || Object.Equals(m_Trans, null) ||
                Object.Equals(mat, null)) return false;

            return true;
        }

        private void UpdateShow()
        {
            if(!CheckIsVisiable()) return;
            
            if (safeMode)
            {
                UpdateShow_SafeMode();
            }
            else
            {
                UpdateShow_Normal();
            }
        }

        private void UpdateShow_Normal()
        {
            int count = m_CloudSettingData.m_LayerCount;
            if (Object.Equals(mat, null)) return;

            if (mpb == null)
            {
                Init((uint)count);
            }

            if (Object.Equals(mpb, null)) return;
            
            
            EngineContext context = EngineContext.instance;

            if (directDraw)
            {
                var v = mat.GetVector(_Param2);
                v.y = count - 1;
                mat.SetVector(_Param2, v);
                mat.SetFloat(_CloudAlpha, m_CloudSettingData.m_CloudAlpha);
                mpb.SetInt(ShaderManager._InstanceOffset, 0);
            }
            else
            {
                int totalInstance = count * 4;
                var v = mat.GetVector(_Param2);
                v.y = totalInstance - 1;
                mat.SetVector(_Param2, v);
                mat.SetFloat(_CloudAlpha, m_CloudSettingData.m_CloudAlpha);
                mpb.SetInt(ShaderManager._InstanceOffset, context.multiLayerFrame * count);
                context.multiLayerFrame++;
                context.multiLayerFrame = context.multiLayerFrame % 4;
            }

            var matrix = m_Trans.localToWorldMatrix;
            mpb.SetMatrix(ShaderManager.custom_ObjectToWorld, matrix);

            if (directDraw)
            {
                // Graphics.DrawMeshInstancedIndirect(m, 0, mat, mr.bounds, argBuffer, 0, mpb);
                mat.enableInstancing = true;
                if (matricies == null || matricies.Length != count)
                {
                    matricies = new Matrix4x4[count];
                    for (int i = 0; i < count; i++)
                    {
                        matricies[i] = m_Trans.localToWorldMatrix;
                    }
                }
                
                //Debug.Log("DrawRender:"+Time.frameCount+"  "+Time.realtimeSinceStartup);
                Graphics.DrawMeshInstanced(m_RenderMesh, 0, mat, matricies, count, mpb);
            }
            else if (argBuffer != null)
            {
                if (WorldSystem.materialConfig != null)
                {
                    var combineMat = WorldSystem.GetEffectMat(EEffectMaterial.MultiLayer);
                    if (combineMat != null)
                    {
                        var db = SharedObjectPool<InstanceDrawBatch>.Get();
                        db.mesh = m_RenderMesh;
                        db.mat = mat;
                        db.passID = 0;
                        db.mpbRef = mpb;
                        db.argBuffer = argBuffer;
                        context.multiLayerObjects.Push(db);
                        Graphics.DrawMesh(m_RenderMesh, matrix, combineMat, DefaultGameObjectLayer.SRPLayer_Scene,
                            null, 0, mpb, false, false, false);
                    }
                }
            }
        }

        public void DrawDepth(CommandBuffer cmd)
        {
            if (!CheckIsVisiable()) return;
            //Debug.Log("DrawDepth:"+Time.frameCount+"  "+Time.realtimeSinceStartup);
            cmd.DrawMeshInstanced(m_RenderMesh, 0, mat, 1, matricies, m_CloudSettingData.m_LayerCount, mpb);
        }

        private void UpdateShow_SafeMode()
        {
            if (safeModeMaterial != null)
            {
                int count = 6;
                switch (GameQualitySetting.MatLevel)
                {
                    case RenderQualityLevel.Ultra:
                        count = 5;
                        break;
                    case RenderQualityLevel.High:
                        count = 4;
                        break;
                    case RenderQualityLevel.Medium:
                        count = 3;
                        break;
                    case RenderQualityLevel.Low:
                        count = 2;
                        break;
                    case RenderQualityLevel.VeryLow:
                        count = 2;
                        break;
                }

                if (safeModeProperty == null)
                {
                    safeModeProperty = new MaterialPropertyBlock();
                    float[] index = new float[count];
                    float[] totalCount = new float[count];
                    for (int i = 0; i < count; i++)
                    {
                        index[i] = i;
                        totalCount[i] = count;
                    }

                    safeModeProperty.SetFloatArray("_Index", index);
                    safeModeProperty.SetFloatArray("_TotalCount", totalCount);
                }

                if (matricies == null || matricies.Length != count)
                {
                    matricies = new Matrix4x4[count];
                    Matrix4x4 matrix = m_Trans.localToWorldMatrix;
                    for (int i = 0; i < count; i++)
                    {
                        matricies[i] = matrix;
                    }
                }

                Graphics.DrawMeshInstanced(m_RenderMesh, 0, safeModeMaterial, matricies, 10, safeModeProperty);
            }
        }



        public void Init(uint count)
        {
            SetRenderMesh(m);
            
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock();
            }
            
            if (m != null)
            {
                if (argBuffer == null)
                    argBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
                SceneMisc.argArray[0] = m.GetIndexCount(0);
                SceneMisc.argArray[1] = count;
                argBuffer.SetData(SceneMisc.argArray);
                //if (paramBuffer != null)
                //    paramBuffer.Dispose();
                //paramBuffer = new ComputeBuffer((int)count, GpuAnimParam.GetSize(), ComputeBufferType.Constant);
                //GpuAnimationData.tmpParam.Clear();
                //for (int i = 0; i < count; ++i)
                //{
                //    GpuAnimationData.tmpParam.Add(new GpuAnimParam() { param = new Vector4(i, 0, 0, 0) });
                //}
                //paramBuffer.SetData(GpuAnimationData.tmpParam);
                //mpb.SetConstantBuffer(ShaderManager.paramArray, paramBuffer, 0, (int)count * GpuAnimParam.GetSize());

            }
        }
        public void Refresh()
        {
            m_Trans = this.transform;
            this.gameObject.TryGetComponent(out mr);
            if (mr != null)
            {
                mat = mr.sharedMaterial;
                mr.enabled = false;
            }
            this.gameObject.TryGetComponent(out mf);
            if (mf != null)
            {
                m = mf.sharedMesh;
            }
            
            SetQuality(quality);
            
            Init((uint)quality);

            safeModeProperty = null;
            matricies = null;
        }

        public void Save()
        {
            Refresh();

#if UNITY_EDITOR

            if (mat != null)
            {
                EditorCommon.SaveAsset(mat);
            }
#endif

        }

        public void Refresh(RenderingManager mgr)
        {

        }
        public void OnDrawGizmo(EngineContext context)
        {

        }
        public void SetAreaMask(uint area)
        {
            areaMask = area;
        }


        //void OnApplicationQuit()
        //{
        //    OnDestory();
        //}
        //void OnDestory()
        //{
        //    if (argBuffer != null)
        //        argBuffer.Release();
        //}
    }


#if UNITY_EDITOR

    [CanEditMultipleObjects, CustomEditor(typeof(MultiLayer))]
    public class MultiLayerEditor : UnityEngineEditor
    {
        SerializedProperty isPlaying;
        SerializedProperty m;
        SerializedProperty bSplitMesh;
        SerializedProperty quality;
        SerializedProperty cloudSetting;

        SerializedProperty safeMode;
        SerializedProperty safeModeMaterial;
      
        SerializedProperty directDraw;
        SerializedProperty mat;
        private void OnEnable()
        {
            isPlaying = serializedObject.FindProperty("isPlaying");
            m = serializedObject.FindProperty("m");
            bSplitMesh = serializedObject.FindProperty("bSplitMesh");
            quality = serializedObject.FindProperty("quality");
            cloudSetting = serializedObject.FindProperty("cloudSetting");

            safeMode = serializedObject.FindProperty("safeMode");
            safeModeMaterial = serializedObject.FindProperty("safeModeMaterial");

            mat = serializedObject.FindProperty("mat");
            directDraw = serializedObject.FindProperty("directDraw");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MultiLayer ml = target as MultiLayer;
            EditorGUILayout.PropertyField(isPlaying);
            EditorGUILayout.PropertyField(directDraw);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(quality);
            EditorGUILayout.PropertyField(cloudSetting);
            if (EditorGUI.EndChangeCheck())
            {
                ml.Init((uint)quality.intValue);
            }
            EditorGUILayout.LabelField(string.Format("LayerCount:{0}", quality.intValue.ToString()));
            EditorGUILayout.PropertyField(m);
            EditorGUILayout.PropertyField(bSplitMesh);

            if (bSplitMesh.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("重新显示", GUILayout.MaxWidth(80)))
                {
                    ml.StartSplitMesh();
                }
            
                if (GUILayout.Button("显影原模型", GUILayout.MaxWidth(80)))
                {
                    MeshRenderer[] mrs=ml.gameObject.GetComponentsInChildren<MeshRenderer>();
                    bool b = !mrs[0].enabled;
                    for (int i = 0; i < mrs.Length; i++)
                    {
                        mrs[i].enabled = b;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }



            EditorGUILayout.PropertyField(safeMode);
            if(safeMode.boolValue == true)
            {
                EditorGUILayout.PropertyField(safeModeMaterial);
            }
            else
            {
                EditorGUILayout.PropertyField(mat);
            }

            
            
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
            {
                ml.Refresh();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}

