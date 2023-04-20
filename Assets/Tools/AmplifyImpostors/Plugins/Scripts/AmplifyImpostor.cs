#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;
using CFEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Text.RegularExpressions;
#endif

namespace AmplifyImpostors
{
#if UNITY_EDITOR
	public class AmplifyTextureImporter : AssetPostprocessor
	{
		void OnPreprocessTexture()//这个代码不能删 会引发项目图片资源重新导入 
		{

		
		}
	}
#endif
}
#endif