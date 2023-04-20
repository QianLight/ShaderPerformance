#if ENABLE_UPO && ENABLE_UPO_OVERDRAW
using System;
using System.IO;
using System.Collections;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UPOHelper.Network;
using UPOHelper.Utils;

public class OverdrawCameraMonitorWithoutCS : MonoBehaviour
{
    public static bool EnableScreenShot = true;
    
    //core var
    public Camera _targetCam;
    public string _camName;
    
    // public Camera targetCamera {
    //     // get => _targetCam;
    //     // set => _targetCam = value;
    // }

    public void SetTargetCamera(Camera targetCam) {
        _targetCam = targetCam;
        _camName = targetCam.name;
    }
    
    Shader replacementShader;
    RenderTexture overdrawTexture;
    Texture2D readingTexture;
    Rect readingRect;
    
    public float OverdrawRatio;
    private float TotalG;

    private bool finishCounter = true;
    private int counter = 60;
    
    int frameBreakSize;
    int MonitorFrequency = 60;

    private string replacementTag;

    private NativeArray<Color> replacementColors;

    void Awake() {
        replacementShader = Shader.Find("Debug/MyOverdraw");
    }
    
    public void OnDestroy() {
        ReleaseRT();
    }
    
    void InitRT() {
        int RTWidth, RTHeight;
        
        RTWidth = _targetCam.pixelWidth;
        RTHeight = _targetCam.pixelHeight;
        
        if (_targetCam.renderingPath == RenderingPath.Forward || _targetCam.renderingPath == RenderingPath.VertexLit)
        {
            replacementTag = "RenderType";
        }
        else
        {
            replacementTag = null;
        }
        
        overdrawTexture = new RenderTexture(RTWidth, RTHeight, 24, RenderTextureFormat.ARGBFloat);
        readingTexture = new Texture2D(RTWidth, RTHeight, TextureFormat.RGBAFloat, false);
        readingRect = new Rect(0, 0, overdrawTexture.width, overdrawTexture.height);
        //do some other things
        frameBreakSize = RTWidth * RTHeight / ((MonitorFrequency - 2) * 2);
        
    }
    
    void ReleaseRT() {
        if (overdrawTexture != null) {
            overdrawTexture.Release();
            overdrawTexture = null;
            Destroy(readingTexture);
            readingTexture = null;
            readingRect = Rect.zero;
        }
    }
    
    private void RecreateTexture(Camera cam) {
        if (overdrawTexture == null) {
            overdrawTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24, RenderTextureFormat.ARGBFloat);
            overdrawTexture.enableRandomWrite = true;
            cam.targetTexture = overdrawTexture;
        }
        if (cam.pixelWidth != overdrawTexture.width || cam.pixelHeight != overdrawTexture.height) {
            overdrawTexture.Release();
            overdrawTexture.width = cam.pixelWidth;
            overdrawTexture.height = cam.pixelHeight;
        }
    }
    
    private void LateUpdate()
    {
        if (finishCounter)
        {
            //Debug.Log("LateUpdate@OverdrawCameraMonitor.cs");
            //start measurement of this frame
            if (_targetCam == null)
            {
                Debug.Log("_targetCam is null");
                return;
            }

            CameraClearFlags originalClearFlags = _targetCam.clearFlags;
            Color originalClearColor = _targetCam.backgroundColor;
            RenderTexture originalTargetTexture = _targetCam.targetTexture;
            bool originalCamEnabled = _targetCam.enabled;

            if (overdrawTexture == null)
            {
                ReleaseRT();
                InitRT();
            }

            _targetCam.clearFlags = CameraClearFlags.SolidColor;
            _targetCam.backgroundColor = Color.clear;
            _targetCam.targetTexture = overdrawTexture;
            _targetCam.enabled = false;

            _targetCam.RenderWithShader(replacementShader, replacementTag);

            //calculate Overdraw every second
            //Debug.Log("DeviceInfo: " + SystemInfo.graphicsDeviceType + " " + SystemInfo.graphicsDeviceName + " "
            //         + SystemInfo.graphicsDeviceVersion + SystemInfo.graphicsShaderLevel + " " + SystemInfo.supportsComputeShaders);

            finishCounter = false;
            StartCoroutine(CalculateOverdraw());
            _targetCam.clearFlags = originalClearFlags;
            _targetCam.backgroundColor = originalClearColor;
            _targetCam.targetTexture = originalTargetTexture;
            _targetCam.enabled = originalCamEnabled;
            //end the measurement
        }
    }
    
    IEnumerator CalculateOverdraw()
    {
        RenderTexture.active = overdrawTexture;
        readingTexture.ReadPixels(readingRect, 0, 0);
        readingTexture.Apply();
        RenderTexture.active = null;
        yield return null;
        //setting up the data
        replacementColors = readingTexture.GetRawTextureData<Color>();
        int totalSize = replacementColors.Length;
        int FramePerRender = frameBreakSize;
        TotalG = 0f;
        float oneDrawTimeG = 0.04f;
        //fetching result
        for (var i = 0; i < totalSize; i++)
        {
            if (replacementColors[i].g <= oneDrawTimeG)
            {
                TotalG += oneDrawTimeG;
            }
            else
            {
                TotalG += replacementColors[i].g;
            }
            if (i == FramePerRender)
            {
                FramePerRender += frameBreakSize;
                yield return null;
            }
        }
        //lets not consider all black situation first
        OverdrawRatio = Convert.ToInt32(TotalG / oneDrawTimeG) / (float) totalSize;

        //Debug.Log(_camName + " OverdrawRatio: " + OverdrawRatio);
        //capture the snapshot
        byte[] byteSnapshot = readingTexture.EncodeToJPG();
        //sending the data
        //1.OverdrawRate 2.CurrentTime 3.Snapshot
        int totalLength = 0;
        byte[] byteOverdraw = UPOTools.ConvertLittleEndian(OverdrawRatio);
        byte[] byteCam = UPOTools.DeserializeString(_camName);
        long currentTime= DateTimeOffset.Now.ToUnixTimeMilliseconds();
        byte[] byteTime = UPOTools.ConvertLittleEndian(currentTime);
        byte[] result = new byte[byteCam.Length + byteTime.Length + byteOverdraw.Length+byteSnapshot.Length ];
        System.Buffer.BlockCopy(byteCam, 0, result, 0, byteCam.Length);
        System.Buffer.BlockCopy(byteTime, 0, result, totalLength+=byteCam.Length, byteTime.Length);
        System.Buffer.BlockCopy(byteOverdraw, 0, result, totalLength+=byteTime.Length, byteOverdraw.Length);
        System.Buffer.BlockCopy(byteSnapshot, 0, result, totalLength += byteOverdraw.Length, byteSnapshot.Length);
        //send it;
        NetworkServer.SendMessage(UPOMessageType.Overdraw, result);
        // string fileName = currentTime.ToString() + ".jpg";
        // BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create));
        // writer.Write(byteSnapshot);
        // writer.Close();
        //yield return StartCoroutine(CalculateOverdraw());
        finishCounter = true;
    }
}
#endif
