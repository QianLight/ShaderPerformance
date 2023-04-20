//#define MRO_ON
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public interface ISceneAnim
    {
        void SetAnim(string exString, uint flag);
        void SetUVOffset(ref Vector2 offset);
    }
    
    public class ObjectCache
    {
        #if MRO_ON
        public ISceneAnim sceneAnim;
        #endif
        public Transform t;
        public Renderer r;
    }

}