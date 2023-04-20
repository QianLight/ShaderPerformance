using System;
using System.Collections.Generic;
using System.Linq;
using Impostors.Attributes;
using Impostors.Jobs;
using Impostors.Managers.QueueSortingMethods;
using Impostors.MemoryUsage;
using Impostors.Structs;
using Impostors.RenderPipelineProxy;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using CFEngine;
using Impostors.TimeProvider;
using MeshLOD;

namespace Impostors.Managers
{
    [DefaultExecutionOrder(-776)]
    public class ImpostorableObjectsManager : MonoBehaviour, IMemoryConsumer
    {
        [Range(1, 100)]
        public int maxUpdatesPerFrame = 20;

        [Range(1, 50)]
        public int maxBackgroundUpdatesPerFrame = 10;

        [SerializeField, DisableAtRuntime]
        private AtlasResolution _atlasResolution = AtlasResolution._1024x1024;

        [SerializeField]
        private TextureResolution  _memoryMinResolution = TextureResolution._64x64;
        
        public int _maxMemory = 120;    //��ʱ�ĳ�120
        
        public bool isFogOpen = true;
        
        [Range(0.5f, 4f)]
        [SerializeField]
        public float textureSizeScale = 1f;

        [FormerlySerializedAs("_mainCamera")]
        [SerializeField]
        public Camera mainCamera = null;

        [SerializeField]
        private Light _directionalLight = null;

        [SerializeField]
        private UpdateType _updateType = UpdateType.OnPreCull;

        [SerializeField]
        public Color backgroundClearColor = new Color(0f, 0f, 0f, 0);

        [SerializeField, Layer]
        public int renderingLayer = 0;

        [FormerlySerializedAs("GraphicsApiProxy")]
        [SerializeField]
        internal RenderPipelineProxyBase _renderPipelineProxy = default;


        public bool isShowByGameRole = true;

        public Action<ImpostorLODGroup> OnObjGoToImpostorEvent;
        public Action<ImpostorLODGroup> OnObjGoToNormalEvent;

        private enum UpdateType
        {
            OnPreCull,
            OnLateUpdate,
            Manual
        }

        [Header("DEBUG")]
        public bool debugModeEnabled = default;

        public bool debugSort;
        public List<ImpostorLODGroup> debugSortImpostorLODGroup = new List<ImpostorLODGroup>();
        public List<ImpostorLODGroup> debugUpdateImpostorLODGroup = new List<ImpostorLODGroup>();
        
        public bool debugCascadesModeEnabled = default;

        public Color debugColor = Color.green;

        static readonly Gradient CascadeGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.green, 0),
                new GradientColorKey(Color.yellow, 128 / 512f),
                new GradientColorKey(new Color(1.0f, 0.64f, 0.0f), 256 / 512f),
                new GradientColorKey(Color.red, 1),
            }
        };

#if UNITY_EDITOR
        [SerializeField]
        private ImpostorableObject[] _debugListOfImpostorableObjects = null;
