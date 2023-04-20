using System.Reflection;
using UnityEngine;

/// <summary>
/// 使用时最好切换到VSCode，会快一些，结束后去Temp文件夹找Shader，使用前最好清空一下Temp文件夹
/// </summary>
public class GetCompiledShaderCode : MonoBehaviour
{
    /// <summary>
    /// 序列化的Shader可以做循环，文件搜取的无法做
    /// </summary>
    public Shader[] m_shaders;
 
    [ContextMenu("make")]
    public static void Make(Shader[] shaders)
    {
        for (int i = 0; i < shaders.Length; i++)
        {
#if UNITY_2020_1
			ShaderUtilEx.OpenCompiledShader(shaders[i], ShaderInspectorPlatformsPopupEx.GetCurrentMode(), ShaderInspectorPlatformsPopupEx.GetCurrentPlatformMask(), ShaderInspectorPlatformsPopupEx.GetCurrentVariantStripping() == 0,true);
#elif UNITY_2020_2_OR_NEWER
			ShaderUtilEx.OpenCompiledShader(shaders[i], ShaderInspectorPlatformsPopupEx.GetCurrentMode(), ShaderInspectorPlatformsPopupEx.GetCurrentPlatformMask(), ShaderInspectorPlatformsPopupEx.GetCurrentVariantStripping() == 0,false,false);
#else
			ShaderUtilEx.OpenCompiledShader(shaders[i], ShaderInspectorPlatformsPopupEx.GetCurrentMode(), ShaderInspectorPlatformsPopupEx.GetCurrentPlatformMask(), ShaderInspectorPlatformsPopupEx.GetCurrentVariantStripping() == 0);
#endif
		}
	}
	class ShaderUtilEx
	{
		private static System.Type type = null;
		public static System.Type Type { get { return (type == null) ? type = System.Type.GetType("UnityEditor.ShaderUtil, UnityEditor") : type; } }



#if UNITY_2020_1
		public static void OpenCompiledShader( Shader shader, int mode, int customPlatformsMask, bool includeAllVariants, bool preprocessOnly )
		{
			ShaderUtilEx.Type.InvokeMember( "OpenCompiledShader", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[] { shader, mode, customPlatformsMask, includeAllVariants, preprocessOnly } );
		}
#elif UNITY_2020_2_OR_NEWER
		public static void OpenCompiledShader(Shader shader, int mode, int customPlatformsMask, bool includeAllVariants, bool preprocessOnly, bool stripLineDirectives)
		{
			ShaderUtilEx.Type.InvokeMember("OpenCompiledShader", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[] { shader, mode, customPlatformsMask, includeAllVariants, preprocessOnly, stripLineDirectives });
		}
#else
		public static void OpenCompiledShader( Shader shader, int mode, int customPlatformsMask, bool includeAllVariants )
		{
			ShaderUtilEx.Type.InvokeMember( "OpenCompiledShader", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[] { shader, mode, customPlatformsMask, includeAllVariants } );
		}
#endif

	}
	class ShaderInspectorPlatformsPopupEx
	{
		private static System.Type type = null;
		public static System.Type Type { get { return (type == null) ? type = System.Type.GetType("UnityEditor.ShaderInspectorPlatformsPopup, UnityEditor") : type; } }

		public static int GetCurrentMode()
		{
			return (int)ShaderInspectorPlatformsPopupEx.Type.GetProperty("currentMode", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
		}

		public static int GetCurrentPlatformMask()
		{
			return (int)ShaderInspectorPlatformsPopupEx.Type.GetProperty("currentPlatformMask", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
		}

		public static int GetCurrentVariantStripping()
		{
			return (int)ShaderInspectorPlatformsPopupEx.Type.GetProperty("currentVariantStripping", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
		}
	}
}
