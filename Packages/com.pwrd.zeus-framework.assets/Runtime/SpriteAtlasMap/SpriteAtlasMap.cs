/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Collections.Generic;
using UnityEngine;

#if !RESOURCE_PROJECT
using Zeus.Core.FileSystem;
#endif

namespace Zeus.Framework.Asset
{
    class SpriteAtlasMap
    {
        public const string MapFileName = "SpriteAtlasMap.bytes";
        //for runtime
        private Dictionary<uint, string> spriteHashAtlasMap = null;
        private Dictionary<string, string> spriteNameAtlasMap = null;

#if !RESOURCE_PROJECT
        public string GetAtlasPath(string spriteName)
        {
            string atlas;
            if (spriteHashAtlasMap == null)
            {
                Debug.LogError("Please use \"LoadSpriteAtlasMap\" to get an instance.");
                return null;
            }
            uint hashCode = AssetBundleUtils.GetHashCode(spriteName);
            if (!spriteHashAtlasMap.TryGetValue(hashCode, out atlas))
            {
                if (spriteNameAtlasMap == null)
                {
                    UnityEngine.Debug.LogError(string.Format("{0} is not in any atlas", spriteName));
                    return null;
                }
                else if (!spriteNameAtlasMap.TryGetValue(spriteName, out atlas))
                {
                    UnityEngine.Debug.LogError(string.Format("{0} is not in any atlas", spriteName));
                    return null;
                }
            }
            return atlas;
        }
#endif


#if !RESOURCE_PROJECT
        public static SpriteAtlasMap LoadSpriteAtlasMap()
        {
            SpriteAtlasMap map = new SpriteAtlasMap();
            string path = Path.Combine(SpriteAtlasFolderSetting.AtlasFolderPath, Path.GetFileNameWithoutExtension(SpriteAtlasMap.MapFileName));
            var mapAssetRef = AssetManager.LoadAsset(path);
            if (mapAssetRef == null)
            {
                UnityEngine.Debug.LogError("Load SpriteAtlasMap Fail:" + path);
            }
            else
            {
                mapAssetRef.Retain();
                byte[] content = (mapAssetRef.AssetObject as TextAsset).bytes;
                ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(content);
                SpriteMapAtlasFB sprite2Atlas = SpriteMapAtlasFB.GetRootAsSpriteMapAtlasFB(buffer);
                map.spriteHashAtlasMap = new Dictionary<uint, string>();
                if (sprite2Atlas.N2aLength > 0)
                {
                    map.spriteNameAtlasMap = new Dictionary<string, string>();
                }
                else
                {
                    map.spriteNameAtlasMap = null;
                }
                for (int i = 0; i < sprite2Atlas.H2aLength; ++i)
                {
                    SpriteHashMapAtlasFB h2a = sprite2Atlas.H2a(i).Value;
                    map.spriteHashAtlasMap[h2a.Hash] = string.Intern(h2a.Atlas);
                }
                for (int i = 0; i < sprite2Atlas.N2aLength; ++i)
                {
                    SpriteNameMapAtlasFB n2a = sprite2Atlas.N2a(i).Value;
                    map.spriteNameAtlasMap[n2a.Name] = string.Intern(n2a.Atlas);
                }

                mapAssetRef.Release();
            }
            return map;
        }
#endif

        public class MapItem
        {
            [System.NonSerialized]
            public string spritePath;
            public string spriteName;
            public string atlasPath;
        }
    }
}
