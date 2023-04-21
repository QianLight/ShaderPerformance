using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.ShaderGraph
{
    static class CreateShaderGraph
    {
        [MenuItem("Assets/Create/Shader Graph/Blank Shader Graph", priority = CoreUtils.editMenuPriority1 + CoreUtils.assetCreateMenuPriority1)]
        public static void CreateBlankShaderGraph()
        {
            GraphUtil.CreateNewGraph();
        }
    }
}
