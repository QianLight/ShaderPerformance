using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct GPUInstancingLod
{
    public Mesh mesh;
    public float distance;

    public bool IsValid()
    {
        return mesh && distance > 0;
    }
}

[Serializable]
public class GPUInstancingGroup
{
    public List<GPUInstancingLod> lods = new List<GPUInstancingLod>();
    public Material material;
    public float cullingRadius;
    public List<MeshRenderer> renderers = new List<MeshRenderer>();

    public bool IsValid()
    {
        foreach (GPUInstancingLod lod in lods)
        {
            if (!lod.IsValid())
                return false;
        }

        return cullingRadius > 0f;
    }
}

public class GPUInstancingManager
{
    private class GroupInternalData : ISharedObject
    {
        public BoundingSphere[] spheres;
        public CullingGroup cullingGroup;
        public List<Batch> batches = new List<Batch>();
        public List<HashSet<int>> lodList = new List<HashSet<int>>();
        public GPUInstancingGroup @group;
        public List<Matrix4x4> transforms = new List<Matrix4x4>();

        private int dirtyCounter;

        public void MarkNextFrameUpdate()
        {
            dirtyCounter = 2;
        }

        public bool NeedUpdate()
        {
            return dirtyCounter-- == 0;
        }

        public void Reset()
        {
        }
    }

    private class Batch : ISharedObject
    {
        public int lodIndex;
        public Matrix4x4[] matrixes = new Matrix4x4[MAX_INSTANCE_PER_DRAW];
        public int count;

        public void Reset()
        {
            count = 0;
        }
    }

    private List<GroupInternalData> datas = new List<GroupInternalData>();

    private static Camera TargetCamera
    {
        get
        {
            if (Time.frameCount != lastFrame || !mainCamera)
            {
                Camera c = EngineUtility.GetMainCamera();
                if (mainCamera != c)
                {
                    mainCamera = c;
                    foreach (GroupInternalData data in Instance.datas)
                    {
                        data.cullingGroup.targetCamera = mainCamera;
                    }

                    lastFrame = Time.frameCount;
                }
            }

            return mainCamera;
        }
    }

    private static Transform DistanceReference
    {
        set
        {
            if (value != distanceReference)
            {
                distanceReference = value;
                foreach (GroupInternalData data in Instance.datas)
                {
                    data.cullingGroup.SetDistanceReferencePoint(distanceReference);
                }
            }
        }
        get
        {
            if (distanceReference)
                return distanceReference;

            if (TargetCamera)
                return TargetCamera.transform;
            
            // 相机不存在，临时随便使用一个transform.
            return UnityEngine.Object.FindObjectOfType<Transform>();
        }
    }

    private static Transform distanceReference;

    private static int lastFrame = -1;
    private static Camera mainCamera;
    public static GPUInstancingManager Instance { get; } = new GPUInstancingManager();

    private static readonly bool supportInstancing = SystemInfo.supportsInstancing;
    private const int MAX_INSTANCE_PER_DRAW = 250;
    private const float VIEW_THRESHOLD = 0.0001f;
    private const float POS_THRESHOLD = 0.1f;
    private const float FOV_THRESHOLD = 1f;
    private Vector3 prevCameraPosition;
    private Vector3 prevCameraForward;
    private float prevFov;
    private float lastUpdateFadeTime;

    private GPUInstancingManager()
    {
        RenderLevelManager.onImposterLevelChanged += OnDistanceScaleChanged;
    }

    ~GPUInstancingManager()
    {
        RenderLevelManager.onImposterLevelChanged -= OnDistanceScaleChanged;
    }

    private void OnDistanceScaleChanged(RenderQualityLevel level)
    {
        foreach (GroupInternalData data in datas)
            UpdateCullingDistances(data);
    }