#endif

        [SerializeField]
        private ImpostorsChunkPool _chunkPool;

        SimplePlane[] simplePlanes = new SimplePlane[6];
        private NativeList<ImpostorableObject> _impostorableObjects;
        private NativeQueue<int> _updateQueue;
        private CommandBufferProxy _commandBufferProxy;

        private MaterialPropertyBlock _chunksRenderingPropertyBlock;
        private List<int> _updateQueueSortingList;
        private List<ImpostorsChunk> _renderedChunks;

        private bool _isDisposed = true;

        public static ImpostorableObjectsManager _instance;


        private int _memoryMinResolutionInt;

        private void Awake()
        {
            maxUpdatesPerFrame = 5;
            maxBackgroundUpdatesPerFrame = 5;
            
            if (GameObject.FindObjectOfType<XScript>() == null)
            {
                isShowByGameRole = false;
            }

            _memoryMinResolutionInt = (int) _memoryMinResolution;
        }

        private void OnEnable()
        {
            _instance = this;

            if (EngineUtility.GetMainCamera() == null)
            {
                this.enabled = false;
                return;
            }

            AllocateNativeCollections();
            ImpostorLODGroupsManager.Instance.RegisterImpostorableObjectsManager(this);

            if (_renderPipelineProxy == null)
            {
                string error = "RenderPipelineProxy is not specified!\n\n" +
                               "Impostors won't work without specifying right RenderPipelineProxy. " +
                               "Please, add appropriate RenderPipelineProxy and place it in corresponding field.\n\n" +
                               $"Suggested proxy type:\n'{RenderPipelineProxyTypeProvider.Get().FullName}'";
                Debug.LogError("[IMPOSTORS] " + error, this);
                enabled = false;
#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.PingObject(this);
                UnityEditor.EditorUtility.DisplayDialog("Impostors Error", error, "Ok");
#endif
                return;
            }
#if UNITY_EDITOR
            var currentProxyType = _renderPipelineProxy.GetType();
            var suggestedProxyType = RenderPipelineProxyTypeProvider.Get();
            if (RenderPipelineProxyTypeProvider.IsOneOfStandardProxy(currentProxyType) &&
                currentProxyType != suggestedProxyType)
            {
                string error = "Looks like you are using wrong RenderPipelineProxy!\n" +
                               $"Current: '{currentProxyType.FullName}'.\n" +
                               $"Suggested: '{suggestedProxyType.FullName}'.\n\n" +
                               $"Look at the Impostors documentation about setup for render pipelines.";
                Debug.LogError("[IMPOSTORS] " + error, this);
                UnityEditor.EditorGUIUtility.PingObject(this);
                UnityEditor.EditorUtility.DisplayDialog("Impostors Error", error, "Ok");
            }
#endif


            if (_updateType == UpdateType.OnPreCull)
            {
                _renderPipelineProxy.PreCullCalled += OnPreCullCallback;
            }
        }

        private void OnDisable()
        {
            //if (Camera.main == null) return;

            DisposeNativeCollections();
            ImpostorLODGroupsManager.Instance.UnregisterImpostorableObjectsManager(this);
            if (_updateType == UpdateType.OnPreCull)
            {
                _renderPipelineProxy.PreCullCalled -= OnPreCullCallback;
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (debugModeEnabled)
            {
                if (_debugListOfImpostorableObjects.Length == _impostorableObjects.Length)
                {
                    NativeArray<ImpostorableObject>.Copy(_impostorableObjects, _debugListOfImpostorableObjects);
                }
                else
                {
                    _debugListOfImpostorableObjects = _impostorableObjects.ToArray();
                }
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && _renderPipelineProxy == null)
            {
                if ((_renderPipelineProxy = GetComponent<RenderPipelineProxyBase>()) == null)
                {
                    Debug.LogError(
                        $"[IMPOSTORS] RenderPipelineProxy is required. Please, assign it in the inspector. " +
                        $"Suggested component '{RenderPipelineProxyTypeProvider.Get().Name}'", this);
                }
            }
        }
#endif

        private void LateUpdate()
        {
            if (_updateType == UpdateType.OnLateUpdate)
                OnPreCullCallback(mainCamera);

            if (!GlobalShaderEffects.IsChangeSceneColor)
            {
                UpdateSceneColorImpostor();
            }
        }

        private void AllocateNativeCollections()
        {

            if (mainCamera == null) mainCamera = EngineUtility.GetMainCamera();

            _isDisposed = false;
            _impostorableObjects = new NativeList<ImpostorableObject>(Allocator.Persistent);
            _updateQueue = new NativeQueue<int>(Allocator.Persistent);
            _commandBufferProxy = new CommandBufferProxy();
            _commandBufferProxy.CommandBuffer.name = $"ExecuteCommandBuffer Render Impostors for '{mainCamera.name}'";
            _chunksRenderingPropertyBlock = new MaterialPropertyBlock();
            _updateQueueSortingList = new List<int>();
            _renderedChunks = new List<ImpostorsChunk>();
            _chunkPool = new ImpostorsChunkPool(
                Enum.GetValues(typeof(TextureResolution)) as int[],
                (int) _atlasResolution,
                ImpostorLODGroupsManager.Instance.TimeProvider,
                ImpostorLODGroupsManager.Instance.RenderTexturePool,
                ImpostorLODGroupsManager.Instance.MaterialObjectPool);
        }

        private void DisposeNativeCollections()
        {
            _isDisposed = true;

            if (_impostorableObjects.IsCreated)
                _impostorableObjects.Dispose();

            if (_updateQueue.IsCreated)
                _updateQueue.Dispose();

            if (_commandBufferProxy != null)
                _commandBufferProxy.Dispose();
            _commandBufferProxy = null;

            if (_chunkPool != null)
                _chunkPool.Dispose();
            _chunkPool = null;

            if (_updateQueueSortingList != null)
                _updateQueueSortingList.Clear();
            _updateQueueSortingList = null;

            if (_renderedChunks != null)
                _renderedChunks.Clear();
            _renderedChunks = null;
        }

        /// <summary>
        /// Adds impostors to rendering queue for specified camera.
        /// </summary>
        /// <param name="camera">Camera, where impostors will be rendered</param>
        public void DrawImpostorsForCamera(Camera camera)
        {
            Shader.SetGlobalVector(ShaderProperties._ImpostorsWorldSpaceCameraPosition, mainCamera.transform.position);
            //_chunksRenderingPropertyBlock.SetVector(ShaderProperties._ImpostorsWorldSpaceCameraPosition,
            //    mainCamera.transform.position);
            _chunksRenderingPropertyBlock.SetColor(ShaderProperties._ImpostorsDebugColor,
                debugModeEnabled ? debugColor : Color.clear);
            var chunks = _chunkPool.Chunks;
            for (int i = 0, count = chunks.Count; i < count; i++)
            {
                ImpostorsChunk chunk = chunks[i];
                if (!chunk.IsEmpty)
                {
                    var materialPropertyBlock = _chunksRenderingPropertyBlock;
                    if (debugModeEnabled && debugCascadesModeEnabled)
                    {
                        var debugColor = CascadeGradient.Evaluate(chunk.TextureResolution / 512f) * 0.5f;
                        materialPropertyBlock.SetColor(ShaderProperties._ImpostorsDebugColor, debugColor);
                    }

                    if (!debugModeEnabled)
                    {
                        materialPropertyBlock = null;
                    }

                    _renderPipelineProxy.DrawMesh(chunk.GetMesh(), Vector3.zero, Quaternion.identity,
                        chunk.GetMaterial(),
                        renderingLayer,
                        camera,
                        0, materialPropertyBlock, castShadows: false, receiveShadows: false,
                        useLightProbes: false);
                }
            }
        }

        private void UpdateSceneColorImpostor()
        {
            if (sceneColorImpostorList == null || sceneColorImpostorList.Count <= 0)
            {
                return;
            }
            
            for (int i = 0; i < sceneColorImpostorList.Count; i++)
            {
                ImpostorLODGroupsManager.Instance.RequestImpostorTextureUpdate(sceneColorImpostorList[i]);
            }
            isStart = false;
            sceneColorImpostorList.Clear();
        }

        public void UpdateImpostorSystem()
        {

            if (isShowByGameRole)
            {
                EngineContext engineContext = EngineContext.instance;
                if (engineContext == null || engineContext.mainRole == null) return;
            }



            ImpostorsCulling_ProfilerMarker.Begin();

            SchedulingCullingJobs_ProfilerMarker.Begin();
            JobHandle cullingJobHandle;
            int impostorsCount = _impostorableObjects.Length;

            #region Objects Visibility

            SchedulingObjectsVisibilityJob_ProfilerMarker.Begin();
            {
                ImpostorsUtility.CalculateFrustumPlanes(mainCamera, simplePlanes, 1);
                NativeArray<SimplePlane> simplePlanesArray =
                    new NativeArray<SimplePlane>(simplePlanes, Allocator.TempJob);
                var jobImpostorableObjectsVisibility = new ImpostorableObjectsVisibilityJob
                {
                    cameraPlanes = simplePlanesArray,
                    impostors = _impostorableObjects
                };

                cullingJobHandle =
                    jobImpostorableObjectsVisibility.Schedule(impostorsCount, 32);
                simplePlanesArray.Dispose(cullingJobHandle);
            }
            SchedulingObjectsVisibilityJob_ProfilerMarker.End();

            #endregion //Objects Visibility

            #region Is Need Update Impostors

            SchedulingIsNeedUpdateJob_ProfilerMarker.Begin();
            {
                var jobIsNeedUpdateImpostors = new IsNeedUpdateImpostorsJob()
                {
                    impostors = _impostorableObjects,
                    multiplier =
                        2 * Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) / QualitySettings.lodBias,
                    cameraPosition = mainCamera.transform.position,
                    lightDirection = _directionalLight ? _directionalLight.transform.forward : Vector3.zero,
                    textureSizeMultiplier = textureSizeScale * Screen.height / QualitySettings.lodBias,
                    memoryMinResolution=_memoryMinResolutionInt,
                    gameTime = ImpostorLODGroupsManager.Instance.TimeProvider.Time
                };
                cullingJobHandle = jobIsNeedUpdateImpostors.Schedule(impostorsCount, 32, cullingJobHandle);
            }
            SchedulingIsNeedUpdateJob_ProfilerMarker.End();

            #endregion //Is Need Update Impostors

            #region Add Impostorable Objects to update queue

            JobHandle sortJobHandle;
            


                
            SchedulingUpdateQueueJob_ProfilerMarker.Begin();
            {
                _updateQueue.Clear();
                ITimeProvider timeProvider = ImpostorLODGroupsManager.Instance.TimeProvider;
                sortJobHandle = ByScreenSizeAndTimeFromLastUpdate.Sort(
                        _impostorableObjects, _updateQueue,
                        maxUpdatesPerFrame, maxBackgroundUpdatesPerFrame,timeProvider.Time,
                        cullingJobHandle);
            }

            SchedulingUpdateQueueJob_ProfilerMarker.End();

            #endregion //Add Impostorable Objects to update queue


            
            var chunks = _chunkPool.Chunks;
            NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(chunks.Count, Allocator.Temp);

            #region Updating Impostors

            SchedulingUpdateImpostorsJob_ProfilerMarker.Begin();
            {
                for (int i = 0, count = chunks.Count; i < count; i++)
                {
                    jobHandles.Add(chunks[i].ScheduleUpdateImpostors(cullingJobHandle));
                }
            }
            SchedulingUpdateImpostorsJob_ProfilerMarker.End();

            #endregion //Updating Impostors

            SchedulingCullingJobs_ProfilerMarker.End();

            JobHandle.CompleteAll(jobHandles);
            cullingJobHandle.Complete();
            jobHandles.Clear();
            sortJobHandle.Complete();

            ImpostorsCulling_ProfilerMarker.End();

            UpdateImpostorTextures();

            DestroyingEmptyChunks_ProfilerMarker.Begin();
            _chunkPool.DestroyEmpty();
            DestroyingEmptyChunks_ProfilerMarker.End();

            #region Impostors Mesh Creation

            ImpostorsMeshCreation_ProfilerMarker.Begin();
            {
                SchedulingMeshJobs_ProfilerMarker.Begin();
                for (int i = 0, count = chunks.Count; i < count; i++)
                {
                    if (chunks[i].NeedToRebuildMesh)
                    {
                        jobHandles.Add(chunks[i].ScheduleMeshCreation(default));
                    }
                }

                SchedulingMeshJobs_ProfilerMarker.End();

                JobHandle.CompleteAll(jobHandles);
                jobHandles.Dispose();
            }
            ImpostorsMeshCreation_ProfilerMarker.End();

            #endregion //Impostors Mesh Creation
            
            DebugSortData();
        }

        private void DebugSortData()
        {
            if(!debugSort) return;

            debugSortImpostorLODGroup.Clear();
            for (int i = 0; i < _impostorableObjects.Length; i++)
            {
                ImpostorableObject itm = _impostorableObjects[i];
                ImpostorLODGroup newItm=  ImpostorLODGroupsManager.Instance.GetByInstanceId(itm.impostorLODGroupInstanceId);
                newItm.nowDistance = itm.nowDistance;
                newItm.nowScreenSize = itm.nowScreenSize;
                newItm.sortIndex = i;
                newItm.nowTextureResolution = (TextureResolution)itm.lastUpdate.textureResolution;
                
                debugSortImpostorLODGroup.Add(newItm);
            }
        }

        private void OnPreCullCallback(Camera cam)
        {
#if UNITY_EDITOR
            if (debugModeEnabled)
            {
                ImpostorsSceneCameraRendering_ProfilerMarker.Begin();
                if (cam.cameraType == CameraType.SceneView)
                {
                    DrawImpostorsForCamera(cam);
                    return;
                }

                ImpostorsSceneCameraRendering_ProfilerMarker.End();
            }
#endif

            if (cam != mainCamera)
                return;

            ImpostorSystem_ProfilerMarker.Begin();

            if (!Application.isEditor || Time.deltaTime > 0) // if paused game in editor don't run update
                UpdateImpostorSystem();

            DrawImpostors_ProfilerMarker.Begin();
            DrawImpostorsForCamera(cam);
            DrawImpostors_ProfilerMarker.End();

            ImpostorSystem_ProfilerMarker.End();
        }

        private void UpdateImpostorTextures()
        {
            if (_updateQueue.Count <= 0)
                return;
            UpdateImpostorTextures_ProfilerMarker.Begin();

            _updateQueueSortingList.Clear();
            if (_updateQueueSortingList.Capacity < _updateQueue.Count)
                _updateQueueSortingList.Capacity = _updateQueue.Count;
            for (int i = 0, count = _updateQueue.Count; i < count; i++)
            {
                int id = _updateQueue.Dequeue();
                ProcessImpostorableObject(id, out bool requiresTextureUpdate);
                if (requiresTextureUpdate)
                    _updateQueueSortingList.Add(id);
            }

            // SortingUpdateQueue_ProfilerMarker.Begin();
            // if (_updateQueueSortingList.Count < 200) // ignore sorting when it's not optimal
            //     _updateQueueSortingList.Sort((i1, i2) =>
            //         _impostorableObjects[i1].ChunkId - _impostorableObjects[i2].ChunkId);
            // SortingUpdateQueue_ProfilerMarker.End();

            CalculateLightProbes_ProfilerMarker.Begin();
            var lightProbes =
                ImpostorsUtility.LightProbsUtility.GetLightProbes(_updateQueueSortingList, _impostorableObjects);
            CalculateLightProbes_ProfilerMarker.End();

            Vector3 cameraPosition = mainCamera.transform.position;
            var bufferProxy = _commandBufferProxy;
            bufferProxy.Clear();
            var cb = bufferProxy.CommandBuffer;
            _renderPipelineProxy.SetFogEnabled(false, cb);
            if (_directionalLight && _directionalLight.gameObject.activeInHierarchy &&
                _directionalLight.isActiveAndEnabled)
            {
                Vector4 lightPos = _directionalLight.transform.localToWorldMatrix.GetColumn(2);
                Vector4 lightDir = new Vector4(-lightPos.x, -lightPos.y, -lightPos.z, 0);
                cb.SetGlobalVector(ShaderProperties._WorldSpaceLightPos0, lightDir);
                cb.SetGlobalColor(ShaderProperties._LightColor0, _directionalLight.color * _directionalLight.intensity);
            }

            // this shitty algorithm is there to minimize SetRenderTarget commands
            _renderedChunks.Clear();
            ImpostorsChunk chunk = null;
            
            debugUpdateImpostorLODGroup.Clear();
            
            for (int i = 0; i < _updateQueueSortingList.Count; i++)
            {
                var id = _updateQueueSortingList[i];
                var impostorableObject = _impostorableObjects[id];
                if (impostorableObject.requiredAction != ImpostorableObject.RequiredAction.GoToImpostorMode &&
                    impostorableObject.requiredAction != ImpostorableObject.RequiredAction.UpdateImpostorTexture)
                {
                    Debug.LogError("Unexpected behaviour. There must be only impostors that require texture update.");
                    continue;
                }

                var impostorLODGroup =
                    ImpostorLODGroupsManager.Instance.GetByInstanceId(impostorableObject.impostorLODGroupInstanceId);
                
                if(impostorLODGroup==null) continue;
                
                if (chunk == null || chunk.Id != impostorableObject.ChunkId)
                {
                    chunk?.EndRendering(cb);
                    chunk = _chunkPool.GetById(impostorableObject.ChunkId);
                    chunk.BeginRendering(cb);
                    _renderedChunks.Add(chunk);
                }
                
                //Debug.Log("chunk.Id:"+chunk.Id+"  impostorableObject.ChunkId:"+impostorableObject.ChunkId+"  impostorableObject.PlaceInChunk:"+impostorableObject.PlaceInChunk);
                
                // set viewport, clear
                chunk.AddCommandBufferCommands(impostorableObject.PlaceInChunk, cb);
                cb.ClearRenderTarget(true, true, backgroundClearColor);
                // set V and P matrices, add renderers  

                impostorLODGroup.AddCommandBufferCommands(bufferProxy, cameraPosition, impostorableObject.nowScreenSize,
                    lightProbes, i);

                if (debugSort)
                {
                    debugUpdateImpostorLODGroup.Add(impostorLODGroup);
                }
            }

            if (chunk != null)
                chunk.EndRendering(cb);

            _renderPipelineProxy.SetFogEnabled(true, cb);
            // restoring projection params to prevent problems with fog
            {
                float farClipPlane;
                cb.SetGlobalVector(ShaderProperties._ProjectionParams,
                    new Vector4(-1, mainCamera.nearClipPlane, (farClipPlane = mainCamera.farClipPlane),
                        1 / farClipPlane));
            }

            SchedulingRenderImpostorTextures_ProfilerMarker.Begin();
            _renderPipelineProxy.ScheduleImpostorTextureRendering(cb);
            SchedulingRenderImpostorTextures_ProfilerMarker.End();

            _updateQueue.Clear();
            UpdateImpostorTextures_ProfilerMarker.End();
        }

        internal void AddImpostorableObject(ImpostorLODGroup impostorLODGroup)
        {
            if (_isDisposed)
                throw new AccessViolationException($"{GetType().Name} is disposed.");

            var impostorableObject = CreateFromImpostorLODGroup(impostorLODGroup);
            _impostorableObjects.Add(impostorableObject);
            UpdateSettings(_impostorableObjects.Length - 1, impostorLODGroup);
        }

        internal void RemoveImpostorableObject(ImpostorLODGroup impostorLodGroup, int index)
        {
            if (_isDisposed)
                return;

            var io = _impostorableObjects[index];
            Assert.AreEqual(io.impostorLODGroupInstanceId, impostorLodGroup.GetInstanceID());
            _impostorableObjects.RemoveAtSwapBack(index);

            var chunks = _chunkPool.Chunks;
            for (int i = 0, count = chunks.Count; i < count; i++)
            {
                chunks[i].RemoveAllImpostorsWithImpostorLODGroupInstanceId(io.impostorLODGroupInstanceId);
            }
        }

        public bool _isOverMemory = false;

        private List<ImpostorLODGroup> sceneColorImpostorList = new List<ImpostorLODGroup>();
        private bool isStart;

        private void ProcessSceneColorAdd(ImpostorLODGroup impostorLODGroup)
        {
            if (!GlobalShaderEffects.IsChangeSceneColor)
            {
                return;
            }

            if (impostorLODGroup == null)
            {
                return;
            }
            
            if (!isStart)
            {
                sceneColorImpostorList.Clear();
                isStart = true;
            }
            
            if (sceneColorImpostorList.Contains(impostorLODGroup))
            {
                return;
            }
            sceneColorImpostorList.Add(impostorLODGroup);
        }
        
        private void ProcessSceneColorRemove(ImpostorLODGroup impostorLODGroup)
        {
            if (!GlobalShaderEffects.IsChangeSceneColor)
            {
                return;
            }
            
            if (impostorLODGroup == null)
            {
                return;
            }
            
            if (!isStart)
            {
                return;
            }
            
            if (!sceneColorImpostorList.Contains(impostorLODGroup))
            {
                return;
            }
            sceneColorImpostorList.Remove(impostorLODGroup);
        }
        
        private void ProcessImpostorableObject(int index, out bool requiresTextureUpdate)
        {
            ProcessImpostorableObject_ProfilerMarker.Begin();
            ImpostorableObject impostorableObject = _impostorableObjects[index];
            requiresTextureUpdate = false;
            switch (impostorableObject.requiredAction)
            {
                case ImpostorableObject.RequiredAction.GoToImpostorMode:
                case ImpostorableObject.RequiredAction.UpdateImpostorTexture:
                    if (impostorableObject.requiredAction == ImpostorableObject.RequiredAction.GoToImpostorMode)
                    {
                        ImpostorLODGroup impostorLODGroup = ImpostorLODGroupsManager.Instance.GetByInstanceId(impostorableObject.impostorLODGroupInstanceId);
                        ProcessSceneColorAdd(impostorLODGroup);
                        OnObjGoToImpostorEvent?.Invoke(impostorLODGroup);
                    }

                    requiresTextureUpdate = true;
                    // mark last impostor as not relevant
                    if (impostorableObject.ChunkId > 0)
                    {
                        var c = _chunkPool.GetById(impostorableObject.ChunkId);
                        c.MarkPlaceAsNotRelevant(impostorableObject.PlaceInChunk,
                            impostorableObject.settings.fadeTransitionTime, false);
                    }

                    // create new impostor
                    int textureResolution =
                        (int) (textureSizeScale * impostorableObject.nowScreenSize * Screen.height /
                               QualitySettings.lodBias);
                    textureResolution = math.ceilpow2(textureResolution);
                    textureResolution = Mathf.Clamp(textureResolution, impostorableObject.settings.minTextureResolution,
                        impostorableObject.settings.maxTextureResolution);

                    ChunkResolve_ProfilerMarker.Begin();

                    if (!_isOverMemory)
                    {
                        int nUsedMemory = ImpostorLODGroupsManager.Instance.RenderTexturePool.CaculateUseMemory();

                        if (_maxMemory > 0 && nUsedMemory > _maxMemory)
                        {
                            _isOverMemory = true;
                        }
                    }

                    int nMemoryMinResolution = _memoryMinResolutionInt;

                    impostorableObject.lastUpdate.screenSize = impostorableObject.nowScreenSize;
                    impostorableObject.lastUpdate.cameraDirection = impostorableObject.nowDirection;
                    impostorableObject.lastUpdate.distance = impostorableObject.nowDistance;
                    impostorableObject.lastUpdate.textureResolution = textureResolution;
                    impostorableObject.lastUpdate.time = ImpostorLODGroupsManager.Instance.TimeProvider.Time;
                    impostorableObject.lastUpdate.lightDirection =
                        _directionalLight ? _directionalLight.transform.forward : Vector3.zero;

                    if (_isOverMemory)
                    {
                        ImpostorLODGroup newItm = ImpostorLODGroupsManager.Instance.GetByInstanceId(impostorableObject.impostorLODGroupInstanceId);

                        if (newItm != null)
                        {
                            if (MeshLODManager.Instance.CheckHasMeshLOD(newItm.gameObject))
                            {
                                OnObjGoToNormalEvent?.Invoke(newItm);
                            }
                            else
                            {
                                newItm.SetLodGroup(true);
                            }
                        }
                    }


                    var chunk = _chunkPool.GetWithPlace(textureResolution);
                    int placeInChunk = chunk.GetPlace(impostorableObject);

                    //Debug.Log("ProcessImpostorableObject:"+index+"  textureResolution:"+textureResolution+"  chunk.Id:"+chunk.Id+"  placeInChunk:"+placeInChunk);

                    impostorableObject.SetChunk(chunk.Id, placeInChunk);
                    ChunkResolve_ProfilerMarker.End();


                    break;
                case ImpostorableObject.RequiredAction.Cull:
                case ImpostorableObject.RequiredAction.GoToNormalMode:
                    RemovingImpostor_ProfilerMarker.Begin();
                    if (impostorableObject.ChunkId > 0)
                    {
                        var c = _chunkPool.GetById(impostorableObject.ChunkId);
                        c.MarkPlaceAsNotRelevant(impostorableObject.PlaceInChunk,
                            impostorableObject.settings.fadeTransitionTime, false);
                    }

                    ImpostorLODGroup newItm1 =
                        ImpostorLODGroupsManager.Instance.GetByInstanceId(impostorableObject
                            .impostorLODGroupInstanceId);

                    if (newItm1)
                        newItm1.ResetDatas();

                    impostorableObject.SetChunk(0, -1);
                    RemovingImpostor_ProfilerMarker.End();

                    if (impostorableObject.requiredAction == ImpostorableObject.RequiredAction.GoToNormalMode)
                    {
                        OnObjGoToNormalEvent?.Invoke(newItm1);
                        ProcessSceneColorRemove(newItm1);
                    }

                    break;
            }

            _impostorableObjects[index] = impostorableObject;
            ProcessImpostorableObject_ProfilerMarker.End();
        }

        internal void UpdateSettings(int index, ImpostorLODGroup impostorLODGroup)
        {
            var impostorableObject = _impostorableObjects[index];

            impostorableObject.data.position = impostorLODGroup.Position;
            impostorableObject.data.size = impostorLODGroup.Size;
            impostorableObject.data.height = impostorLODGroup.LocalHeight;
            impostorableObject.data.quadSize = impostorLODGroup.QuadSize;
            impostorableObject.data.zOffset = impostorLODGroup.ZOffsetWorld;
            impostorableObject.settings.isStatic = (byte) (impostorLODGroup.isStatic ? 1 : 0);
            impostorableObject.settings.fadeInTime = impostorLODGroup.FadeInTime;
            impostorableObject.settings.fadeOutTime = impostorLODGroup.FadeOutTime;
            impostorableObject.settings.fadeTransitionTime = impostorLODGroup.fadeTransitionTime;
            impostorableObject.settings.deltaCameraAngle = impostorLODGroup.deltaCameraAngle;
            impostorableObject.settings.useUpdateByTime = (byte) (impostorLODGroup.useUpdateByTime ? 1 : 0);
            impostorableObject.settings.timeInterval = impostorLODGroup.timeInterval;
            impostorableObject.settings.useDeltaLightAngle = (byte) (impostorLODGroup.useDeltaLightAngle ? 1 : 0);
            impostorableObject.settings.deltaLightAngle = impostorLODGroup.deltaLightAngle;
            impostorableObject.settings.deltaDistance = impostorLODGroup.deltaDistance;
            impostorableObject.settings.minTextureResolution = (int) impostorLODGroup.minTextureResolution;
            impostorableObject.settings.maxTextureResolution = (int) impostorLODGroup.maxTextureResolution;
            impostorableObject.settings.screenRelativeTransitionHeight =
                impostorLODGroup.ScreenRelativeTransitionHeight;
            impostorableObject.settings.screenRelativeTransitionHeightCull =
                impostorLODGroup.ScreenRelativeTransitionHeightCull;

            _impostorableObjects[index] = impostorableObject;
        }

        private ImpostorableObject CreateFromImpostorLODGroup(ImpostorLODGroup impostorLODGroup)
        {
            var impostorableObject = new ImpostorableObject()
            {
                impostorLODGroupInstanceId = impostorLODGroup.GetInstanceID(),
            };
            return impostorableObject;
        }

        public int GetUsedBytes()
        {
            int res = 0;
            res += MemoryUsageUtility.GetMemoryUsage(_impostorableObjects);
            res += MemoryUsageUtility.GetMemoryUsage(_updateQueue);
            // todo chunk pool memory
            res += _commandBufferProxy.CommandBuffer.sizeInBytes;

            return res;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;
            if (!debugModeEnabled)
                return;

            var gradient = new Gradient()
            {
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.green, 0),
                    new GradientColorKey(Color.blue, 1),
                }
            };
            var tempChunks = _chunkPool.Chunks.OrderByDescending(x => x.TextureResolution).ToList();
            for (int i = 0; i < tempChunks.Count; i++)
            {
                ImpostorsChunk chunk = tempChunks[i];
                Gizmos.color = gradient.Evaluate((float) i / tempChunks.Count);
                if (chunk.IsEmpty == false)
                {
                    var b = chunk.GetMesh().bounds;
                    Gizmos.DrawWireCube(b.center, b.size);
                }
            }
        }
