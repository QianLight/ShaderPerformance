using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using CFClient;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class FPDebugHandle : MonoBehaviour
{
    private static FPDebugHandle insatance;
    public static FPDebugHandle Instance
    {
        get
        {
            if (insatance == null)
            {
                GameObject go = new GameObject("FPDebug");
                insatance = go.AddComponent<FPDebugHandle>();
                DontDestroyOnLoad(go);

            }       
            return insatance;
        }

    }

    public void SetFpDebugActive(bool active)
    {
        this.enabled = active;
        if (active)
        {
            serverStart();
        }
        else
        {
            serverStop();
        }
    }
    
    private void Awake()
    {
        
    }
    public bool Deprecated { get; set; }

    public long fps = 0, minFps, maxFps = 0;
    void Update()
    {

        fps = (long)(1.0f / Time.deltaTime);
        if (reqList.Count > 0)
        {
            ThreadTask task = reqList.Dequeue();
            doCmd(task);
        }

        if(minFps > fps)
        {
            minFps = fps;
        }
        if (maxFps < fps)
        {
            maxFps = fps;
        }
    }
    void OnEnable()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        FPGameObject.InstanceID = gameObject.GetInstanceID();
        FPGameObject.CurrentScene = gameObject.scene;
#if UNITY_EDITOR
        serverStart();
#endif
    }
    void OnDisable()
    {
        serverStop();
    }

    private AssetBundle lastAssetBundle = null;
    private HttpListener httpListener = null;
    //private Thread serverThread = null;
    private bool run = false;
    private Queue<ThreadTask> reqList = new Queue<ThreadTask>();
    //void serverStart()
    //{
    //    if (run)
    //        return;
    //    httpListener = new HttpListener();
    //    httpListener.Prefixes.Add("http://+:8820/");
    //    httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
    //    httpListener.Start();
    //    Debug.Log("listen at 8820");
    //    run = true;
    //    serverThread = new Thread(serverThreadRun);
    //    serverThread.Start();
    //}
    //void serverThreadRun()
    //{
    //    while (run)
    //    {
    //        var context = httpListener.GetContext();
    //        ThreadTask task = new ThreadTask();
    //        HttpListenerRequest request = context.Request;
    //        HttpListenerResponse response = context.Response;

    //        response.ContentEncoding = Encoding.UTF8;
    //        if(request.HttpMethod == "POST")
    //        {
    //            task.Data = new byte[request.ContentLength64];
    //            using (Stream st = request.InputStream)
    //            {
    //                //st.Read(task.Data, 0, task.Data.Length);
    //                int index = 0;
    //                while(true)
    //                {
    //                    int b = request.InputStream.ReadByte();
    //                    if (b < 0)
    //                        break;
    //                    task.Data[index] = (byte)b;
    //                    index++;
    //                }
    //                st.Close();
    //            }

    //            //using(StreamReader input = new StreamReader(request.InputStream, request.ContentEncoding))
    //            //{
    //            //    Debug.Log(input.ReadToEnd());
    //            //}
    //        }

    //        task.Para = request.QueryString;
    //        task.Output = response.OutputStream;
    //        reqList.Enqueue(task);
    //    }
    //}

    void serverStart()
    {
        if (run)
            return;
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://+:8820/");
        httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

        try
        {
            httpListener.Start();
            Debug.Log("listen at 8820");
            run = true;
            httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), null);
        }
        catch (Exception ex)
        {
            if (run)
            {
                //Debug.LogError(ex);
            }
        }

        //serverThread = new Thread(serverThreadRun);
        //serverThread.Start();
    }
    void GetContextCallBack(IAsyncResult ar)
    {
        try
        {
            HttpListenerContext context = httpListener.EndGetContext(ar);
            if(!run)
            {
                return;
            }
            httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), null);

            ThreadTask task = new ThreadTask();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            response.ContentEncoding = Encoding.UTF8;
            if (request.HttpMethod == "POST")
            {
                task.Data = new byte[request.ContentLength64];
                using (Stream st = request.InputStream)
                {
                    //st.Read(task.Data, 0, task.Data.Length);
                    int index = 0;
                    while (true)
                    {
                        int b = request.InputStream.ReadByte();
                        if (b < 0)
                            break;
                        task.Data[index] = (byte)b;
                        index++;
                    }
                    st.Close();
                }

                //using(StreamReader input = new StreamReader(request.InputStream, request.ContentEncoding))
                //{
                //    Debug.Log(input.ReadToEnd());
                //}
            }

            task.Para = request.QueryString;
            task.Output = response.OutputStream;
            reqList.Enqueue(task);
        }
        catch (Exception ex)
        {
            if (run)
            {
                //Debug.LogError(ex);
            }
        }
    }

    void serverStop()
    {
        run = false;
        try
        {
            if (httpListener != null)
            {
                httpListener.Stop();
                httpListener = null;
            }
        }
        catch(Exception ex)
        {
            //Debug.LogError(ex);
        }
        finally
        {
            //if (serverThread != null)
            //{
            //    serverThread.Abort();
            //    serverThread = null;
            //}
        }
    }


}