using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace UsingTheirs.ShaderHotSwap
{

    public class ServerHttpJsonPost : MonoBehaviour
    {
        // Return: jsonResponse
        public delegate string Handler(string jsonRequest);

        public short listenPort = 8090;

        // Key: request path
        Dictionary<string, Handler> handlers = new Dictionary<string, Handler>();

        HttpListener httpListener;

        Queue<Action> mainThreadJobs = new Queue<Action>();

        public void AddHandler(string urlPath, Handler handler)
        {
            handlers[urlPath] = handler;
        }

        public void RemoveHandler(string urlPath)
        {
            handlers.Remove(urlPath);
        }

        void Start()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://*:" + listenPort + "/");

            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);

            Debug.Log("[HttpJsonPostServer] Listening " + listenPort + " ...");
            MemoryLogger.Log("[HttpJsonPostServer] Listening " + listenPort + " ...");
        }

        void OnDestroy()
        {
            if (httpListener != null)
            {
                httpListener.Close();
                httpListener = null;

                MemoryLogger.Log("[HttpJsonPostServer] Stop Listening " + listenPort + " ...");
            }
        }

        void Update()
        {
            // One job a frame
            TryRunAMainThreadJob();
        }

        void TryRunAMainThreadJob()
        {
            Action job = null;
            if (mainThreadJobs.Count > 0)
            {
                lock (mainThreadJobs)
                {
                    if (mainThreadJobs.Count > 0)
                        job = mainThreadJobs.Dequeue();
                }
            }

            if (job != null)
                job();
        }

        void SendMainThreadJob(Action job)
        {
            lock (mainThreadJobs)
            {
                mainThreadJobs.Enqueue(job);
            }
        }


        void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);

            HttpListenerRequest req = context.Request;
            HttpListenerResponse res = context.Response;

            MemoryLogger.Log("[HttpJsonPostServer] req.Url.AbsolutePath = " + req.Url.AbsolutePath);

            Handler handler;
            if (handlers.TryGetValue(req.Url.AbsolutePath, out handler))
            {
                HandleRequest(req, res, handler);
            }
            else
            {
                HandleError(req, res);
            }

            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
        }

        void HandleRequest(HttpListenerRequest req, HttpListenerResponse res, Handler handler)
        {
            string jsonRequest = new System.IO.StreamReader(req.InputStream).ReadToEnd();
            jsonRequest = WWW.UnEscapeURL(jsonRequest);
            SendMainThreadJob(() =>
           {
               var jsonResponse = handler(jsonRequest);

               MemoryLogger.Log("[HttpJsonPostServer] url:{0}, res:{1}", req.Url.AbsolutePath, jsonResponse);

               byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
               res.ContentLength64 = buffer.Length;
               res.OutputStream.Write(buffer, 0, buffer.Length);
               res.Close();
           });
        }

        void HandleError(HttpListenerRequest req, HttpListenerResponse res)
        {
            res.StatusCode = 404;
            res.Close();
        }
    }

}
