//using System;
//using UnityEngine.Assertions;
//using UnityEngine;

//namespace CFEngine
//{
//    [Serializable]
//    public sealed class Dithering
//    {
//        int m_NoiseTextureIndex = 0;

//        internal void Render(RenderContext context)
//        {
//            var blueNoise = context.resources.blueNoise64;
//            Assert.IsTrue(blueNoise != null && blueNoise.Length > 0);

//        #if POSTFX_DEBUG_STATIC_DITHERING // Used by QA for automated testing
//            m_NoiseTextureIndex = 0;
//            float rndOffsetX = 0f;
//            float rndOffsetY = 0f;
//        #else
//            if (++m_NoiseTextureIndex >= blueNoise.Length)
//                m_NoiseTextureIndex = 0;

//            float rndOffsetX = UnityEngine.Random.value;
//            float rndOffsetY = UnityEngine.Random.value;
//        #endif

//            var noiseTex = blueNoise[m_NoiseTextureIndex];
//            var uberSheet = context.uberSheet;

//            uberSheet.properties.SetTexture(ShaderIDs.Dithering_Tex, noiseTex);
//            uberSheet.properties.SetVector(ShaderIDs.Dithering_Coords, new Vector4(
//                (float)context.pixelWidth / (float)noiseTex.width,
//                (float)context.pixelHeight / (float)noiseTex.height,
//                rndOffsetX,
//                rndOffsetY
//            ));
//        }
//    }
//}
