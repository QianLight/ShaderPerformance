/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

namespace Zeus.Framework.Asset
{
    /// <summary>
    /// AtlasAssetHelper，实现更新Image的sprite功能
    /// </summary>
    public class AtlasRequestHandler
    {
        private const string ATLAS_RELATIVE_DIRECTORY = "UI/Atlas/";

        public AtlasRequestHandler()
        {
            //SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;
        }


        void RequestLateBindingAtlas(string assetName, System.Action<SpriteAtlas> action)
        {
            //Debug.Log("RequestLateBindingAtlas ，assetName: " + assetName);
            string atlasPath = ATLAS_RELATIVE_DIRECTORY + assetName;
            IAssetRef assetRef = AssetManager.LoadAsset(atlasPath, typeof(SpriteAtlas));
            if (assetRef == null)
            {
                //Debug.LogError("SpriteAtlas is not exist in AssetBundle or Resources, Path: " + atlasPath);
                return;
            }
            SpriteAtlas atlas = assetRef.AssetObject as SpriteAtlas;
            action(atlas);
        }
    }
}
