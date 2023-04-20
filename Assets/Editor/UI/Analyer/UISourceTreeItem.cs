using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace UIAnalyer
{
    public class UISourceTreeItem : TreeViewItem
    {
        private UISourceInfo m_source;
        public UISourceInfo Source { get { return m_source; } }

        public UISourceTreeItem(UISourceInfo s, int depth, Texture iconTexture) : base(s.hashCode, depth, s.displayName)
        {
            m_source = s;
            icon = iconTexture as Texture2D;
            children = new List<TreeViewItem>();
        }

        public void SetParent(TreeViewItem value){

                UISourceTreeItem item = value as UISourceTreeItem;
                m_source.SetParent(item.Source as UISourceFolderInfo);
                base.parent = value;
        }

        public override string displayName
        {
            get
            {
                return m_source.shortName + m_source.Info ;
            }
        }
    }
}