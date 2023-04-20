using UnityEngine;
using System.Collections.Generic;

namespace com.pwrd.hlod.editor
{
    public static class HLODProviderUtils
    {
        public static bool HasAlphaTest(Renderer renderer)
        {
            var mats = renderer.sharedMaterials;

            //贴花不能被视为开启了AlphaTest的Entity
            var decalTag = renderer.transform.GetComponent<HLODDecalTag>();
            if (decalTag != null)
                return false;

            foreach (var mat in mats)
            {
                if (mat.IsKeywordEnabled("_ALPHATEST_ON") || mat.GetTag("RenderType", false) == "Transparent")
                    return true;
            }

            return false;
        }
        
        public static bool HasAlphaTest(List<Renderer> renderers)
        {
            foreach (var renderer in renderers)
            {
                var mats = renderer.sharedMaterials;

                //贴花不能被视为开启了AlphaTest的Entity
                var decalTag = renderer.transform.GetComponent<HLODDecalTag>();
                if (decalTag != null)
                    continue;

                foreach (var mat in mats)
                {
                    if (mat.IsKeywordEnabled("_ALPHATEST_ON")  || mat.GetTag("RenderType", false) == "Transparent")
                        return true;
                }
            }

            return false;
        }
        
        public static  bool IsIgnoreNode(GameObject curGO)
        {
            if (curGO == null)
                return false;

            var ignoreNode = curGO.GetComponent<HLODIgnoreNode>();
            return ignoreNode != null;
        }
    }
}