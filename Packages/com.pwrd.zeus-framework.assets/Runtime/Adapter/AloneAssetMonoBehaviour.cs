/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zeus.Framework.Asset
{
    public class AloneAssetMonoBehaviour : MonoBehaviour
    {
        private IAssetRef m_AssetRef = null;
        public IAssetRef AssetRef
        {
            set
            {
                if (m_AssetRef != null)
                {
                    m_AssetRef.Release();
                }
                m_AssetRef = value;
                if (m_AssetRef != null)
                {
                    m_AssetRef.Retain();
                }
            }
        }

        public virtual void OnDestroy()
        {
            Clear();
            System.GC.SuppressFinalize(this);
        }

        public void SetRef(IAssetRef assetRef)
        {
            AssetRef = assetRef;
        }

        public void Clear()
        {
            AssetRef = null;
        }

        ~AloneAssetMonoBehaviour()
        {
            Clear();
        }
    }
}
