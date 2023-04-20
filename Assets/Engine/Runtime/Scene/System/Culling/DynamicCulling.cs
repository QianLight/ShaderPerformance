/********************************************************************
	created:	2021年11月18日 16:15:06
	file base:	DynamicCulling.cs
	author:		c a o   f e n g
	
	purpose:	动态物体的遮挡剔除
*********************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace CFEngine
{
    public class DynamicCulling : MonoBehaviour
    {
       // public static DynamicCulling Instance { get; private set; }

       [SerializeField]
       private List<Collider> _occluders = new List<Collider>();
       
        [SerializeField] private List<Camera> _cameras = new List<Camera>();
        public int _jobsPerFrame = 500;
        public float _objectsLifetime = 1.5f;
        public float _fieldOfView = 60;
        public float _AreaRay = 16;
        
        private Dictionary<int, DynamicRenderData> _indexToRenderer = new Dictionary<int, DynamicRenderData>();
        
        private Dictionary<Collider, int> _colliderToIndex = new Dictionary<Collider, int>();
        private List<Collider> _occludeColliders = new List<Collider>();

        private List<Camera> _camerasForRemove = new List<Camera>();
        private List<int> _renderersForRemoveIDs = new List<int>();

        private NativeArray<float3> _rayDirs;
        private NativeList<int> _visibleObjects;
        private NativeList<int> _hittedObjects;
        private NativeList<float> _timers;
        private NativeList<JobHandle> _handles;
        private List<NativeArray<RaycastCommand>> _rayCommands;
        private List<NativeArray<RaycastHit>> _hitResults;

        private int _mask;
        private int _layer;
        public int _dirsOffsetIndex;
        public int _newJobsCount;
        private bool _onUpdateJobsPerFrame;

        public bool DebugState = false;
        public List<GameObject> DebugAllRenders = new List<GameObject>();
        public List<GameObject> DebugVisibleRenders= new List<GameObject>();
        public int DebugRayDirs = 0;

        private void Awake()
        {
            LoadMgr.dynamicCulling = AddObjectsForCulling;
            //Instance = this;

         
            CreateRayDirs();
            
            _layer = CullingLayer;
            _mask = CullingMask;

            _visibleObjects = new NativeList<int>(Allocator.Persistent);
            _hittedObjects = new NativeList<int>(Allocator.Persistent);
            _timers = new NativeList<float>(Allocator.Persistent);
            _handles = new NativeList<JobHandle>(Allocator.Persistent);
            _rayCommands = new List<NativeArray<RaycastCommand>>();
            _hitResults = new List<NativeArray<RaycastHit>>();

            AddOccluders(_occluders.ToArray());
            AddSceneOccluder();
            _cameras.Clear();
            AddCamera(Camera.main);
        }

        private void AddSceneOccluder()
        {
            GameObject editorScene = GameObject.Find("EditorScene");
            if (editorScene == null) return;

            Transform colliders = editorScene.transform.Find("Collider");
            Collider[] allColliders = colliders.GetComponentsInChildren<Collider>();

            int layerTerrain = LayerMask.NameToLayer(TerrainLayerName);
            for (int i = 0; i < allColliders.Length; i++)
            {
                Collider col = allColliders[i];
                if (col.gameObject.layer != layerTerrain) continue;
                AddOccluder(col);
            }

            SkinnedMeshRenderer[] allRenders = editorScene.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < allRenders.Length; i++)
            {
                Renderer rnd = allRenders[i];
                if (!rnd.enabled) continue;
                AddObjectsForCulling(rnd.gameObject);
            }

        }

        private void Update()
        {
            try
            {
         
                
                FindDestroyedCameras();

                OnRemoveCamerasFromList();

                if (!CheckCameras())
                {
                    Disable();
                    return;
                }

                _hittedObjects.Clear();
                _handles.Clear();

                for (int i = 0; i < _cameras.Count; i++)
                {
                    _handles.Add(new CreateRayCommandsJob()
                    {

                        position = _cameras[i].transform.position,
                        rotation = _cameras[i].transform.rotation,

                        dirsOffsetIdx = _dirsOffsetIndex,
                        rayDirs = _rayDirs,

                        mask = _mask,
                        rayCommands = _rayCommands[i]

                    }.Schedule(_jobsPerFrame, 64, default));
                }

                if ((_dirsOffsetIndex += _jobsPerFrame) >= (_rayDirs.Length - _jobsPerFrame))
                    _dirsOffsetIndex = 0;

                JobHandle.CompleteAll(_handles);

                _handles.Clear();
                for (int i = 0; i < _cameras.Count; i++)
                    _handles.Add(RaycastCommand.ScheduleBatch(_rayCommands[i], _hitResults[i], 1, default));

                _handles.Add(new UpdateTimersJob()
                {

                    timers = _timers,
                    deltaTime = Time.deltaTime

                }.Schedule());
            }
            catch (System.Exception ex)
            {
                Debug.Log("Dynamic Culling will be disabled");
                Debug.Log("Cause : " + ex.Message + " " + ex.StackTrace);

                Disable();
            }
        }

        public float scaleGizmosRay = 1;
        private void OnDrawGizmos()
        {
            if (!DebugState) return;

            int nMax = math.min(_rayDirs.Length, _dirsOffsetIndex + _jobsPerFrame);
            Camera camer = _cameras[0];

            Vector3 position = camer.transform.position;
            quaternion rotation = camer.transform.rotation;


            for (int i = _dirsOffsetIndex; i < nMax; i++)
            {
                Vector3 dir = _rayDirs[i];

                float3 direction = math.mul(rotation, dir);
                Gizmos.color = Color.red;
                
                Gizmos.DrawRay(position, direction*scaleGizmosRay);
            }
        }


        private void LateUpdate()
        {
            try
            {
                JobHandle.CompleteAll(_handles);

                for (int i = 0; i < _hitResults.Count; i++)
                {
                    NativeArray<RaycastHit> hits = _hitResults[i];

                    for (int j = 0; j < hits.Length; j++)
                    {
                        RaycastHit rch = hits[j];
                        Collider collider = hits[j].collider;
                        if (collider == null) continue;
                        if (_occludeColliders.Contains(collider)) continue;
                        
                        _hittedObjects.Add(_colliderToIndex[collider]);
                    }
                }

                new ComputeResultsJob()
                {

                    visibleObjects = _visibleObjects,
                    hittedObjects = _hittedObjects,
                    timers = _timers

                }.Schedule().Complete();


                DebugShowData();
                
                int c = 0;
                while (c < _visibleObjects.Length)
                {
                    int id = _visibleObjects[c];

                    try
                    {
                        if (_timers[c] > _objectsLifetime)
                        {
                            _indexToRenderer[id].SetVisiable(false);
                            _visibleObjects.RemoveAtSwapBack(c);
                            _timers.RemoveAtSwapBack(c);
                        }
                        else
                        {
                            _indexToRenderer[id].SetVisiable(true);

                            c++;
                        }
                    }
                    catch (MissingReferenceException)
                    {
                        _renderersForRemoveIDs.Add(id);
                        c++;
                    }
                }

                OnRemoveRenderersFromList();

                if (_onUpdateJobsPerFrame)
                    OnUpdateJobsPerFrame();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Dynamic Culling will be disabled");
                Debug.Log("Cause : " + ex.Message + " " + ex.StackTrace);

                Disable();
            }
        }

        private void DebugShowData()
        {
            if (!DebugState) return;
            DebugAllRenders.Clear();
            DebugVisibleRenders.Clear();

            foreach (KeyValuePair<int, DynamicRenderData> itm in _indexToRenderer)
            {
                DebugAllRenders.Add(itm.Value.gameObject);
            }

            int c = 0;
            while (c < _visibleObjects.Length)
            {
                int id = _visibleObjects[c];
                
                DebugVisibleRenders.Add(_indexToRenderer[id].gameObject);
                c++;
            }

            DebugRayDirs = _rayDirs.Length;
            
        }

        //蒙特卡洛模拟算法
        private float HaltonSequence(int index, int b)
        {
            float res = 0f;
            float f = 1f / b;

            int i = index;

            while (i > 0)
            {
                res = res + f * (i % b);
                i = Mathf.FloorToInt(i / b);
                f = f / b;
            }

            return res;
        }
        
        private void CreateRayDirs()
        {
            
            int dirsCount = Mathf.RoundToInt(((Screen.width * Screen.height) / _AreaRay) / _jobsPerFrame) * _jobsPerFrame;

            _rayDirs = new NativeArray<float3>(dirsCount, Allocator.Persistent);

            Camera camera = new GameObject().AddComponent<Camera>();
            camera.fieldOfView = _fieldOfView + 1;

            for (int i = 0; i < _rayDirs.Length; i++)
            {
                Vector2 screenPoint = new Vector2(HaltonSequence(i, 2), HaltonSequence(i, 3));

                Ray ray = camera.ViewportPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0));

                _rayDirs[i] = ray.direction;
            }

            Destroy(camera.gameObject);
        }

        private void FindDestroyedCameras()
        {
            for (int i = 0; i < _cameras.Count; i++)
            {
                if (_cameras[i] == null)
                {
                    Debug.Log("DynamicCulling::Looks like camera was destroyed");
                    _camerasForRemove.Add(_cameras[i]);
                }
            }
        }

        private bool CheckCameras()
        {
            if (_cameras.Count == 0)
            {
                Debug.Log("DynamicCulling::no cameras assigned");
                return false;
            }

            return true;
        }

        public const string TerrainLayerName = "Terrain";
        public const string CullingLayerName = "DynamicCulling";

        public static int CullingLayer
        {
            get
            {
                int layer = LayerMask.NameToLayer(CullingLayerName);
                
                return layer;
            }
        }
        
        public static int CullingMask
        {
            get
            {
                int mask = LayerMask.GetMask(CullingLayerName);
                
                return mask;
            }
        }
        private void OnRemoveCamerasFromList()
        {
            if (_camerasForRemove.Count == 0)
                return;

            for (int i = 0; i < _camerasForRemove.Count; i++)
                RemoveCameraFromList(_camerasForRemove[i]);

            _camerasForRemove.Clear();
        }

        private void OnRemoveRenderersFromList()
        {
            if (_renderersForRemoveIDs.Count == 0)
                return;

            for (int i = 0; i < _renderersForRemoveIDs.Count; i++)
                RemoveRendererFromList(_renderersForRemoveIDs[i]);

            _renderersForRemoveIDs.Clear();

            RemoveEmptyCollidersRefs();
        }

        private void OnUpdateJobsPerFrame()
        {
            _jobsPerFrame = _newJobsCount;

            for (int i = 0; i < _rayCommands.Count; i++)
            {
                _rayCommands[i].Dispose();
                _hitResults[i].Dispose();

                _rayCommands[i] = new NativeArray<RaycastCommand>(_jobsPerFrame, Allocator.Persistent);
                _hitResults[i] = new NativeArray<RaycastHit>(_jobsPerFrame, Allocator.Persistent);
            }

            _onUpdateJobsPerFrame = false;
        }

        private void OnDestroy()
        {
            if (_handles.IsCreated && _handles.Length > 0)
            {
                JobHandle.CompleteAll(_handles);
                _handles.Dispose();
            }

            if (_rayDirs.IsCreated)
                _rayDirs.Dispose();

            if (_visibleObjects.IsCreated)
                _visibleObjects.Dispose();

            if (_hittedObjects.IsCreated)
                _hittedObjects.Dispose();

            if (_timers.IsCreated)
                _timers.Dispose();

            for (int i = 0; i < _rayCommands.Count; i++)
            {
                _rayCommands[i].Dispose();
                _hitResults[i].Dispose();
            }

            LoadMgr.dynamicCulling = null;
        }


        private void RemoveCameraFromList(Camera camera)
        {
            int index = _cameras.IndexOf(camera);

            if (index < 0)
                return;

            _cameras.RemoveAt(index);

            _rayCommands[index].Dispose();
            _rayCommands.RemoveAt(index);

            _hitResults[index].Dispose();
            _hitResults.RemoveAt(index);
        }

        private void RemoveRendererFromList(int id)
        {
            if (!_indexToRenderer.ContainsKey(id))
                return;

            DynamicRenderData renderer = _indexToRenderer[id];

            if (renderer != null)
            {
                renderer.SetVisiable(true);

                Collider collider = _colliderToIndex.First(dic => dic.Value == id).Key;

                Destroy(collider.gameObject);
            }

            _indexToRenderer.Remove(id);

            int idx = _visibleObjects.IndexOf(id);

            if (idx < 0)
                return;

            _visibleObjects.RemoveAtSwapBack(idx);
            _timers.RemoveAtSwapBack(idx);
        }

        private void RemoveEmptyCollidersRefs()
        {
            int i = 0;
            while (i < _colliderToIndex.Count)
            {
                Collider key = _colliderToIndex.Keys.ElementAt(i);

                if (key == null)
                    _colliderToIndex.Remove(key);

                else
                    i++;
            }
        }



        public void Enable()
        {
            for (int i = 0; i < _indexToRenderer.Count; i++)
            {
                int id = _indexToRenderer.Keys.ElementAt(i);

                try
                {
                    _indexToRenderer[id].SetVisiable(false);
                }
                catch (MissingReferenceException)
                {
                    _renderersForRemoveIDs.Add(id);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Dynamic Culling has errors");
                    Debug.LogError("Cause : " + ex.Message + " " + ex.StackTrace);
                }
            }

            FindDestroyedCameras();

            OnRemoveCamerasFromList();
            OnRemoveRenderersFromList();

            if (!CheckCameras())
            {
                Disable();
                return;
            }

            enabled = true;
        }

        public void Disable()
        {
            for (int i = 0; i < _indexToRenderer.Count; i++)
            {
                int id = _indexToRenderer.Keys.ElementAt(i);

                try
                {
                    _indexToRenderer[id].SetVisiable(true);
                }
                catch (MissingReferenceException)
                {
                    _renderersForRemoveIDs.Add(id);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Dynamic Culling has errors");
                    Debug.LogError("Cause : " + ex.Message + " " + ex.StackTrace);
                }
            }

            OnRemoveRenderersFromList();

            enabled = false;
        }

        public void SetObjectsLifetime(int value)
        {
            _objectsLifetime = Mathf.Max(0.25f, value);
        }

        public void SetJobsPerFrame(int value)
        {
            if (_jobsPerFrame == value)
                return;

            _newJobsCount = Mathf.Max(1, value);

            _onUpdateJobsPerFrame = true;
        }


        public static Camera cameraActive;
        public void AddCamera(Camera camera)
        {
            cameraActive = camera;
            
            if (_cameras.Contains(camera))
                return;

            _cameras.Add(camera);

            _rayCommands.Add(new NativeArray<RaycastCommand>(_jobsPerFrame, Allocator.Persistent));
            _hitResults.Add(new NativeArray<RaycastHit>(_jobsPerFrame, Allocator.Persistent));

            if (!enabled)
                Enable();
        }

        public void AddCameras(Camera[] cameras)
        {
            if (cameras == null)
                return;

            for (int i = 0; i < cameras.Length; i++)
                if (cameras[i] != null)
                    AddCamera(cameras[i]);
        }


        public void RemoveCamera(Camera camera)
        {
            if (!_cameras.Contains(camera))
                return;

            _camerasForRemove.Add(camera);
        }
        
        
        
        public void AddObjectsForCulling(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            DynamicRenderData renderData = new DynamicRenderData(gameObject);
            AddObjectForCulling(renderData,gameObject.transform);
        }

        public void AddObjectForCulling(DynamicRenderData renderData, Transform parent)
        {
            if (renderData.size.x == 0)
                return;

            int id = parent.GetInstanceID();

            if (_indexToRenderer.ContainsKey(id))
                return;

            BoxCollider collider = new GameObject("DynamicChildCollider").AddComponent<BoxCollider>();

            collider.transform.parent = parent;

            collider.transform.localPosition = Vector3.zero;
            collider.transform.localRotation = Quaternion.identity;
            collider.transform.localScale = Vector3.one;

            collider.gameObject.layer = _layer;
            collider.isTrigger = true;
            collider.size = renderData.size;
            collider.center = new Vector3(0, renderData.size.y / 2, 0);


            _indexToRenderer.Add(id, renderData);
            _colliderToIndex.Add(collider, id);
            
            if (enabled)
                renderData.SetVisiable(false);
        }

        public void AddOccluders(Collider[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    AddOccluder(colliders[i]);
            }
        }

        public void AddOccluder(Collider collider)
        {
            Collider occluder = Instantiate(collider);

            foreach (Component comp in occluder.GetComponents<Component>())
            {
                if (!(comp is Transform) && !(comp is Collider))
                    Destroy(comp);
            }

            occluder.transform.position = collider.transform.position;
            occluder.transform.rotation = collider.transform.rotation;
            occluder.transform.localScale = collider.transform.lossyScale;

            occluder.transform.parent = collider.transform;
            occluder.gameObject.layer = _layer;
            
            if (occluder is MeshCollider)
            {
                (occluder as MeshCollider).convex = true;
            }
            
            occluder.isTrigger = true;
            
            _occludeColliders.Add(occluder);
        }
        
    }

    [BurstCompile]
    public struct UpdateTimersJob : IJob
    {
        public NativeList<float> timers;

        [ReadOnly] public float deltaTime;

        public void Execute()
        {
            for (int i = 0; i < timers.Length; i++)
                timers[i] += deltaTime;
        }
    }


    public class DynamicRenderData
    {
        public class RenderType
        {
            public SkinnedMeshRenderer sms;
            public MeshRenderer mr;
            public MeshFilter mf;
            public Mesh mesh;

            public RenderType(Renderer render)
            {
                if (render is SkinnedMeshRenderer)
                {
                    sms = render as SkinnedMeshRenderer;
                    mesh = sms.sharedMesh;
                }
                else if (render is MeshRenderer)
                {
                    mr = render as MeshRenderer;
                    mf = render.GetComponent<MeshFilter>();
                    mesh = mf.sharedMesh;
                }
            }

            public void SetVisiable(bool b)
            {
                if (sms != null)
                {
                    sms.enabled = b;
                    
                    // if (b) sms.sharedMesh = mesh;
                    // else sms.sharedMesh = null;
                }
                else if (mf != null)
                {
                    mr.enabled = b;
                    // if (b) mf.sharedMesh = mesh;
                    //  else mf.sharedMesh = null;
                }
            }
        }

        public List<RenderType> renderDatas = new List<RenderType>();
        public GameObject gameObject;
        public Vector3 size;

        public Vector3 min;
        public Vector3 max;

        public void SetVisiable(bool b)
        {
            for (int i = 0; i < renderDatas.Count; i++)
            {
                RenderType render = renderDatas[i];
                if (render == null) continue;
                render.SetVisiable(b);
            }
        }

        public DynamicRenderData(GameObject game)
        {
            gameObject = game;
            Renderer[] renders = game.GetComponentsInChildren<Renderer>();

            renderDatas.Clear();

            for (int i = 0; i < renders.Length; i++)
            {
                Renderer rnd = renders[i];
                if (!rnd.enabled) continue;

                renderDatas.Add(new RenderType(rnd));
            }

            RecalculateBounds(renders);
        }

        public void RecalculateBounds(Renderer[] renders)
        {
            Bounds bound = new Bounds();
            if (renders == null || renders.Length == 0) return;

            foreach (Renderer r in renders)
            {
                if (r == null || r.enabled == false)
                    continue;

                if (bound.extents == Vector3.zero)
                    bound = r.bounds;
                else
                    bound.Encapsulate(r.bounds);
            }

            size = bound.size;
        }
    }

    [BurstCompile]
    public struct CreateRayCommandsJob : IJobParallelFor
    {
        [ReadOnly] public float3 position;
        [ReadOnly] public quaternion rotation;

        [ReadOnly] public int dirsOffsetIdx;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float3> rayDirs;

        [ReadOnly] public int mask;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<RaycastCommand> rayCommands;

        public void Execute(int index)
        {
            float3 direction = math.mul(rotation, rayDirs[dirsOffsetIdx + index]);

            RaycastCommand command = new RaycastCommand(position, direction, layerMask: mask);

            rayCommands[index] = command;
        }
    }

    [BurstCompile]
    public struct ComputeResultsJob : IJob
    {
        public NativeList<int> visibleObjects;

        [ReadOnly] public NativeList<int> hittedObjects;

        [WriteOnly] public NativeList<float> timers;

        public void Execute()
        {
            for (int i = 0; i < hittedObjects.Length; i++)
            {
                int id = hittedObjects[i];
                int index = visibleObjects.IndexOf(id);

                if (index < 0)
                {
                    visibleObjects.Add(id);
                    timers.Add(0);
                }
                else
                    timers[index] = 0;
            }
        }
    }

}
