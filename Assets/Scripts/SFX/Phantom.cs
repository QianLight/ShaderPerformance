using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class PhantomData
{
    public bool isOn;
    public Mesh mesh;
    public float time;
    public Material material;
    public MaterialPropertyBlock block;
    public Vector3 position;
    public Quaternion rotation;
    public PhantomData(float time, Material material){
        mesh = new Mesh();
        mesh.MarkDynamic();
        isOn = false;
        this.material = material;
        block = new MaterialPropertyBlock();
    }
}
[RequireComponent(typeof(Animator))]
public class Phantom : MonoBehaviour
{
    public Material shadowMaterial;
    public string colorParamName;
    public int maxCount;
    public float duration;
    public float minDistance;
    public float minInterval;

    public Animator SyncAnimator;
    // public GameObject lowPolyModel;
    // private GameObject _lowPolySkinnedMesh;
    private Animator _characterAnimator;
    private SkinnedMeshRenderer[] _skinnedMeshRenderer;
    [SerializeField]private List<PhantomData> meshData;
    private Vector3 _oldpos;
    private MaterialPropertyBlock _block;
    private float _timer = 0;
    private int _paramID;
    private CombineInstance[] _combineInstance;
    // private CombineInstance[] _parentCombine;
    // private Mesh _totalMesh;

    public void Awake(){
        // _totalMesh = new Mesh();
        _skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        _characterAnimator = GetComponent<Animator>();
        _characterAnimator.Rebind();
        _combineInstance = new CombineInstance[_skinnedMeshRenderer.Length];
        for (int i = 0; i < _skinnedMeshRenderer.Length; i++)
        {
            _combineInstance[i] = new CombineInstance();
            _combineInstance[i].mesh = new Mesh();
            // _skinnedMeshRenderer[i].enabled = false;
        }
        meshData = new List<PhantomData>();
        _block = new MaterialPropertyBlock();
        for (int i = 0; i < maxCount; i++)
        {
            meshData.Add(new PhantomData(duration, shadowMaterial));
        }
        _paramID = Shader.PropertyToID(colorParamName);
    }

    public void Update(){
        // _totalMesh.Clear();

        for (int i = 0; i < _characterAnimator.layerCount; i++)
        {
            _characterAnimator.Play(SyncAnimator.GetCurrentAnimatorStateInfo(i).fullPathHash, i, SyncAnimator.GetCurrentAnimatorStateInfo(i).normalizedTime);
            _characterAnimator.SetLayerWeight(i, SyncAnimator.GetLayerWeight(i));
        }
        
        for (int i = 0; i < maxCount; i++)
        {
            if(meshData[i].time >= duration){
                PhantomData data = meshData[i];
                data.mesh.Clear();
                data.time = 0;
                data.isOn = false;
            }
            if(meshData[i].isOn)
            {
                meshData[i].time += Time.deltaTime;
                // block.SetFloat(_paramID, (Duration - meshData[i].time) / Duration * 0.8f);
                float level = (duration - meshData[i].time) / duration * 0.8f;
                _block.SetColor(_paramID, new Color(level, 0, 0, level * level * level));
                // for (int j = 0; j < meshData[i].mesh.uv.Length; j++)
                // {
                //     meshData[i].mesh.uv[j] = new Vector2(level, 0);
                // }
                Graphics.DrawMesh(meshData[i].mesh,Vector3.zero, meshData[i].rotation, shadowMaterial, 0, 
                null, 0, _block, false, false, false);
            }
        }

       
        // Graphics.DrawMesh(_totalMesh,Vector3.zero, Quaternion.identity, shadowMaterial, 0, null);
        // CreateTotalMesh();
        if(Vector3.Magnitude(transform.position - _oldpos) <= minDistance){
            return;
        }else{
            _timer += Time.deltaTime;
            if(_timer > minInterval){
                PhantomData data = meshData[0];
                meshData.Remove(data);
                data.position = transform.position;
                data.rotation = Quaternion.identity;
                data.time = 0;
                for (int j = 0; j < _skinnedMeshRenderer.Length; j++)
                {
                    _combineInstance[j].mesh.Clear();
                    _skinnedMeshRenderer[j].BakeMesh(_combineInstance[j].mesh);
                    _combineInstance[j].transform = _skinnedMeshRenderer[j].transform.localToWorldMatrix;
                }
                data.mesh.CombineMeshes(_combineInstance,true);
                data.isOn = true;
                _timer = 0;
                
                // _parentCombine = new CombineInstance[activeNum];
                // int lazyIndex = 0;
                // for (int i = 0; i < Num; i++)
                // {
                //     if (meshData[i].isOn)
                //     {
                //         _parentCombine[lazyIndex].mesh = meshData[i].mesh;
                //         _parentCombine[lazyIndex].transform = Matrix4x4.identity;
                //         lazyIndex++;
                //     }
                // }
                // _totalMesh.CombineMeshes(_parentCombine);
                
                meshData.Add(data);
            }
            _oldpos = transform.position;
        }
    }

    // private void CreateTotalMesh()
    // {
    //     vertList.Clear();
    //     tri.Clear();
    //     uvList.Clear();
    //     // List<Color> vertColors = new List<Color>();
    //     _totalMesh.Clear();
    //     int triSerial = 0;
    //     for (int i = 0; i < Num; i++)
    //     {
    //         vertList.AddRange(meshData[i].mesh.vertices);
    //         List<int> newTri = new List<int>();
    //         foreach (var meshTriangle in meshData[i].mesh.triangles)
    //         {
    //             newTri.Add(meshTriangle);
    //         }
    //         if (i != 0)
    //         {
    //             for (int j = 0; j < newTri.Count; j++)
    //             {
    //                 newTri[i] += triSerial;
    //             }
    //         }
    //         triSerial += meshData[i].mesh.vertices.Length;
    //         tri.AddRange(newTri);
    //         uvList.AddRange(meshData[i].mesh.uv);
    //     }
    //
    //     _totalMesh.vertices = vertList.ToArray();
    //     _totalMesh.uv = uvList.ToArray();
    //     _totalMesh.triangles = tri.ToArray();
    //     Graphics.DrawMesh(_totalMesh,transform.position,Quaternion.identity,shadowMaterial,0);
    // }

    // private void OnDestroy()
    // {
    //     Destroy(_totalMesh);
    // }
}
