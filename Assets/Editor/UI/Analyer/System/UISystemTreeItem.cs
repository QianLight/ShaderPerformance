using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace UIAnalyer
{
    public class UISystemTreeItem : TreeViewItem
    {
        private UISystemInfo m_source;
        public UISystemInfo Source { get { return m_source; } }

        public UISystemTreeItem(UISystemInfo s, int depth, Texture iconTexture) : base(s.hasCode, depth, s.displayName)
        {
            m_source = s;
            icon = iconTexture as Texture2D;
            children = new List<TreeViewItem>();
        }

        public override string displayName
        {
            get
            {
                return m_source.displayName;
            }
        }
    }

    public class UISystemFolderInfo : UISystemInfo
    {
        public Dictionary<int,UISystemInfo> m_children = null;

        public override void Setup(SystemDefineData _define, UISystemFolderInfo parent)
        {
            base.Setup(_define, parent);
            m_children =new Dictionary<int, UISystemInfo>();
        }

        public TSource AddFolder<TSource>(SystemDefineData define) where TSource : UISystemInfo, new()
        {
            TSource source = null;
            AddFolder<TSource>(define, ref source);
            return source;
        }

    
        public virtual void AddFolder<TSource>(SystemDefineData define, ref TSource folder) where TSource : UISystemInfo, new()
        {
            UISystemInfo source = null;
            int hasCode = define.id;
            if (m_children.TryGetValue(hasCode, out source))
            {
                folder = source as TSource;
                return;
            }
            folder = XDataPool<TSource>.GetData();
            folder.Setup(define, this);
            m_children.Add(hasCode, folder);
        }


        public override UISystemTreeItem CreateTreeItem(int depth)
        {
            var result = new UISystemTreeItem(this, depth,UISourceUtils.Icon(UISourceUtils.FolderIcon));

            foreach (var child in m_children)
            {
                result.AddChild(child.Value.CreateTreeItem(depth + 1));
            }
            return result;
        }

        public override void Clear()
        {
            if (m_children != null)
            {
                foreach (KeyValuePair<int, UISystemInfo> pair in m_children)
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
    }

    public class UISystemInfo : XDataBase
    {
        private SystemDefineData m_data;

        private UISystemFolderInfo m_parent;
        private int m_hashCode;

        public int hasCode { get { return m_hashCode; } }

        public SystemDefineData data { get { return m_data; } }

        public virtual void Setup(SystemDefineData _define = null, UISystemFolderInfo parent = null)
        {
            m_data = _define;
            m_parent = parent;
            m_hashCode = (_define == null ? 1:_define.id);
        }

        public virtual string displayName
        {
        
            get { 
                if(m_data == null) return "root";
                return m_data.displayName; }
        }

        public virtual UISystemTreeItem CreateTreeItem(int depth)
        {
            return new UISystemTreeItem(this, depth, UISourceUtils.Icon(UISourceUtils.FolderIcon));
        }
        

        public virtual void Clear(){
            
        }

        public static void ReturnInstance<T>(T source) where T : UISystemInfo, new()
        {
            if (source == null) return;
            source.Recycle();
            XDataPool<T>.Recycle(source);
        }

    }
}