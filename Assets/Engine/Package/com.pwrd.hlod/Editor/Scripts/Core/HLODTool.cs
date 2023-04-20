using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace com.pwrd.hlod.editor
{
    public static class HLODTool
    {
        [MenuItem("Assets/Athena/HLOD/打印选中文件夹中预制体引用的Mesh内存")]
        public static string GetHLODMemory()
        {
            if (Selection.activeObject)
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                {
                    path = Path.GetDirectoryName(path);
                }
                var directoryPath = path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                if (!string.IsNullOrEmpty(path) && Directory.Exists(directoryPath))
                {
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { directoryPath });
                    float memorySize = 0;
                    for (var i = 0; i < guids.Length; i++)
                    {
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                        var mfs = obj.GetComponentsInChildren<MeshFilter>();
                        if (mfs != null)
                        {
                            foreach (var mf in mfs)
                            {
                                if (mf && mf.sharedMesh) memorySize += Profiler.GetRuntimeMemorySizeLong(mf.sharedMesh);
                            }
                        }
                    }
                    var memory = GetItemSizeContent(memorySize);
                    HLODDebug.Log(memory);
                    return memory;
                }
            }
            return "0";
        }
        
        public static string GetRelativePath(string fullPath)
        {
            return fullPath.Substring(Application.dataPath.Length - 6);
        }
        
        internal static GameObject GetObj(this string name)
        {
            GameObject obj = FindObj(name);
            return obj;
        }

        internal static string GetObjNamePath(this GameObject obj)
        {
            var name = obj?.GetFullName();
            return name;
        }
        
        internal static GameObject[] GetObjs(this string[] names)
        {
            if (names == null) return null;
            GameObject[] objs = new GameObject[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                objs[i] = FindObj(names[i]);
            }
            return objs;
        }
        
        internal static string[] GetObjNamePaths(this GameObject[] objs)
        {
            if (objs == null) return null;
            string[] names = new string[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                names[i] = objs[i]?.GetFullName();
            }
            return names;
        }
        
        private static string GetFullName(this GameObject obj)
        {
            if (obj == null) return "";
            try
            {
                var test = obj.name;
            }
            catch
            {
                return "";
            }
            
            var name = obj.name;
            while (obj.transform.parent != null)
            {
                name = obj.transform.parent.name + "/" + name;
                obj = obj.transform.parent.gameObject;
            }
            return name;
        }
        
        private static string GetFullNameByIndex(this GameObject obj)
        {
            if (obj == null) return "";
            try
            {
                var test = obj.name;
            }
            catch
            {
                return "";
            }
            
            var name = obj.name + GetNameIndex(obj);
            while (obj.transform.parent != null)
            {
                name = obj.transform.parent.name + "/" + name;
                obj = obj.transform.parent.gameObject;
            }
            return name;
        }

        private static GameObject FindObj(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var obj = GameObject.Find(name);
            return null;
        }
        
        private static GameObject FindObjByIndex(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var nameArr = name.Split('~');
            var fullname = nameArr[0];
            var index = 0;
            if (nameArr.Length == 2) index = int.Parse(nameArr[1]);
            var obj = GameObject.Find(fullname);
            if (index <= 0) return obj;
            try
            {
                var parent = obj.transform.parent;
                if (parent)
                {
                    List<GameObject> children = new List<GameObject>();
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        if (parent.GetChild(i).name.Equals(obj.name))
                        {
                            children.Add(parent.GetChild(i).gameObject);
                        }
                    }

                    return children[index];
                }
            }
            catch
            {

            }

            return null;
        }

        private static string GetNameIndex(GameObject obj)
        {
            int index = 0;
            var parent = obj.transform.parent;
            if (parent)
            {
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < parent.childCount; i++)
                {
                    if (parent.GetChild(i).name.Equals(obj.name))
                    {
                        children.Add(parent.GetChild(i).gameObject);
                    }
                }
                index = children.FindIndex(s => s.Equals(obj));
            }
            return "~" + index;
        }
        
        private static string GetItemSizeContent(float size)
        {
            if (size < 1024)
                return size.ToString("0B");
            else if (size < 1024 * 1024f)
                return (size / 1024).ToString("0.0KB");
            else if (size < 1024 * 1024 * 1024f)
                return (size / (1024 * 1024f)).ToString("0.0MB");
            return "-";
        }
    }
}