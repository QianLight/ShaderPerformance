using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace UsingTheirs.ShaderHotSwap
{

    [RequireComponent(typeof(ServerHttpJsonPost))]
    public class ServerShaderHotSwap : MonoBehaviour
    {
        ServerHttpJsonPost httpServer;

        public static string toggleshow = "toggleshow";
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);

            httpServer = GetComponent<ServerHttpJsonPost>();
            httpServer.AddHandler("/swapShaders", HandlerSwapShaders.HandlerMain);
            httpServer.AddHandler("/queryEnv", HandlerQueryEnv.HandlerMain);
            httpServer.AddHandler("/", HandlerConnectionTest);
            httpServer.AddHandler("/"+toggleshow, HandlerToggleShow.HandlerMain);
        }

        void OnDestroy()
        {
            httpServer.RemoveHandler("/swapShaders");
            httpServer.RemoveHandler("/queryEnv");
            httpServer.RemoveHandler("/" + toggleshow);
            httpServer.RemoveHandler("/");
        }

        string HandlerConnectionTest( string jsonRequest )
        {
            return "Welcome to Shader HotSwap!";
        }

    }

}