    public void Add(GPUInstancingGroup group)
    {
        if (group == null)
        {
            Debug.LogError("Tempalte is null.");
            return;
        }

        if (group.renderers.Count == 0)
        {
            return;
        }

        GroupInternalData data = SharedObjectPool<GroupInternalData>.Get();
        datas.Add(data);
        CullingGroup cullingGroup = new CullingGroup();
        data.@group = group;
        data.transforms = new List<Matrix4x4>(group.renderers.Count);
        data.cullingGroup = cullingGroup;
        data.spheres = new BoundingSphere[group.renderers.Count];
        cullingGroup.targetCamera = TargetCamera;
        cullingGroup.SetDistanceReferencePoint(DistanceReference);
        for (int i = 0; i < group.renderers.Count; i++)
        {
            if (!group.renderers[i])
                continue;
            Matrix4x4 matrix = group.renderers[i].localToWorldMatrix;
            Vector3 pos = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            float scale = (matrix.m00 + matrix.m11 + matrix.m12) * 0.3333f;
            data.spheres[i] = new BoundingSphere(pos, scale * group.cullingRadius);
            data.transforms.Add(matrix);
        }

        UpdateCullingDistances(data);
        cullingGroup.SetBoundingSpheres(data.spheres);
        cullingGroup.SetBoundingSphereCount(group.renderers.Count);
        cullingGroup.onStateChanged = x => OnChange(data, x);
        cullingGroup.enabled = true;

        int count = group.lods.Count;
        data.lodList = new List<HashSet<int>>(count);
        for (int i = 0; i < count; i++)
        {
            data.lodList.Add(new HashSet<int>());
        }

        data.MarkNextFrameUpdate();
    }

    private static void UpdateCullingDistances(GroupInternalData data)
    {
        float scale = 1;

        if (EngineContext.IsRunning)
        {
            scale = RenderLevelManager.GetCurrentGpuInstancingDistanceScale();
        }
#if UNITY_EDITOR
        else
        {
            string path = "Assets/BundleRes/Config/RenderLevelConfig.asset";
            RenderLevelConfig renderLevelConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<RenderLevelConfig>(path);
            if (renderLevelConfig != null && renderLevelConfig.valueConfigs != null && renderLevelConfig.valueConfigs.gpuInstancingDistanceScale != null)
            {
                int renderLevel = (int) RenderQualityLevel.Ultra;
                if (renderLevel < renderLevelConfig.valueConfigs.gpuInstancingDistanceScale.Count)
                {
                    scale = renderLevelConfig.valueConfigs.gpuInstancingDistanceScale[renderLevel];
                }
            }
        }
#endif
        float[] distances = ArrayPool<float>.Get(data.@group.lods.Count);
        for (int i = 0; i < data.@group.lods.Count; i++)
            distances[i] = data.@group.lods[i].distance * scale;
        data.cullingGroup.SetBoundingDistances(distances);
        ArrayPool<float>.Release(distances);
    }

    public void Remove(GPUInstancingGroup group)
    {
        for (int i = 0; i < datas.Count; i++)
        {
            GroupInternalData data = datas[i];
            if (data.@group == group)
            {
                data.@group = null;
                data.transforms = null;
                data.cullingGroup.Dispose();
                data.cullingGroup.onStateChanged = null;
                ClearBatchData(data);
                datas.RemoveAt(i);
                SharedObjectPool<GroupInternalData>.Release(data);
                return;
            }
        }
    }

