using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Prefab", "检查prefab是否missing的prefab(重要，请优先处理，可能会导致其他数据不准确！！！)", "t:Prefab", "")]
    public class PrefabMissCheck : RuleBase
    {
        private const string Pattern = "[A-Za-z0-9]{32}";
        private const string PrefabPrefix = "PrefabInstance";
        private const string SourcePrefabPrefix = "m_SourcePrefab";
        private const string PrefabDeletedStr = "__DELETED_GUID_Trash";
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;

            string prefabFullPath = Directory.GetParent(Application.dataPath).FullName + "/" + path;
            FileStream fs = new FileStream(prefabFullPath, FileMode.Open);
            StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8);
            string lineContent;
            Regex regex = new Regex(Pattern);
            while ((lineContent = reader.ReadLine()) != null)
            {
                lineContent = lineContent.Trim().ToString();
                if (lineContent.StartsWith(PrefabPrefix))
                {
                    while ((lineContent = reader.ReadLine()) != null)
                    {
                        lineContent = lineContent.Trim().ToString();
                        if (lineContent.StartsWith(SourcePrefabPrefix))
                        {
                            Match match = regex.Match(lineContent);
                            if (match != null && match.Groups != null && match.Groups.Count > 0)
                            {
                                string guid = match.Groups[0].Value;
                                string referencePrefabPath = AssetDatabase.GUIDToAssetPath(guid);
                                string referencePrefabName = "";
                                bool isMissing = referencePrefabPath.Contains(PrefabDeletedStr) || AssetDatabase.LoadAssetAtPath<GameObject>(referencePrefabPath) == null;
                                if (!isMissing)
                                {
                                    referencePrefabName = AssetDatabase.LoadAssetAtPath<GameObject>(referencePrefabPath).name;
                                }
                                fs.Close();
                                output = "prefab有missing的prefab";
                                return false;
                            }

                        }
                    }
                }
            }
            fs.Close();

            return true;

        }

        public  bool FindMissingScriptRecursively(Transform trans, string hierarchy)
        {
            hierarchy += $"/{trans.name}";

            var comps = trans.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                var tempComp = comps[i];
                if (tempComp == null)
                {
                    return false;
                }
            }
            var childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                FindMissingScriptRecursively(trans.GetChild(i), hierarchy);
            }
            return true;
        }
    }


    [CheckRuleDescription("Prefab", "检查prefab层级中object引用组件丢失(重要，请优先处理，可能会导致其他数据不准确！！！)", "t:Prefab", "")]
    public class PrefabMissScriptsCheck : RuleBase
    {

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            bool find = FindMissingScriptRecursively(gObject.transform, gObject.name);
            return find;
        }
        public bool FindMissingScriptRecursively(Transform trans, string hierarchy)
        {
            hierarchy += $"/{trans.name}";

            var comps = trans.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                var tempComp = comps[i];
                if (tempComp == null)
                {
                    return false;
                }
            }
            var childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                FindMissingScriptRecursively(trans.GetChild(i), hierarchy);
            }
            return true;
        }
    }
}

