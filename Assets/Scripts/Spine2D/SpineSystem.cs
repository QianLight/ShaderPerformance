using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine;
using CFUtilPoolLib;
using CFUtilPoolLib.Spine;
using Spine.Unity;
using UnityEngine;

public class SpineSystem : XSingleton<SpineSystem>, ISpineSystem 
{
    public SpineSystem()
    {
        
    }

    public bool Deprecated { get; set; }

    // private AssetHandler skeletonAssetHandle;

    public void InitializeSpineAnimation(GameObject target, string assetPath ,ref  AssetHandler m_ah)
    {
        LoadMgr.GetAssetHandler(ref m_ah, assetPath, ResObject.ResExt_Asset);
        LoadMgr.loadContext.Init(null, null);
        LoadMgr.loadContext.flag = LoadMgr.LoadForceImmediate;
        LoadMgr.singleton.LoadAsset<SkeletonDataAsset>(m_ah, ResObject.ResExt_Asset,true);

        SkeletonDataAsset asset = m_ah.obj as SkeletonDataAsset;

        if (asset == null)
            return;

        SkeletonAnimation ani = target.GetComponent<SkeletonAnimation>();
        if (ani == null)
        {
            ani = SkeletonAnimation.AddToGameObject(target, asset); 
        } 
        else
        {
            ani.skeletonDataAsset = asset;
        }

        if(ani != null)
        {
            ani.initialSkinName = "default";
            ani.Initialize(true);

            ani.loop = true;
            ani.AnimationName = "animation";
            ani.LateUpdate();
        }
    }

    public void UnInitializeSpineAnimation(GameObject target)
    {
        XGameObject.RemoveComponent<SkeletonAnimation>(target);
        XGameObject.RemoveComponent<MeshFilter>(target);
        XGameObject.RemoveComponent<MeshRenderer>(target);

    }
}
