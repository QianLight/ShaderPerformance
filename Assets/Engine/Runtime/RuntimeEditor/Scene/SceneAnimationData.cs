using System.IO;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class SceneAnimationObject
    {
        public SceneAnimationData profile;

        // public List<int> sceneObjects = new List<int> ();

        public void Save (BinaryWriter bw)
        {
            if (profile != null)
            {
                profile.Save (bw);
            }
            else
            {
                bw.Write ((byte) 255);
            }
        }
    }

    [System.Serializable]
    public class SceneAnimationData : ScriptableObject
    {
        public string exString = "";
        public float duration = -1;
        public bool autoPlay = true;

        public virtual void Save (BinaryWriter bw)
        {
            uint hash = EngineUtility.XHashLowerRelpaceDot (0, exString);
            bw.Write (hash);
            bw.Write (duration);
            bw.Write (autoPlay);
        }
    }

}