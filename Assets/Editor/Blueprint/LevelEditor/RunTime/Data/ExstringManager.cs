using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelEditor
{
    class ExstringManager
    {
        static Dictionary<string, LevelRTExstringNode> stringPool = new Dictionary<string, LevelRTExstringNode>();

        public static void Clear()
        {
            stringPool.Clear();
        }

        public static void RegisterListenString(string exstring, LevelRTExstringNode node)
        {
            if(exstring != null)
                stringPool.Add(exstring, node);
        }

        public static void ActiveString(string exstring)
        {
            if(stringPool.ContainsKey(exstring))
            {
                LevelRTExstringNode node = stringPool[exstring];
                node.OnStringActive();
            }

        }
    }
}
