#if UNITY_EDITOR

using System;
using UnityEngine;

namespace CFEngine
{
    public class RoleToolBoxConfig : ScriptableObject
    {
        [Serializable]
        public struct RolePhotographerConfig
        {
            [SerializeField]public int height;
            [SerializeField]public int width;
            [SerializeField]public Color color;
            [SerializeField]public string path;
            [SerializeField]public bool usingPostProcess;
            public RolePhotographerConfig(int height, int width, Color color,string path,bool usingPostProcess)
            {
                this.height = height;
                this.width = width;
                this.color = color;
                this.path = path;
                this.usingPostProcess = usingPostProcess;
            }
            public void Reset()
            {
                this.height = 1080;
                this.width = 2160;
                this.color = new Color(0.2f,0.3f,0.7f);
                this.path = "Assets/RolePicShot";
            }
        }
        [SerializeField]public RolePhotographerConfig RolePhotographer = new RolePhotographerConfig(1080,2160,new Color(0.2f,0.3f,0.7f),"Assets/RolePicShot",true);
    }
}
#endif
