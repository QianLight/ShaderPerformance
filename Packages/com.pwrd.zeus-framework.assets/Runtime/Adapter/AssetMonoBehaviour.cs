/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;

namespace Zeus.Framework.Asset
{
    public class AssetMonoBehaviour : MonoBehaviour
    {
        LinkedList<IAssetRef> _assetList = new LinkedList<IAssetRef>();
        Dictionary<Texture, IAssetRef> _texture2AssetRef;

        public void AddRef(IAssetRef assetRef)
        {
            assetRef.Retain();
            _assetList.AddLast(assetRef);
        }

        public void Clear()
        {
            foreach (var assetNode in _assetList)
            {
                assetNode.Release();
            }
            _assetList.Clear();
        }

        protected virtual void OnDestroy()
        {
            this.Clear();
            if (_texture2AssetRef != null)
                _texture2AssetRef.Clear();
            System.GC.SuppressFinalize(this);
        }

        ~AssetMonoBehaviour()
        {
            Clear();
        }

        private void RemoveFirstRef(IAssetRef assetRef)
        {
            assetRef.Release();
            _assetList.Remove(assetRef);
        }

        public void SetRawImageTexture(CFRawImage image, string assetPath)
        {
            if (_texture2AssetRef == null)
                _texture2AssetRef = new Dictionary<Texture, IAssetRef>();

            IAssetRef originRef = null;
            if (_texture2AssetRef.TryGetValue(image.texture, out originRef))
            {

            }
            if (string.IsNullOrEmpty(assetPath))
            {
                if (originRef != null)
                {
                    RemoveFirstRef(originRef);
                }
                image.texture = null;
                return;
            }

            AssetManager.LoadAssetAsync(assetPath, typeof(Texture), (assetRef, param) =>
            {
                if (originRef != null)
                {
                    RemoveFirstRef(originRef);
                }
                if (assetRef == null || assetRef.AssetObject == null)
                {
                    Debug.LogError("Load texture falied " + assetPath);
                    return;
                }
                Texture assetTexture = assetRef.AssetObject as Texture;
                AddRef(assetRef);
                image.texture = assetTexture;
                _texture2AssetRef[assetTexture] = assetRef;
            }, null);
        }
    }
}

