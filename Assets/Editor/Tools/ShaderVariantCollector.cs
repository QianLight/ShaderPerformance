using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class ShaderVariantCollector : CommonToolTemplate
    {
        public static string ShaderVariantPath = "Assets/Res/ShaderCollector";
        static  string ResPath = "Assets";
        #region OnGUI
        private  bool isCollect = false;
        private  bool isClear = false;
        private static int shaderNumber;
        private static int matNumber;
        private static int svcNumber;
        #endregion
        // [MenuItem("Tools/引擎/Shader变体/Shader变体收集配置")]
        // private static void ShowWindow()
        // {
        //     var window = ScriptableWizard.DisplayWizard<ShaderVariantCollector>("Shader变体收集配置");
        //     // window.position = new Rect(500, 500, 500, 600);
        //     // window.minSize = new Vector2(400, 200);
        //     // window.maxSize = new Vector2(500, 300);
        // }
        [MenuItem("Tools/引擎/Shader变体/收集Shader变体集")]
        public static void CollectShaders()
        {
            try
            {
                DoCollect(); // 开始收集
            }
            catch (Exception e)
            {
                Debug.LogError("收集变体集报错: "+$"{e.Message} {e.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();      // 移除进度条
            }
            EditorUtility.DisplayDialog("生成变体集", "已生成指定目录下所有shader变体集!", "确定");        // 显示弹窗
        }
        [MenuItem("Tools/引擎/Shader变体/删除所有shader变体")]
        public static void ClearShaderVariant()
        {
            SafeClearDir(ShaderVariantPath);
            AssetDatabase.Refresh();
            //Debug.Log("删除所有已完成的着色器变体!");
            EditorUtility.DisplayDialog("清除变体集", "删除所有已完成的着色器变体!", "确定");        // 显示弹窗
        }
        static void CreateDir(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
        static void DeleteDirectory(string ShaderVariantPath)
        {
            string[] files = Directory.GetFiles(ShaderVariantPath);
            string[] dirs = Directory.GetDirectories(ShaderVariantPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(ShaderVariantPath, false);
        }
        static bool SafeClearDir(string ShaderVariantPath) 
        {
            try
            {
                if (Directory.Exists(ShaderVariantPath))
                {
                    DeleteDirectory(ShaderVariantPath);
                }
                Directory.CreateDirectory(ShaderVariantPath);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeClearDir 失败! path = {0} 错误 = {1}", ShaderVariantPath, ex.Message));
                return false;
            }

        }
        public struct ShaderVariantData
        {
            public int[] passTypes;                // pass类型列表
            public string[] keywordLists;          // 关键字列表
            public string[] remainingKeywords;     // 遗留的关键字列表
        }
        static MethodInfo GetShaderVariantEntries = null;
        private static ShaderVariantData GetShaderVariantEntriesFiltered(Shader shader, string[] SelectedKeywords)
        {
            if (GetShaderVariantEntries == null)
            {
                GetShaderVariantEntries = typeof(ShaderUtil).GetMethod("GetShaderVariantEntriesFiltered", BindingFlags.NonPublic | BindingFlags.Static);
            }
            int[] types = null;
            string[] keywords = null;
            string[] remainingKeywords = null;
            object[] args = new object[]{
                shader, 
                32,
                SelectedKeywords,
                new ShaderVariantCollection(),
                types, 
                keywords,
                remainingKeywords
            };
            GetShaderVariantEntries?.Invoke(null, args);
            var passTypes = new List<int>();
            var allTypes = (args[4] as int[]);
            foreach(var type in allTypes)
            {
                if (!passTypes.Contains(type))
                {
                    passTypes.Add(type);
                }
            }
            ShaderVariantData svd = new ShaderVariantData() {
                passTypes = passTypes.ToArray(), 
                keywordLists = args[5] as string[], 
                remainingKeywords = args[6] as string[] 
            };

            return svd;
        }
        private static Dictionary<string, List<Material>> FindAllMaterials(string path)
        {
            string[] materials = AssetDatabase.FindAssets("t:Material", new string[] { path });
            int idx = 0;
            var matDic = new Dictionary<string, List<Material>>();
            foreach (var guid in materials)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(guid);

                EditorUtility.DisplayProgressBar($"收集材质 {matPath}", "找到所有材料", (float)idx++ / materials.Length);        // 显示或更新进度条
                
                Material mat = AssetDatabase.LoadMainAssetAtPath(matPath) as Material;
                if (mat)
                {
                    if (matDic.TryGetValue(mat.shader.name, out List<Material> list) == false)
                    {
                        list = new List<Material>();
                        matDic.Add(mat.shader.name, list);
                    }
                    list.Add(mat);
                }
            }
            
            return matDic;
        }
        public static void DoCollect(bool increment = true) 
        {
            if (increment)
            {
                CreateDir(ShaderVariantPath);
            }
            else
            {
                ClearShaderVariant();
            }
            Dictionary<string,List<Material>> matDic = FindAllMaterials(ResPath);
            shaderNumber = matDic.Count;
            Dictionary<string, Dictionary<string, Material>> finalMats = new Dictionary<string, Dictionary<string, Material>>();
            List<string> temp = new List<string>();
            int idx = 0;
            foreach (var item in matDic)
            {
                #region 通过matDic中获取的ShaderName来判断finalMats中是否存在来创建
                string shaderName = item.Key;
                EditorUtility.DisplayProgressBar($"收集材质 {shaderName}", "收集关键字", (float)idx++ / matDic.Count);        // 显示或更新进度条
                
                // 判断finalMats里Value是否包含shaderName
                if (finalMats.TryGetValue(shaderName, out Dictionary<string,Material> matList) == false)
                {
                    matList = new Dictionary<string, Material>();

                    finalMats.Add(shaderName, matList);
                }
                #endregion

                
                //填充 matList
                foreach (var mat in item.Value)
                {
                    matNumber++;
                    
                    temp.Clear();
                    string[] keyWords = mat.shaderKeywords;                   // 只会收集定义了的变体名字
                    // foreach (var key in keyWords)
                    // {
                    //     Debug.Log(key);
                    // }
                    temp.AddRange(keyWords);                                  // 变体名字添加到temp中
                    // 获取和设置是否为此材质启用 GPU 实例化
                    if (mat.enableInstancing)
                    {
                        // temp添加enableInstancing
                        temp.Add("enableInstancing");
                    }

                    if (temp.Count == 0)
                    {
                        continue;                                            // 如果变体关键字数为0 跳出当前的循环，强迫开始下一次循环
                    }


                    temp.Sort();
                    // foreach (var t in temp)
                    // {
                    //     Debug.Log(t);
                    // }
                    
                    string pattern = string.Join("_", temp);        // 把单个材质的变体关键字组合在一起
                    //Debug.Log(pattern);
                    
                    // 如果 matList 中 不包含 pattern
                    if (!matList.ContainsKey(pattern))
                    {
                        matList.Add(pattern, mat);                         // 变体关键字组合对应的材质球
                    }
                }
            }
            idx = 0;
            foreach (var kv in finalMats)
            {
                string shaderFullName = kv.Key;                             // shader的名字
                //Debug.Log(shaderFullName);
                Dictionary<string,Material> matList = kv.Value;             // 变体关键字组合对应的材质球

                if (matList.Count == 0)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar($"收集材质 {shaderFullName}", "一般材质变体", (float)idx++ / finalMats.Count);        // 显示或更新进度条
                
                // 跳过遗留的shader
                if (shaderFullName.Contains("InternalErrorShader"))
                {
                    continue;
                }

                
                // 变体集存放路径+shaderName.shadervariants
                var path = $"{ShaderVariantPath}/{shaderFullName.Replace("/", "_")}.shadervariants";
                //Debug.Log(path);
                bool alreadyExsit = true; 
                 
                ShaderVariantCollection shaderCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(path);   // 加载SVC
                
                if (shaderCollection == null)
                {
                    alreadyExsit = false;
                    shaderCollection = new ShaderVariantCollection();                                                     // 实例化SVC
                }

                Shader shader = Shader.Find(shaderFullName);
                foreach (var kv2 in matList)
                {
                    ShaderVariantData svd = GetShaderVariantEntriesFiltered(shader, kv2.Value.shaderKeywords);            // 获得着色器变体条目过滤
                    // 遍历Shader的pass类型
                    foreach (var passType in svd.passTypes)
                    {
                        // 创建shader变体，在构造参数填入shader，pass类型，以及关键字
                        ShaderVariantCollection.ShaderVariant shaderVariant = new ShaderVariantCollection.ShaderVariant()
                        {
                            shader = shader,
                            passType = (UnityEngine.Rendering.PassType)passType,
                            keywords = kv2.Value.shaderKeywords,
                        };
                        // 如果变体集不包含改变体则添加进去
                        if (!shaderCollection.Contains(shaderVariant))
                        {
                            shaderCollection.Add(shaderVariant);            // Shader变体添加到变体集
                        }
                    }
                }
                
                if (alreadyExsit)
                {
                    EditorUtility.SetDirty(shaderCollection);                             // 表示已经创建
                }
                else
                {
                    AssetDatabase.CreateAsset(shaderCollection, path);                   // 创建SVC资产
                    svcNumber++;
                }
            }

            EditorUtility.DisplayProgressBar("收集材质", "保存资产", 1f);        // 显示或更新进度条
            AssetDatabase.Refresh();                                                         // 刷新导入所有更改的资源
            AssetDatabase.SaveAssets();                                                      // 将所有未保存的资源更改写入磁盘
            EditorUtility.ClearProgressBar();                                                // 移除进度条

            //Debug.Log("完成所有shader变量的收集!");
            
        }
        // void OnGUI()
        // {
        //     
        //     EditorGUILayout.BeginHorizontal("box");
        //     {
        //         GUILayout.Label("变体集文件创建到此目录下: ");
        //         ShaderVariantPath = EditorGUILayout.TextArea(ShaderVariantPath);
        //     }
        //     EditorGUILayout.EndHorizontal();
        //     EditorGUILayout.BeginHorizontal("box");
        //     {
        //         GUILayout.Label("收集此目录下所有材质: ");
        //         ResPath = EditorGUILayout.TextArea(ResPath);
        //     }
        //     EditorGUILayout.EndHorizontal();
        //     
        //     isCollect = GUILayout.Button("开始收集");
        //     isClear = GUILayout.Button("删除所有变体集");
        //
        //     if (isCollect)
        //     {
        //         matNumber = 0;
        //         CollectShaders();
        //     }
        //
        //
        //     
        //     if (isClear)
        //     {
        //         shaderNumber = 0;
        //         matNumber = 0;
        //         svcNumber = 0;
        //         ClearShaderVariant();
        //     }
        //
        //     EditorGUILayout.BeginVertical("box");
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //         {
        //             GUILayout.Label("Shader Numbers: "+shaderNumber.ToString());
        //         }
        //         EditorGUILayout.EndHorizontal();
        //         EditorGUILayout.BeginHorizontal();
        //         {
        //             GUILayout.Label("Materials Numbers: "+matNumber.ToString());
        //         }
        //         EditorGUILayout.EndHorizontal();
        //         EditorGUILayout.BeginHorizontal();
        //         {
        //             GUILayout.Label("SVC Numbers: "+svcNumber.ToString());
        //         }
        //         EditorGUILayout.EndHorizontal();
        //     }
        //     EditorGUILayout.EndVertical();
        // }
        public override void OnInit ()
        {
            base.OnInit ();
        }
        public override void OnUninit ()
        {
            base.OnUninit ();
        }
        public override void DrawGUI(ref Rect rect)
        {


            EditorGUILayout.BeginHorizontal("box");
            {
                GUILayout.Label("收集目录下材质: ");
                ResPath = EditorGUILayout.TextArea(ResPath);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal("box");
            {
                GUILayout.Label("变体集生成目录: ");
                ShaderVariantPath = EditorGUILayout.TextArea(ShaderVariantPath);
            }
            EditorGUILayout.EndHorizontal();
            
            
            isCollect = GUILayout.Button("开始收集");
            isClear = GUILayout.Button("删除所有变体集");
            
            if (isCollect)
            {
                matNumber = 0;
                CollectShaders();
            }
            
            if (isClear)
            {
                shaderNumber = 0;
                matNumber = 0;
                svcNumber = 0;
                ClearShaderVariant();
            }
            
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Shader Numbers: "+shaderNumber.ToString());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Materials Numbers: "+matNumber.ToString());
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("SVC Numbers: "+svcNumber.ToString());
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
