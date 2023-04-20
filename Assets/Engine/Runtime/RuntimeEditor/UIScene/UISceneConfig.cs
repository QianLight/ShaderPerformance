#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{

    public class UISceneConfig : ScriptableObject
    {
        public List<UIScene> uiSceneList = new List<UIScene>();
    }

}
#endif