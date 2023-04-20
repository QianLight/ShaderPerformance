using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;
using System.IO;

using UnityEngine;

public class SceneRecorderStatus : MonoBehaviour
{
    [MenuItem("GameObject/录制场景性能数据/开", false, 0)]
    public static void RecordSceneInfoOpen()
    {
        SceneRecorderStatus _SceneRecorderStatus = GameObject.FindObjectOfType<SceneRecorderStatus>();
        if (!_SceneRecorderStatus)
        {
            GameObject newObj = new GameObject("SceneRecorderStatus");
            _SceneRecorderStatus = newObj.AddComponent<SceneRecorderStatus>();
        }
    }

    [MenuItem("GameObject/录制场景性能数据/关", false, 0)]
    public static void RecordSceneInfoClose()
    {
        SceneRecorderStatus _SceneRecorderStatus = GameObject.FindObjectOfType<SceneRecorderStatus>();
        if (_SceneRecorderStatus)
        {
            GameObject.Destroy(_SceneRecorderStatus.gameObject);
        }
    }
    
    
    public List<PerformanceInfo> framesTexture = new List<PerformanceInfo>();
    IEnumerator RecordFrame()
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        PerformanceInfo newInfo = new PerformanceInfo();
        newInfo.Record();
        newInfo.texture2d = texture;
        newInfo.frameIndex = Time.frameCount;
        
        framesTexture.Add(newInfo);
    }

    private Camera mainCamera;
    private void Awake()
    {
        mainCamera=Camera.main;
    }

    private Vector3 lastPos;
    public void LateUpdate()
    {
        if (Vector3.Distance(lastPos, mainCamera.transform.position) > 1)
        {
            lastPos = mainCamera.transform.position;
            StartCoroutine(RecordFrame());
        }
    }

    private void OnDisable()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        string targetFolder = Application.dataPath.Replace("/Assets", "/Library/SceneRecord/" + scene.name + "/");
        
        
        Debug.Log(targetFolder);
        
        if (Directory.Exists(targetFolder))
        {
            Directory.Delete(targetFolder,true);
        }
        
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        string allFileInfo = targetFolder + "SceneInfo.txt";

        List<string> allShowDatas = new List<string>();
        for (int i = 0; i < framesTexture.Count; i++)
        {
            PerformanceInfo newInfo = framesTexture[i];
            allShowDatas.Add(newInfo.ToString());

            byte[] allBytes = newInfo.texture2d.EncodeToJPG();
            string texFile = targetFolder + newInfo.frameIndex+".jpg";
            File.WriteAllBytes(texFile,allBytes);
            GameObject.Destroy(newInfo.texture2d);
        }
        
        File.WriteAllLines(allFileInfo,allShowDatas);
        
        framesTexture.Clear();
    }
    
    public class PerformanceInfo
    {
        public Texture2D texture2d;

        public int BatchesCount;
        public int DynamicBatchesCount;
        public int StaticBatchesCount;
        public int InstancingBatchesCount;

        public int DrawCallCount;
        public int DynamicBatchesDrawCallsCount;
        public int StaticBatchedDrawCallsCount;
        public int InstancingBatchedDrawCallsCount;

        public int SetPassCallCount;
        public int ShadowCasterCount;

        public int TriangleCount;
        public int VertexCount;

        public int frameIndex = 0;
        
        public void Record()
        {
//#if UNITY_EDITOR
            BatchesCount = UnityEditor.UnityStats.batches;
            DynamicBatchesCount = UnityEditor.UnityStats.dynamicBatches;
            StaticBatchesCount = UnityEditor.UnityStats.staticBatches;
            InstancingBatchesCount = UnityEditor.UnityStats.instancedBatches;

            DrawCallCount = UnityEditor.UnityStats.drawCalls;
            DynamicBatchesDrawCallsCount = UnityEditor.UnityStats.dynamicBatchedDrawCalls;
            StaticBatchedDrawCallsCount = UnityEditor.UnityStats.staticBatchedDrawCalls;
            InstancingBatchedDrawCallsCount = UnityEditor.UnityStats.instancedBatchedDrawCalls;

            SetPassCallCount = UnityEditor.UnityStats.setPassCalls;
            ShadowCasterCount = UnityEditor.UnityStats.shadowCasters;

            TriangleCount = UnityEditor.UnityStats.triangles;
            VertexCount = UnityEditor.UnityStats.vertices;
            
//#endif
            RecordSkinMesh();
        }

        public int allCharacterCount = 0;
        public int allCharactervertexCount = 0;
        public void RecordSkinMesh()
        {
            Animator[] _Animatora = GameObject.FindObjectsOfType<Animator>();
            if(_Animatora==null) return;
            
            allCharacterCount = _Animatora.Length;
            foreach (var Animator in _Animatora)
            {
                SkinnedMeshRenderer[] smrs = Animator.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if(smrs==null) continue;
                
                foreach (SkinnedMeshRenderer smr in smrs)
                {
                    if(smr==null||smr.sharedMesh==null) continue;
                    
                    allCharactervertexCount += smr.sharedMesh.vertexCount;
                }
            }
        }




        public override string ToString()
        {
            var s = $"{frameIndex} ** {TriangleCount}-{BatchesCount}-{SetPassCallCount}  {VertexCount}  {allCharactervertexCount} {allCharacterCount}  {DynamicBatchesCount} {DynamicBatchesCount} {DynamicBatchesCount} ";
            s += $"{DrawCallCount} {DynamicBatchesDrawCallsCount} {StaticBatchedDrawCallsCount} {InstancingBatchedDrawCallsCount} ";
            s += $"{ShadowCasterCount}  ";
            return s;
            //return $"{BatchesCount}";
        }
    }
}
#endif