    public void Update()
    {
        bool IsCameraDirty()
        {
            if (!TargetCamera)
                return false;
            Transform cameraTransform = TargetCamera.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 cameraForward = cameraTransform.forward;

            bool result = Vector3.SqrMagnitude(cameraPosition - prevCameraPosition) > POS_THRESHOLD
                          || Vector3.Dot(cameraForward, prevCameraForward) < 1f - VIEW_THRESHOLD
                          || Mathf.Abs(prevFov - TargetCamera.fieldOfView) > FOV_THRESHOLD;

            if (result)
            {
                prevCameraPosition = cameraPosition;
                prevCameraForward = cameraForward;
                prevFov = TargetCamera.fieldOfView;
            }

            return result;
        }
        
        Batch GetBatchData(int lodIndex)
        {
            Batch b = SharedObjectPool<Batch>.Get();
            b.lodIndex = lodIndex;
            return b;
        }

        void UpdateBatchData(GroupInternalData data)
        {
            ClearBatchData(data);

            float updateFadeInterval = 0.2f;
            bool updateFade = (int) (Time.time * updateFadeInterval) != (int) (lastUpdateFadeTime * updateFadeInterval);
            float invLerpA = data.@group.lods[0].distance;
            invLerpA *= invLerpA;
            float invLerpB = data.@group.lods[data.@group.lods.Count - 1].distance;
            invLerpB *= invLerpB;
            float invLerpC = 1 / (invLerpB - invLerpA);
            Vector3 cameraPos; 
            if (updateFade)
                lastUpdateFadeTime = Time.time;

            if (TargetCamera != null)
            {
                cameraPos = TargetCamera.transform.position;
                int count = data.lodList.Count;
                for (int lodIndex = 0; lodIndex < count; lodIndex++)
                {
                    HashSet<int> lod = data.lodList[lodIndex];
                    
                    if (lod.Count == 0)
                        continue;

                    Batch b = GetBatchData(lodIndex);
                    foreach (int instanceIndex in lod)
                    {
                        Matrix4x4 m = data.transforms[instanceIndex];

                        // 密度衰减
                        if (lodIndex > 0)
                        {
                            float random = m.m03 * 3123.3123f;
                            random -= Mathf.Floor(random);
                            random *= random;
                            float deltaX = m.m03 - cameraPos.x;
                            float deltaY = m.m13 - cameraPos.y;
                            float deltaZ = m.m23 - cameraPos.z;
                            float sqrLen = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
                            float distanceRatio = (sqrLen - invLerpA) * invLerpC;
                            if (random < distanceRatio)
                                continue;
                        }

                        b.matrixes[b.count++] = m;
                        if (b.count == MAX_INSTANCE_PER_DRAW)
                        {
                            data.batches.Add(b);
                            b = GetBatchData(lodIndex);
                        }
                    }

                    if (b.count > 0)
                        data.batches.Add(b);
                    else
                        ReleaseBatchData(b);
                }
                
            }
        }

        // 有时ReferencePoint会丢失，需要重新赋值。
        int groupInternalDataCount = datas.Count;
        if (!distanceReference && TargetCamera)
        {
            DistanceReference = TargetCamera.transform;
            
            for (int i = 0; i < groupInternalDataCount; i++)
            {
                datas[i].cullingGroup.SetDistanceReferencePoint(DistanceReference);
            }
        }

        if (supportInstancing)
        {
            // bool cameraDirty = IsCameraDirty();
            // bool needUpdate;
            for (int i = 0; i < groupInternalDataCount; i++)
            {
                // needUpdate = datas[groupInternalDataCount].NeedUpdate();
                //if (cameraDirty || needUpdate)
                    UpdateBatchData(datas[i]);
            }
        }
    }

    public bool NeedRender()
    {
        return supportInstancing;
    } 
    
    public void Render(CommandBuffer cmd, int passIndex)
    {
        foreach (GroupInternalData data in datas)
        {
            for (int j = 0; j < data.batches.Count; j++)
            {
                Batch b = data.batches[j];
                Material mat = data.@group.material;
                Mesh mesh = data.@group.lods[b.lodIndex].mesh;

                if (mesh == null || mat == null)
                {
                    continue;
                }
                
                cmd.DrawMeshInstanced(mesh, 0, mat, passIndex, b.matrixes, b.count);
            }
        }
    }

    #region Private

    private void OnChange(GroupInternalData data, CullingGroupEvent sphere)
    {
        if (supportInstancing)
        {
            List<HashSet<int>> lodList = data.lodList;
            if (sphere.isVisible)
            {
                if (sphere.previousDistance < lodList.Count)
                {
                    lodList[sphere.previousDistance].Remove(sphere.index);
                }

                lodList[sphere.currentDistance].Add(sphere.index);
            }
            else
            {
                if (sphere.previousDistance < lodList.Count)
                {
                    lodList[sphere.previousDistance].Remove(sphere.index);
                }

                if (sphere.currentDistance < lodList.Count)
                {
                    lodList[sphere.currentDistance].Remove(sphere.index);
                }
            }
        }
        else
        {
            data.@group.renderers[sphere.index].enabled = sphere.isVisible;
        }
    }

    private void ReleaseBatchData(Batch b)
    {
        b.count = 0;
        b.lodIndex = 0;
        SharedObjectPool<Batch>.Release(b);
    }

    private void ClearBatchData(GroupInternalData data)
    {
        if (data.batches == null)
        {
            return;
        }
        
        int len = data.batches.Count;
        for (int i = 0; i < len; i++)
        {
            ReleaseBatchData(data.batches[i]);
        }
        
        data.batches.Clear();
    }

    #endregion
}
