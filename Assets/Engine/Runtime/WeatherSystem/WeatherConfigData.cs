using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.WeatherSystem
{
    [Serializable]
    public class WeatherConfigData
    {
        public Light light;
        public Volume volume;
        public VolumeProfile volumeProfile;
        public Cubemap reflectCubemap;
        public Cubemap skyboxBaseMap;
        public Cubemap skyboxMaskMap;
        public Material seaMaterial;
    }
}