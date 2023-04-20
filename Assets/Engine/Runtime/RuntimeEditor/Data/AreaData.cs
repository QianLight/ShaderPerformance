#if UNITY_EDITOR
using UnityEngine;
namespace CFEngine
{
    [System.Serializable]
    public unsafe struct LightProbeAreaData
    {
        public float flag;
        public Vector4 height;

        public static int GetSize ()
        {
            return sizeof(float) + sizeof(float) * 4;
        }
    }
    public class AreaData : ScriptableObject
    {
        public Vector4Int lightProbeArea = new Vector4Int ();
        public LightProbeAreaData[] lightProbeData;
    }
}
#endif