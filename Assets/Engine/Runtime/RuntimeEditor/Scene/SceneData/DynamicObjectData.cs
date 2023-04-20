#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    public enum DynamicObjectType
    {
        None,
        Dummy,
        Spawn,
        Terminal,
        Transfer,
        Circle,
    }

    [System.Serializable]
    public class DynamicScene
    {
        public int parentID = -1; //Root
        public string dynamicSceneName = "";       
        public List<SceneDynamicObject> dynamicObjects = new List<SceneDynamicObject> ();
    }
}
#endif