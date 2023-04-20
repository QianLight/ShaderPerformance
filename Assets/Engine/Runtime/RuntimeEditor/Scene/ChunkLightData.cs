using System;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine
{
    [Serializable]
    public class LightingData
    {
        public string path;
        [NonSerialized]
        public Light light;
        public Vector3 pos;
        public float range;
        public Color c;
        public float intensity;
        public float rangeBias = 1;

        public int priority = 0;

        public int key;//index key

        public Light GetLight ()
        {
            if (light == null && !string.IsNullOrEmpty (path))
            {
                GameObject go = GameObject.Find (path);
                light = go != null?go.GetComponent<Light> () : null;
            }
            return light;
        }
    }

    [Serializable]
    public class LightingChunkIndex
    {
        public List<int> lightIndex = new List<int> ();
    }
    public class ChunkLightData : ScriptableObject
    {
        public string dir;
        public int maxXGrid;
        public int maxZGrid;

        public List<LightingData> lights = new List<LightingData> ();

        public LightingChunkIndex[] chunkLightIndex;

        public Texture2D lightIndexTex;
        
        public void Copy(ChunkLightData src)
        {
            lights.Clear();
            lights.AddRange(src.lights);
            chunkLightIndex = src.chunkLightIndex;
        }
    }
}