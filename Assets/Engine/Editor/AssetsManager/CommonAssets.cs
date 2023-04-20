using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CFEngine;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    internal class ObjectInfo
    {
        public UnityEngine.Object obj = null;
        public string path = "";
    }
    internal class CommonAssets
    {

        internal delegate void EnumAssetPreprocessCallback (string path);

        internal delegate bool EnumAssetImportCallback<T, I> (T obj, I assetImporter, string path)
        where T : UnityEngine.Object where I : UnityEditor.AssetImporter;

        internal delegate void EnumAssetCallback<T> (T obj, string path)
        where T : UnityEngine.Object;

        internal interface IAssetLoadCallback
        {
            bool verbose { get; set; }
            List<ObjectInfo> GetObjects (string dir, bool includeChild);

            void PreProcess (string path);
            bool Process (UnityEngine.Object asset, string path, System.Object context);
            void PostProcess (string path);
        }
        internal class BaseAssetLoadCallback<T> where T : UnityEngine.Object
        {
            public bool is_verbose = true;
            public string extFilter = "";
            public string extFilter1 = "";
            public string extFilter2 = "";
            protected List<ObjectInfo> m_Objects = new List<ObjectInfo> ();

            public BaseAssetLoadCallback (string ext)
            {
                extFilter = ext;
            }
            public BaseAssetLoadCallback (string ext, string ext1)
            {
                extFilter = ext;
                extFilter1 = ext1;
            }
            public BaseAssetLoadCallback (string ext, string ext1, string ext2)
            {
                extFilter = ext;
                extFilter1 = ext1;
                extFilter2 = ext2;
            }

            public bool verbose { get { return is_verbose; } set { is_verbose = value; } }

            private void GetObjectsInfolder (string path, bool includeChild)
            {
                CommonAssets.GetObjectsInfolder<T> (path, m_Objects, includeChild, extFilter, extFilter1, extFilter2);
            }

            private void GetObjectsInfolder (UnityEditor.DefaultAsset folder, bool includeChild)
            {
                string path = AssetDatabase.GetAssetPath (folder);
                GetObjectsInfolder (path, includeChild);
            }

            public List<ObjectInfo> GetObjects (string dir, bool includeChild)
            {
                m_Objects.Clear ();
                if (string.IsNullOrEmpty (dir))
                {
                    UnityEngine.Object[] objs = Selection.GetFiltered (typeof (UnityEngine.Object), SelectionMode.Assets);
                    for (int i = 0; i < objs.Length; ++i)
                    {
                        UnityEngine.Object obj = objs[i];
                        if (obj is UnityEditor.DefaultAsset)
                        {
                            GetObjectsInfolder (obj as UnityEditor.DefaultAsset, true);
                        }
                        else
                        {
                            if (obj is T)
                            {
                                string path = AssetDatabase.GetAssetPath (obj);
                                ObjectInfo oi = new ObjectInfo ();
                                oi.obj = obj;
                                oi.path = path;
                                m_Objects.Add (oi);
                            }
                        }
                    }
                }
                else
                {
                    GetObjectsInfolder (dir, includeChild);
                }
                return m_Objects;
            }
        }

        internal class AssetLoadCallback<T, I> : BaseAssetLoadCallback<T>, IAssetLoadCallback
        where T : UnityEngine.Object where I : UnityEditor.AssetImporter
        {
            public EnumAssetPreprocessCallback preprocess = null;
            public Func<T, I, string, System.Object, bool> cb = null;

            public AssetLoadCallback (string ext) : base (ext) { }
            public AssetLoadCallback (string ext, string ext1) : base (ext, ext1) { }
            public AssetLoadCallback (string ext, string ext1, string ext2) : base (ext, ext1, ext2) { }
            public virtual void PreProcess (string path)
            {
                if (preprocess != null)
                {
                    preprocess (path);
                }
            }
            public virtual bool Process (UnityEngine.Object asset, string path, System.Object context)
            {
                T obj = asset as T;
                if (cb != null && obj != null)
                {
                    I assetImporter = AssetImporter.GetAtPath (path) as I;
                    return cb (obj, assetImporter, path, context);
                }
                return false;
            }

            public virtual void PostProcess (string path)
            {
                AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
            }
        }
        internal class AssetLoadCallback<T> : BaseAssetLoadCallback<T>, IAssetLoadCallback where T : UnityEngine.Object
        {
            public Action<T, string, System.Object> cb = null;

            public AssetLoadCallback (string ext) : base (ext) { }
            public AssetLoadCallback (string ext, string ext1) : base (ext, ext1) { }
            public virtual void PreProcess (string path)
            {

            }
            public virtual bool Process (UnityEngine.Object asset, string path, System.Object context)
            {
                T obj = asset as T;
                if (cb != null && obj != null)
                {
                    cb (obj, path, context);
                }
                return false;
            }

            public virtual void PostProcess (string path) { }
        }

        internal delegate bool EnumFbxCallback<GameObject, ModelImporter> (GameObject fbx, ModelImporter modelImporter, string path);
        internal delegate bool EnumTex2DCallback<Texture2D, TextureImporter> (Texture2D tex, TextureImporter textureImporter, string path);

        internal static AssetLoadCallback<GameObject, ModelImporter> enumFbx = new AssetLoadCallback<GameObject, ModelImporter> ("*.fbx");
        internal static AssetLoadCallback<Texture2D, TextureImporter> enumTex2D = new AssetLoadCallback<Texture2D, TextureImporter> ("*.png", "*.tga", "*.exr");
        internal static AssetLoadCallback<Cubemap, TextureImporter> enumTexCube = new AssetLoadCallback<Cubemap, TextureImporter> ("*.hdr", "*.tga", "*.exr");

        internal delegate void EnumScriptableObjectCallback<ScriptableObject> (ScriptableObject so, string path);
        internal delegate void EnumPrefabCallback<GameObject> (GameObject prefab, string path);
        internal delegate void EnumTxtCallback<TextAsset> (TextAsset txt, string path);
        internal delegate void EnumMaterialCallback<Material> (Material mat, string path);
        internal delegate void EnumMeshCallback<Mesh> (Mesh mesh, string path);
        internal delegate void EnumAnimationCallback<AnimationClip> (AnimationClip animClip, string path);
        internal delegate void EnumBytesCallback<TextAsset> (TextAsset bytes, string path);
        internal delegate void EnumScriptCallback<DefaultAsset> (DefaultAsset cs, string path);

        internal static AssetLoadCallback<ScriptableObject> enumSO = new AssetLoadCallback<ScriptableObject> ("*.asset");
        internal static AssetLoadCallback<GameObject> enumPrefab = new AssetLoadCallback<GameObject> ("*.prefab");
        internal static AssetLoadCallback<TextAsset> enumTxt = new AssetLoadCallback<TextAsset> ("*.bytes", "*.txt");
        internal static AssetLoadCallback<Material> enumMat = new AssetLoadCallback<Material> ("*.mat");
        internal static AssetLoadCallback<Mesh> enumMesh = new AssetLoadCallback<Mesh> ("*.asset");
        internal static AssetLoadCallback<AnimationClip> enumAnimationClip = new AssetLoadCallback<AnimationClip> ("*.anim");
        internal static AssetLoadCallback<SceneAsset> enumSceneAsset = new AssetLoadCallback<SceneAsset> ("*.unity");
        internal static AssetLoadCallback<DefaultAsset> enumScriptAsset = new AssetLoadCallback<DefaultAsset> ("*.cs");
        internal static void GetObjectsInfolder<T> (FileInfo[] files, List<ObjectInfo> objects) where T : UnityEngine.Object
        {
            for (int i = 0; i < files.Length; ++i)
            {
                FileInfo file = files[i];
                string fileName = file.FullName.Replace ("\\", "/");
                int index = fileName.IndexOf ("Assets/");
                fileName = fileName.Substring (index);
                var obj = AssetDatabase.LoadAssetAtPath<T> (fileName);
                if (obj != null && objects.FindIndex (x => x.obj == obj) == -1)
                {
                    ObjectInfo oi = new ObjectInfo ();
                    oi.path = fileName;
                    oi.obj = obj;
                    objects.Add (oi);
                }
            }

        }
        internal static void GetObjectsInfolder<T> (string path, List<ObjectInfo> objects, bool includeChild, params string[] filter) where T : UnityEngine.Object
        {
            DirectoryInfo di = new DirectoryInfo (path);
            for (int i = 0; i < filter.Length; ++i)
            {
                if (!string.IsNullOrEmpty (filter[i]))
                {
                    FileInfo[] files = di.GetFiles (filter[i], includeChild?SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    GetObjectsInfolder<T> (files, objects);
                }
            }
        }
        internal static void EnumAsset<T> (IAssetLoadCallback cb, string title, string dir = "", object context = null, bool verbose = true, bool includeChildDir = false) where T : UnityEngine.Object
        {
            if (cb != null)
            {
                cb.verbose = verbose;
                List<ObjectInfo> objInfoLst = cb.GetObjects (dir, includeChildDir);
                for (int i = 0; i < objInfoLst.Count; ++i)
                {
                    ObjectInfo oi = objInfoLst[i];
                    T asset = oi.obj as T;
                    if (asset != null)
                    {
                        cb.PreProcess (oi.path);
                        if (cb.Process (asset, oi.path, context))
                        {
                            cb.PostProcess (oi.path);
                        }
                    }
                    if (cb.verbose)
                        EditorUtility.DisplayProgressBar (string.Format ("{0}-{1}/{2}", title, i, objInfoLst.Count), oi.path, (float) i / objInfoLst.Count);
                }
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();
                if (cb.verbose)
                {
                    EditorUtility.ClearProgressBar ();
                    EditorUtility.DisplayDialog ("Finish", "All assets processed finish", "OK");
                }
                cb.verbose = false;
            }
        }
        internal static void EnumAssetInside<T> (List<ObjectInfo> objInfoLst, IAssetLoadCallback cb, string title, string dir = "", object context = null, bool verbose = true, bool includeChildDir = false) where T : UnityEngine.Object
        {
            if (cb != null)
            {
                cb.verbose = verbose;
                // List<ObjectInfo> objInfoLst = cb.GetObjects (dir, includeChildDir);
                for (int i = 0; i < objInfoLst.Count; ++i)
                {
                    ObjectInfo oi = objInfoLst[i];
                    T asset = oi.obj as T;
                    if (asset != null)
                    {
                        cb.PreProcess (oi.path);
                        if (cb.Process (asset, oi.path, context))
                        {
                            cb.PostProcess (oi.path);
                        }
                    }
                    if (cb.verbose)
                        EditorUtility.DisplayProgressBar (string.Format ("{0}-{1}/{2}", title, i, objInfoLst.Count), oi.path, (float) i / objInfoLst.Count);
                }
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();
                if (cb.verbose)
                {
                    EditorUtility.ClearProgressBar ();
                    EditorUtility.DisplayDialog ("Finish", "All assets processed finish", "OK");
                }
                cb.verbose = false;
            }
        }
        internal static void GetObjectsInfolder (EnumAssetPreprocessCallback cb, string path, string ext)
        {
            DirectoryInfo di = new DirectoryInfo (path);
            FileInfo[] files = di.GetFiles (ext, SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; ++i)
            {
                var file = files[i];
                string fileName = file.FullName.Replace ("\\", "/");
                int index = fileName.IndexOf ("Assets/");
                fileName = fileName.Substring (index);
                cb (fileName);
            }

        }
        internal static void EnumBytesAsset (EnumAssetPreprocessCallback cb, string title, string ext, string dir = "", object context = null)
        {
            if (cb != null)
            {
                if (string.IsNullOrEmpty (dir))
                {
                    UnityEngine.Object[] objs = Selection.GetFiltered (typeof (UnityEngine.Object), SelectionMode.Assets);
                    for (int i = 0; i < objs.Length; ++i)
                    {
                        UnityEngine.Object obj = objs[i];
                        string path = AssetDatabase.GetAssetPath (obj);
                        if (obj is UnityEditor.DefaultAsset)
                        {
                            if (path.EndsWith (ext))
                            {
                                cb (path);
                            }
                            else
                            {
                                GetObjectsInfolder (cb, path, ext);
                            }
                        }
                        else
                        {
                            if (path.EndsWith (ext))
                            {
                                cb (path);
                            }
                        }
                    }
                }
                else
                {
                    GetObjectsInfolder (cb, dir, ext);
                }
            }
        }
        internal static void EnumScript (IAssetLoadCallback cb, string title, string dir = "")
        {
            UnityEngine.Object[] objs = Selection.GetFiltered (typeof (UnityEngine.Object), SelectionMode.Assets);
            foreach (UnityEngine.Object obj in objs)
            {
                Debug.LogError (obj.name);
            }
        }

        internal static T CreateAsset<T> (string path, string ext, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            return EditorCommon.CreateAsset<T> (path, ext, asset);
        }

        internal static T CreateAsset<T> (string dirPath, string targetName, string ext, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            return EditorCommon.CreateAsset<T> (dirPath + "/" + targetName + ext, ext, asset);
        }

        internal static void DeleteAsset (UnityEngine.Object asset)
        {

            if (asset == null)
                return;

            string path = AssetDatabase.GetAssetPath (asset);
            AssetDatabase.DeleteAsset (path);
        }

        internal static string GetAssetFolder (UnityEngine.Object obj)
        {
            string matPath = AssetDatabase.GetAssetPath (obj);
            int index = matPath.LastIndexOf ("/");
            if (index > 0)
            {
                return matPath.Substring (0, index);
            }
            return "";
        }

        internal static void StandardRender (Renderer render)
        {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            render.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            render.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            render.receiveShadows = false;
            render.allowOcclusionWhenDynamic = false;
            if (render is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = render as SkinnedMeshRenderer;
                smr.updateWhenOffscreen = false;
                smr.skinnedMotionVectors = false;
            }
        }
        internal static void BakeRender (Renderer render)
        {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            render.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            render.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            render.receiveShadows = true;
            render.allowOcclusionWhenDynamic = false;
            if (render is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = render as SkinnedMeshRenderer;
                smr.updateWhenOffscreen = false;
                smr.skinnedMotionVectors = false;
            }
        }

        internal static SerializedProperty GetSerializeProperty (UnityEngine.Object obj, string name)
        {
            SerializedObject so = new SerializedObject (obj);
            return so.FindProperty (name);
        }

        internal static SerializedProperty GetSerializeProperty (SerializedObject so, string name)
        {
            return so.FindProperty (name);
        }

        internal static float GetSerializeValue (UnityEngine.Object obj, string name)
        {
            SerializedProperty sp = GetSerializeProperty (obj, name);
            if (sp != null)
            {
                return sp.floatValue;
            }
            return 0.0f;
        }

        internal static void SetSerializeValue (UnityEngine.Object obj, string name, float value)
        {
            SerializedProperty sp = GetSerializeProperty (obj, name);
            if (sp != null)
            {
                sp.floatValue = value;
                sp.serializedObject.ApplyModifiedProperties ();
            }
        }

        internal static void SetSerializeValue (UnityEngine.Object obj, string name, UnityEngine.Object value)
        {
            SerializedProperty sp = GetSerializeProperty (obj, name);
            if (sp != null)
            {
                sp.objectReferenceValue = value;
                sp.serializedObject.ApplyModifiedProperties ();
            }
        }
        internal static void SaveAsset (UnityEngine.Object obj)
        {
            EditorCommon.SaveAsset (obj);
        }

        internal static bool IsSameProperty (Color color0, Color color1)
        {
            int deltaR = (int) (color0.r * 10) - (int) (color1.r * 10);
            int deltaG = (int) (color0.g * 10) - (int) (color1.g * 10);
            int deltaB = (int) (color0.b * 10) - (int) (color1.b * 10);
            int deltaA = (int) (color0.a * 10) - (int) (color1.a * 10);
            return deltaR == 0 && deltaG == 0 && deltaB == 0 && deltaA == 0;
        }
        internal static bool IsSameProperty (Vector4 vector0, Vector4 vector1)
        {
            int deltaX = (int) (vector0.x * 10) - (int) (vector1.x * 10);
            int deltaY = (int) (vector0.y * 10) - (int) (vector1.y * 10);
            int deltaZ = (int) (vector0.z * 10) - (int) (vector1.z * 10);
            int deltaW = (int) (vector0.w * 10) - (int) (vector1.w * 10);
            return deltaX == 0 && deltaY == 0 && deltaZ == 0 && deltaW == 0;
        }

        internal static bool StrFilter (string filter, string path, string fileName)
        {
            // 正则匹配
            const string regexPrefix = "regex:";
            if (filter.StartsWith(regexPrefix))
                return Regex.IsMatch(path, filter.Substring(regexPrefix.Length));
            
            // 以'_'或者'-'为结尾的是前缀
            if ((filter.EndsWith ("_") || filter.EndsWith ("-")) && fileName.StartsWith (filter))
                return true;
            
            // 以'_'为开头的是后缀
            if (filter.StartsWith ("_") && fileName.EndsWith (filter))
                return true;
            
            // 以'/'为开头的是目录
            // /Textures/
            if (filter.StartsWith ("/") && path.Contains (filter))
                return true;
            
            // 文件名完全匹配（忽略大小写）
            if (string.Equals(filter, fileName, StringComparison.OrdinalIgnoreCase))
                return true;
            
            return false;
        }

        internal static bool IsPhysicAsset (UnityEngine.Object asset)
        {
            if (asset != null)
            {
                return AssetDatabase.Contains (asset);
            }
            return false;
        }

        [MenuItem ("Assets/Tool/Config/Assets_Create")]
        static void CreateConfig ()
        {
            AssetsConfig ac = ScriptableObject.CreateInstance<AssetsConfig> ();
            CreateAsset<AssetsConfig> ("Assets/Editor/EditorResources", "AssetsConfig", ".asset", ac);
        }

        [MenuItem ("Assets/Tool/Config/Script_SetAsEditor")]
        static void ScriptSetAsEditor ()
        {
            UnityEngine.Object[] objs = Selection.GetFiltered (typeof (UnityEngine.Object), SelectionMode.Assets);
            for (int i = 0; i < objs.Length; ++i)
            {
                UnityEngine.Object obj = objs[i];
                if (obj is UnityEditor.DefaultAsset)
                {
                    string path = AssetDatabase.GetAssetPath (obj);

                    DirectoryInfo di = new DirectoryInfo (path);
                    FileInfo[] files = di.GetFiles ("*.cs", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; ++j)
                    {
                        var fi = files[j];
                        string data = File.ReadAllText (fi.FullName);
                        if (!data.StartsWith ("#if UNITY_EDITOR"))
                        {
                            data = "#if UNITY_EDITOR\r\n" + data + "\r\n#endif";
                            File.WriteAllText (fi.FullName, data);
                        }
                    }
                }
                else if (obj is TextAsset)
                {
                    string path = AssetDatabase.GetAssetPath (obj);
                    if (path.EndsWith (".cs"))
                    {
                        string data = File.ReadAllText (path);
                        if (!data.StartsWith ("#if UNITY_EDITOR"))
                        {
                            data = "#if UNITY_EDITOR\r\n" + data + "\r\n#endif";
                            File.WriteAllText (path, data);
                        }
                    }

                }
            }
        }

        [MenuItem ("Assets/Tool/Test")]
        public static void Test () 
        {
            //CommonAssets.enumSO.cb = (so, path, context) =>
            //{
            //    if(so is EnvAreaProfile)
            //    {
            //        var vap = so as EnvAreaProfile;
            //        vap.envBlock.Convert();
            //        SaveAsset(vap);
            //    }
            //};

            //CommonAssets.EnumAsset<ScriptableObject>(CommonAssets.enumSO, "ConvertEnv");
            WorldSystem.createFrame = 0;
            //ref var cc = ref GameObjectCreateContext.createContext;
            //cc.Reset();
            //cc.location = "Role_Chopper";
            //cc.name = "Role_Chopper";
            //cc.immediate = false;
            //XGameObject xgo = XGameObject.CreateXGameObject(ref cc);
            //xgo.EndLoad(ref cc);

            //cc.Reset();
            //cc.location = "Role_Chopper_rxt";
            //cc.name = "Role_Chopper_rxt";
            //cc.immediate = false;
            //XGameObject.Reload(xgo, ref cc);
            //xgo.EndLoad(ref cc);

            //cc.Reset();
            //cc.location = "Role_Chopper";
            //cc.name = "Role_Chopper";
            //cc.immediate = false;
            //XGameObject.Reload(xgo, ref cc);
            //xgo.EndLoad(ref cc);
        }
    }
}