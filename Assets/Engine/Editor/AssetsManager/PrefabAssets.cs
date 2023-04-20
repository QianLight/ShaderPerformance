using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    internal class PrefabAssets
    {
        internal static GameObject SavePrefab (string path, GameObject go)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject> (path);
            if (prefab != null)
            {
                return PrefabUtility.SaveAsPrefabAssetAndConnect (go, path, InteractionMode.AutomatedAction);
            }

            else
                return PrefabUtility.SaveAsPrefabAsset (go, path);
        }

        internal static Bounds GetBound (Transform trans)
        {
            // GetRenderers(trans.gameObject);
            Bounds bound = new Bounds (trans.position, Vector3.one);
            // for (int i = 0; i < renders.Count; ++i)
            // {
            //     bound.Encapsulate(renders[i].bounds);
            // }
            return bound;
        }

        // [MenuItem ("Assets/Tool/Prefab_RemoveMissingAsset")]
        // static void Prefab_RemoveMissingAsset ()
        // {
        //     CommonAssets.enumPrefab.cb = (prefab, path, context) =>
        //     {
        //         var renders = EditorCommon.GetRenderers (prefab);
        //         for (int i = 0; i < renders.Count; ++i)
        //         {
        //             ParticleSystemRenderer psr = renders[i] as ParticleSystemRenderer;
        //             if (psr != null && psr.renderMode != ParticleSystemRenderMode.Mesh && psr.mesh != null)
        //             {
        //                 psr.mesh = null;
        //             }
        //         }
        //     };
        //     CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "RemoveMissingAsset");
        // }

        [MenuItem ("Assets/Tool/Sfx_Save")]
        static void Sfx_Save ()
        {
            CommonAssets.enumPrefab.cb = (prefab, path, context) =>
            {
                if (!prefab.TryGetComponent (out SFXWrapper sfxWrapper))
                {
                    sfxWrapper = prefab.AddComponent<SFXWrapper> ();
                }
                sfxWrapper.Process (prefab);

            };
            CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "SfxSave");
        }

        [MenuItem ("Assets/Tool/Sfx_SaveNotControl")]
        static void Sfx_SaveNotControl ()
        {
            CommonAssets.enumPrefab.cb = (prefab, path, context) =>
            {
                if (!prefab.TryGetComponent (out SFXWrapper sfxWrapper))
                {
                    sfxWrapper = prefab.AddComponent<SFXWrapper> ();
                }
                sfxWrapper.notControl = true;
                sfxWrapper.Process (prefab);

            };
            CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "SaveNotControl");
        }
    }
}