#if UNITY_EDITOR
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class AssetsPath
    {

        public static bool GetCreatureFolderName (string path, out string folderName)
        {
            folderName = "";
            path = path.Substring (AssetsConfig.instance.Creature_Path.Length + 1);
            int index = path.IndexOf ("/");
            if (index >= 0)
            {
                folderName = path.Substring (0, index);
                return true;
            }
            return false;
        }
        public static string GetParentFolderName (string path)
        {
            string folder = Path.GetDirectoryName (path);
            folder = folder.Replace ("\\", "/");
            int index = folder.LastIndexOf ("/");
            if (index >= 0)
            {
                folder = folder.Substring (index + 1);
                return folder;
            }
            return "";
        }

        public static bool GetFileName (string path, out string fileName)
        {
            fileName = "";
            int index = path.LastIndexOf (".");
            if (index > 0)
            {
                path = path.Substring (0, index);
            }
            index = path.LastIndexOf ("/");
            if (index > 0)
            {
                fileName = path.Substring (index + 1);
            }
            else
            {
                fileName = path;
            }
            return true;
        }
        public static string GetPath (string path, out string ext)
        {
            ext = "";
            if (path.StartsWith (AssetsConfig.instance.ResourcePath))
            {
                path = path.Substring (AssetsConfig.instance.ResourcePath.Length + 1);
                int index = path.LastIndexOf (".");
                if (index >= 0)
                {
                    ext = path.Substring (index);
                    return path.Substring (0, index);
                }
                return path;
            }
            return "";
        }

        public static string GetAssetPath (UnityEngine.Object obj, out string ext)
        {
            ext = "";
            string path = AssetDatabase.GetAssetPath (obj);
            return GetPath (path, out ext);
        }

        public static string GetAssetFullPath (UnityEngine.Object obj, out string ext)
        {
            ext = "";
            string path = AssetDatabase.GetAssetPath (obj);
            int index = path.LastIndexOf (".");
            if (index >= 0)
            {
                ext = path.Substring (index);
                return path.Substring (0, index);
            }
            return "";
        }
        public static string GetDir (string path)
        {
            int index = path.LastIndexOf ("/");
            if (index >= 0)
            {
                return path.Substring (0, index);
            }
            return "";
        }
        public static string GetDir (string path, out string name)
        {
            int index = path.LastIndexOf ("/");
            if (index >= 0)
            {
                name = path.Substring (index + 1);
                return path.Substring (0, index);
            }
            name = path;
            return "";
        }

        public static string GetDirName (string path)
        {
            int index = path.LastIndexOf ("/");
            if (index >= 0)
            {
                return path.Substring (index + 1);
            }
            return "";
        }
        public static string GetAssetDir (UnityEngine.Object obj, out string assetPath)
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath (obj);
                assetPath = path;
                return GetDir (path);
            }
            else
            {
                assetPath = "";
                return "";
            }

        }
    }
}
#endif