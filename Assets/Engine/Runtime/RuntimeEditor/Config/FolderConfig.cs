#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public interface IFolderHash
    {
        string GetHash ();
    }

    [Serializable]
    public class BaseFolderHash : IFolderHash
    {
        public string hash = "";
        public string GetHash ()
        {
            if (string.IsNullOrEmpty (hash))
            {
                hash = FolderConfig.Hash ();
            }
            return hash;
        }
    }

    [Serializable]
    public class FolderState
    {
        public string path = "";
        public bool folder;
    }

    [Serializable]
    public class FolderConfig : ScriptableObject
    {
        public List<FolderState> folderStates = new List<FolderState> ();
        private Dictionary<string, FolderState> folderMap = new Dictionary<string, FolderState> ();

        public static string Hash ()
        {
            return Guid.NewGuid ().ToString ();
        }
        private void Init ()
        {
            folderMap.Clear ();
            foreach (var fs in folderStates)
            {
                if (!string.IsNullOrEmpty (fs.path))
                {
                    folderMap[fs.path] = fs;
                }
            }
        }

        public static FolderConfig Load (string name, string dir = "")
        {
            string path = "";
            if (string.IsNullOrEmpty (dir))
            {
                path = string.Format ("Assets/Engine/Editor/EditorResources/{0}.asset", name);
            }
            else
            {
                path = string.Format ("{0}/{1}.asset", dir, name);
            }
            FolderConfig fc = EditorCommon.LoadAsset<FolderConfig> (path);
            fc.Init ();
            return fc;
        }

        public void Save (string name, string dir = "")
        {
            folderStates.Clear ();
            foreach (var fs in folderMap.Values)
            {
                folderStates.Add (fs);
            }
            string path = "";
            if (string.IsNullOrEmpty (dir))
            {
                path = string.Format ("Assets/Engine/Editor/EditorResources/{0}.asset", name);
            }
            else
            {
                path = string.Format ("{0}/{1}.asset", dir, name);
            }
            EditorCommon.SaveAsset (path, this);
        }

        public bool IsFolder (string path)
        {
            if (path != null)
            {
                FolderState fs;
                if (folderMap.TryGetValue (path, out fs))
                {
                    return fs.folder;
                }
            }

            return false;
        }

        public void SetFolder (string path, bool folder)
        {
            if (path != null)
            {
                FolderState fs;
                if (!folderMap.TryGetValue (path, out fs))
                {
                    fs = new FolderState ();
                    fs.path = path;
                    folderMap.Add (path, fs);
                }
                fs.folder = folder;
            }

        }

        public bool Folder (string path, string name)
        {
            bool folder = IsFolder (path);
            folder = EditorGUILayout.Foldout (folder, name);
            SetFolder (path, folder);
            return folder;
        }
        public bool FolderGroup (string path, string name, float width, float x = 0)
        {
            bool folder = IsFolder (path);
            EditorCommon.BeginFolderGroup (name, ref folder, width, 100, x);
            SetFolder (path, folder);
            return folder;
        }
    }

    [CustomEditor (typeof (FolderConfig))]
    public partial class FolderConfigEdit : UnityEditor.Editor
    {
        public override void OnInspectorGUI () { }

    }
}
#endif