#endif


        private static ProfilerMarker ImpostorsCulling_ProfilerMarker = new ProfilerMarker("Impostors Culling");

        private static ProfilerMarker SchedulingCullingJobs_ProfilerMarker =
            new ProfilerMarker("Scheduling Culling Jobs");

        private static ProfilerMarker SchedulingObjectsVisibilityJob_ProfilerMarker =
            new ProfilerMarker("Scheduling ObjectsVisibility Job");

        private static ProfilerMarker SchedulingIsNeedUpdateJob_ProfilerMarker =
            new ProfilerMarker("Scheduling IsNeedUpdate Job");

        private static ProfilerMarker SchedulingUpdateQueueJob_ProfilerMarker =
            new ProfilerMarker("Scheduling UpdateQueue Job");

        private static ProfilerMarker SchedulingUpdateImpostorsJob_ProfilerMarker =
            new ProfilerMarker("Scheduling UpdateImpostors Job");

        private static ProfilerMarker DestroyingEmptyChunks_ProfilerMarker =
            new ProfilerMarker("Destroying Empty Chunks");

        private static ProfilerMarker ImpostorsMeshCreation_ProfilerMarker =
            new ProfilerMarker("Impostors Mesh Creation");

        private static ProfilerMarker SchedulingMeshJobs_ProfilerMarker = new ProfilerMarker("Scheduling Mesh Jobs");

        private static ProfilerMarker ImpostorsSceneCameraRendering_ProfilerMarker =
            new ProfilerMarker("Impostors Scene Camera Rendering");

        private static ProfilerMarker ImpostorSystem_ProfilerMarker = new ProfilerMarker("Impostor System");
        private static ProfilerMarker DrawImpostors_ProfilerMarker = new ProfilerMarker("Draw Impostors");

        private static ProfilerMarker UpdateImpostorTextures_ProfilerMarker =
            new ProfilerMarker("Update Impostor Textures");

        private static ProfilerMarker SortingUpdateQueue_ProfilerMarker = new ProfilerMarker("Sorting Update Queue");

        private static ProfilerMarker CalculateLightProbes_ProfilerMarker =
            new ProfilerMarker("Calculate Light Probes");

        private static ProfilerMarker SchedulingRenderImpostorTextures_ProfilerMarker =
            new ProfilerMarker("Scheduling Render Impostor Textures");

        private static ProfilerMarker ProcessImpostorableObject_ProfilerMarker =
            new ProfilerMarker("Process Impostorable Object");

        private static ProfilerMarker ChunkResolve_ProfilerMarker = new ProfilerMarker("Chunk Resolve");
        private static ProfilerMarker RemovingImpostor_ProfilerMarker = new ProfilerMarker("Removing Impostor");

        public void RequestImpostorTextureUpdate(ImpostorLODGroup impostorLODGroup)
        {
            var index = impostorLODGroup.IndexInImpostorsManager;
            var impostorableObject = _impostorableObjects[index];
            impostorableObject.requiredAction = ImpostorableObject.RequiredAction.ForcedUpdateImpostorTexture;
            _impostorableObjects[index] = impostorableObject;
        }
    }
}
