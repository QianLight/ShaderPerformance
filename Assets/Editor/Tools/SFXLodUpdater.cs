using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;

public class SFXLodUpdater : Editor
{
   [MenuItem("Tools/特效/特效LOD升级")]
   public static void UpdateSFXLodConfig()
   {
      PrefabLodConfig old;
      bool newDoc = false;
      try
      {
         old = AssetDatabase.LoadAssetAtPath<PrefabLodConfig>("Assets/BundleRes/Config/SFXPrefabLodConfig.asset");
      }
      catch (Exception e)
      {
         DebugLog.AddErrorLog("没有旧配置");
         throw;
      }

      SFXPrefabLodConfig newConfig;
      newConfig = AssetDatabase.LoadAssetAtPath<SFXPrefabLodConfig>("Assets/BundleRes/Config/SFXLodConfigList.asset");
      if (newConfig == null)
      {
         newConfig = ScriptableObject.CreateInstance<SFXPrefabLodConfig>();
         newDoc = true;
      }

      for (int i = 0; i < old.items.Count; i++)
      {
         int getOldLod = 0;
         if (old.items[i].lodFlag.HasFlag(PrefabLodItem.Flag_Lod1)) getOldLod = 1;
         if (old.items[i].lodFlag.HasFlag(PrefabLodItem.Flag_Lod2)) getOldLod = 2;
         SFXPrefabLodItem newItem = new SFXPrefabLodItem();
         newItem.SetLod(old.items[i].name, getOldLod);
         newConfig.items.Add(newItem);
      }

      if (newDoc)
      {
         AssetDatabase.CreateAsset(newConfig,"Assets/BundleRes/Config/SFXLodConfigList.asset" );
      }
      
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

   }
}
