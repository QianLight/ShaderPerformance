#if UNITY_EDITOR

using UnityEditor;
using System.Collections.Generic;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using System.IO;

namespace XEditor
{

    public class XReactBatch
    {
        [MenuItem("XEditor/React/Search")]
        public static void BatchSearchText()
        {
            string str = "tht";
            SearchText(str);
        }

        [MenuItem("XEditor/React/Batch")]
        public static void BatchReact()
        {
            Through();
        }


        public const string fullPath = "Assets\\BundleRes\\ReactPackage\\";
        public static bool SearchText(string searchStr, List<string> outs = null)
        {
            string _DebugStr = string.Empty;
            bool find = false;
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);

                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                int count = 0;
                int reactCount = files.Length;
                foreach (var file in files)
                {
                    if (file.Name.EndsWith(".meta"))
                        continue;

                    EditorUtility.DisplayProgressBar("批处理React配置文件", "搜索中...", count * 1.0f / reactCount);
                    count++;

                    string strPrefabFile = file.FullName;
                    try
                    {
                        using (FileStream fs = new FileStream(strPrefabFile, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buff = new byte[fs.Length];
                            fs.Read(buff, 0, (int)fs.Length);
                            string strText = System.Text.Encoding.Default.GetString(buff);
                            if (strText.IndexOf(searchStr) != -1)
                            {
                                if (outs != null)
                                {
                                    outs.Add(strPrefabFile);
                                }
                                _DebugStr += strPrefabFile + "\r\n";
                                find = true;
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        EditorUtility.DisplayDialog("Error", "read React error ! " + strPrefabFile+" "+ ex.ToString(), "ok");
                        return false;
                    }

                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("查找", find ? _DebugStr : string.Format("没有包含\"{0}\"的配置", searchStr), "ok");
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("查找", "资源路径不对。", "ok");
                return false;
            }
        }


        public static bool Through()
        {
            string _DebugStr = string.Empty;
            bool find = false;

            List<string> pathlist = new List<string>();
            List<XReactData> dataList = new List<XReactData>();

            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);

                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                int count = 0;
                int reactCount = files.Length;
                foreach (var file in files)
                {
                    if (file.Name.EndsWith(".meta"))
                        continue;

                    EditorUtility.DisplayProgressBar("批处理React配置文件", "遍历中...", count * 1.0f / reactCount);
                    count++;

                    //改成.bytes
                    //file.MoveTo(Path.ChangeExtension(file.FullName, ".bytes"));

                    string strPrefabFile = file.FullName;
                    try
                    {
                        using (FileStream fs = new FileStream(strPrefabFile, FileMode.Open, FileAccess.Read))
                        {
                            XmlSerializer formatter = new XmlSerializer(typeof(XReactData));
                            var data = formatter.Deserialize(fs);

                            if (data != null)
                            {
                                XReactData reactData = data as XReactData;
                                if (reactData != null)
                                {
                                    _DebugStr += strPrefabFile + "\r\n";
                                    find = true;

                                    //save again
                                    int assetIdx = strPrefabFile.IndexOf("Assets");
                                    string skp = strPrefabFile.Substring(assetIdx);
                                    pathlist.Add(skp);
                                    dataList.Add(reactData);
                                    
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        EditorUtility.DisplayDialog("Error", "read React error ! " + strPrefabFile+" "+ex.ToString(), "ok");
                        return false;
                    }

                }

                //.write.
                for (int i = 0; i < pathlist.Count; ++i)
                {
                    XDataIO<XReactData>.singleton.SerializeData(pathlist[i], dataList[i]);
                }


                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("查找", find ? _DebugStr :"没有要处理的的配置", "ok");
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("查找", "资源路径不对。", "ok");
                return false;
            }
        }
    }
}
#endif