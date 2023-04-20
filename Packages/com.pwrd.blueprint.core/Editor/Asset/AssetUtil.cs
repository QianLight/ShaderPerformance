using UnityEngine;
using System.IO;

namespace Blueprint.UtilEditor
{

    public static class AssetUtil
    {
        /// <summary>
        /// 将asset路径转化为完整路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToCompletePath(string path)
        {
            return Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
        }

        /// <summary>
        /// 将完整路径转换为unity路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ToUnityPath(string path)
        {
            path = path.Substring(path.IndexOf("Assets", System.StringComparison.Ordinal));
            if (path.EndsWith("/", System.StringComparison.Ordinal) || path.EndsWith("\\", System.StringComparison.Ordinal)) path = path.Substring(0, path.Length - 1);
            return path.Replace("\\", "/");
        }

        public static bool CheckNameValid(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return true;
        }
    }
}