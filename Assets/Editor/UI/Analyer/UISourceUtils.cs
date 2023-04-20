using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.CFUI;
using CFEngine;
namespace UIAnalyer
{
    public class UISourceUtils
    {
        // public static string m_prefab_url = "BundleRes/UI/OPsystemprefab";      
        public static string[] filePaths = {"BundleRes/UI/OPsystemprefab" , "BundleRes/UI/system"};

        private static string[] m_files;
        private static string keyStr = "{0}_{1}";
        private static UISourceFolderInfo m_current;
        private static Dictionary<string, UISourceFolderInfo> m_uses;
        public static string[] files { get { return m_files; } }

        public static void InitSource()
        {
            if (m_uses == null) m_uses = new Dictionary<string, UISourceFolderInfo>();
            m_uses.Clear();
            // string path = Application.dataPath + "/" + m_prefab_url;
            // m_files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
            List<string> temp = ListPool<string>.Get();
            for( int i = 0; i < filePaths.Length ;i++ )
            {           
                string path = Application.dataPath + "/" + filePaths[i];
                temp.AddRange(Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories));
            }

            m_files = temp.ToArray();
            ListPool<string>.Release(temp);
        }

        #region Selection

        public static string GetKey(string f, string s)
        {
            return string.Format(keyStr, f, s);
        }
        public static string GetFileName(string path)
        {
            return path.Substring(path.LastIndexOf("\\") + 1).Replace(".png", "");
        }

        public static string GetPath(Transform transform)
        {
            string fname = transform.name;
            while (transform.parent != null && transform.parent.parent != null)
            {
                fname = transform.parent.name + "/" + fname;
                transform = transform.parent;
            }
            return fname;
        }

        public static void FindDependences(UISourceFileInfo file, UISourceType sourceType, ref UISourceFolderInfo folder)
        {
            string useFile = "";
            string[] files = UISourceUtils.files;
            for (int i = 0; i < files.Length; i++)
            {
                useFile = files[i].Substring(files[i].IndexOf("Assets"));
                GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject>(useFile);
                if (temp == null) continue;
                FindDependencesInPrefab(file, temp.transform, temp.name, sourceType, ref folder);
            }
        }

        private static void FindDependencesInPrefab(UISourceFileInfo file, Transform transform, string url, UISourceType sourceType, ref UISourceFolderInfo folder)
        {

            UISourcePrefabInfo prefabInfo = null;
            UISourceFolderInfo childfolder = null;
            if (sourceType == UISourceType.Sprite)
            {
                CFImage[] images = transform.GetComponentsInChildren<CFImage>(true);
                if (images == null || images.Length == 0) return;

                for (int i = 0; i < images.Length; i++)
                {
                    if (string.IsNullOrEmpty(images[i].m_AtlasName) || string.IsNullOrEmpty(images[i].m_SpriteName)) continue;
                    if (file.shortName == images[i].m_SpriteName && file.parent.shortName == images[i].m_AtlasName)
                    {
                        if (!folder.Contains(transform.name))
                        {
                            childfolder = folder.AddFolder<UISourceFolderInfo>(transform.name);
                            childfolder.temp = transform;
                        }
                        else
                            childfolder = folder.children[transform.name] as UISourceFolderInfo;

                        childfolder.AddFolder<UISourcePrefabInfo>(GetKey(transform.name, i.ToString()), ref prefabInfo);
                        prefabInfo.usePath = GetPath(images[i].transform);
                    }
                }
            }
            else if (sourceType == UISourceType.Texture)
            {
                CFRawImage[] raws = transform.GetComponentsInChildren<CFRawImage>(true);
                if (raws == null || raws.Length == 0) return;
                for (int i = 0; i < raws.Length; i++)
                {
                    if (string.IsNullOrEmpty(raws[i].m_TexPath)) continue;
                    int last = raws[i].m_TexPath.LastIndexOf('/');
                    string fileName = raws[i].m_TexPath.Substring(last + 1);
                    string path = raws[i].m_TexPath.Substring(0, last);
                    string dirName = path.Substring(path.LastIndexOf('/') + 1);

                    if (file.shortName == fileName && file.parent.shortName == dirName)
                    {
                        Debug.Log("FindDependencesInPrefab:");
                        if (!folder.Contains(transform.name))
                        {
                            childfolder = folder.AddFolder<UISourceFolderInfo>(transform.name);
                            childfolder.temp = transform;
                        }
                        else
                            childfolder = folder.children[transform.name] as UISourceFolderInfo;

                        childfolder.AddFolder<UISourcePrefabInfo>(GetKey(transform.name, i.ToString()), ref prefabInfo);
                        prefabInfo.usePath = GetPath(raws[i].transform);
                    }
                }
            }
        }

        public static void OpenAsset(UISourceInfo prefab, UISourceInfo target = null)
        {

            GameObject peart = GameObject.Find("Canvas");
            GameObject go = null;
            GameObject newGo;
            if (peart != null)
                go = GameObject.Find("Canvas/" + prefab.shortName.Replace(".prefab", ""));
            else
                go = GameObject.Find(prefab.shortName.Replace(".prefab", ""));
            if (go == null)
            {
                string fullname = prefab.fullname;
                if(fullname.IndexOf("Assets") >= 0){
                    fullname = fullname.Substring(fullname.IndexOf("Assets"));
                }
                go = AssetDatabase.LoadAssetAtPath<GameObject>(fullname);
                if (peart != null)
                {
                    newGo = PrefabUtility.InstantiatePrefab(go, peart.transform) as GameObject;
                }
                else
                {
                    newGo = PrefabUtility.InstantiatePrefab(go) as GameObject;
                }
            }
            else
            {

                newGo = go;

            }
            if (target != null)
            {
                Transform node = newGo.transform.Find(target.fullname);
                Selection.activeTransform = node;
            }
        }
        #endregion

        #region  Icon

        public static string atlas_url = "Assets/BundleRes/UI/atlas";
        public static string SpriteAtlasIcon = "SpriteAtlas Icon";
        public static string PrefabIcon = "Prefab Icon";

        public static string SpriteIcon = "Sprite Icon";

        public static string FolderIcon = "Folder Icon";

        public static string FolderEmptyIcon = "FolderEmpty Icon";
        public static string TransformIcon = "Transform Icon";
        public static Texture Icon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName).image;
        }
        #endregion
    }
    #region Data
    public abstract class XDataBase
    {
        public bool bRecycled = true;
        public virtual void Recycle()
        {
        }

        public virtual void Init()
        {
        }
    }
    public class XDataPool<T> where T : XDataBase, new()
    {
        private static Queue<T> _pool = new Queue<T>();

        public static T GetData()
        {
            if (_pool.Count > 0)
            {
                T t = _pool.Dequeue();

                t.Init();
                t.bRecycled = false;
                return t;
            }
            else
            {
                T t = new T();
                t.Init();
                t.bRecycled = false;
                return t;
            }
        }

        public static void Recycle(T data)
        {
            if (!data.bRecycled)
            {
                _pool.Enqueue(data);
                data.bRecycled = true;
            }
        }
    }
    #endregion
}