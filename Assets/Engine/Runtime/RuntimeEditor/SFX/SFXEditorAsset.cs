#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
using System.Text;
using UnityEngine.CFUI;
#endif
using UnityObject = UnityEngine.Object;
namespace CFEngine
{

    public class SFXEditorAsset : ScriptableObject
    {
        public GameObject srcSFX;
        [NonSerialized]
        public List<SFXComponent> components = new List<SFXComponent> ();
    }
}
#endif