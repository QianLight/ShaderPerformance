#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [System.Serializable]
    public class DynamicSceneData : ScriptableObject
    {
        public List<GameObjectGroupData> groups = new List<GameObjectGroupData> ();
        public List<DynamicScene> dynamicScenes = new List<DynamicScene> ();
        public void Clear ()
        {
            groups.Clear();
            dynamicScenes.Clear ();
        }
    }
}
#endif