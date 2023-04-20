/********************************************************************
	created:	2021/08/30  20:54
	file base:	CameraAvoidBlock
	author:		c a o   f e n g
	
	purpose:	相机中间物体透明
*********************************************************************/

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CFEngine
{
    // TODO: 解决共享材质问题    
    public class CameraAvoidBlock : MonoBehaviour, IOccludeAlpha
    {
        private static DitherTransparentParams distanceFadeConfig;
        public Dictionary<Material, List<Material>> materialPool = new Dictionary<Material, List<Material>>();
        public static int Layer => LayerMask.NameToLayer("CameraBlock");
        public const string suffix = "_Block";
        private const string keyword = "_DITHER_TRANSPARENCY";
        private static readonly int ditherTransparency = Shader.PropertyToID("_DitherTransparency");
        private static readonly int _ScreenParams = Shader.PropertyToID("_ScreenParams");
        private static readonly int _CameraBlockParams0 = Shader.PropertyToID("_CameraBlockParams0");
        private static readonly int _CameraBlockParams1 = Shader.PropertyToID("_CameraBlockParams1");

        /// <summary>
        /// 遮挡检测的碰撞盒xy尺寸（米）。
        /// </summary>
        private static readonly Vector2 boxCastSizeXZ = new Vector2(0.2f, 0.5f);

        /// <summary>
        /// 渲染时的中心高度偏移（米）。
        /// </summary>
        private const float heightOffset = 1.5f;

        /// <summary>
        /// 渲染时的半透明球体区域Z方向扩大(米)。
        /// </summary>
        private const float zOffset = 3.0f;

        private Vector3 targetPos;
        private Vector3 cameraPos;
        private int hitCount = 4;
        private static readonly RaycastHit[] raycastHits = new RaycastHit[32];
        private readonly List<RenderData> lastHitRenderData = new List<RenderData>();
        private static CameraBlockConfig roleBlockConfig;
        private static CameraBlockConfig sceneBlockConfig;

        private static readonly Dictionary<Collider, CameraBlockGroup>
            groups = new Dictionary<Collider, CameraBlockGroup>();

        public static CameraAvoidBlock Instance { get; private set; }

#if UNITY_EDITOR
        public static SavedBool drawBlockGizmos =
            new SavedBool($"{nameof(CameraAvoidBlock)}.{nameof(drawBlockGizmos)}");

        public static SavedBool featureEnabled =
            new SavedBool($"{nameof(CameraAvoidBlock)}.{nameof(featureEnabled)}");
#endif

        private bool occludeAlpha = true;

        private void Awake()
        {
            Instance = this;
            EngineUtility.RegistCameraAvoidBlockHandle(CameraAvoidBlockState);
            MaterialBaseEffect.SetMaterialEffectHandle(SetMaterials);
            
            EngineContext.occludeAlpha = this;
            InitRayCommond();
        }

        private Dictionary<Renderer, Material[]> fadeMaterialsDic = new Dictionary<Renderer, Material[]>();
        public void SetMaterials(Renderer render, Material[] mats)
        {
            if (!fadeMaterialsDic.ContainsKey(render)) return; //没有处于fade状态的 就别进来了
            fadeMaterialsDic[render] = mats;
        }

        private void CameraAvoidBlockState(bool b)
        {
            if (Instance == null) return;
            Instance.enabled = b;
        }

        private void OnDestroy()
        {
            Instance = null;
            EngineUtility.RegistCameraAvoidBlockHandle(null);
            MaterialBaseEffect.SetMaterialEffectHandle(null)
                ;
            ReleaseRayCommond();
        }

        public static void RegisterGroup(CameraBlockGroup cbg)
        {
            foreach (Collider cbgCollider in cbg.colliders)
            {
                if (cbgCollider)
                    groups[cbgCollider] = cbg;
            }
        }

        public static void UnregisterGroup(CameraBlockGroup cbg)
        {
            foreach (Collider cbgCollider in cbg.colliders)
            {
                // 在保存资源时已经检查过collider重复引用的问题，这里不需要再次检查了。
                if (cbgCollider && !groups.Remove(cbgCollider))
                {
                    Debug.LogError("重复移除摄像机遮挡组的碰撞体");
                }
            }
        }

        public class RenderData : ISharedObject
        {
            public Transform positionReference;
            public List<Renderer> renderers = new List<Renderer>();
            public List<Material[]> mats = new List<Material[]>();
            public List<Material[]> orginMats = new List<Material[]>();
            public bool isFade;
            public CameraBlockState state = new CameraBlockState();

            public bool Equals(RenderData other)
            {
                return ListValueEquals(renderers, other?.renderers);
            }

            public void Reset()
            {
                renderers.Clear();
                foreach (Material[] materials in mats)
                {
                    ReleaseMaterialArray(materials);
                }
                foreach (Material[] materials in orginMats)
                {
                    ReleaseMaterialArray(materials);
                }
                orginMats.Clear();
                mats.Clear();
                isFade = false;
                state.Reset();
            }
        }

        private GameObject m_focusTransform;
        private bool hasfocusTransform;

        private Vector3 lastFocus;
        private Vector3 lastEulerAngles;
        private static float changeDis = 0.1f;

        private bool FindCameraAndFocus(out Vector3 focus, out Camera camera)
        {
            hasfocusTransform = EngineContext.instance != null;
            Transform focusTransform = EngineContext.cameraFocusGetter?.Invoke();
            hasfocusTransform &= focusTransform;

            if (hasfocusTransform)
            {
                m_focusTransform = focusTransform.gameObject;
                focus = focusTransform.position;
                camera = EngineContext.instance.CameraRef;
                //todo 美术说灵敏度不够暂时去掉优化
                // if (!Equals(camera, null))
                // {
                //     Vector3 curEuler = camera.transform.eulerAngles;
                //
                //     if (Vector3.Distance(lastFocus, focus) < changeDis &&
                //         Vector3.Distance(curEuler, lastEulerAngles) < changeDis)
                //     {
                //         return false;
                //     }
                //
                //     lastFocus = focus;
                //     lastEulerAngles = curEuler;
                // }
            }
            else
            {
                focus = default;
                camera = null;
            }


            return hasfocusTransform && camera;
        }

        public void LateUpdate()
        {
            if (occludeAlpha && FindCameraAndFocus(out Vector3 focus, out Camera camera))
            {
                if (InitDistanceFadeConfig())
                {
                    if (Time.frameCount % 6 == 0)
                        UpdatePhysics(focus, camera.transform.position);
                    UpdateEffect();
                    ApplyGlobalShaderParams(focus, camera);
                }
            }
        }

        public void ApplyUpdate()
        {
            if (occludeAlpha && FindCameraAndFocus(out Vector3 focus, out Camera camera))
            {
                if (InitDistanceFadeConfig())
                {
                    UpdatePhysics(focus, camera.transform.position, sceneBlockConfig);
                    UpdateEffect();
                    ApplyGlobalShaderParams(focus, camera);
                }
            }
        }


        private static bool InitDistanceFadeConfig()
        {
            if (distanceFadeConfig != null)
                return true;

#if UNITY_EDITOR
            if (!WorldSystem.miscConfig)
            {
                distanceFadeConfig = new DitherTransparentParams()
                {
                    minDistance = 0.5f,
                    maxDistance = 3.0f,
                    minAlpha = 0.2f,
                    maxAlpha = 1.0f,
                    enterDelayTime = 0f,
                    enterLerpTime = 0.2f,
                    leaveDelayTime = 0f,
                    sceneEnterDelayTime = 0f,
                    sceneEnterLerpTime = 0.2f,
                    sceneLeaveDelayTime = 0f,
                    minRoleTransparency = 0.7f,
                    minSceneTransparency = 0.4f
                };
            }
            else
#endif
            {
                distanceFadeConfig = WorldSystem.miscConfig.ditherTransparent;
            }

            roleBlockConfig = new CameraBlockConfig(distanceFadeConfig.enterDelayTime, distanceFadeConfig.enterLerpTime,
                distanceFadeConfig.leaveDelayTime, distanceFadeConfig.minRoleTransparency);
            sceneBlockConfig = new CameraBlockConfig(distanceFadeConfig.sceneEnterDelayTime,
                distanceFadeConfig.sceneEnterLerpTime, distanceFadeConfig.sceneLeaveDelayTime,
                distanceFadeConfig.minSceneTransparency);
            return distanceFadeConfig != null;
        }

        private static void ApplyGlobalShaderParams(Vector3 targetPos, Camera camera)
        {
            targetPos.y += heightOffset;
            Vector3 positionCS = camera.transform.InverseTransformPoint(targetPos);
            Vector3 positionVS = camera.WorldToViewportPoint(targetPos);
            float roleDistance = positionCS.z + zOffset;
            float2 rolePositionVS = new float2(positionVS.x, positionVS.y);
            float2 blockRange = new float2(0.3f, 1.0f);
            float4 screenParams = Shader.GetGlobalVector(_ScreenParams);
            float4 cameraBlockParams1 = new float4(1.0f / roleDistance, blockRange.x, blockRange.y, 0);
            float2 xzScale = new float2(1.5f, 1.5f) * new float2(1.0f, screenParams.y / screenParams.x);
            float4 cameraBlockParams0 = new float4(1.0f / screenParams.xy * xzScale, -rolePositionVS * xzScale);
            Shader.SetGlobalVector(_CameraBlockParams0, cameraBlockParams0);
            Shader.SetGlobalVector(_CameraBlockParams1, cameraBlockParams1);
        }

        // 获取新材质
        Material[] GetMaterialsNoneAlloc(Renderer r, bool isShare)
        {
            List<Material> materials = ListPool<Material>.Get();
            if (isShare)
            {
                r.GetSharedMaterials(materials);
            }
            else
            {
                r.GetMaterials(materials);
            }
            Material[] result = ArrayPool<Material>.Get(materials.Count);
            for (int i = 0; i < materials.Count; i++)
                result[i] = materials[i];
            ListPool<Material>.Release(ref materials);
            return result;
        }


        private static void ReleaseMaterialArray(Material[] materials)
        {
            for (int i = 0; i < materials.Length; i++)
                materials[i] = null;
            ArrayPool<Material>.Release(materials);
        }

        static void ApplyTransparency(Transform position, Renderer renderer, Material[] materials, float transparency)
        {
            float distanceFactor;
            Camera camera = Camera.main;

            if (camera && distanceFadeConfig != null && position)
            {
                float sqrDistance = (camera.transform.position - position.position).sqrMagnitude;
                float distanceRatio = Mathf.InverseLerp(
                    distanceFadeConfig.minDistance * distanceFadeConfig.minDistance,
                    distanceFadeConfig.maxDistance * distanceFadeConfig.maxDistance,
                    sqrDistance);
                distanceFactor = Mathf.Lerp(distanceFadeConfig.minAlpha, distanceFadeConfig.maxAlpha, distanceRatio);
            }
            else
            {
                distanceFactor = 1;
            }

            foreach (Material mat in materials)
            {
                if (mat)
                {
                    if (transparency < 1)
                    {
                        if (mat.renderQueue == 2450)
                        {
                            mat.renderQueue = 2450;
                        }
                        else
                        {
                            mat.renderQueue = 3000;
                        }

                        mat.EnableKeyword(keyword);
                        mat.SetFloat(ditherTransparency, transparency * distanceFactor);
                    }
                }
            }
        }


        void UpdatePhysics(Vector3 targetPos, Vector3 cameraPos, CameraBlockConfig targetConfig = null)
        {
            // TODO: 偏移值可配置化
            Vector3 backDir = (cameraPos - targetPos).normalized;
            backDir.x += 1e-4f;
            targetPos += backDir * 0.5f;
            this.targetPos = targetPos;
            this.cameraPos = cameraPos;


            void AddBlockRenderers(Transform positionReference, List<Renderer> renderers,
                CameraBlockConfig overrideConfig)
            {
                if (renderers == null) return;

                RenderData rd = null;
                foreach (RenderData renderData in lastHitRenderData)
                    if (ListValueEquals(renderData?.renderers, renderers))
                        rd = renderData;

                if (rd != null)
                {
                    rd.isFade = true;
                    // 材质被其他逻辑修改时，需要关闭被修改的材质的keyword，并对新材质应用半透明效果。
                    for (int i = 0; i < rd.renderers.Count; i++)
                    {
                        Renderer renderer = renderers[i];
                        if (renderer)
                        {
                            Material[] newMats = GetMaterialsNoneAlloc(renderer, true);
                            Material[] lastMats = rd.mats[i];
                            if (!ListValueEquals(newMats, lastMats))
                            {
                                ApplyTransparency(rd.positionReference, renderer, lastMats, 1);
                                ReleaseMaterialArray(lastMats);
                                rd.mats[i] = newMats;
                            }
                            else
                            {
                                ReleaseMaterialArray(newMats);
                            }
                        }
                    }
                }
                else
                {
                    RenderData newData = SharedObjectPool<RenderData>.Get();
                    newData.renderers.AddRange(renderers);
                    for (int i = 0; i < renderers.Count; i++)
                    {
                        Renderer renderer = renderers[i];
                        if (renderer)
                        {
                            var orginMats = GetMaterialsNoneAlloc(renderers[i], true);
                            var mat = GetMaterialsNoneAlloc(renderers[i], false);
                            newData.mats.Add(mat);
                            newData.orginMats.Add(orginMats);
                            renderer.sharedMaterials = mat;
                            if (!fadeMaterialsDic.ContainsKey(renderer))
                            {
                                fadeMaterialsDic.Add(renderer, null);
                            }
                        }
                    }

                    newData.isFade = true;
                    Action<float, CameraBlockState.OcclusionStage> applyCallback = (transparency, status) =>
                    {
                        for (int i = 0; i < newData.renderers.Count; i++)
                        {
                            ApplyTransparency(newData.positionReference, newData.renderers[i],newData.mats[i], transparency);

                        }
                    };
                    newData.state.Init(applyCallback);
                    lastHitRenderData.Add(newData);
                    newData.state.StartOccluding(overrideConfig ?? roleBlockConfig);
                    newData.positionReference = positionReference;
                }
            }

            foreach (RenderData renderData in lastHitRenderData)
                renderData.isFade = false;


            CalculateCastInfo();
            // 碰撞无法内部穿透 起始点后移 可能会出现最近的无dither 第二近的出现dither bug
            var start = targetPos - backDir * distanceFadeConfig.offsetDistance;
            mRayCommand[0] = new RaycastCommand(start, -mDirection, fDistance, 1 << Layer, 1);
            for (int i = 0; i < hitCount; i++)
            {
                mHitResult[i] = default;
            }

            mJobHandle = RaycastCommandMultihit.ScheduleBatch(mRayCommand, mHitResult, 4, hitCount, default(JobHandle));
            mJobHandle.Complete();

            bool isNewTick = false;
            for (int i = 0; i < mHitResult.Length; i++)
            {
                Collider tempCollider = mHitResult[i].collider;
                if (tempCollider == null) continue;

                if (groups.TryGetValue(tempCollider, out CameraBlockGroup group))
                {
                    if (hasfocusTransform && ReferenceEquals(group.gameObject, m_focusTransform)) continue; //射到自己不算
                    CameraBlockConfig tempBlockConfig;
                    if (group.blockType == CameraBlockGroup.BlockType.RoleOrMonster)
                    {
                        Vector3 colliderPos = tempCollider.transform.position;
                        colliderPos.y = targetPos.y;
                        Vector3 targetDir = (colliderPos - targetPos).normalized;
                        Vector3 camDir = (cameraPos - targetPos).normalized;
                        float dotResult = Vector3.Dot(targetDir, camDir);
                        if (Mathf.Abs(dotResult) < 0.32f)
                        {
                            continue;
                        }

                        tempBlockConfig = group.isOverrideMiscConfig ? group.config : roleBlockConfig;
                    }
                    else
                    {
                        tempBlockConfig = group.isOverrideMiscConfig ? group.config : sceneBlockConfig;
                    }

                    AddBlockRenderers(group.transform, group.renderers, tempBlockConfig);
                }
                else
                {
                    Transform parent = tempCollider.transform.parent;
                    if (parent && parent.TryGetComponent(out Renderer rendererCom))
                    {
                        List<Renderer> list = ListPool<Renderer>.Get();
                        list.Add(rendererCom);
                        AddBlockRenderers(rendererCom.transform, list,
                            targetConfig == null ? roleBlockConfig : targetConfig);
                        ListPool<Renderer>.Release(ref list);
                    }
                }
            }

            // 检查不再遮挡的物体并淡出
            for (int j = lastHitRenderData.Count - 1; j >= 0; j--)
            {
                RenderData rd = lastHitRenderData[j];
                if (rd.isFade)
                    continue;
                if (rd.state.stage == CameraBlockState.OcclusionStage.LeaveDelay
                    || rd.state.stage == CameraBlockState.OcclusionStage.NoOcclusion)
                    continue;
                rd.state.EndOccluding();
            }
        }


        private NativeArray<RaycastCommand> mRayCommand;
        private NativeArray<RaycastHit> mHitResult;
        private JobHandle mJobHandle;

        private void InitRayCommond()
        {
            mHitResult = new NativeArray<RaycastHit>(hitCount, Allocator.Persistent);
            mRayCommand = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
        }

        private void ReleaseRayCommond()
        {
            if (mJobHandle.IsCompleted)
            {
                mJobHandle.Complete();
            }

            mHitResult.Dispose();
            mRayCommand.Dispose();
        }


        private void UpdateEffect()
        {
            // 更新渲染进度
            for (int i = 0; i < lastHitRenderData.Count; i++)
                lastHitRenderData[i].state.Update();

            // 移除已经完全失效的对象
            int j;
            for (j = 0; j < lastHitRenderData.Count; j++)
            {
                RenderData rd = lastHitRenderData[j];


                if (!rd.isFade && rd.state.stage == CameraBlockState.OcclusionStage.NoOcclusion)
                {
                    for (int i = 0; i < rd.renderers.Count; i++)
                    {
                        Material[] mats = rd.orginMats[i];
                        Renderer tmpRd = rd.renderers[i];
                        if (tmpRd != null && mats != null && mats.Length > 0 && mats[0] != null)
                        {
                            Material[] tmpMat = null;
                            if (fadeMaterialsDic.TryGetValue(tmpRd, out tmpMat))
                            {
                                if (tmpMat != null && tmpMat[0] != null)
                                {
                                    mats = tmpMat;
                                }

                                fadeMaterialsDic.Remove(tmpRd);
                            }

                            mats[0].DisableKeyword(keyword);
                            tmpRd.sharedMaterials = mats;
                        }



                        //不知道为什么手动删除会造成悬空指针问题 留给系统GC把
                        // foreach (var mat in rd.mats[i])
                        // {
                        //     Destroy(mat);
                        // }
                        for (int k = 0; k < rd.mats[i].Length; k++)
                        {
                            rd.mats[i][k] = null;
                        }
                    }

                    rd.mats.Clear();
                    SharedObjectPool<RenderData>.Release(rd);
                    lastHitRenderData.RemoveAt(j);
                }
            }
        }

        private static bool ListValueEquals<T>(IList<T> a, IList<T> b)
        {
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] == null && b[i] != null)
                    return false;
                if (a[i] != null)
                {
                    if (!a[i].Equals(b[i]))
                        return false;
                }
            }

            return true;
        }

        public float mExtentSize = 1;

        private void OnDrawGizmos()
        {
            
#if UNITY_EDITOR
            if(Camera.main==null) return;
            
            Gizmos.color = new Color(1, 1, 0);

            Gizmos.DrawLine(this.cameraPos, this.targetPos);
            if (occludeAlpha && drawBlockGizmos.Value && targetPos != cameraPos)
            {
                // Draw focus.
                Gizmos.color = new Color(0, 1, 0, 0.35f);
                Gizmos.DrawSphere(targetPos, 0.05f);

                // Draw cast collider
                Gizmos.color = new Color(0, 1, 0, 0.35f);
                CalculateCastInfo(out Quaternion cameraRot, out Vector3 extent, out Vector3 middle);
                Gizmos.matrix = Matrix4x4.TRS(middle, cameraRot, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, extent * 2);

                // Debug.Log("OnDrawGizmos:"+middle+"  extent:"+extent);


                // Draw block colliders
                Gizmos.color = new Color(1, 0, 0, 0.35f);
                for (int i = 0; i < hitCount; i++)
                {
                    BoxCollider c = raycastHits[i].collider as BoxCollider;
                    if (c)
                    {
                        Gizmos.matrix = c.transform.localToWorldMatrix;
                        Gizmos.DrawCube(c.center, c.size);
                    }
                }
            }
#endif
        }

        public float fDistance = 0;
        public Vector3 mDirection;

        private void CalculateCastInfo()
        {
            Vector3 delta = targetPos - cameraPos;
            fDistance = delta.magnitude;
            mDirection = (delta).normalized;
        }

        private void CalculateCastInfo(out Quaternion cameraRot, out Vector3 extent, out Vector3 middle)
        {
            Vector3 ori = targetPos;
            Vector3 delta = cameraPos - ori;
            middle = (cameraPos + ori) * 0.5f;

            float distance = delta.magnitude;

            fDistance = distance;

            Vector3 direction = (delta).normalized;
            mDirection = -direction;

            cameraRot = Quaternion.LookRotation(direction, Vector3.up);
            extent = new Vector3(boxCastSizeXZ.x * distanceFadeConfig.boxCastScaleX,
                boxCastSizeXZ.y * distanceFadeConfig.boxCastScaleY, distance * 0.5f);

            middle = cameraPos;
            extent = new Vector3(boxCastSizeXZ.x * distanceFadeConfig.boxCastScaleX,
                boxCastSizeXZ.y * distanceFadeConfig.boxCastScaleY, 0.01f);
            cameraRot = Camera.main.transform.rotation;
        }

        public void TurnOnOcclude()
        {
            occludeAlpha = true;
        }

        public void TurnOffOcclude()
        {
            occludeAlpha = false;
        }
    }
}