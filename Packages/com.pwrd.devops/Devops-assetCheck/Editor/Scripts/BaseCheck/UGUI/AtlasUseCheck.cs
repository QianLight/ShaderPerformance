using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "使用图集统计", "t:Prefab", "如果使用的Sprite在不同文件夹，就认为使用的是不同的图集")]
    public class AtlasUseCheck : RuleBase
    {
        [PublicParam("最大图集使用量", eGUIType.Input)]
        public int MaxAtlasCount = 1;

        HashSet<string> atlasSet = new HashSet<string>();

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                return true;
            }
            Image[] imgs = go.GetComponentsInChildren<Image>(true);
            foreach (var img in imgs)
            {
                if (img.sprite == null)
                    continue;
                string spritePath = AssetDatabase.GetAssetPath(img.sprite);
                string atlasName = GenAtlasName(spritePath);
                if (!string.IsNullOrEmpty(atlasName))
                {
                    atlasSet.Add(atlasName);
                }
            }
            output = $"{atlasSet.Count}/{MaxAtlasCount}:" + string.Join("，", atlasSet);
            return atlasSet.Count <= MaxAtlasCount;
        }

        private string GenAtlasName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            string key = "Assets/Res/Gui/Share/";
            if (path.IndexOf(key) == -1)
            { // Share目录下的资源不用管，不是图片
                return string.Empty;
            }
            string relPath = path.Substring(key.Length);

            // 进行图集和单图的区分
            if (relPath.StartsWith("Atlas"))
            { // 图集，名称是路径拼装，用 _ 连接
                string[] dirs = relPath.Split('/');
                if (dirs.Length > 0)
                {
                    string name = dirs[0].ToLower();
                    for (int i = 1; i < dirs.Length - 1; i++)
                    {
                        name += "_" + dirs[i].ToLower();
                    }

                    for (int i = 1; i < dirs.Length - 1; i++)
                    {
                        name += "_" + dirs[i].ToLower();
                    }
                    return name;
                }
            }
            else if (relPath.StartsWith("Texture"))
            { // 单图，名称是 uitex_ + 文件名
                int dot = relPath.LastIndexOf(".");
                int dir = relPath.LastIndexOf("/", dot) + 1;
                if (dir != -1 && dot != -1)
                {
                    return "uitex_" + relPath.Substring(dir, dot - dir).ToLower();
                }
            }
            return string.Empty;
        }
    }
}