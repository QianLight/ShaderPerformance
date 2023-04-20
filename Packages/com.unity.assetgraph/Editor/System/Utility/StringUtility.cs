using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AssetGraph
{
    public class StringUtility
    {
        public static uint GetHashCode(string content)
        {
            uint seed = 131; // 31 131 1313 13131 131313 etc..
            uint hash = 0;

            for (int i = 0; i < content.Length; i++)
            {
                hash = hash * seed + content[i];
            }
            return (hash & 0x7FFFFFFF);
        }
    }
}

