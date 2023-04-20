#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class LightprobeAreaEdit : MonoBehaviour
    {
        public AreaData areaData;
        public float[] areadatamousepos;
        [System.NonSerialized]
        public ComputeBuffer areadatamouseposCb;
        public Bounds placeBox;
        [System.NonSerialized]
        public bool editLightProbeArea = false;
        [System.NonSerialized]
        public ComputeBuffer lightProbeCb;
        [System.NonSerialized]
        public List<RenderBatch> batchs = new List<RenderBatch> ();
        [System.NonSerialized]
        public bool showhideunterrainlayer = true;
        [Range(1,5)]
        public int scalevalue=1;//用于光探头工具大小缩放
        private List<int> _recordpickpos;
        private double lastCbUpdate;
        private double dirtyTime = 0;
        private double lastMouseUpdate = 0.0;
        private int modifyCount = 0;
        private Vector2Int startPos;
        private Vector2 minMaxH = Vector2.zero;

        private void OnDisable ()
        {
            if (lightProbeCb != null)
            {
                lightProbeCb.Dispose ();
            }
            if (areadatamouseposCb != null)
            {
                areadatamouseposCb.Dispose();
            }
            batchs.Clear ();
        }

        public void ReSize (int minX, int maxX, int minZ, int maxZ, string path)
        {
            if (areaData == null)
            {
                areaData = EditorCommon.LoadAsset<AreaData> (path);
                EditorCommon.SaveAsset (path, areaData);
            }
            int sizeX = maxX - minX;
            int sizeZ = maxZ - minZ;
            int srcMinX = areaData.lightProbeArea.x;
            int srcMaxX = areaData.lightProbeArea.y;
            int srcMinZ = areaData.lightProbeArea.z;
            int srcMaxZ = areaData.lightProbeArea.w;
            int sX = srcMaxX - srcMinX;
            int sZ = srcMaxZ - srcMinZ;
            if (sX != sizeX || sizeZ != sZ || areaData.lightProbeData == null)
            {
                Vector4 _vec = new Vector4(-1,-1,-1,-1);
                LightProbeAreaData[] lightProbeData = new LightProbeAreaData[(sizeX + 1) * (sizeZ + 1)];
                areadatamousepos = new float[(sizeX + 1) * (sizeZ + 1)];
                if (lightProbeData.Length > 0)
                {
                    for (int i = 0; i < lightProbeData.Length; i++)
                    {
                        lightProbeData[i].flag = 0;
                        lightProbeData[i].height = _vec;
                        areadatamousepos[i] = 0f;
                    }
                }
                if (areaData.lightProbeData != null)
                {
                    Dictionary<int, LightProbeAreaData> data = new Dictionary<int, LightProbeAreaData> ();
                    for (int z = minZ; z <= maxZ; ++z)
                    {
                        if (z >= srcMinZ && z <= srcMaxZ)
                        {
                            int indexZ = (z - minZ) * (sizeX + 1);
                            int srcIndexZ = (z - srcMinZ) * (sZ + 1);
                            for (int x = minX; x <= maxX; ++x)
                            {
                                if (x >= srcMinX && x <= srcMaxX)
                                {
                                    int index = (x - minX) + indexZ;
                                    int srcIndex = (x - srcMinX) + srcIndexZ;
                                    if (srcIndex >= 0 && srcIndex < areaData.lightProbeData.Length)
                                        lightProbeData[index] = areaData.lightProbeData[srcIndex];
                                }
                            }
                        }
                    }
                }
                areaData.lightProbeData = lightProbeData;
                areaData.lightProbeArea.x = minX;
                areaData.lightProbeArea.y = maxX;
                areaData.lightProbeArea.z = minZ;
                areaData.lightProbeArea.w = maxZ;
            }
            editLightProbeArea = false;
        }
        public void Init ()
        {
            if (areaData != null && areaData.lightProbeData != null)
            {
                if (lightProbeCb != null)
                {
                    lightProbeCb.Dispose ();
                }
                var data = areaData.lightProbeData;
                lightProbeCb = new ComputeBuffer (data.Length,
                    LightProbeAreaData.GetSize (), ComputeBufferType.Default);
                lightProbeCb.SetData (data);
                Shader.SetGlobalBuffer ("_LightProbeAreaData", lightProbeCb);
                if (areadatamouseposCb != null)
                {
                    areadatamouseposCb.Dispose();
                }
          
                areadatamousepos = new float[data.Length];
                
                if (_recordpickpos == null)
                {
                    _recordpickpos = new List<int>();
                }
                else
                {
                    _recordpickpos.Clear();
                }
                areadatamouseposCb =new ComputeBuffer(data.Length,
                sizeof(float), ComputeBufferType.Default);
                areadatamouseposCb.SetData(areadatamousepos);
                Shader.SetGlobalBuffer("_AreaDataMousePos", areadatamouseposCb);
                Shader.SetGlobalVector ("_AreaRange",
                    new Vector4 (areaData.lightProbeArea.x * EngineContext.LightProbeAreaSize,
                        areaData.lightProbeArea.z * EngineContext.LightProbeAreaSize,
                        areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1, 0));
                Shader.SetGlobalFloat ("_AreaSize", EngineContext.LightProbeAreaSize);


                batchs.Clear ();
            }
        }
        void Update ()
        {
            if (editLightProbeArea && lightProbeCb != null)
            {
                if (EditorApplication.timeSinceStartup - lastCbUpdate >.2 &&
                    lastCbUpdate < dirtyTime)
                {
                    lastCbUpdate = EditorApplication.timeSinceStartup;
                    lightProbeCb.SetData (areaData.lightProbeData);
                    Shader.SetGlobalBuffer ("_LightProbeAreaData", lightProbeCb);
               
                }
                if (areadatamouseposCb != null)
                {
                    areadatamouseposCb.SetData(areadatamousepos);
                    Shader.SetGlobalBuffer("_AreaDataMousePos", areadatamouseposCb);
                }
                if (_recordpickpos.Count > (scalevalue*2-1)* (scalevalue * 2 - 1))
                {                   
                    for (int i = 0; i < _recordpickpos.Count; i++)
                    {          
                        areadatamousepos[_recordpickpos[i]] = 0;
                    }
                    _recordpickpos.Clear();
                }

                if (batchs.Count > 0 && lightProbeCb != null)
                {
                    var mat = AssetsConfig.instance.LightProbeAreaEdit;
                    if (mat != null)
                    {
                        for (int i = 0; i < batchs.Count; ++i)
                        {
                            var batch = batchs[i];
                            Graphics.DrawMesh (batch.mesh, batch.matrix, mat, 0, null, 0, batch.mpbRef);
                          //  Graphics.DrawMesh(batch.mesh, batch.matrix, mat, 0, null, 0);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos ()
        {
            var c = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube (placeBox.center, placeBox.size);
            Gizmos.color = c;
        }
        private bool RayCast (Event e, out Vector3 pos)
        {
            pos = Vector3.zero;
            int layermask = 1 << DefaultGameObjectLayer.TerrainLayer;
            RaycastHit hit;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity, layermask))
            {
                string pickname = hit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh.name;
                if (batchs.Count > 0)
                {
                    for (int i = 0; i < batchs.Count; i++)
                    {        
                        if(batchs[i].mesh.name== pickname)
                        {
                            batchs[i].mpbRef.SetFloat("_PickRange", 1f);
                        }
                        else
                        {
                            batchs[i].mpbRef.SetFloat("_PickRange", 0f);
                        }
                    }
                }
                pos = hit.point;
                return true;
            }
            return false;


        }
        private void CalcXZ (ref Vector3 pos, ref int x, ref int z)
        {
            float minX = areaData.lightProbeArea.x * EngineContext.LightProbeAreaSize;
            float maxX = areaData.lightProbeArea.y * EngineContext.LightProbeAreaSize;
            float minZ = areaData.lightProbeArea.z * EngineContext.LightProbeAreaSize;
            float maxZ = areaData.lightProbeArea.w * EngineContext.LightProbeAreaSize;
            x = Mathf.FloorToInt ((pos.x - minX) / EngineContext.LightProbeAreaSize);
            z = Mathf.FloorToInt ((pos.z - minZ) / EngineContext.LightProbeAreaSize);
        }
        private void ModifyFlag (int x, int z)
        {
            int minX = areaData.lightProbeArea.x;
            int maxX = areaData.lightProbeArea.y;
            int minZ = areaData.lightProbeArea.z;
            int maxZ = areaData.lightProbeArea.w;
            if (x >= minX && x <= maxX && z >= minZ && z <= maxZ)
            {
                int index = x + z * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                if (index >= 0 && index < areaData.lightProbeData.Length)
                {
                    ref var data = ref areaData.lightProbeData[index];
                    for (int i = 0; i < 4; ++i)
                    {
                        float h = data.height[i];
                        if (h >= minMaxH.x && h <= minMaxH.y)
                        {
                            data.height[i] = -1;
                        }
                    }
                    data.flag = 0;
                    dirtyTime = lastMouseUpdate;
                }
            }
        }

        private void ModifyFlag (bool add, ref Vector3 pos, bool clear = false)
        {
            float minX = areaData.lightProbeArea.x * EngineContext.LightProbeAreaSize;
            float maxX = areaData.lightProbeArea.y * EngineContext.LightProbeAreaSize;
            float minZ = areaData.lightProbeArea.z * EngineContext.LightProbeAreaSize;
            float maxZ = areaData.lightProbeArea.w * EngineContext.LightProbeAreaSize;
            if (pos.x >= minX && pos.x <= maxX && pos.z >= minZ && pos.z <= maxZ)
            {
                int x = Mathf.FloorToInt ((pos.x - minX) / EngineContext.LightProbeAreaSize);
                int z = Mathf.FloorToInt ((pos.z - minZ) / EngineContext.LightProbeAreaSize);
                int index = x + z * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                if (index >= 0 && index < areaData.lightProbeData.Length)
                {
                    if (scalevalue >1)
                    {
                       // int num = (scalevalue * 2 - 1) * (scalevalue * 2 - 1);
                       // int[] _needindexs = new int[num];
                        //float maxx=  pos.x + (scalevalue - 1) * EngineContext.LightProbeAreaSize;
                        //float maxz = pos.z + (scalevalue - 1) * EngineContext.LightProbeAreaSize;
                        float minx = pos.x - (scalevalue - 1) * EngineContext.LightProbeAreaSize;
                        float minz = pos.z - (scalevalue - 1) * EngineContext.LightProbeAreaSize;
                        //if (maxx > maxX) { maxx = maxX; }
                        //if (maxz > maxZ) { maxz = maxZ; }
                        //if (minx < minX) { minx = minX; }
                        //if (minz < minZ) { minz = minZ; }
                        //int maxxint = Mathf.FloorToInt((maxx - minX) / EngineContext.LightProbeAreaSize);
                        //int maxzint = Mathf.FloorToInt((maxz - minZ) / EngineContext.LightProbeAreaSize);
                        //int maxindex = maxxint + maxzint * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                        int minxint = Mathf.FloorToInt((minx - minX) / EngineContext.LightProbeAreaSize);
                        int minzint = Mathf.FloorToInt((minz - minZ) / EngineContext.LightProbeAreaSize);
                        int minindex = minxint + minzint * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                        int maxxnum = scalevalue * 2 - 1;
                        int maxznum = scalevalue * 2 - 1;
                        for (int Z = 0; Z < maxznum; Z++)
                        {
                            for (int X = 0; X < maxxnum; X++)
                            {
                               // _needindexs[X + Z * maxznum] = minindex + X + Z * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                               int INDEXNUM= minindex + X + Z * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                                if(INDEXNUM >= 0 && INDEXNUM < areaData.lightProbeData.Length)
                                {
                                    SetAreaDataValue(add, INDEXNUM, ref pos, clear);
                                }
                            }
                        }
                    }
                    else
                    {
                        SetAreaDataValue(add, index, ref pos, clear);
                    }
                }
            
            }
        }

        private void SetAreaDataValue (bool add,int index, ref Vector3 pos, bool clear = false)
        {     
                ref var data = ref areaData.lightProbeData[index];
                bool hasFlag = false;
                bool find = false;
                for (int i = 0; i < 4; ++i)
                {
                    float h = data.height[i];
                    if (Mathf.Abs(h - pos.y) < 0.5f)
                    {
                        if (add)
                        {
                            data.height[i] = pos.y;
                        }
                        else
                        {
                            data.height[i] = -1;
                        }
                        find = true;
                        break;
                    }
                    else if (h < -1 || clear)
                    {
                        data.height[i] = -1;
                    }
                }

                for (int i = 0; i < 4; ++i)
                {
                    float h = data.height[i];
                    if (h < 0.1f && !find)
                    {
                        if (add)
                        {
                            data.height[i] = pos.y;
                        }
                        else
                        {
                            data.height[i] = -1;
                        }
                        find = true;
                    }
                    hasFlag |= data.height[i] > 0;                          
                }
                data.flag = hasFlag ? 1 : 0;
                dirtyTime = lastMouseUpdate;      
        }
        public void OnSceneGUI ()
        {
            if (editLightProbeArea && areaData != null && areaData.lightProbeData != null)
            {
                Event e = Event.current;
                int controlID = GUIUtility.GetControlID (FocusType.Passive);
                switch (e.GetTypeForControl (controlID))
                {
                    case EventType.MouseMove:
                        if (EditorApplication.timeSinceStartup - lastMouseUpdate >.03)
                        {
                            lastMouseUpdate = EditorApplication.timeSinceStartup;
                            Vector3 pos;
                            if (RayCast (e, out pos))
                            {
                            //    Shader.SetGlobalVector ("_MousePos", pos);
                                ScaleMousePostion(pos);
                                bool add = !e.shift && e.control;
                                bool remove = e.shift && e.control;
                                if (add || remove)
                                {
                                    ModifyFlag(add, ref pos);
                                }
                            }
                        }
                        break;
                    case EventType.KeyUp:
                        {              
                            bool remove = e.keyCode == KeyCode.D;       
                            if (remove)
                            {
                                int x = 0;
                                int z = 0;
                                Vector3 pos;
                                if (RayCast (e, out pos))
                                {
                                    CalcXZ (ref pos, ref x, ref z);
                                }
                                if (modifyCount == 0)
                                {
                                    startPos.x = x;
                                    startPos.y = z;
                                    minMaxH.x = pos.y;
                                    modifyCount = 1;

                                }
                                else if (modifyCount == 1)
                                {
                                    if (pos.y < minMaxH.x)
                                    {
                                        minMaxH.y = minMaxH.x;
                                    }
                                    minMaxH.y = pos.y;
                                    lastMouseUpdate = EditorApplication.timeSinceStartup;
                                   int minx = x;
                                    int maxx = x;
                                    if (x < startPos.x) { minx = x; maxx = startPos.x; }
                                    else { minx = startPos.x; maxx = x; }

                                    int minz = z;
                                    int maxz = z;
                                    if (z < startPos.y) { minz = z; maxz = startPos.y; }
                                    else { minz = startPos.y; maxz = z; }

                                    for (int zz = minz; zz <= maxz; ++zz)
                                    {
                                        for (int xx = minx; xx <= maxx; ++xx)
                                        {
                                            ModifyFlag (xx, zz);
                                        }
                                    }
                                    dirtyTime = lastMouseUpdate;
                                    modifyCount = 0;
                                }

                            }
                            bool scalePlus = e.keyCode == KeyCode.O;
                            bool scalereduce = e.keyCode == KeyCode.P;
                            if (scalePlus&& editLightProbeArea)
                            {
                                scalevalue++;
                                if (scalevalue >5)
                                {
                                    scalevalue = 5;
                                }
                            }
                            if(scalereduce && editLightProbeArea)
                            {
                                scalevalue--;
                                if (scalevalue < 1)
                                {
                                    scalevalue = 1;
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void ScaleMousePostion(Vector3 pos)
        {     
            float minX = areaData.lightProbeArea.x * EngineContext.LightProbeAreaSize;
            float maxX = areaData.lightProbeArea.y * EngineContext.LightProbeAreaSize;
            float minZ = areaData.lightProbeArea.z * EngineContext.LightProbeAreaSize;
            float maxZ = areaData.lightProbeArea.w * EngineContext.LightProbeAreaSize;
            if (pos.x >= minX && pos.x <= maxX && pos.z >= minZ && pos.z <= maxZ)
            {
                int x = Mathf.FloorToInt((pos.x - minX) / EngineContext.LightProbeAreaSize);
                int z = Mathf.FloorToInt((pos.z - minZ) / EngineContext.LightProbeAreaSize);
                int index = x + z * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                if (index >= 0 && index < areaData.lightProbeData.Length)
                {
                    if (scalevalue > 1)
                    {
                     //   int num = (scalevalue * 2 - 1) * (scalevalue * 2 - 1);
                        float minx = pos.x - (scalevalue - 1) * EngineContext.LightProbeAreaSize;
                        float minz = pos.z - (scalevalue - 1) * EngineContext.LightProbeAreaSize;
                        int minxint = Mathf.FloorToInt((minx - minX) / EngineContext.LightProbeAreaSize);
                        int minzint = Mathf.FloorToInt((minz - minZ) / EngineContext.LightProbeAreaSize);
                        int minindex = minxint + minzint * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                        int maxxnum = scalevalue * 2 - 1;
                        int maxznum = scalevalue * 2 - 1;
                        for (int Z = 0; Z < maxznum; Z++)
                        {
                            for (int X = 0; X < maxxnum; X++)
                            {                     
                                int INDEXNUM = minindex + X + Z * (areaData.lightProbeArea.y - areaData.lightProbeArea.x + 1);
                                if (INDEXNUM >= 0 && INDEXNUM < areaData.lightProbeData.Length)
                                {
                                    areadatamousepos[INDEXNUM] = 1f;
                                    if (!_recordpickpos.Contains(INDEXNUM))
                                    {
                                        _recordpickpos.Add(INDEXNUM);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        areadatamousepos[index] = 1f;
                        if (!_recordpickpos.Contains(index))
                        {
                            _recordpickpos.Add(index);
                        }
                    }        
                }
            }
        }

        public void AddRemove (bool add)
        {
            var min = placeBox.min;
            var max = placeBox.max;
            float y = max.y;
            int countZ = (int) ((max.z - min.z) / EngineContext.LightProbeAreaSize);
            int countX = (int) ((max.x - min.x) / EngineContext.LightProbeAreaSize);
            int layermask = 1 << DefaultGameObjectLayer.TerrainLayer;
            for (int z = 0; z < countZ; ++z)
            {
                float zz = min.z + z * EngineContext.LightProbeAreaSize;
                for (int x = 0; x < countX; ++x)
                {
                    float xx = min.x + x * EngineContext.LightProbeAreaSize;
                    RaycastHit hitInfo;
                    Vector3 pos = new Vector3 (xx, y, zz);
                    if (Physics.Raycast (pos, Vector3.down, out hitInfo, max.y - min.y, layermask))
                    {
                        ModifyFlag (add, ref pos, !add);
                    }

                }

            }
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (LightprobeAreaEdit))]
    public class LightprobeAreaEditEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            var lpae = target as LightprobeAreaEdit;
            if (GUILayout.Button ("Add"))
            {
                lpae.AddRemove (true);
            }
            if (GUILayout.Button ("Remove"))
            {
                lpae.AddRemove (false);
            }
        }
        private void OnSceneGUI ()
        {
            var lpae = target as LightprobeAreaEdit;
            var center = lpae.placeBox.center;
            Quaternion rot = Quaternion.identity;
            Vector3 scale = lpae.placeBox.size;
            EditorGUI.BeginChangeCheck ();
            Handles.TransformHandle (ref center, ref rot, ref scale);
            if (EditorGUI.EndChangeCheck ())
            {
                Undo.RecordObject (this, "placeAABB");
                lpae.placeBox.center = center;
                lpae.placeBox.size = scale;
            }
        }
    }
}

#endif