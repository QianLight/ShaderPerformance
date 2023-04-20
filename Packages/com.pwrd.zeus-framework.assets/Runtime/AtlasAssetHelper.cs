/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Zeus.Framework.Asset
{
    /// <summary>
    /// AtlasAssetHelper，实现更新Image的sprite功能
    /// </summary>
    public static class AtlasAssetHelper
    {
        static SpriteAtlasMap _spriteAtlasMap;
        static Dictionary<Sprite, IAssetRef> _sprite2AtlasDict = new Dictionary<Sprite, IAssetRef>();

        static public void InitSpriteAtlasMap()
        {
            _spriteAtlasMap = SpriteAtlasMap.LoadSpriteAtlasMap();
        }

        //  由此接口获取的sprite，必须调用ReleaseAtlasSprite进行释放，否则造成内存泄漏
        static public Sprite LoadAtlasSprite(string spriteName)
        {
            var atlasResPath = GetAtlasResPath(spriteName);
            return LoadAtlasSprite(atlasResPath, spriteName);
        }

        //  由此接口获取的sprite，必须调用ReleaseAtlasSprite进行释放，否则造成内存泄漏
        static public Sprite LoadAtlasSprite(string atlasPath, string spriteName)
        {
            var atlasRef = AssetManager.LoadAsset(atlasPath, typeof(SpriteAtlas));
            if (atlasRef == null || atlasRef.AssetObject == null)
                return null;

            var atlas = atlasRef.AssetObject as SpriteAtlas;
            if(atlas == null)
                return null;

            Sprite sprite = atlas.GetSprite(spriteName);
            if (sprite == null)
                return null;

            _sprite2AtlasDict.Add(sprite, atlasRef);
            atlasRef.Retain();
            return sprite;
        }

        //  由此接口获取的sprite，必须调用ReleaseAtlasSprite进行释放，否则造成内存泄漏
        //  在callBack内接收加载的sprite
        static public void LoadAtlasSpriteAsync(string spriteName, Action<string, Sprite> callBack)
        {
            AssetManager.LoadAssetAsync(GetAtlasResPath(spriteName), typeof(SpriteAtlas), OnLoadAtlasReady, new AAHParam { callBack = callBack, spriteName = spriteName });
        }

        //  由此接口获取的sprite，必须调用ReleaseAtlasSprite进行释放，否则造成内存泄漏
        //  在callBack内接收加载的sprite
        static public void LoadAtlasSpriteAsync(string atlasPath, string spriteName, Action<string, Sprite> callBack)
        {
            AssetManager.LoadAssetAsync(atlasPath, typeof(SpriteAtlas), OnLoadAtlasReady, new AAHParam { callBack = callBack, spriteName = spriteName });
        }


        static public void ReleaseAtlasSprite(Sprite sprite)
        {
            if (_sprite2AtlasDict.ContainsKey(sprite))
            {
                var atlasRef = _sprite2AtlasDict[sprite];
                atlasRef.Release();
                _sprite2AtlasDict.Remove(sprite);
            }
        }

        static public string GetAtlasResPath(string spriteName)
        {
            var apath = _spriteAtlasMap.GetAtlasPath(spriteName);
            return apath;
        }

        static private void OnLoadAtlasReady(IAssetRef atlasRef, object param)
        {
            if (atlasRef == null)
                return;

            var aahParam = (AAHParam)param;
            var atlas = atlasRef.AssetObject as SpriteAtlas;
            Sprite sprite = atlas.GetSprite(aahParam.spriteName);
            if (sprite == null)
                return ;

            _sprite2AtlasDict.Add(sprite, atlasRef);
            atlasRef.Retain();
            aahParam.callBack(aahParam.spriteName, sprite);
        }

        class AAHParam
        {
           public  Action<string, Sprite> callBack;
           public  string spriteName;
        }
    }
}