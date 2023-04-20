using System.Security.Cryptography.X509Certificates;

using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine.CFUI;
// using UnityEngine.CFUI;

namespace UIAnalyer
{

    public class UISourceModel
    {
        public static string m_uisource_url = "BundleRes/UI/UISource";
        public static string m_uibackground_url = "BundleRes/UIBackground";

        private UISourceFolderInfo m_folder;

        private UISourceType m_sourceType;
        private int useFileCount = 0;
        private int useDirCount = 0;

        public UISourceFolderInfo folder { get { return m_folder; } }


        public override string ToString()
        {
            return string.Format("识别文件夹:{0} , 识别文件:{1}", useDirCount, useFileCount);
        }

        #region Prefab


        #endregion


        #region File

        public void Init(){
            if(m_folder == null) {
                m_folder = new UISourceFolderInfo();
                m_folder.Setup("root", null);
            }

            if(m_selection == null){
                m_selection = new UISourceFolderInfo();
                m_selection.Setup("root",null);
            }
        }

        public  void Clear(){
            if(m_folder != null) m_folder.Clear();
            if(m_selection != null) m_selection.Clear();
        }

        public void InitUISourceInfo( UISourceType sourceType)
        {
            m_folder.Clear();
            m_selection.Clear();
            m_sourceType = sourceType;
            string path = string.Empty;
            if(sourceType == UISourceType.Texture)
                path = Application.dataPath + "/" + m_uibackground_url;
            else
                path = Application.dataPath +"/" + m_uisource_url;        
            AddDirectoryToDic(path, m_folder);
        }

        
        private void AddDirectoryToDic(string path, UISourceFolderInfo folder)
        {
            AddFolders(path, folder);
            AddFiles(path, folder);
        }

        private void AddFiles(string path, UISourceFolderInfo folder)
        {
            string[] files = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
            if (files == null || files.Length == 0) return;
            for (int i = 0; i < files.Length; i++)
            {
                folder.AddFolder<UISourceFileInfo>(files[i]);
                useFileCount++;
            }
        }

        private void AddFolders(string path, UISourceFolderInfo folder)
        {
            string[] folders = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            if (folders == null || folders.Length == 0) return;
            UISourceFolderInfo childfolder = null;
            for (int i = 0; i < folders.Length; i++)
            {
                folder.AddFolder(folders[i], ref childfolder);
                AddDirectoryToDic(folders[i], childfolder);
                useDirCount++;
            }
        }
        #endregion


#region  Selection

        private UISourceFolderInfo m_selection ;
        public UISourceFolderInfo selection{get{ return m_selection;}}

        public UISourceInfo m_selectInfo;

        public bool ContainsSelection(UISourceInfo sourceInfo){
            if(m_selectInfo ==  null || m_selection.children.Count == 0) return true;
            if(m_selection.Contains(sourceInfo.shortName)){
                UISourceFolderInfo folder = m_selection.children[sourceInfo.shortName]as UISourceFolderInfo;
                return folder.children.Count > 0;
            }
            return true;
        }

        public  void SetSeletion(UISourceInfo select)
        {
            if (m_selectInfo == select) return;
            m_selection.Clear();
            m_selectInfo = select;
            InitUseFileInfo(m_selectInfo);
            FindDependences();
        }
        private  void AddUseFolder(UISourceFileInfo source)
        {
            source.Need = true;
            m_selection.AddFolder<UISourceFolderInfo>(source.shortName);
        }

        private  void InitUseFolderInfo(UISourceFolderInfo source)
        {
            if (source.children == null || source.children.Count == 0) return;
            foreach (KeyValuePair<string, UISourceInfo> pair in source.children)
            {
                InitUseFileInfo(pair.Value);
            }
        }

        private  void InitUseFileInfo(UISourceInfo source)
        {

            if (source.IsSource(UISource.File))
            {
                AddUseFolder(source as UISourceFileInfo);
            }
            else if (source.IsSource(UISource.Folder))
            {
                InitUseFolderInfo(source as UISourceFolderInfo);
            }
        }

        private  void FindDependences()
        {
            string useFile = "";
            string[] files = UISourceUtils.files;
            for (int i = 0; i < files.Length; i++)
            {
                useFile = files[i].Substring(files[i].IndexOf("Assets"));
                GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject>(useFile);
                if (temp == null) continue;
                AddPrefabToUseFolder(temp.transform, useFile);
            }
        }

        private  void AddPrefabToUseFolder(Transform transform, string url)
        {
            UISourceFolderInfo folder = null;
            UISourceFolderInfo prefab = null;
            if (m_sourceType == UISourceType.Sprite)
            {
                CFImage[] images = transform.GetComponentsInChildren<CFImage>(true);
                if (images == null || images.Length == 0) return;

                for (int i = 0; i < images.Length; i++)
                {
                    if (string.IsNullOrEmpty(images[i].m_AtlasName) || string.IsNullOrEmpty(images[i].m_SpriteName)) continue;
                    if(!m_selection.Contains(images[i].m_SpriteName)) continue;
                    folder =m_selection.children[images[i].m_SpriteName] as UISourceFolderInfo;
                    prefab = folder.Contains(url)?folder.children[url] as UISourceFolderInfo : folder.AddFolder<UISourceFolderInfo>(url,UISourceUtils.PrefabIcon); 
                    string path = UISourceUtils.GetPath(images[i].transform);
                    prefab.AddFolder<UISourceTransInfo>(path,UISourceUtils.TransformIcon);
                }
            }
            else if (m_sourceType == UISourceType.Texture)
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

                    if(!m_selection.Contains(fileName)) continue;
                    folder =m_selection.children[fileName] as UISourceFolderInfo;
                    prefab = folder.Contains(url)?folder.children[url] as UISourceFolderInfo : folder.AddFolder<UISourceFolderInfo>(url,UISourceUtils.PrefabIcon); 
                    string node =UISourceUtils.GetPath(raws[i].transform);
                    prefab.AddFolder<UISourceTransInfo>(node,UISourceUtils.TransformIcon);
                }
            }
        }
        #endregion

    }
}