using System.Collections.Generic;
using UnityEngine;
namespace CFEngine.Editor
{
    //[System.Serializable]
    //public class LightmapObjectInfo
    //{
    //    public int lightmapIndex;
    //    public string hashStr;
    //    [System.NonSerialized]
    //    public Texture2D lightmapRef;
    //    [System.NonSerialized]
    //    public ushort lightmapID;
    //}

    [System.Serializable]
    public class LightmapData
    {
        public string name = "";
        public List<LigthmapRes> ligthmapRes = new List<LigthmapRes> ();
    }

    public class LightmapVolumnData : ScriptableObject
    {
        public List<LightmapData> volumns = new List<LightmapData> ();

        public void GetLightMap (int volumnIndex, int index,out string volumnName ,out Texture2D lightmap,out Texture2D shadowmask)
        {
            volumnName = "";
            lightmap = null;
            shadowmask = null;
            if (volumnIndex >= 0 && volumnIndex < volumns.Count)
            {
                var v = volumns[volumnIndex];
                if (index >= 0 && index < v.ligthmapRes.Count)
                {
                    volumnName = v.name;
                    if (v.ligthmapRes[index].colorCombineShadowMask != null && LightmapCombineManager.Instance.CheckIsUseCombineLightmap())
                    {
                        lightmap = v.ligthmapRes[index].colorCombineShadowMask;
                    }
                    else
                    {
                        lightmap = v.ligthmapRes[index].color;
                    }
                    
                    shadowmask = v.ligthmapRes[index].shadowMask;
                   // return v.ligthmapRes[index].color;
                }
            }
          //  return null;
        }

    }
}