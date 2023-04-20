using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;
using System.Collections.Generic;

namespace AdventureEditor
{
    [CreateAssetMenu(fileName = "AdventureEditorConfig", menuName = "Level/AdventureEditorConfig", order = 1)]
    public class AdventureEditorConfig : ScriptableObject
    {
        public float ClickOffsetY = 0.5f;
        public int WatchDistance = 1;
        public float CreateRotateY = 180f;
    }
}
