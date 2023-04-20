using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEditor.IMGUI.Controls;
namespace UIAnalyer
{
    public enum UISource
    {
        Folder,
        File,
        Prefab,
        Transform
    }
    public class UISourceInfo : XDataBase
    {
        public static string displayUseFolderStr = "{0} ({1})";
        public static string displayPrefabStr = "{0}  ({1})";
        public static void ReturnInstance<T>(T source) where T : UISourceInfo, new()
        {
            if (source == null) return;
            source.Recycle();
            XDataPool<T>.Recycle(source);
        }

        public void Remove()
        {
            parent.RemoveFile(m_fullname);
            ReturnInstance(this);
        }

        private string m_shortname;

        private string m_fullname;

        private int m_hashCode;

        public string shortName { get { return m_shortname; } }
        protected string normalIcon = string.Empty;


        public int hashCode { get { return m_hashCode; } }

        public string fullname { get { return m_fullname; } }

        public virtual string Info{get {return  "";}}



        private UISourceFolderInfo m_forder;


        public override void Init()
        {
            m_shortname = string.Empty;
            m_fullname = string.Empty;
        }
        public virtual void Clear() { }
        public override void Recycle()
        {
            Clear();
            m_forder = null;
            base.Recycle();
        }
        public virtual UISource type { get { return UISource.File; } }

        public virtual string displayName
        {
            get { return m_shortname; }
        }

        public UISourceFolderInfo parent { get { return m_forder; } }

        public void SetParent(UISourceFolderInfo folder){
            Remove();
            folder.children.Add(fullname , this);
            this.m_forder = folder;
        }

        public bool IsSource(UISource st) { return type == st; }
        public virtual void Setup(string name, UISourceFolderInfo parent, string iconName = "")
        {
            m_fullname = name;
            m_forder = parent;
            normalIcon = iconName;
            m_shortname = ToShortName(m_fullname);
            m_hashCode = m_fullname.GetHashCode();
        }

        public virtual bool RowGUI(Rect rect){
            
            return false;
        }

        public virtual string ToShortName(string path)
        {
            return path.Substring(path.LastIndexOf("\\") + 1);
        }

        protected virtual string GetIcon()
        {
            return string.IsNullOrEmpty(normalIcon) ? UISourceUtils.FolderIcon : normalIcon;
        }
        public virtual UISourceTreeItem CreateTreeItem(int depth)
        {
            return new UISourceTreeItem(this, depth, UISourceUtils.Icon(GetIcon()));
        }

        public virtual bool TryGetChild(string name, ref UISourceInfo source)
        {
            return false;
        }
    }


    #region  Prefab

    public class UISourcePrefabInfo : UISourceInfo
    {

        public override UISource type { get { return UISource.Prefab; } }

        public string usePath;

        public Transform transform = null;

        public override string ToShortName(string path)
        {
            return base.ToShortName(path).Replace(".prefab", "");
        }

        public override string displayName
        {

            get
            {
                return string.Format(displayPrefabStr, shortName, fullname);
            }
        }

        protected override string GetIcon()
        {
            return UISourceUtils.PrefabIcon;
        }
        public override void Clear()
        {
            usePath = string.Empty;
            transform = null;
            base.Clear();
        }
    }

    public class UISourceTransInfo : UISourceInfo
    {
        public override UISource type { get { return UISource.Transform; } }

        public UISourceInfo target = null;
        public string sprite;  //节点路径

        public UISourceType uType;
        public bool IsExist = true;

        public override void Clear()
        {
            sprite = string.Empty;
            IsExist = false;
            base.Clear();
        }

        protected override string GetIcon()
        {
            return UISourceUtils.TransformIcon;
        }

        public override bool RowGUI(Rect rect)
        {
            var with = rect.width *0.7f;
            var pos = new  Rect(rect.x+with,rect.y , rect.width - with, rect.height);
            EditorGUI.LabelField(pos,sprite);
            return false;
        }

    }

    #endregion

    #region  Png

    public class UISourceFileInfo : UISourceInfo
    {
        public override UISource type { get { return UISource.File; } }
        public bool Need { get; set; }

        public Texture texture = null;
        public override string ToShortName(string path)
        {
            return base.ToShortName(path).Replace(".png", "");
        }

        public override string Info {
            get
            {

                if(texture == null){
                    string target = fullname.Substring(fullname.IndexOf("Assets"));
                    texture =  AssetDatabase.LoadAssetAtPath<Texture>(target);
                }
               if(texture == null) return "";
                return string.Format("       {0}*{1}" ,texture.width,texture.height );
            }
        }

        public override void Clear()
        {
            Need = false;
            base.Clear();
        }



        public override UISourceTreeItem CreateTreeItem(int depth)
        {
            return new UISourceTreeItem(this, depth, UISourceUtils.Icon(UISourceUtils.SpriteIcon));
        }
    }
    public class UISourceFolderInfo : UISourceInfo
    {
        private Dictionary<string, UISourceInfo> m_children;

        public Dictionary<string, UISourceInfo> children { get { return m_children; } }
        public override UISource type { get { return UISource.Folder; } }

        public Transform temp;

        public override void Setup(string name, UISourceFolderInfo parent, string normalIcon = "")
        {
            base.Setup(name, parent, normalIcon);
            if (m_children == null) m_children = new Dictionary<string, UISourceInfo>();
        }

        public override void Clear()
        {
            if (m_children != null)
            {
                foreach (KeyValuePair<string, UISourceInfo> pair in m_children)
                {
                    if (pair.Value != null)
                    {
                        pair.Value.Clear();
                        ReturnInstance(pair.Value);
                    }
                }
                m_children.Clear();
            }
        }
        public override void Recycle()
        {
            Clear();
            base.Recycle();
        }

        public virtual void RemoveFile(string path)
        {
            if (m_children.ContainsKey(path))
            {
                m_children[path].Recycle();
                ReturnInstance(m_children[path]);
                m_children.Remove(path);
            }
        }

        public virtual bool Contains(string path){
            return m_children.ContainsKey(path);
        }

        public TSource AddFolder<TSource>(string path, string normalIcon = "") where TSource : UISourceInfo, new()
        {
            TSource source = null;
            AddFolder<TSource>(path, ref source, normalIcon);
            return source;
        }
        public virtual void AddFolder<TSource>(string path, ref TSource folder, string normalIcon = "") where TSource : UISourceInfo, new()
        {
            UISourceInfo source = null;
            if (m_children.TryGetValue(path, out source))
            {
                folder = source as TSource;
                return;
            }
            folder = XDataPool<TSource>.GetData();
            folder.Setup(path, this, normalIcon);
            m_children.Add(path, folder);
        }


        public override UISourceTreeItem CreateTreeItem(int depth)
        {
            var result = new UISourceTreeItem(this, depth, UISourceUtils.Icon(string.IsNullOrEmpty(normalIcon) ? UISourceUtils.FolderIcon : normalIcon));

            foreach (var child in m_children)
            {
                result.AddChild(child.Value.CreateTreeItem(depth + 1));
            }
            return result;
        }
    }
    #endregion

}