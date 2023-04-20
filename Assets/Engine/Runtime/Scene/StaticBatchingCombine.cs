using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBatchingCombine : MonoBehaviour
{
    private void findGameObject(Transform root, List<GameObject> list)
    {
        foreach(Transform t in root)
        {
            findGameObject(t, list);
        }
        MeshFilter mf = root.GetComponent<MeshFilter>();
        if(mf != null)
        {
            list.Add(root.gameObject);
        }
    }
    public int DelayFrame = 5;
    private int delay = 0;
    //private void OnEnable()
    //{
    //    //List<GameObject> list = new List<GameObject>();
    //    //findGameObject(transform, list);
    //    //StaticBatchingUtility.Combine(list.ToArray(), gameObject);
    //    StaticBatchingUtility.Combine(gameObject);
    //}
    private void Update()
    {
        delay++;
        if(delay >= DelayFrame)
        {
            StaticBatchingUtility.Combine(gameObject);
            enabled = false;
        }
    }
    //private void OnDisable()
    //{
        
    //}
}
