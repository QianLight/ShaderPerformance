using System;
using UnityEditor.ShaderGraph;
using UnityEngine.Rendering;

namespace UnityEditor.Rendering.CustomRenderTexture.ShaderGraph
{
    public static class CustomTextureShaderGraphMenu
    {
        [MenuItem("Assets/Create/Shader Graph/Custom Render Texture", priority = CoreUtils.editMenuPriority3 + CoreUtils.gameObjectMenuPriority)]
        public static void CreateCustomTextureShaderGraph()
        {
            var target = (CustomRenderTextureTarget)Activator.CreateInstance(typeof(CustomRenderTextureTarget));
            target.TrySetActiveSubTarget(typeof(CustomTextureSubTarget));

            var blockDescriptors = new[]
            {
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.Alpha,
            };

            GraphUtil.CreateNewGraphWithOutputs(new[] { target }, blockDescriptors);
        }
    }
}
