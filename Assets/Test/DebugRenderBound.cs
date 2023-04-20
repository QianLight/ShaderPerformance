using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRenderBound : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public List<Vector4> Tangents=new List<Vector4>();
    // Update is called once per frame
    void Update()
    {
        MeshFilter mr = gameObject.GetComponent<MeshFilter>();
        mr.sharedMesh.GetTangents(Tangents);
    }


    public void OnDrawGizmos()
    {
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        Gizmos.DrawCube(mr.bounds.center, mr.bounds.size);
    }
}
