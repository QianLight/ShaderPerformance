using Impostors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CFEngine
{
    public class ImpostorsCfg : MonoBehaviour
    {
        public float screenRelativeTransitionHeight = 0.1f;
        public float deltaCameraAngle = 10f;
        public bool IsForceRes = false;
        public bool IsDiscardOriLOD = false;
        public TextureResolution forceMinTextureResolution = TextureResolution._64x64;
        public TextureResolution forceMaxTextureResolution = TextureResolution._128x128;

    }
}