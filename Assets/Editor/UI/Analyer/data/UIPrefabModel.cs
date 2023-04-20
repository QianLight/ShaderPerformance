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

    public class UIPrefabModel
    {
        private string m_prefab_url = "BundleRes/UI/OPsystemprefab";
        private string m_prefab_url2 = "BundleRes/UI/system";
        // private string m_uisource_url = "BundleRes/UI/UISource";
        //private string m_uibackground_url = "BundleRes/UIBackground";

        private string m_atlas_url = "BundleRes/UI/atlas";

        private UISourceFolderInfo m_folder;

        private UISourcePrefabInfo m_prefabInfo;
        private UISourceFolderInfo m_current;
        private UISourceType m_sourceType;
        private int useFileCount = 0;
        private int useDirCount = 0;

        private StringBuilder m_message = new StringBuilder();

        public string Message{
            get{ return m_message.ToString(); }
        }

        public UISourceFolderInfo folder { get { return m_folder; } }
        public UISourceFolderInfo current { get { return m_current; } }
        private UISourceFolderInfo m_textureFolder;
        private UISourceFolderInfo m_spriteFolder;

        public void SetSeletion(UISourcePrefabInfo select)
        {
            m_prefabInfo = select;
            ClearSelection();
            LoadSelection();
        }

        private void LoadSelection()
        {

            string useFile = m_prefabInfo.fullname.Substring(m_prefabInfo.fullname.IndexOf("Assets"));
            GameObject temp = AssetDatabase.LoadAssetAtPath<GameObject>(useFile);
            if (temp == null) return;
            AddPrefabToUseFolder(temp.transform, useFile);
        }

        private bool IsValid(string atlasName)
        {
            return File.Exists(atlasName);
        }

    

        private void AddMessage(string message)
        {
            Debug.Log("message:" + message);
            if (m_message.Length > 0)
                m_message.Append("、");
            m_message.Append(message);
        }

        private void AddPrefabToUseFolder(Transform transform, string url)
        {

            UISourceFolderInfo folder = null;
            CFImage[] images = transform.GetComponentsInChildren<CFImage>(true);

            if (images != null && images.Length > 0)
            {

                for (int i = 0; i < images.Length; i++)
                {
                    folder = null;
                    if (string.IsNullOrEmpty(images[i].m_AtlasName)) continue;
                    UISourceInfo source = null;
                    if (m_spriteFolder.TryGetChild(images[i].m_AtlasName, ref source))
                    {
                        folder = source as UISourceFolderInfo;
                    }
                    else if (IsValid(Application.dataPath + "/"+ m_atlas_url + "/" + images[i].m_AtlasName + ".spriteatlas"))
                    {
                        m_spriteFolder.AddFolder<UISourceFolderInfo>(images[i].m_AtlasName, ref folder,UISourceUtils.SpriteAtlasIcon);
                    }
                    else
                    {
                        AddMessage(images[i].m_AtlasName);
                        continue;
                    }

                    if (folder != null)
                    {
                        string node = UISourceUtils.GetPath(images[i].transform);
                        UISourceTransInfo trans = folder.AddFolder<UISourceTransInfo>(node);
                        trans.sprite = images[i].m_SpriteName;
                        trans.uType = UISourceType.Sprite;
                        trans.target = m_prefabInfo;
                    }

                }
            }

            CFRawImage[] raws = transform.GetComponentsInChildren<CFRawImage>(true);
            if (raws != null && raws.Length > 0)
            {
                for (int i = 0; i < raws.Length; i++)
                {
                    if (string.IsNullOrEmpty(raws[i].m_TexPath)) continue;
                    int last = raws[i].m_TexPath.LastIndexOf('/');
                    string fileName = raws[i].m_TexPath.Substring(last + 1);
                    string path = raws[i].m_TexPath.Substring(0, last);
                    string dirName = path.Substring(path.LastIndexOf('/') + 1);
                    UISourceInfo source = null;
                    if (m_textureFolder.TryGetChild(dirName, ref source))
                    {
                        folder = source as UISourceFolderInfo;
                    }
                    else
                    {
                        string fullPath = Application.dataPath + "/BundleRes/" + raws[i].m_TexPath + ".png";
                        if (IsValid(fullPath))
                        {
                            m_textureFolder.AddFolder<UISourceFolderInfo>(dirName, ref folder);
                        }
                        else
                        {
                            AddMessage(UISourceUtils.GetPath(raws[i].transform));
                            continue;
                        }
                    }
                    UISourceTransInfo trans = folder.AddFolder<UISourceTransInfo>(UISourceUtils.GetPath(raws[i].transform));
                    trans.sprite = fileName;
                    trans.uType = UISourceType.Texture;
                    trans.target = m_prefabInfo;
                }
            }

        }
        public void Clear()
        {
            
    ClearSelection();
            if(m_folder != null) m_folder.Clear();
        }

        private void ClearSelection(){
        if (m_textureFolder != null) m_spriteFolder.Clear();
            if (m_spriteFolder != null) m_textureFolder.Clear();
        }

        public override string ToString()
        {
            return string.Format("识别文件夹:{0} , 识别文件:{1}", useDirCount, useFileCount);
        }

        #region Prefab


        #endregion


        #region Prefab


        public void InitUISourceInfo()
        {
            if (m_folder == null)
            {
                m_folder = new UISourceFolderInfo();
                m_folder.Setup("root", null);
            }
            else
            {
                m_folder.Clear();
            }

            for(int i = 0; i < UISourceUtils.filePaths.Length ; i ++){
                string path = Application.dataPath + "/" + UISourceUtils.filePaths[i];
                AddDirectoryToDic(path, m_folder);
            }

            
            // string path = Application.dataPath + "/" + m_prefab_url;
            // AddDirectoryToDic(path, m_folder);

            if (m_current == null)
            {
                m_current = new UISourceFolderInfo();
                m_current.Setup("root", null);
                m_current.AddFolder<UISourceFolderInfo>("Sprite", ref m_spriteFolder);
                m_current.AddFolder<UISourceFolderInfo>("Texture", ref m_textureFolder);
            }
            else
            {
                m_spriteFolder.Clear();
                m_textureFolder.Clear();
            }

        }


        private void AddDirectoryToDic(string path, UISourceFolderInfo folder)
        {
            AddFolders(path, folder);
            AddFiles(path, folder);
        }

        private void AddFiles(string path, UISourceFolderInfo folder)
        {
            string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.TopDirectoryOnly);
            if (files == null || files.Length == 0) return;
            for (int i = 0; i < files.Length; i++)
            {
                folder.AddFolder<UISourcePrefabInfo>(files[i]);
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
    }
}