#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    [RequireComponent (typeof (CharacterController))]
    public class CharacterControllerTest : MonoBehaviour
    {
        public bool test = true;
        public bool raycast = false;
        private Transform t;
        private Transform child;
        private CharacterController cc;

        [NonSerialized]
        public string result = "";

        public bool sampleSH = false;
        [NonSerialized]
        public SHVector shData;

        [NonSerialized]
        public Vector3[] shPos = new Vector3[4];
        public Material mat;
        [Range (0, 1)]
        public float shWeight = 0;

        public static bool forceUpdateSH = false;  
        private LightprobeArea lpa;
        private Vector2Int Chunkindex;
        private Vector3 tempshpos = new Vector3();
        private List<useindexdata> _contactindex = new List<useindexdata>(); 
        private List<int> _indexnumleft = new List<int>();

        public struct useindexdata
        {
            public SHVector data;
            public Vector3 shpos;
            public float dis;            
            public Vector2Int indexnum;
            public bool havevalue;
        }

        void Update ()
        {
            if (t == null)
                t = this.transform;

            Vector3 pos = t.position;
            if (test)
            {

                if (child == null && t.childCount > 0)
                {
                    child = t.GetChild (0);
                }

                float terrainY = -1;
                if (GlobalContex.ee != null)
                {
                    terrainY = GlobalContex.ee.GetTerrainY (ref pos);
                }
                if (raycast)
                {
                    pos.y += 0.05f;
                    Ray ray = new Ray (pos + new Vector3 (0, 0.1f, 0), Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast (ray, out hit, 500))
                    {

                        if (terrainY < hit.point.y)
                        {
                            terrainY = hit.point.y;
                        }
                        result = string.Format ("hit collider:{0} y:{1}", hit.collider.name, hit.point.y);
                    }
                    else
                    {
                        result = string.Format ("hit terrain y:{0}", hit.point.y);
                    }

                    if (child != null)
                        terrainY -= child.localPosition.y;
                }
                else
                {
                    if (cc == null)
                    {
                        cc = GetComponent<CharacterController> ();
                    }
                    Vector3 delta = Physics.gravity * Time.deltaTime;
                    var cf = cc.Move (delta);
                    result = string.Format ("move result y:{0}", cf);
                    pos = t.position;
                    if (terrainY < pos.y && (cf & CollisionFlags.Below) != 0)
                    {
                        terrainY = pos.y;
                    }
                    else // if (cf == CollisionFlags.None)
                    {
                        if (child != null)
                            terrainY -= child.localPosition.y;
                    }
                }
                if (terrainY > -1000)
                {
                    pos.y = terrainY;
                    t.position = pos;
                }
                //}

            }
            if (sampleSH)
                SampleSH (ref pos, ref shData);
        }

        private static LightprobeArea GetChunkByPos4 (EngineContext context, int x4, int z4, ref Vector2Int _chunkindex, LightprobeArea lpa)
        {

            float x = x4 * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
            float z = z4 * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
            int xx = (int) (x / EngineContext.ChunkSize);
            int zz = (int) (z / EngineContext.ChunkSize);

            if (_chunkindex == null)
            {
                _chunkindex = new Vector2Int();
            }
      
            if (_chunkindex.x != xx || _chunkindex.y != zz|| lpa==null)
            {
                _chunkindex.x = xx;
                _chunkindex.y = zz;
                string lpaName = string.Format("LightProbeArea_{0}_{1}", xx, zz);
                GameObject go = GameObject.Find(lpaName);          
                go.TryGetComponent(out lpa);
            }    
            return lpa;
        }

        private bool BlendSH (EngineContext context, int x, int z, Vector3 pos, float weight, int posIndex,ref List<useindexdata> _yushifongindex)
        {
            useindexdata _usindexdata = Uidinst(_yushifongindex[posIndex],x,z);         
     
            if (shPos[posIndex] == null)
            {
                shPos[posIndex] = new Vector3();
            }     
            lpa = GetChunkByPos4 (context, x, z,ref Chunkindex, lpa);
            if (lpa != null)
            {
                lpa.PrepareEdit (forceUpdateSH);
                forceUpdateSH = false;
                var lpaContext = lpa.context;
                int minX = lpaContext.area.x;
                int maxX = lpaContext.area.y;
                int minZ = lpaContext.area.z;
                int maxZ = lpaContext.area.w;

                if (x >= minX && x <= maxX && z >= minZ && z <= maxZ)
                {
                    int xOffset = x - minX;
                    int zOffset = z - minZ;
                    int index = xOffset + zOffset * (maxX - minX + 1);
                    if (index >= 0 && index < lpaContext.shIndex.Length)
                    {
                        ref var reIndex = ref lpaContext.shIndex[index];
                        float juli = float.MaxValue;            
                            for (int i = 0; i < reIndex.y; ++i)
                            {
                                ref var sh = ref lpaContext.sHVectors[i + reIndex.x];
                                // ref var lpi = ref lpaContext.sHVectors[i];
                                float hWeight = Math.Abs(pos.y - sh.h) / EngineContext.LightProbeAreaSize;
                                if (hWeight < juli)
                                {
                                     juli = hWeight;
                                    // weight = (1 - hWeight * hWeight * hWeight) * weight;                            
                                    _usindexdata.data = sh;
                                    shPos[posIndex].x = x * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
                                    shPos[posIndex].y = sh.h;
                                    shPos[posIndex].z = z * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
                                    tempshpos.x= x * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
                                    tempshpos.y = sh.h;
                                    tempshpos.z = z * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;

                                    _usindexdata.havevalue = true;
                                    _usindexdata.shpos = shPos[posIndex];
                                    _usindexdata.dis = Vector3.Distance(shPos[posIndex], pos);
                                    _yushifongindex[posIndex] = _usindexdata;
                                    return true;
                                }
                            }
                       
                    }
                    shPos[posIndex].x =0f;
                    shPos[posIndex].y =-1f;
                    shPos[posIndex].z = 0f;
                }
            }
            _usindexdata.havevalue = false;
            _yushifongindex[posIndex] = _usindexdata;
            return false;
        }



        private bool AddCheckValue(EngineContext context, int x, int z, float h,ref SHVector _shv,ref bool inrange)
        {
            if (lpa != null)
            {
                var lpaContext = lpa.context;
                int minX = lpaContext.area.x;
                int maxX = lpaContext.area.y;
                int minZ = lpaContext.area.z;
                int maxZ = lpaContext.area.w;
                if (x >= minX && x <= maxX && z >= minZ && z <= maxZ)
                {
                    int xOffset = x - minX;
                    int zOffset = z - minZ;
                    int index = xOffset + zOffset * (maxX - minX + 1);
                    if (index >= 0 && index < lpaContext.shIndex.Length)
                    {
                        ref var reIndex = ref lpaContext.shIndex[index];
                        if (reIndex.y <=0 )//为空值
                        {
                            return false;
                        }                 
                        else
                        {
                            float juli = float.MaxValue;                  
                            for (int i = 0; i < reIndex.y; ++i)
                            {
                                ref var sh = ref lpaContext.sHVectors[i + reIndex.x];
                                float hWeight = Math.Abs(h - sh.h) / EngineContext.LightProbeAreaSize;
                                if (hWeight < juli)
                                {
                                    juli = hWeight;                           
                                    _shv = sh;
                                }
                            }                          
                                return true;
                        }                     
                    }
                    else
                    {
                        inrange = false;
                    }
                }
                else
                {
                    inrange = false;
                }             
            }
            inrange = false;
            return false;
        } 
 
        private void SampleSH (ref Vector3 pos, ref SHVector shData)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && mat != null)
            {
                Vector4Int lightProbePos;
                float xWeight;
                float zWeight;    
                SceneMiscSystem.ClacProbeIndex (ref pos, ref shData, out lightProbePos, out xWeight, out zWeight);        
                if (_contactindex.Count <= 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        useindexdata _useindexdd = new useindexdata();
                        _useindexdd.data = new SHVector();
                        _useindexdd.indexnum = Vector2Int.zero;
                        _useindexdd.shpos = Vector3.zero;
                        _useindexdd.havevalue = false;
                        _contactindex.Add(_useindexdd);
                    }
                }
                 BlendSH (context, lightProbePos.x, lightProbePos.z, pos, (1 - xWeight) * (1 - zWeight), 0,ref _contactindex);
                 BlendSH (context, lightProbePos.y, lightProbePos.z, pos, xWeight * (1 - zWeight), 1, ref _contactindex);
                 BlendSH (context, lightProbePos.y, lightProbePos.w, pos, xWeight * zWeight, 2,ref _contactindex);
                 BlendSH (context, lightProbePos.x, lightProbePos.w, pos, (1 - xWeight) * zWeight, 3,ref _contactindex);       
                _indexnumleft.Clear();
                for (int i = 0; i < _contactindex.Count; i++)
                {
                    if (!_contactindex[i].havevalue) { _indexnumleft.Add(i); }                         
                }
                for (int i = 0; i < _indexnumleft.Count; i++)
                {
                    Vector2Int _vecadd = GetAddValueAll(pos, _contactindex, _indexnumleft[i]);
                    Vector2Int _vecadd0 = Vector2Int.zero;
                    Vector2Int _vecadd1 = Vector2Int.zero;
                    _vecadd0.x = _vecadd.x;
                    _vecadd1.y = _vecadd.y;
                    CalUseValue(context, pos, _indexnumleft[i], _vecadd0, ref _contactindex);
                    CalUseValue(context, pos, _indexnumleft[i], _vecadd1, ref _contactindex);
                }
                //寻找结束后的整理
                _indexnumleft.Clear();//找到的点
                for (int i = 0; i < _contactindex.Count; i++)
                {
                    if (_contactindex[i].havevalue)
                    {
                        _indexnumleft.Add(i); 
                    }
                }             
                if (_indexnumleft.Count == 0)//完全没有SH灯！！
                {
                    SetValueToShader(context,0.6f, shData);
                }else
                {
                    FillEmptyData(pos, _contactindex,ref shData);
                    SetValueToShader(context, 0.1f, shData);
                }                                                     
            }
        }

        private void SetValueToShader(EngineContext context,float weight,SHVector shData)
        {
            shData.shC.w = shWeight;
            context.roleSH.shC.w = shWeight;
            mat.SetVector("custom_SHAr", Vector4.Lerp(shData.shAr, context.roleSH.shAr, weight));
            mat.SetVector("custom_SHAg", Vector4.Lerp(shData.shAg, context.roleSH.shAg, weight));
            mat.SetVector("custom_SHAb", Vector4.Lerp(shData.shAb, context.roleSH.shAb, weight));
            mat.SetVector("custom_SHBr", Vector4.Lerp(shData.shBr, context.roleSH.shBr, weight));
            mat.SetVector("custom_SHBg", Vector4.Lerp(shData.shBg, context.roleSH.shBg, weight));
            mat.SetVector("custom_SHBb", Vector4.Lerp(shData.shBb, context.roleSH.shBb, weight));
            mat.SetVector("custom_SHC", Vector4.Lerp(shData.shC, context.roleSH.shC, weight));

            shData.shC.w = shWeight;
            context.roleShV2.shC.w = shWeight;
            mat.SetVector("custom_ShV2Ar", Vector4.Lerp(shData.shAr, context.roleShV2.shAr, weight));
            mat.SetVector("custom_ShV2Ag", Vector4.Lerp(shData.shAg, context.roleShV2.shAg, weight));
            mat.SetVector("custom_ShV2Ab", Vector4.Lerp(shData.shAb, context.roleShV2.shAb, weight));
            mat.SetVector("custom_ShV2Br", Vector4.Lerp(shData.shBr, context.roleShV2.shBr, weight));
            mat.SetVector("custom_ShV2Bg", Vector4.Lerp(shData.shBg, context.roleShV2.shBg, weight));
            mat.SetVector("custom_ShV2Bb", Vector4.Lerp(shData.shBb, context.roleShV2.shBb, weight));
            mat.SetVector("custom_ShV2C", Vector4.Lerp(shData.shC, context.roleShV2.shC, weight));
        }

        private void FillEmptyData(Vector3 pos, List<useindexdata> _yushifongindex,ref SHVector shData)
        {       
            float disadd = 0;
            float xnum = 0; 
            for (int i = 0; i < _yushifongindex.Count; i++)
            {
                if (_yushifongindex[i].havevalue)
                {            
                    disadd +=1/ _yushifongindex[i].dis;         
                }
            }
            xnum = 1 / disadd;
            for (int i = 0; i < _yushifongindex.Count; i++)
            {
                if (_yushifongindex[i].havevalue)
                {                  
                         float weight = xnum / _yushifongindex[i].dis;
                        shData.shAr += _yushifongindex[i].data.shAr * weight;
                        shData.shAg += _yushifongindex[i].data.shAg * weight;
                        shData.shAb += _yushifongindex[i].data.shAb * weight;
                        shData.shBr += _yushifongindex[i].data.shBr * weight;
                        shData.shBg += _yushifongindex[i].data.shBg * weight;
                        shData.shBb += _yushifongindex[i].data.shBb * weight;
                        shData.shC += _yushifongindex[i].data.shC * weight;                  
                }
            }
        }


        private Vector2Int GetAddValueAll(Vector3 pos, List<useindexdata> _contactindex, int index)
        {
            Vector2Int _testvalue = Vector2Int.zero;
            _testvalue.x = pos.x - EngineContext.LightProbeAreaSize * (_contactindex[index].indexnum.x + 0.5f) >= 0f ? -1 : 1;
            _testvalue.y = pos.z - EngineContext.LightProbeAreaSize * (_contactindex[index].indexnum.y + 0.5f) >= 0f ? -1 : 1;  
            return _testvalue;
        }

        private void CalUseValue(EngineContext context,Vector3 pos, int indexnumleft, Vector2Int _vecadd,  ref List<useindexdata> _contactindex)
        {
            bool addhavevalue = false;
            bool inrange = true;
            int xstart = _contactindex[indexnumleft].indexnum.x;
            int zstart = _contactindex[indexnumleft].indexnum.y;
            useindexdata _usidata = _contactindex[indexnumleft];
            tempshpos = Vector3.zero;
            int movenum=0;
            while (!addhavevalue && inrange&& movenum<=15)
            {
                movenum++;
                xstart = xstart + _vecadd.x;
                zstart = zstart + _vecadd.y;
                SHVector _shdata = _usidata.data;
                addhavevalue = AddCheckValue(context, xstart, zstart, pos.y, ref _shdata, ref inrange);
                if (addhavevalue && inrange)
                {
                    tempshpos.x = xstart * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
                    tempshpos.y = _shdata.h;
                    tempshpos.z = zstart * EngineContext.LightProbeAreaSize + EngineContext.LightProbeAreaSize * 0.5f;
                    if (!_usidata.havevalue|| _usidata.havevalue&& Vector3.Distance(pos, tempshpos) < Vector3.Distance(pos, _usidata.shpos))
                    {                                     
                         _usidata = Uidset(_usidata, tempshpos, pos, indexnumleft, _shdata);                                                
                    }
                }
            }
            _contactindex[indexnumleft] = _usidata;
        }

        private useindexdata Uidinst(useindexdata _usindexdata,int x,int z)
        {
            _usindexdata.shpos = Vector3.zero;
            _usindexdata.dis = 0;
            _usindexdata.havevalue = false;
            _usindexdata.data.Reset();
            Vector2Int inde = _usindexdata.indexnum;
            inde.x = x;
            inde.y = z;
            _usindexdata.indexnum = inde;
            return _usindexdata;
        }
        private useindexdata Uidset(useindexdata _usidata,Vector3 tempshpos,Vector3 pos,int indexnumleft,SHVector _shdata)
        {
            _usidata.data = _shdata;
            _usidata.shpos.x = tempshpos.x;
            _usidata.shpos.y = tempshpos.y;
            _usidata.shpos.z = tempshpos.z;
            shPos[indexnumleft] = tempshpos;
            _usidata.dis = Vector3.Distance(pos, tempshpos);
            _usidata.havevalue = true;
            return _usidata;
        }

        void OnDrawGizmos ()
        {
            if (sampleSH)
            {
                if (t == null)
                    t = this.transform;

                Vector3 pos = t.position;
                var c = Gizmos.color;
                Gizmos.color = Color.green;
                for (int i = 0; i < shPos.Length; ++i)
                {
                    ref var p = ref shPos[i];
                    if (p.y > 0)
                        Gizmos.DrawLine (pos, p);
                }
                Gizmos.color = c;
            }
        }

    
    }

    [CustomEditor (typeof (CharacterControllerTest))]
    public class CharacterControllerTestEditor : UnityEngineEditor
    {
        private void OnEnable () { }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            if (GUILayout.Button ("RefreshTerrainHeight"))
            {
                EnvironmentExtra.FillTerrainVertex ();
            }
            var cct = target as CharacterControllerTest;
            if (cct.test)
            {
                EditorGUILayout.LabelField (cct.result);
            }
            if (GUILayout.Button ("ReSampleSH"))
            {
                CharacterControllerTest.forceUpdateSH = true;
            }
        }
    }
}
#endif