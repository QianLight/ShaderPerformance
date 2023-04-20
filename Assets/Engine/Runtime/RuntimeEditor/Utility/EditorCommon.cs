#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine
{
    public enum EditorMessageType
    {
        Silence,
        LogDebug,
        LogWarning,
        LogError,
        Dialog,
    }

    public class BaseCopy<T>
    {
        public virtual void Copy (T src) { }
    }
    public class TransformRotationGUIWrapper
    {
        public Transform t;
        public System.Object guiObj;
        public MethodInfo mi;
        // public 
        public void OnGUI ()
        {
            if (guiObj != null && mi != null)
            {
                mi.Invoke (guiObj, null);
            }
        }
    }
    public class ReflectFun
    {
        public MethodInfo fun;

        public object Call (object instance, object[] parameters)
        {
            if (fun != null)
            {
                return fun.Invoke (instance, parameters);
            }
            return null;
        }
    }
    public struct ListElementContext
    {
        public Rect rect;
        public bool draw;
        public float height;
        public float width;
        public float lineStart;
        public float lineHeight;
        public float lineOffset;
        public float lastWidth;
    }
    
    public class TransformLog
    {
        public string path;
        public string log;
        public int type;
    }
    public class GameObjectLog
    {
        public string name;
        public string path;
        public List<TransformLog> logs = new List<TransformLog>();

        public void Reset()
        {
            name = "";
            path = "";
            logs.Clear();
        }
    }
    
    public static class EditorCommon
    {
        public delegate void EnumTransform (Transform t, object param);
        static Type transformRotationGUIType = null;
        static Assembly unityEditorAssembly = null;
        static Assembly unityEditorInternalAssembly = null;
        public static bool needRefreshShaderGui = false;



        public static uint[] argArray = new uint[5];

        public static object CallInternalFunction (Type type, string function, bool isStatic, bool isPrivate, bool isInstance, object obj, object[] parameters)
        {
            System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Default;
            if (isStatic)
            {
                flag |= System.Reflection.BindingFlags.Static;
            }
            if (isPrivate)
            {
                flag |= System.Reflection.BindingFlags.NonPublic;
            }
            else
            {
                flag |= System.Reflection.BindingFlags.Public;
            }
            if (isInstance)
            {
                flag |= System.Reflection.BindingFlags.Instance;
            }
            System.Reflection.MethodInfo mi = type.GetMethod (function, flag);
            if (mi != null)
            {
                return mi.Invoke (obj, parameters);
            }
            return null;
        }

        public static ReflectFun GetInternalFunction (Type type, string function, bool isStatic, bool isPrivate, bool isInstance, bool baseType, bool all = false)
        {
            System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Default;
            if (all)
            {
                flag = BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.Static;
            }
            else
            {
                if (isStatic)
                {
                    flag |= System.Reflection.BindingFlags.Static;
                }
                if (isPrivate)
                {
                    flag |= System.Reflection.BindingFlags.NonPublic;
                }
                else
                {
                    flag |= System.Reflection.BindingFlags.Public;
                }
                if (isInstance)
                {
                    flag |= System.Reflection.BindingFlags.Instance;
                }
            }

            System.Reflection.MethodInfo mi = baseType ? type.BaseType.GetMethod (function, flag) : type.GetMethod (function, flag);
            if (mi != null)
            {
                return new ReflectFun () { fun = mi };
            }
            return null;
        }

        public static Assembly GetUnityEditorAssembly ()
        {
            if (unityEditorAssembly == null)
            {
                unityEditorAssembly = System.Reflection.Assembly.GetAssembly (typeof (UnityEditor.Editor));
            }
            return unityEditorAssembly;
        }
        public static Assembly GetUnityEditorInternalAssembly()
        {
            if (unityEditorInternalAssembly == null)
            {
                unityEditorInternalAssembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditorInternal.AssemblyDefinitionAsset));
            }
            return unityEditorInternalAssembly;
        }
        public static TransformRotationGUIWrapper GetTransformRotatGUI (Transform tran)
        {
            if (transformRotationGUIType == null)
            {
                GetUnityEditorAssembly ();
                if (unityEditorAssembly != null)
                {
                    transformRotationGUIType = unityEditorAssembly.GetType ("UnityEditor.TransformRotationGUI");
                }
            }
            TransformRotationGUIWrapper wrapper = null;
            if (transformRotationGUIType != null)
            {
                System.Object guiObj = Activator.CreateInstance (transformRotationGUIType);
                if (guiObj != null)
                {
                    wrapper = new TransformRotationGUIWrapper ();
                    wrapper.t = tran;
                    wrapper.guiObj = guiObj;
                    CallInternalFunction (transformRotationGUIType, "OnEnable", false, false, true, guiObj, new object[] { new SerializedObject (tran).FindProperty ("m_LocalRotation"), EditorGUIUtility.TrTextContent ("Rotation", "The local rotation.") });

                    wrapper.mi = transformRotationGUIType.GetMethod ("RotationField", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
                }
            }
            return wrapper;
        }
        static Type MaskFieldGUIType = null;
        static System.Reflection.MethodInfo MaskFieldGUIFun;
        public static int DoMaskPopup (ref Rect position, int mask, string[] flagNames, int[] flagValues)
        {
            var id = GUIUtility.GetControlID (1111, FocusType.Keyboard, position);

            if (MaskFieldGUIType == null)
            {
                GetUnityEditorAssembly ();
                if (unityEditorAssembly != null)
                {
                    MaskFieldGUIType = unityEditorAssembly.GetType ("UnityEditor.MaskFieldGUI");
                }
            }
            if (MaskFieldGUIFun == null)
            {
                MaskFieldGUIFun = MaskFieldGUIType.GetMethod ("DoMaskField",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof (Rect), typeof (int), typeof (int), typeof (string[]), typeof (int[]), typeof (GUIStyle) },
                    null);
            }
            if (MaskFieldGUIFun != null)
            {
                return (int) MaskFieldGUIFun.Invoke (null, new object[] { position, id, mask, flagNames, flagValues, EditorStyles.popup });
                // DebugLog.AddEngineLog(o.GetType().ToString());
            }
            return 0;
        }
        public static void SaveFieldInfo (Type src, Type des, object srcObj, object desObj, bool shaowError = true)
        {
            System.Reflection.BindingFlags flag = System.Reflection.BindingFlags.Default;
            flag |= System.Reflection.BindingFlags.Public;
            flag |= System.Reflection.BindingFlags.Instance;
            FieldInfo[] fields = src.GetFields (flag);
            FieldInfo[] fieldsSave = des.GetFields (flag);
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo fi = fields[i];
                FieldInfo saveFi = Array.Find (fieldsSave, (field) => { return field.Name == fi.Name; });
                if (saveFi == null)
                {
                    if (shaowError)
                    {
                        var attr = fi.GetCustomAttributes (typeof (NonSerializedAttribute), false).FirstOrDefault ();
                        if (attr == null)
                        {
                            attr = fi.GetCustomAttributes (typeof (CFNoSerializedAttribute), false).FirstOrDefault ();
                            if (attr == null)
                                Debug.LogError (string.Format ("Field {0} not find.", fi.Name));
                        }
                    }
                }
                else
                {
                    object value = fi.GetValue (srcObj);
                    saveFi.SetValue (desObj, value);
                }

            }
        }

        public static string GetSceneObjectPath (Transform trans, bool includeRoot = true)
        {
            if (trans == null)
                return "";
            string sceneObjectPath = trans.name;
            Transform parent = trans.parent;
            if (includeRoot)
            {
                while (parent != null)
                {
                    sceneObjectPath = parent.name + "/" + sceneObjectPath;
                    parent = parent.parent;
                }
            }
            else
            {
                if (parent != null)
                {
                    var pp = parent.parent;
                    while (pp != null)
                    {
                        if (string.IsNullOrEmpty (sceneObjectPath))
                        {
                            sceneObjectPath = parent.name;
                        }
                        else
                        {
                            sceneObjectPath = parent.name + "/" + sceneObjectPath;
                        }

                        parent = parent.parent;
                        pp = pp.parent;
                    }
                }
                else
                {
                    sceneObjectPath = "";
                }

            }

            return sceneObjectPath;
        }

        public static void WriteMatrix (BinaryWriter bw, Matrix4x4 mat)
        {
            for (int m = 0; m < 16; ++m)
            {
                bw.Write (mat[m]);
            }
        }

        public static void WriteVector (BinaryWriter bw, Vector4 vec)
        {
            bw.Write (vec.x);
            bw.Write (vec.y);
            bw.Write (vec.z);
            bw.Write (vec.w);
        }

        public static void WriteVector (BinaryWriter bw, Vector3 vec)
        {
            bw.Write (vec.x);
            bw.Write (vec.y);
            bw.Write (vec.z);
        }

        public static void WriteVector (BinaryWriter bw, Vector2 vec)
        {
            bw.Write (vec.x);
            bw.Write (vec.y);
        }
        public static void WriteVector (BinaryWriter bw, Vector4Int vec)
        {
            bw.Write (vec.x);
            bw.Write (vec.y);
            bw.Write (vec.z);
            bw.Write (vec.w);
        }

        public static void WriteVector (BinaryWriter bw, Vector3Int vec)
        {
            bw.Write (vec.x);
            bw.Write (vec.y);
            bw.Write (vec.z);
        }

        public static void WriteVector (BinaryWriter bw, Vector2Int vec)
        {
            bw.Write (vec.x);
            bw.Write (vec.y);
        }
        public static void WriteColor3(BinaryWriter bw, ref Color c)
        {
            bw.Write(c.r);
            bw.Write(c.g);
            bw.Write(c.b);
        }
        public static void WriteQuaternion (BinaryWriter bw, Quaternion q)
        {
            bw.Write (q.x);
            bw.Write (q.y);
            bw.Write (q.z);
            bw.Write (q.w);
        }

        public static void SaveAABB (BinaryWriter bw, Bounds aabb)
        {
            WriteVector (bw, aabb.min);
            WriteVector (bw, aabb.max);
        }

        public static void SaveAABB (BinaryWriter bw, ref Bounds aabb)
        {
            WriteVector (bw, aabb.min);
            WriteVector (bw, aabb.max);
        }
        public static void SaveAABB (BinaryWriter bw, ref AABB aabb)
        {
            WriteVector (bw, aabb.min);
            WriteVector (bw, aabb.max);
        }
        public static void SaveAABB (BinaryWriter bw, AABB aabb)
        {
            WriteVector (bw, aabb.min);
            WriteVector (bw, aabb.max);
        }

        public static void WriteRes (BinaryWriter bw, ResParam res, bool needRedirect = true)
        {
            if (needRedirect && ResParam.addRes != null)
            {
                if (res.asset != null)
                {
                    ResParam.addRes(res.asset, res.asset.name);
                }
                else if (!string.IsNullOrEmpty (res.value))
                {
                    DebugLog.AddErrorLog2 ("res not find:{0}", res.value);
                }
            }

            res.Save (bw);
        }
        public static Vector2 ReadVector2 (BinaryReader br)
        {
            Vector2 vector;
            vector.x = br.ReadSingle ();
            vector.y = br.ReadSingle ();
            return vector;
        }

        public static Vector3 ReadVector3 (BinaryReader br)
        {
            Vector3 vector;
            vector.x = br.ReadSingle ();
            vector.y = br.ReadSingle ();
            vector.z = br.ReadSingle ();
            return vector;
        }

        public static Vector4 ReadVector4 (BinaryReader br)
        {
            Vector4 vector;
            vector.x = br.ReadSingle ();
            vector.y = br.ReadSingle ();
            vector.z = br.ReadSingle ();
            vector.w = br.ReadSingle ();
            return vector;
        }

        public static void CreateDir (string dir)
        {
            if (!Directory.Exists(dir))
            {
                string name = Path.GetFileNameWithoutExtension(dir);
                string newdir = Path.GetDirectoryName(dir);
                newdir = newdir.Replace("\\", "/");
                if (newdir != dir)
                {
                    CreateDir(newdir);
                    if (!string.IsNullOrEmpty(name))
                        AssetDatabase.CreateFolder(newdir, name);
                }
            }
        }
        public static string GetAssetDir (UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath (obj);
            {
                int index = path.LastIndexOf ("/");
                if (index >= 0)
                {
                    return path.Substring (0, index);
                }
            }
            return "";
        }
        public static void EnumRootObject (EnumTransform cb, object param = null)
        {
            UnityEngine.SceneManagement.Scene s = SceneManager.GetActiveScene ();
            GameObject[] roots = s.GetRootGameObjects ();
            for (int i = 0, imax = roots.Length; i < imax; ++i)
            {
                Transform t = roots[i].transform;
                cb (t, param);
            }
        }
        public static void EnumTargetObject (string goPath, EnumTransform cb, object param = null)
        {
            GameObject go = GameObject.Find (goPath);
            if (go != null)
            {
                Transform t = go.transform;
                for (int i = 0, imax = t.childCount; i < imax; ++i)
                {
                    Transform child = t.GetChild (i);
                    cb (child, param);
                }
            }
        }

        public static void EnumChildObject (Transform t, object param, EnumTransform cb)
        {
            if (cb != null)
            {
                for (int i = 0; i < t.childCount; ++i)
                {
                    Transform child = t.GetChild (i);
                    cb (child, param);
                }
            }
        }

        public static void DestroyChildObjects (GameObject go, string name = "")
        {
            if (go != null)
            {
                Transform t = go.transform;
                if (t.childCount > 0)
                {
                    List<GameObject> children = new List<GameObject> ();
                    for (int i = 0; i < t.childCount; ++i)
                    {
                        Transform child = t.GetChild (i);
                        if (string.IsNullOrEmpty (name) || child.name.StartsWith (name))
                            children.Add (child.gameObject);
                    }

                    for (int i = 0; i < children.Count; ++i)
                    {
                        GameObject.DestroyImmediate (children[i]);
                    }
                }
            }
        }

        public static string GetReplaceStr (string str, string repalceStr = ".")
        {
            str = str.Replace (",", repalceStr);
            str = str.Replace ("٫", repalceStr);
            return str;
        }
        public static string Vec4Str (string name, ref Vector4 vec)
        {
            return string.Format ("{0}:({1:F2},{2:F2},{3:F2},{4:F2})", name, vec.x, vec.y, vec.z, vec.w);
        }

        public static string GetAssetPath (UnityEngine.Object asset, bool allowAnyPath)
        {
            if (asset == null)
                return "";
            string path = AssetDatabase.GetAssetPath (asset);
            if (path.StartsWith (AssetsConfig.instance.ResourcePath))
            {
                path = path.Substring (AssetsConfig.instance.ResourcePath.Length + 1);
                int index = path.LastIndexOf (".");
                if (index >= 0)
                {
                    path = path.Substring (0, index);
                }
            }
            else
            {
                int index = path.LastIndexOf (".");
                if (index >= 0)
                {
                    path = path.Substring (0, index);
                }
                if (!allowAnyPath)
                {
                    path = string.Format ("error path:{0}", path);
                    DebugLog.AddErrorLog2 ("res must under path:{0}", AssetsConfig.instance.ResourcePath);
                }

            }
            return path;
        }
        public static int GetSize (this SpriteSize size)
        {
            int powerof2 = (int) size;
            return 1 << powerof2;
        }
        public static bool HasFlag (uint flag, TexFlag f)
        {
            return (flag & (uint) f) != 0;
        }

        public static void SetFlag (ref uint flag, TexFlag f, bool add)
        {
            if (add)
            {
                flag |= (uint) f;
            }
            else
            {
                flag &= ~((uint) f);
            }
        }
        public static void SetLayer (Transform t, int layer, bool withChild)
        {
            t.gameObject.layer = layer;
            if (withChild)
            {
                for (int i = 0; i < t.childCount; ++i)
                {
                    Transform child = t.GetChild (i);
                    SetLayer (child, layer, withChild);
                }
            }
        }
        public static void SetRenderEnable (Transform t, bool enable, bool withChild)
        {
            Renderer r = t.GetComponent<Renderer> ();
            if (r != null)
                r.enabled = enable;
            if (withChild)
            {
                for (int i = 0; i < t.childCount; ++i)
                {
                    Transform child = t.GetChild (i);
                    SetRenderEnable (child, enable, withChild);
                }
            }
        }

        static List<Renderer> renders = new List<Renderer> ();
        public static List<Renderer> GetRenderers (GameObject go)
        {
            renders.Clear ();
            go.GetComponentsInChildren<Renderer> (true, renders);
            return renders;
        }

        public static int FindRenderIndex (GameObject go, Renderer r)
        {
            var tran = go.transform;
            var t = r.transform;
            for (int i = 0; i < tran.childCount; ++i)
            {
                if (tran.GetChild (i) == t)
                {
                    return i;
                }
            }
            return -1;
        }

        static List<MonoBehaviour> tmpmono = new List<MonoBehaviour> ();
        static List<MonoBehaviour> monos = new List<MonoBehaviour> ();
        public static List<MonoBehaviour> GetScripts<T> (GameObject go) where T : MonoBehaviour
        {
            tmpmono.Clear ();
            monos.Clear ();
            go.GetComponentsInChildren<MonoBehaviour> (true, tmpmono);
            foreach (var mono in tmpmono)
            {
                if (mono is T)
                {
                    monos.Add (mono);
                }
            }
            return monos;
        }

        static List<Component> tmpcomp = new List<Component> ();
        static List<Component> comps = new List<Component> ();
        public static List<Component> GetComps<T> (GameObject go) where T : Component
        {
            tmpcomp.Clear ();
            comps.Clear ();
            go.GetComponentsInChildren<Component> (true, tmpcomp);
            foreach (var comp in tmpcomp)
            {
                if (comp is T)
                {
                    comps.Add (comp);
                }
            }
            return comps;
        }
        private static ReflectFun matGetPropertyFun;
        public static ReflectFun GetMaterialPropertiesFun ()
        {
            if (matGetPropertyFun == null)
            {
                matGetPropertyFun = GetInternalFunction (typeof (ShaderUtil), "GetMaterialProperties", true, true, false, false);
            }
            return matGetPropertyFun;
        }

        public static T CreateAsset<T> (string path, string ext, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            if (asset == null)
                return default (T);

            T existingAsset;

            var assetPath = path;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
            asset.name = fileNameWithoutExtension;

            if (asset is Texture2D)
            {
                Texture2D tex = asset as Texture2D;
                if (ext == ".asset")
                {
                    AssetDatabase.CreateAsset (asset, assetPath);
                }
                else
                {
                    byte[] png = tex.EncodeToPNG ();
                    File.WriteAllBytes (assetPath, png);
                    AssetDatabase.ImportAsset (assetPath);
                }

                // AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();
                existingAsset = AssetDatabase.LoadAssetAtPath<T> (assetPath);
            }
            else if (asset is RenderTexture && typeof (T) == typeof (Texture2D))
            {
                RenderTexture rt = asset as RenderTexture;
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = rt;
                bool hdr = ext == ".exr";
                TextureFormat format = hdr ? TextureFormat.RGBAFloat : TextureFormat.ARGB32;
                Texture2D tex = new Texture2D (rt.width, rt.height, format, false);
                tex.name = fileNameWithoutExtension;
                tex.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
                byte[] bytes = hdr ? tex.EncodeToEXR () : tex.EncodeToTGA ();
                FileStream file = File.Open (assetPath, FileMode.Create);
                BinaryWriter writer = new BinaryWriter (file);
                writer.Write (bytes);
                file.Close ();
                Texture2D.DestroyImmediate (tex);
                tex = null;
                RenderTexture.active = prev;
                AssetDatabase.SaveAssets ();
                AssetDatabase.Refresh ();
                existingAsset = AssetDatabase.LoadAssetAtPath<T> (assetPath);
            }
            else
            {
                existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (existingAsset)
                {
                    if (existingAsset != asset)
                    {
                        if (existingAsset is Material)
                        {
                            Material mat = asset as Material;
                            Material src = existingAsset as Material;
                            src.shader = mat.shader;
                            src.CopyPropertiesFromMaterial(mat);
                            src.shaderKeywords = mat.shaderKeywords;
                        }
                        else
                        {
                            string metaPath = assetPath + ".meta";
                            byte[] metaContent = File.ReadAllBytes(metaPath);
                            AssetDatabase.DeleteAsset(assetPath);
                            AssetDatabase.CreateAsset(asset, assetPath);
                            existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                            if (!existingAsset)
                                DebugLog.AddErrorLog("Load failed: " + assetPath);
                            else
                                existingAsset.name = asset.name;
                            File.Delete(metaPath);
                            File.WriteAllBytes(metaPath, metaContent);
                            AssetDatabase.ImportAsset(assetPath);
                        }
                    }
                    else
                    {
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    AssetDatabase.CreateAsset(asset, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
            }

            return existingAsset;
        }

        public static T CreateAsset<T> (string dirPath, string targetName, string ext, UnityEngine.Object asset) where T : UnityEngine.Object
        {
            return CreateAsset<T> (dirPath + "/" + targetName + ext, ext, asset);
        }

        public static void SaveAsset (UnityEngine.Object obj)
        {
            if (obj && AssetDatabase.Contains(obj))
            {
                EditorUtility.SetDirty (obj);
                AssetDatabase.SaveAssetIfDirty (obj);
            }
        }
        public static void SaveAsset<T> (string path, T obj) where T : ScriptableObject
        {
            if (File.Exists (path))
            {
                SaveAsset (obj);
            }
            else
            {
                string objPath = AssetDatabase.GetAssetPath (obj);
                if (File.Exists (objPath))
                {
                    SaveAsset (obj);
                }
                else
                {
                    CreateAsset<T> (path, ".asset", obj);
                }

            }
        }
        public static void SaveAsset<T> (string path, string ext, T obj) where T : UnityEngine.Object
        {
            if (File.Exists (path))
            {
                SaveAsset (obj);
            }
            else
            {
                string objPath = AssetDatabase.GetAssetPath (obj);
                if (File.Exists (objPath))
                {
                    SaveAsset (obj);
                }
                else
                {
                    CreateAsset<T> (path, ext, obj);
                }

            }
        }
        public static T LoadAsset<T> (string path, bool save = false) where T : ScriptableObject
        {
            ScriptableObject so = null;
            if (File.Exists (path))
            {
                so = AssetDatabase.LoadAssetAtPath<T> (path);
            }
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<T> ();
                if (save)
                {
                    so = CreateAsset<T> (path, ".asset", so);
                }
            }
            return so as T;
        }

        public static void EnumPath (EditorSceneObjectType type, EditorCommon.EnumTransform fun, object context)
        {
            string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[(int) type];
            EditorCommon.EnumTargetObject (path, (trans, param) =>
            {
                fun (trans, context);
            });
        }

        public static void EnumPath (string subPath, EditorCommon.EnumTransform fun, object context)
        {
            string path = string.Format ("{0}/{1}", SceneResConfig.SceneRoot, subPath);
            EditorCommon.EnumTargetObject (path, (trans, param) =>
            {
                fun (trans, context);
            });
        }

        public static bool IsPrefabOrFbx (GameObject go)
        {
            var type = PrefabUtility.GetPrefabAssetType (go);
            return type == PrefabAssetType.Model || type == PrefabAssetType.Regular;
        }
        private static ReflectFun getOriginalSourceOrVariantRoot;
        public static GameObject GetPrefabOrignal (GameObject go)
        {
            if (getOriginalSourceOrVariantRoot == null)
            {
                getOriginalSourceOrVariantRoot = GetInternalFunction (typeof (PrefabUtility), "GetOriginalSourceOrVariantRoot", true, true, false, false);
            }
            if (getOriginalSourceOrVariantRoot != null)
            {
                return getOriginalSourceOrVariantRoot.Call (null, new object[] { go }) as GameObject;
            }
            return null;
        }
        public static string GetPrefabOrignalPath (GameObject go, out string name, out GameObject srcGo)
        {
            name = "";
            srcGo = GetPrefabOrignal (go);
            if (srcGo != null)
            {
                name = srcGo.name;
                return AssetDatabase.GetAssetPath (srcGo);
            }
            return "";
        }

        public static void BeginGroup (string name, bool beginHorizontal = true, float width = 1000, float height = 100, float x = 0)
        {
            BeginGroup (name, new Vector4 (x, 0, width, height), beginHorizontal);
        }

        public static void BeginGroup (string name, Vector4 minMax, bool beginHorizontal)
        {
            if (beginHorizontal)
                EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.BeginVertical (GUI.skin.box,
                GUILayout.MinWidth (minMax.x),
                GUILayout.MinHeight (minMax.y),
                GUILayout.MaxWidth (minMax.z),
                GUILayout.MaxHeight (minMax.w));
            if (!string.IsNullOrEmpty (name))
                EditorGUILayout.LabelField (name, EditorStyles.boldLabel);
        }

        public static void EndGroup (bool endHorizontal = true)
        {
            EditorGUILayout.EndVertical ();
            if (endHorizontal)
                EditorGUILayout.EndHorizontal ();
        }

        public static bool BeginFolderGroup (string name, ref bool folder, float width = 1000, float height = 100, float x = 0)
        {
            folder = EditorGUILayout.Foldout (folder, name);
            if (folder)
            {
                EditorCommon.BeginGroup ("", true, width);

            }
            return folder;
        }
        
        public static void EndFolderGroup ()
        {
            EditorCommon.EndGroup ();
        }
        
        public static void FoldoutGroup<T>(string name, Action gui)
        {
            using (new GUILayout.VerticalScope("box"))
            {
                string modelKey = $"{nameof(EditorCommon)}.{nameof(FoldoutGroup)}.{typeof(T).FullName}.{name}";
                bool foldout = EditorPrefs.GetBool(modelKey, true);
                EditorGUI.BeginChangeCheck();
                foldout = EditorGUILayout.Foldout(foldout, name);
                if (EditorGUI.EndChangeCheck())
                    EditorPrefs.SetBool(modelKey, foldout);
                EditorGUI.indentLevel++;
                if (foldout)
                    gui?.Invoke();
                EditorGUI.indentLevel--;
            }
        }

        public static void BeginScroll (ref Vector2 scroll, int count, int maxLine = 10, float height = -1, float minWidth = 1000)
        {
            count = count > maxLine ? maxLine : count;
            if (height < 0)
            {
                height = count * 21;
            }
            scroll = EditorGUILayout.BeginScrollView (scroll, GUILayout.MinWidth (minWidth), GUILayout.MinHeight (height + 10));
        }

        public static void EndScroll ()
        {
            EditorGUILayout.EndScrollView ();
        }

        public static void FolderSelect(SerializedProperty path, float width = 300)
        {
            EditorGUILayout.PropertyField(path);
            EditorGUI.BeginChangeCheck();
            DefaultAsset asset = null;
            asset = EditorGUILayout.ObjectField("", asset, typeof(DefaultAsset), false, GUILayout.MaxWidth(50)) as DefaultAsset;
            if (EditorGUI.EndChangeCheck())
            {
                path.stringValue = AssetDatabase.GetAssetPath(asset);
            }
        }
        public static BlendMode GetBlendMode (Material material)
        {
            bool alphaTest = material.IsKeywordEnabled ("_ALPHA_TEST");
            bool alphaBlend = material.HasProperty ("_DstBlend") &&
                material.GetInt ("_DstBlend") == (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
            if (!alphaTest && !alphaBlend)
            {
                return BlendMode.Opaque;
            }
            else if (alphaTest)
            {
                return BlendMode.Cutout;
            }
            else if (alphaBlend)
            {
                if (material.HasProperty ("_ZWrite"))
                {
                    int zwrite = material.GetInt ("_ZWrite");
                    if (zwrite == 1)
                        return BlendMode.DepthTransparent;
                }
            }
            return BlendMode.Transparent;
        }

        public static void SetStandardCutoutMode (Material material)
        {
            material.SetFloat ("_Mode", 1);
            material.SetOverrideTag ("RenderType", "TransparentCutout");
            material.SetInt ("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
            material.SetInt ("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt ("_ZWrite", 1);
            material.EnableKeyword ("_ALPHATEST_ON");
            material.DisableKeyword ("_ALPHABLEND_ON");
            material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
        }

        public static bool AssetSelect (ref string path, float width = 300)
        {
            EditorGUILayout.LabelField ("AssetPath", GUILayout.MaxWidth (40));
            path = EditorGUILayout.TextField ("", path, GUILayout.MaxWidth (width));
            EditorGUI.BeginChangeCheck ();
            UnityEngine.Object asset = null;
            asset = EditorGUILayout.ObjectField ("Drag Asset Here", asset, typeof (UnityEngine.Object), false, GUILayout.MaxWidth (300)) as UnityEngine.Object;
            if (EditorGUI.EndChangeCheck ())
            {
                path = AssetsPath.GetAssetPath (asset, out var ext);
                return true;
            }
            return false;
        }

        static List<Component> compList = new List<Component>();
        public static int FindMissingScript(Transform trans,bool delete)
        {
            compList.Clear();
            trans.GetComponents(compList);
            int count = compList.Count;
            if(delete)
            {
                RemoveMissingScript(trans);
            }
            return count;
        }

        public static void RemoveMissingScript(Transform trans)
        {
            SerializedObject so = null;
            for (int i = compList.Count - 1; i >= 0; --i)
            {
                var comp = compList[i];
                if (comp == null)
                {
                    if (so == null)
                    {
                        so = new SerializedObject(trans.gameObject);
                    }
                    var sp = so.FindProperty("m_Component");
                    if (sp != null)
                        sp.DeleteArrayElementAtIndex(i);
                }
            }
            compList.Clear();
        }

        public static List<string> CheckInvalidAssetPaths(IReadOnlyList<string> assetPaths, bool logError)
        {
            List<string> invalidPaths = new List<string>();
            for (int i = 0; i < assetPaths.Count; i++)
            {
                string path = assetPaths[i];
                if (!IsAssetPathValid(path))
                {
                    invalidPaths.Add(path);
                }
            }

            if (logError && invalidPaths.Count > 0)
            {
                int logLength = 0;
                for (int i = 0; i < invalidPaths.Count; i++)
                    logLength += invalidPaths[i].Length + 1;
                invalidPaths.Sort();
                string title = $"{invalidPaths.Count}个不规范路径:\n";
                logLength += title.Length;
                StringBuilder sb = new StringBuilder(logLength);
                sb.Append(title);
                for (int i = 0; i < invalidPaths.Count; i++)
                {
                    sb.AppendLine(invalidPaths[i]);
                }
                Debug.LogError(sb.ToString());
            }

            return invalidPaths;
        }

        public static List<UnityEngine.Object> CheckInvalidSubAssets(UnityEngine.Object asset, EditorMessageType logError)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(assetPath))
            {
                CheckInvalidSubAssetNames(assetPath, logError);
            }
            return null;
        }

        public static List<UnityEngine.Object> CheckInvalidSubAssetNames(string assetPath, EditorMessageType logType, Func<UnityEngine.Object, bool> filter = null)
        {
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            List<UnityEngine.Object> invalidSubAssets = new List<UnityEngine.Object>();
            foreach (var subAsset in objs)
            {
                if (AssetDatabase.IsSubAsset(subAsset))
                {
                    if ((filter == null || filter(subAsset)) && !IsAssetNameValid(subAsset.name))
                    {
                        invalidSubAssets.Add(subAsset);
                    }
                }
            }

            if (logType != EditorMessageType.Silence && invalidSubAssets.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"子资源命名不规范, 资源路径 = \"{assetPath}\":");
                foreach (UnityEngine.Object invalidSubAsset in invalidSubAssets)
                {
                    stringBuilder.Append("\t(");
                    stringBuilder.Append(invalidSubAsset.GetType());
                    stringBuilder.Append(") : ");
                    stringBuilder.Append(invalidSubAsset.name);
                    stringBuilder.AppendLine();
                }
                Notificate("检查子资源路径结果", stringBuilder.ToString(), logType);
            }

            return invalidSubAssets;
        }

        public static void Notificate(string title, string message, EditorMessageType type)
        {
            switch (type)
            {
                case EditorMessageType.Silence:
                    break;
                case EditorMessageType.LogDebug:
                    Debug.Log($"{title}\n{message}");
                    break;
                case EditorMessageType.LogWarning:
                    Debug.LogWarning($"{title}\n{message}");
                    break;
                case EditorMessageType.LogError:
                    Debug.LogError($"{title}\n{message}");
                    break;
                case EditorMessageType.Dialog:
                    EditorUtility.DisplayDialog(title, message, "OK");
                    break;
                default:
                    break;
            }
        }

        [MenuItem("Tools/引擎/资源检查/打印不正确的路径")]
        public static void LogInvalidPaths()
        {
            string[] paths = AssetDatabase.GetAllAssetPaths();
            CheckInvalidAssetPaths(paths, true);
        }

        private static readonly string[] ignoredStarts = new string[]
        {
            "Packages/",
            "Assets/Editor Default Resources/",
            "Assets/Tools/Azure[Sky] Dynamic Skybox/",
            "Assets/Plugins/",
            "Assets/Engine/Package/",
        };

        private static readonly string[] ignoredContains = new string[]
        {
            "/Editor/",
            "/Doc/",
            "/Test/",
        };

        private static readonly string[] ignoredEnds = new string[]
        {
            ".unitypackage",
        };

        public static bool IsAssetPathValid(string assetPath)
        {
            for (int i = 0; i < assetPath.Length; i++)
            {
                foreach (string start in ignoredStarts)
                    if (assetPath.StartsWith(start))
                        return true;
                foreach (string end in ignoredEnds)
                    if (assetPath.EndsWith(end))
                        return true;
                foreach (string contains in ignoredContains)
                    if (assetPath.Contains(contains))
                        return true;

                char c = assetPath[i];
                if (   (c < 'a' || c > 'z') 
                    && (c < 'A' || c > 'Z') 
                    && (c < '0' || c > '9') 
                    && c != '_' 
                    && c != '/' 
                    && c != '\\'
                    && c != '.')
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsAssetNameValid(string assetName)
        {
            for (int i = 0; i < assetName.Length; i++)
            {
                char c = assetName[i];
                if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && c != '_' && (c < '0' || c > '9') && c != '-')
                {
                    return false;
                }
            }
            return true;
        }

        public static void ProcessCheckableComponents(GameObject gameObject)
        {
            ICheckableComponent[] components = gameObject.GetComponentsInChildren<ICheckableComponent>(true);
            foreach (ICheckableComponent component in components)
            {
                component.Process();
            }
        }
    }

}
#endif