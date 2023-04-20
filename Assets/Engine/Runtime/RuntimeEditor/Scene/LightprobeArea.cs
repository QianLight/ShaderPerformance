#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [System.Serializable]
    public struct SHData
    {
        public float y;
        public SphericalHarmonicsL2 sh;
    }

    [System.Serializable]
    public class LightProbeInfo
    {
        public int x;
        public int z;
        public SHData[] shData;
        public bool draw = true;

    }
    public class LightProbeContext
    {
        public Vector4Int area = new Vector4Int();
        public Vector3Int[] shIndex;
        public SHVector[] sHVectors;
    }

    [System.Serializable]
    public class LightProbeObject
    {
        public List<LightProbeInfo> probPostions = new List<LightProbeInfo>();

        public void Copy(LightProbeObject src)
        {
            probPostions.Clear();
            probPostions.AddRange(src.probPostions);
        }
    }

    [RequireComponent(typeof(LightProbeGroup))]
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class LightprobeArea : MonoBehaviour
    {
        public LightProbeObject lpo = new LightProbeObject();
        public bool drawProbes = false;
        public LightProbes lightProbes;
        public string areaID;
        public AreaData areaData;
        public BoxCollider selectBox;

        [HideInInspector]
        public int chunkID;

        private BoxCollider boxCollider;

        [System.NonSerialized]
        public LightProbeContext context;

        private LightProbePreview lpp = new LightProbePreview();
        public Vector3 GetSize()
        {
            if (boxCollider == null)
            {
                boxCollider = this.gameObject.GetComponent<BoxCollider>();
            }
            return boxCollider.size;
        }

        void Update()
        {
            if (drawProbes)
            {
                var m = AssetsConfig.instance.sphereMesh;
                //var mat = AssetsConfig.instance.PreviewSH;
                var mat = AssetsConfig.instance.PreviewSH2;
                lpp.Draw(m, mat);
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 s = GetSize();
            var c = Gizmos.color;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, s);
            if (selectBox != null)
            {
                var bounds = selectBox.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            Gizmos.color = c;
        }

        public void GenProbes(AreaData ad, int chunkX, int chunkZ)
        {
            lpo.probPostions.Clear();
            var trans = transform;
            int layermask = 1 << DefaultGameObjectLayer.TerrainLayer;

            Vector3 size = GetSize() * 0.5f;
            var center = trans.position;
            var min = center - size;
            var max = center + size;

            int chunkLightProbeCount = (int)(EngineContext.ChunkSize / EngineContext.LightProbeAreaSize);
            int minX = chunkX * chunkLightProbeCount;
            int minZ = chunkZ * chunkLightProbeCount;

            int maxX = minX + chunkLightProbeCount;
            int maxZ = minZ + chunkLightProbeCount;

            int minChunkX = maxX;
            int maxChunkX = minX;
            int minChunkZ = maxZ;
            int maxChunkZ = minZ;

            int minAreaX = ad.lightProbeArea.x;
            int maxAreaX = ad.lightProbeArea.y;
            int minAreaZ = ad.lightProbeArea.z;
            int maxAreaZ = ad.lightProbeArea.w;

            float halfLPaSize = EngineContext.LightProbeAreaSize * 0.5f;
            var transPos = new List<Vector3>();
            RaycastHit[] hits = new RaycastHit[3];
            bool[] realIndex = new bool[3];
            int sizeX = maxAreaX - minAreaX + 1;
            for (int z = minZ; z <= maxZ; ++z)
            {
                if (z >= minAreaZ && z <= maxAreaZ)
                {
                    int indexZ = (z - minAreaZ) * sizeX;
                    float zz = z * EngineContext.LightProbeAreaSize + halfLPaSize;
                    if (zz >= min.z && zz <= max.z)
                    {
                        for (int x = minX; x <= maxX; ++x)
                        {
                            if (x >= minAreaX && x <= maxAreaX)
                            {
                                float xx = x * EngineContext.LightProbeAreaSize + halfLPaSize;
                                if (xx >= min.x && xx <= max.x)
                                {
                                    Vector3 pos = new Vector3(xx, max.y + 10, zz);
                                    //float terrainY = -1;
                                    //if (GlobalContex.ee != null)
                                    //{
                                    //    terrainY = GlobalContex.ee.GetTerrainY (ref pos);
                                    //}
                                    int index = x - minAreaX + indexZ;
                                    ref var data = ref ad.lightProbeData[index];

                                    if (data.flag > 0)
                                    {
                                        realIndex[0] = false;
                                        realIndex[1] = false;
                                        realIndex[2] = false;
                                        int count = Physics.RaycastNonAlloc(pos, Vector3.down, hits, size.y * 2 + 10, layermask);
                                        if (count > 0)
                                        {
                                            //if (x < minChunkX) { minChunkX = x; }
                                            //if (x > maxChunkX) { maxChunkX = x; }
                                            //if (z < minChunkZ) { minChunkZ = z; }
                                            //if (z > maxChunkZ) { maxChunkZ = z; }

                                            int hitCount = 0;
                                            float lastHeight = -1;
                                            for (int i = 0; i < count; ++i)
                                            {
                                                ref var hit = ref hits[i];
                                                pos.y = hit.point.y + EngineContext.LightProbeAreaHeight;
                                                for (int j = 0; j < 4; ++j)
                                                {
                                                    if (data.height[j] >= -.5f)
                                                    {
                                                        //float h = Mathf.Abs(data.height[j] - hit.point.y);
                                                        //if (h < EngineContext.LightProbeAreaHeight * 0.5f)
                                                        {
                                                            float realH = hit.point.y + EngineContext.LightProbeAreaHeight;
                                                            if (lastHeight < -0.5f || Mathf.Abs(realH - lastHeight) > EngineContext.LightProbeAreaHeight)
                                                            {
                                                                lastHeight = realH;
                                                                realIndex[i] = true;
                                                                hitCount++;
                                                                break;
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            if (hitCount>0)
                                            {
                                                LightProbeInfo lpi = new LightProbeInfo() { x = x, z = z }; ;
                                                lpi.shData = new SHData[hitCount];
                                                lpo.probPostions.Add(lpi);
                                                hitCount = 0;
                                                for (int i = 0; i < count; ++i)
                                                {
                                                    if(realIndex[i])
                                                    {
                                                        ref var hit = ref hits[i];
                                                        ref var sd = ref lpi.shData[hitCount++];
                                                        sd.y = hit.point.y + EngineContext.LightProbeAreaHeight;
                                                        transPos.Add(trans.InverseTransformPoint(pos));
                                                    }
                             
                                                }
                                            }
                                           
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            LightProbeGroup lightprobeGroup;
            if (!gameObject.TryGetComponent(out lightprobeGroup))
            {
                lightprobeGroup = gameObject.AddComponent<LightProbeGroup>();
            }
            lightprobeGroup.probePositions = transPos.ToArray();
        }

        private void SyncLightProbes()
        {
            var trans = transform;
            LightProbeGroup lightprobeGroup;
            if (!gameObject.TryGetComponent(out lightprobeGroup))
            {
                lightprobeGroup = gameObject.AddComponent<LightProbeGroup>();
            }
            var transPos = new List<Vector3>();
            float halfLPaSize = EngineContext.LightProbeAreaSize * 0.5f;
            for (int i = 0; i < lpo.probPostions.Count; ++i)
            {
                var lpi = lpo.probPostions[i];
                for (int j = 0; j < lpi.shData.Length; ++j)
                {
                    var pos = new Vector3(lpi.x * EngineContext.LightProbeAreaSize + halfLPaSize,
                        lpi.shData[j].y,
                        lpi.z * EngineContext.LightProbeAreaSize + halfLPaSize);
                    transPos.Add(trans.InverseTransformPoint(pos));
                }
            }
            lightprobeGroup.probePositions = transPos.ToArray();
        }

        public void Copy(LightProbeObject src)
        {
            lpo.Copy(src);
            SyncLightProbes();
        }
        public void PrepareEdit(bool forceUpdate = false)
        {
            bool update = forceUpdate || context == null;
            if (context == null)
            {
                context = new LightProbeContext();
            }
            if (update)
            {
                int minChunkX = 10000;
                int maxChunkX = 0;
                int minChunkZ = 10000;
                int maxChunkZ = 0;
                int shDataCount = 0;
                for (int i = 0; i < lpo.probPostions.Count; ++i)
                {
                    var lpi = lpo.probPostions[i];
                    if (lpi.x < minChunkX) { minChunkX = lpi.x; }
                    if (lpi.x > maxChunkX) { maxChunkX = lpi.x; }
                    if (lpi.z < minChunkZ) { minChunkZ = lpi.z; }
                    if (lpi.z > maxChunkZ) { maxChunkZ = lpi.z; }
                    shDataCount += lpi.shData != null ? lpi.shData.Length : 0;
                }
                context.area = new Vector4Int(minChunkX, maxChunkX, minChunkZ, maxChunkZ);
                int sizeX = maxChunkX - minChunkX + 1;
                int sizeZ = maxChunkZ - minChunkZ + 1;
                context.shIndex = new Vector3Int[sizeX * sizeZ];
                context.sHVectors = new SHVector[shDataCount];
                int shDataIndex = 0;
                for (int i = 0; i < lpo.probPostions.Count; ++i)
                {
                    var lpi = lpo.probPostions[i];
                    int xx = lpi.x - minChunkX;
                    int zz = lpi.z - minChunkZ;
                    int index = xx + zz * (maxChunkX - minChunkX + 1);
                    int count = lpi.shData != null ? lpi.shData.Length : 0;
                    context.shIndex[index] = new Vector3Int(shDataIndex, count, i);
                    for (int j = 0; j < count; ++j)
                    {
                        ref var sh = ref lpi.shData[j];
                        ref var shv = ref context.sHVectors[j + shDataIndex];
                        shv.h = sh.y;
                        #if !UNITY_ANDROID
                        EnviromentSHBakerHelper.PrepareCoefs(ref sh.sh,
                            ref shv.shAr, ref shv.shAg, ref shv.shAb,
                            ref shv.shBr, ref shv.shBg, ref shv.shBb,
                            ref shv.shC);
                        #endif
                    }
                    shDataIndex += count;
                }

            }
        }

        public void Sample(AreaData ad, BoxCollider selectCollider = null)
        {
            if (ad != null)
                areaData = ad;
            if (areaData != null && lpo.probPostions.Count > 0 && lightProbes != null)
            {
                AABB aabb = selectCollider != null ? AABB.Create(selectCollider.bounds) : new AABB();
                int minX = areaData.lightProbeArea.x;
                int maxX = areaData.lightProbeArea.y;
                int minZ = areaData.lightProbeArea.z;
                int maxZ = areaData.lightProbeArea.w;
                Dictionary<int, List<int>> dataMap = new Dictionary<int, List<int>>();
                var positions = lightProbes.positions;
                for (int i = 0; i < positions.Length; ++i)
                {
                    ref var pos = ref positions[i];
                    if (selectCollider != null && !EngineUtility.ContainPoint(ref aabb, ref pos))
                        continue;
                    int x = (int)(pos.x / EngineContext.LightProbeAreaSize);
                    int z = (int)(pos.z / EngineContext.LightProbeAreaSize);
                    if (x >= minX && x <= maxX && z >= minZ && z <= maxZ)
                    {
                        int index = x - minX + (z - minZ) * (maxX - minX + 1);
                        List<int> indexLst;
                        if (!dataMap.TryGetValue(index, out indexLst))
                        {
                            indexLst = new List<int>();
                            dataMap[index] = indexLst;
                        }
                        indexLst.Add(i);
                    }
                }

                for (int i = 0; i < lpo.probPostions.Count; ++i)
                {
                    var lpi = lpo.probPostions[i];
                    lpi.draw = selectCollider == null;
                    for (int k = 0; k < lpi.shData.Length; ++k)
                    {
                        ref var sd = ref lpi.shData[k];
                        if (selectCollider == null)
                        {
                            sd.sh.Clear();
                        }
                    }
                    if (lpi.x >= minX && lpi.x <= maxX && lpi.z >= minZ && lpi.z <= maxZ)
                    {
                        int index = lpi.x - minX + (lpi.z - minZ) * (maxX - minX + 1);
                        List<int> indexLst;
                        if (dataMap.TryGetValue(index, out indexLst))
                        {
                            for (int j = 0; j < indexLst.Count; ++j)
                            {
                                int dataIndex = indexLst[j];
                                ref var pos = ref lightProbes.positions[dataIndex];
                                for (int k = 0; k < lpi.shData.Length; ++k)
                                {
                                    ref var sd = ref lpi.shData[k];
                                    if (Mathf.Abs(pos.y - sd.y) < EngineContext.LightProbeAreaSize)
                                    {
                                        sd.sh = lightProbes.bakedProbes[dataIndex];
                                        lpi.draw = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void GenBatch()
        {
            lpp.Clear();
            for (int i = 0; i < lpo.probPostions.Count; ++i)
            {
                var lpi = lpo.probPostions[i];
                if (lpi.draw)
                {
                    for (int j = 0; j < lpi.shData.Length; ++j)
                    {
                        ref var sd = ref lpi.shData[j];
                        SHVector shv = new SHVector();
                        shv.h = sd.y;
                        #if !UNITY_ANDROID
                        EnviromentSHBakerHelper.PrepareCoefs(ref sd.sh,
                            ref shv.shAr, ref shv.shAg, ref shv.shAb,
                            ref shv.shBr, ref shv.shBg, ref shv.shBb,
                            ref shv.shC);
#endif
                        lpp.Add(lpi.x, lpi.z, ref shv);
                    }
                }
            }
            lpp.EndAdd();
        }

        public void ExportTest()
        {
            var currentProbes = LightmapSettings.lightProbes;
            if (currentProbes != null)
            {
                currentProbes.name = "TestLightProbes";
                SceneContext sceneContext = new SceneContext();
                SceneAssets.GetCurrentSceneContext(ref sceneContext);
                string folder = SceneAssets.CreateFolder(sceneContext.configDir, "SceneLightmapBackup");
                string path = string.Format("{0}/TestLightProbes.asset", folder);
                EditorCommon.SaveAsset(path, ".asset", currentProbes);
                lightProbes = AssetDatabase.LoadAssetAtPath<LightProbes>(path);
            }
        }
    }

    [CanEditMultipleObjects, CustomEditor(typeof(LightprobeArea))]
    public class LightprobeAreaEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var lpa = target as LightprobeArea;
            //if (GUILayout.Button("ExportTest"))
            //{
            //    lpa.ExportTest();
            //}
            EditorGUILayout.LabelField(string.Format("ChunkID:{0}", lpa.chunkID));
            if (GUILayout.Button("Sample"))
            {
                lpa.Sample(null);
                lpa.GenBatch();
                lpa.drawProbes = true;
            }
            if (lpa.selectBox != null)
            {
                if (GUILayout.Button("SampleSelect"))
                {
                    lpa.Sample(null, lpa.selectBox);
                    lpa.GenBatch();
                    lpa.drawProbes = true;
                }
            }
            // if (GUILayout.Button ("Sync Probes"))
            // {
            //     lpa.SyncLightProbes ();
            // }
            //if (GUILayout.Button ("Refresh Probes"))
            //{
            //    lpa.Sample ();
            //    lpa.GenBatch ();
            //}
        }
    }
}

#endif