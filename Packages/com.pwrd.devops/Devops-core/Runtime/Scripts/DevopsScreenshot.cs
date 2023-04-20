using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace Devops.Core
{
    class BmpFileHeader
    {
        public ushort bfType = 0x4D42;
        public uint bfSize = 0;
        public ushort bfReserved1 = 0;
        public ushort bfReserved2 = 0;
        public uint bfOffBits = 54;
    }
    // 定义 BMP 信息头结构体。
    class BmpInfoHeader
    {
        public uint biSize = 40;
        public int biWidth = 0;
        public int biHeight = 0;
        public ushort biPlanes = 1;
        public ushort biBitCount = 32;
        public uint biCompression = 0;
        public uint biSizeImage = 0;
        public int biXPelsPerMeter = 0;
        public int biYPelsPerMeter = 0;
        public uint biClrUsed = 0;
        public uint biClrImportant = 0;
    }
    public class ScreenshotInfo
    {
        public string textureName = string.Empty;
        public float executionTime = 0.0f;
        public float getTime = 0.0f;
        public bool sendToServer = false;
        public bool hasSendToServer = false;
        public List<string> sendPaths = new List<string>();
        public Texture2D texture = null;
        public byte[] jpgBuffer = null;
        public byte[] bmpBuffer = null;
        public string base64 = string.Empty;
        public int screenshotWidth = 0;
        public int screenshotHeight = 0;
        static BmpFileHeader bmpFileHeader = new BmpFileHeader();
        static BmpInfoHeader bmpInfoHeader = new BmpInfoHeader();
        public bool HasGet()
        {
            return texture != null || jpgBuffer != null || bmpBuffer != null || !string.IsNullOrEmpty(base64);
        }

        public void SetSendToServer(bool send)
        {
            sendToServer |= send;
        }

        public Texture2D GetTexture()
        {
            if (texture != null)
                return texture;
            if (GetJPGBuffer() != null)
            {
                texture = new Texture2D(10, 10);
                texture.LoadImage(GetJPGBuffer());
            }
            return texture;
        }
        public byte[] GetJPGBuffer()
        {
            if (jpgBuffer != null)
            {
                return jpgBuffer;
            }
            if (texture != null)
            {
                jpgBuffer = texture.EncodeToJPG();
            }
            else if (base64 != null)
            {
                jpgBuffer = Convert.FromBase64String(base64);
            }
            return jpgBuffer;
        }

        public void SetJPGBuffer(byte[] buffer)
        {
            jpgBuffer = buffer;
        }

        public byte[] GetBMPBuffer()
        {
            return bmpBuffer;
        }

        public void SetColorBuffer(byte[] buffer)
        {
            // 计算图像数据大小
            int imageSize = buffer.Length;
            // 计算bmp文件的大小
            int fileSize = 14 + 40 + imageSize;
            bmpFileHeader.bfSize = (uint)fileSize;
            bmpInfoHeader.biWidth = screenshotWidth;
            bmpInfoHeader.biHeight = -screenshotHeight;
            bmpInfoHeader.biSizeImage = (uint)imageSize;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(bmpFileHeader.bfType);
                    writer.Write(bmpFileHeader.bfSize);
                    writer.Write(bmpFileHeader.bfReserved1);
                    writer.Write(bmpFileHeader.bfReserved2);
                    writer.Write(bmpFileHeader.bfOffBits);
                    // 写入 BMP 信息头。
                    writer.Write(bmpInfoHeader.biSize);
                    writer.Write(bmpInfoHeader.biWidth);
                    writer.Write(bmpInfoHeader.biHeight);
                    writer.Write(bmpInfoHeader.biPlanes);
                    writer.Write(bmpInfoHeader.biBitCount);
                    writer.Write(bmpInfoHeader.biCompression);
                    writer.Write(bmpInfoHeader.biSizeImage);
                    writer.Write(bmpInfoHeader.biXPelsPerMeter);
                    writer.Write(bmpInfoHeader.biYPelsPerMeter);
                    writer.Write(bmpInfoHeader.biClrUsed);
                    writer.Write(bmpInfoHeader.biClrImportant);

                    writer.Write(buffer);
                }
                bmpBuffer = stream.ToArray();
            }
        }

        public string GetBase64()
        {
            if (!string.IsNullOrEmpty(base64))
            {
                return base64;
            }
            if (bmpBuffer != null)
            {
                base64 = Convert.ToBase64String(bmpBuffer);
            }
            else
            {
                base64 = Convert.ToBase64String(GetJPGBuffer());
            }
            return base64;
        }

        public void Destroy()
        {
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
            }
        }
    }
    public class DevopsScreenshot : MonoBehaviour
    {
        static DevopsScreenshot instance;
        public static DevopsScreenshot Instance()
        {
            if (instance == null)
            {
                GameObject gObject = new GameObject("DevopsScreenshot");
                gObject.AddComponent<DevopsScreenshot>();
                DontDestroyOnLoad(gObject);
            }
            return instance;
        }

#if UNITY_ANDROID
        AndroidJavaClass capUtils = null;
#elif UNITY_IOS
        [DllImport("__Internal")]
        private static extern string sreenShot();
        [DllImport("__Internal")]
        private static extern void screenShotAsync();
#endif
        const float ScreenShotCD = 0.9f;
        ScreenshotInfo lastScreenshotInfo = null;
        bool needSendScreenShotInfo = false;
        Action<ScreenshotInfo> EventGetScreenShot;
        AndroidJavaObject ajo;
        int TextureIndex = 0;
        NativeArray<byte> renderTextureNativeArrayData;
        NativeArray<Color32> renderTextureNativeArrayColorData;
        byte[] textureBuffer;
        Color32[] textureColorBuffer;

        List<ScreenshotInfo> ThreadSendBufferList = new List<ScreenshotInfo>();

        private async void Awake()
        {
            instance = this;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                ajo = ajc.GetStatic<AndroidJavaObject>("currentActivity");
                StartActivity("com.pwrd.upsdk.PwrdActivity");
                capUtils = new AndroidJavaClass("com.pwrd.upsdk.CaptureUtils");
            }
#endif
            renderTextureNativeArrayData = new NativeArray<byte>(Screen.width * Screen.height * 4, Allocator.Persistent);
            renderTextureNativeArrayColorData = new NativeArray<Color32>(Screen.width * Screen.height, Allocator.Persistent);
            textureBuffer = new byte[Screen.width * Screen.height * 4];
            textureColorBuffer = new Color32[Screen.width * Screen.height];
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        }

        void Start()
        {

        }

        private void OnDestroy()
        {
            if (renderTextureNativeArrayData != null)
            {
                renderTextureNativeArrayData.Dispose();
            }
            if (renderTextureNativeArrayColorData != null)
            {
                renderTextureNativeArrayColorData.Dispose();
            }
            if (renderTexture != null)
            {
                UnityEngine.GameObject.Destroy(renderTexture);
                renderTexture = null;
            }
        }

        private void Update()
        {
            if (needSendScreenShotInfo)
            {
                needSendScreenShotInfo = false;
                SendEvent();
            }
        }

        private RenderTexture renderTexture;
        IEnumerator GetTextureAsyncGPU()
        {
            yield return new WaitForEndOfFrame();
            lastScreenshotInfo.screenshotWidth = Screen.width;
            lastScreenshotInfo.screenshotHeight = Screen.height;
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            }
            else
            {
                if (renderTexture.width != Screen.width)
                {
                    renderTexture.width = Screen.width;
                }
                if (renderTexture.height != Screen.height)
                {
                    renderTexture.height = Screen.height;
                }
            }
            ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture);
            if (renderTextureNativeArrayData.Length != Screen.width * Screen.height * 4)
            {
                renderTextureNativeArrayData = new NativeArray<byte>(Screen.width * Screen.height * 4, Allocator.Persistent);
            }
            if (renderTextureNativeArrayColorData.Length != Screen.width * Screen.height)
            {
                renderTextureNativeArrayColorData = new NativeArray<Color32>(Screen.width * Screen.height, Allocator.Persistent);
            }
            /*
            AsyncGPUReadbackRequest gpuRequest = AsyncGPUReadback.RequestIntoNativeArray(ref renderTextureNativeArrayData, renderTexture, 0, TextureFormat.ARGB32);
            while (!gpuRequest.done)
            {
                yield return null;
            }
            if (textureBuffer.Length != renderTextureNativeArrayData.Length)
                textureBuffer = new byte[renderTextureNativeArrayData.Length];
            renderTextureNativeArrayData.CopyTo(textureBuffer);
            */
            AsyncGPUReadbackRequest gpuRequest = AsyncGPUReadback.RequestIntoNativeArray(ref renderTextureNativeArrayColorData, renderTexture, 0, TextureFormat.RGBA32);
            while (!gpuRequest.done)
            {
                yield return null;
            }
            if (textureColorBuffer.Length != renderTextureNativeArrayColorData.Length)
                textureColorBuffer = new Color32[renderTextureNativeArrayColorData.Length];
            renderTextureNativeArrayColorData.CopyTo(textureColorBuffer);
            JPGEncoder jpgEncoder = new JPGEncoder(lastScreenshotInfo.screenshotWidth, lastScreenshotInfo.screenshotHeight, textureColorBuffer, 0.5f, 100, "");
            while (!jpgEncoder.isDone)
                yield return null;
            lastScreenshotInfo.SetJPGBuffer(jpgEncoder.GetBytes());

            needSendScreenShotInfo = true;
            //Task.Run(() =>
            //{
            //    //SwapChannel(textureBuffer);
            //    lastScreenshotInfo.SetColorBuffer(textureBuffer);
            //    while (true)
            //    {
            //        if (jpgEncoder.isDone)
            //        {
            //            break;
            //        }
            //    }
            //    needSendScreenShotInfo = true;
            //});
        }

        public static byte[] ConvertColor32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        System.IntPtr ToIntPtr(NativeArray<byte> array)
        {
            unsafe
            {
                return (System.IntPtr)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(array);
            }
        }

        void SwapBuffer(byte[] data, int width, int height)
        {
            unsafe
            {
                fixed (byte* pData = data)
                {
                    int pixelSize = 4;
                    int rowSize = width * pixelSize;

                    byte* row = pData;
                    byte* lastRow = pData + (height - 1) * rowSize;

                    for (int i = 0; i < height / 2; i++)
                    {
                        SwapRows(row, lastRow, rowSize);
                        row += rowSize;
                        lastRow -= rowSize;
                    }
                }
            }
        }

        // argb to bgr
        void SwapChannel(byte[] data)
        {
            for (int i = 0; i < data.Length; i += 4)
            {
                byte temp = data[i + 1];
                data[i] = data[i + 3];
                data[i + 1] = data[i + 2];
                data[i + 2] = temp;
                data[i + 3] = 0;
            }
        }

        unsafe void SwapRows(byte* row1, byte* row2, int rowSize)
        {
            for (int i = 0; i < rowSize; i++)
            {
                byte temp = row1[i];
                row1[i] = row2[i];
                row2[i] = temp;
            }
        }

        IEnumerator GetTexture()
        {
            lastScreenshotInfo.screenshotWidth = Screen.width;
            lastScreenshotInfo.screenshotHeight = Screen.height;

            if (Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.WindowsEditor)
            {
                yield return new WaitForEndOfFrame();

                lastScreenshotInfo.texture = ScreenCapture.CaptureScreenshotAsTexture(0);
                lastScreenshotInfo.texture.Apply();
                SendEvent();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
#if UNITY_ANDROID
                if (capUtils != null)
                {
                    capUtils.CallStatic("takeCapture");
                }
#endif
                yield return null;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                //lastScreenshotInfo.base64 = sreenShot();
                screenShotAsync();
#endif
                yield return null;
            }
            else { }
        }

        public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
            float incX = 1.0f / targetWidth;
            float incY = 1.0f / targetHeight;
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    Color color = source.GetPixelBilinear((float)x / (float)targetWidth, (float)y / (float)targetHeight); ;
                    result.SetPixel(x, y, color);
                }
            }
            result.Apply();
            return result;
        }

        void ReceiveImageMethod(string imageAbsolutePath)
        {
            if (File.Exists(imageAbsolutePath))
            {
                FileStream fs = new FileStream(imageAbsolutePath, FileMode.Open);
                lastScreenshotInfo.jpgBuffer = new byte[fs.Length];
                fs.Read(lastScreenshotInfo.jpgBuffer, 0, lastScreenshotInfo.jpgBuffer.Length);
                fs.Close();
                fs.Dispose();
                SendEvent();
            }
        }

        void ReceiveImageBase64Method(string base64)
        {
            lastScreenshotInfo.base64 = base64;
            SendEvent();
        }

        void SendEvent()
        {
            if (lastScreenshotInfo == null)
                return;
            if (!lastScreenshotInfo.HasGet())
                return;
            if (lastScreenshotInfo.sendToServer/* && !lastScreenshotInfo.hasSendToServer*/)
            {
                SendScreenshotToServer();
            }
            EventGetScreenShot?.Invoke(lastScreenshotInfo);
            EventGetScreenShot = null;
        }

        private static bool SkipServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            return true;
        }

        async void SendScreenshotToServer()
        {
            string sendServerUrl = await DevopsInfoSettings.Instance().GetDiskUploadURL();
            lastScreenshotInfo.hasSendToServer = true;
            string savePathParam = string.Join(",", lastScreenshotInfo.sendPaths);
            string url = $"{sendServerUrl}/devops/image/storage/upload?path={savePathParam}";
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Headers.Add("ContentType", "multipart/form-data");
                    byte[] bmpBuffer = lastScreenshotInfo.GetBMPBuffer();
                    byte[] jpgBuffer = lastScreenshotInfo.GetJPGBuffer();
                    if (bmpBuffer != null)
                    {
                        var byteContent = new ByteArrayContent(bmpBuffer);
                        content.Add(byteContent, "image", lastScreenshotInfo.textureName);
                    }
                    else if (jpgBuffer != null)
                    {
                        var byteContent = new ByteArrayContent(jpgBuffer);
                        content.Add(byteContent, "image", lastScreenshotInfo.textureName);
                    }
                    else if (!string.IsNullOrEmpty(lastScreenshotInfo.base64))
                    {
                        var stringContent = new StringContent(lastScreenshotInfo.GetBase64());
                        content.Add(stringContent, "base64Image", lastScreenshotInfo.textureName);
                    }
                    else
                    {
                        Debug.LogError("send imagebuffer is null");
                    }
                    Debug.Log($"url:{url}");
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public string GetScreenShot(bool send, string sendPath, Action<ScreenshotInfo> action)
        {
            EventGetScreenShot += action;
            if (lastScreenshotInfo != null
                && Time.realtimeSinceStartup - lastScreenshotInfo.executionTime < ScreenShotCD
                && lastScreenshotInfo.HasGet())
            {
                if (!lastScreenshotInfo.sendPaths.Contains(sendPath))
                    lastScreenshotInfo.sendPaths.Add(sendPath);
                lastScreenshotInfo.SetSendToServer(send);
                SendEvent();
                return lastScreenshotInfo.textureName;
            }
            if (lastScreenshotInfo != null && !lastScreenshotInfo.HasGet())
            {
                if (!lastScreenshotInfo.sendPaths.Contains(sendPath))
                    lastScreenshotInfo.sendPaths.Add(sendPath);
                lastScreenshotInfo.SetSendToServer(send);
                return lastScreenshotInfo.textureName;
            }
            if (lastScreenshotInfo != null)
            {
                lastScreenshotInfo.Destroy();
            }
            lastScreenshotInfo = new ScreenshotInfo()
            {
                textureName = $"{TextureIndex++}_{GetCurrentTimeStamp()}.jpg".ToString(),
                executionTime = Time.realtimeSinceStartup,
                sendToServer = send,
            };
            lastScreenshotInfo.sendPaths.Add(sendPath);
            if (Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (SystemInfo.supportsAsyncGPUReadback)
                {
                    StartCoroutine(GetTextureAsyncGPU());
                }
                else
                {
                    StartCoroutine(GetTexture());
                }
            }
            else
            {
                StartCoroutine(GetTexture());
            }
            return lastScreenshotInfo.textureName;
        }

        string GetCurrentTimeStamp()
        {
            TimeSpan timeSpan = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
            return Math.Round(timeSpan.TotalMilliseconds).ToString();
        }

        bool TryGetLastScreenShotInMemory(ref string name)
        {
            if (lastScreenshotInfo == null)
                return false;
            if (!lastScreenshotInfo.HasGet())
            {
                name = lastScreenshotInfo.textureName;
                return true;
            }
            if (lastScreenshotInfo.executionTime - Time.realtimeSinceStartup <= ScreenShotCD)
            {
                name = lastScreenshotInfo.textureName;
                return true;
            }
            return false;
        }

        public void StartActivity(string activityName)
        {
#if UNITY_ANDROID
            var intentObj = new AndroidJavaObject("android.content.Intent");
            var activityCls = new AndroidJavaClass(activityName);
            intentObj.Call<AndroidJavaObject>("setClass", ajo, activityCls);

            ajo.Call("startActivity", intentObj);
#endif
        }
    }
}