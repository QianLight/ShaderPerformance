using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace TDTools
{
    public class SkillNodeWikiHelper
    {
        public static Dictionary<string, string> WikiDic
        {
            get
            {
                if (wikiDic.Count == 0) LoadTranslateDic();
                return wikiDic;
            }
        }
        private static Dictionary<string, string> wikiDic = new Dictionary<string, string>();

        [MenuItem("Tools/TDTools/关卡相关工具/技能编辑器wiki按钮重载")]
        private static void LoadTranslateDic()
        {
            string path = Application.dataPath + "/Editor/TDTools/SkillEditorExtension/SkillEditorHelper/NodeWiki.txt";
            wikiDic.Clear();

            using (StreamReader reader = File.OpenText(path))
            {
                if (reader == null) return;

                while (!reader.EndOfStream)
                {
                    string[] keys = reader.ReadLine().Split('\t');
                    if(keys.Length > 1)
                        wikiDic[keys[0]] = keys[1];
                }
            }
        }
    }
}
