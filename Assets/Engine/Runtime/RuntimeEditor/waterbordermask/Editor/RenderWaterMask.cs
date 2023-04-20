using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using UnityEditor;
namespace CFEngine
{
public class RenderWaterMask : MonoBehaviour
{
    public Vector3 m_center;

    public int sizeXZ = 128;

    public Camera m_cam;

    public Material m_waterMaterial;

    public  int m_rtResolution = 1024;

    public static bool m_debugMode;

    public enum Mode
    {
        Once,
        Always
    }
    public Mode m_currentMode = Mode.Once;

    public RenderTexture m_rt;

    private string _path;

    public Texture2D m_tex;

    [ContextMenu("ReRenderWaterMask")]
    void ReRenderWaterMask()
    {

        m_cam.aspect = 1f;
        m_cam.orthographicSize = (float)sizeXZ /2f;
        m_rt = RenderTexture.GetTemporary(m_rtResolution, m_rtResolution, 0);
        SetParam();
        // m_cam.targetTexture = m_rt;
    }

    
    private void OnEnable()
    {

        m_cam = this.GetComponent<Camera>();
        m_cam.aspect = 1f;
        m_cam.orthographicSize = (float)sizeXZ /2f;
        SetParam();
        // m_cam.targetTexture = m_rt;
        
    }

    void Update()
    {
        if(m_currentMode == Mode.Always)
        {
             m_cam.aspect = 1f;
            m_cam.orthographicSize = (float)sizeXZ /2f;
            SetParam();
        }
    }

    void SetParam()
    {
        if (m_rt == null)
        {
            m_rt = RenderTexture.GetTemporary(m_rtResolution, m_rtResolution, 0);
        }
        float halfXZ = sizeXZ * 0.5f;
        m_waterMaterial.SetFloat("_waterMaskBoxSize",(float)sizeXZ);
        m_waterMaterial.SetVector("_WaterMaskBox",new Vector4(m_center.x - halfXZ, m_center.z -halfXZ, 0f, 0f));

    }

    private void OnDrawGizmosSelected ()
    {
        var c = Gizmos.color;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube (new Vector3(m_center.x,this.transform.position.y,m_center.z), new Vector3 (sizeXZ, m_cam.farClipPlane - m_cam.nearClipPlane , sizeXZ));
        Gizmos.color = c;
        this.transform.position =new Vector3(m_center.x,this.transform.position.y,m_center.z);
    } 

    
    [ContextMenu("SaveWaterMaskTexuture")]
    public void SaveWaterMaskTexture()
    {
        string matDir = EditorCommon.GetAssetDir(m_waterMaterial);
        _path = string.Format ("{0}/{1}_WaterMask.png", matDir, m_waterMaterial.name);
  
        string filename = _path;
        ScreenCapture.CaptureScreenshot(filename);
        StartCoroutine(ModifyImporter());




    }

    IEnumerator ModifyImporter()
    {
        yield return new WaitForSecondsRealtime(5);
        TextureImporter import = AssetImporter.GetAtPath(_path) as TextureImporter;
        import.wrapMode = TextureWrapMode.Clamp;
        import.maxTextureSize = m_rtResolution;
        import.isReadable = true;
        import.mipmapEnabled = false;
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D> (_path);
        m_tex = tex;
        m_waterMaterial.SetTexture("_WaterMask",m_tex);
        //    byte[] bytes = screenShot.EncodeToPNG();


        //File.WriteAllBytes(filename, bytes);
        AssetDatabase.Refresh();
    }

    IEnumerator  CaptureScreen(Texture2D texture)
    {
        yield return new WaitForEndOfFrame();
       
 
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
 
        texture.Apply();
    }
}
}
