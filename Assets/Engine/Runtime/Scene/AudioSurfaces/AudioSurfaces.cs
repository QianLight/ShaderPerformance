using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    public class AudioSurfaces : ScriptableObject
    {
        
        public EnumAudioSurfacesDefines sufaceType;
        public Color debugColor;
        
        public List<Texture> textures;


        public bool CheckHasTex(Texture tex)
        {
            return textures.Contains(tex);
        }
    }
}

