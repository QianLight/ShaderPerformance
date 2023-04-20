using System;
using System.Collections.Generic;

namespace com.pwrd.hlod.editor
{
    public partial class HLODMessageCenter
    {
        private static HLODMessageCenter m_instance = new HLODMessageCenter();

        private Dictionary<string, List<Action<object>>> m_map = new Dictionary<string, List<Action<object>>>();

        public static void SendMessage(string messageName, params object[] args)
        {
            if (!m_instance.m_map.ContainsKey(messageName))
            {
                return;
            }

            var list = m_instance.m_map[messageName];
            foreach (var action in list)
            {
                if (action != null)
                    action.Invoke(args != null && args.Length == 1 ? args[0] : args);
            }
        }

        public static void Register(string messageName, Action<object> action)
        {
            if (!m_instance.m_map.ContainsKey(messageName))
            {
                m_instance.m_map[messageName] = new List<Action<object>>();
            }

            var list = m_instance.m_map[messageName];
            if (!list.Contains(action))
                list.Add(action);
        }

        public static void UnRegister(string messageName, Action<object> action)
        {
            if (!m_instance.m_map.ContainsKey(messageName))
            {
                return;
            }

            var list = m_instance.m_map[messageName];
            if (list.Contains(action))
                list.Remove(action);
        }
    }


    public static class HLODMesssages
    {
        public const string REPAINT_WINDOW = "RepaintWindow";
        public const string REBUILD_TREE_VIEW = "ReBuildTreeView";
        public const string SCENE_EDITOR_DATA_DESTORYED = "SceneEditorDataDestoryed";
        public const string HLOD_TREE_LAYER_ITEM_CHANGED = "HlodTreeLayerItemChanged";
        public const string HLOD_SUB_CLUSTER_DOUBLE_CLICK = "HLodSubClusterDoubleClick";
        public const string HLOD_WORK_DATA_CHANGED = "HLODWorkDataChanged";
    }
}