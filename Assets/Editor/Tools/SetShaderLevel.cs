using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SetShaderLevel : Editor
{
	const string menuFolder = "Tools/引擎/材质特效分级宏/";
	const string StrShaderLevelHigh = menuFolder + "材质高";
	const string StrShaderLevelMedium = menuFolder + "材质中";
	const string StrShaderLevelLow = menuFolder + "材质低";
	const string StrFxLevelHigh = menuFolder + "特效高";
	const string StrFxLevelMedium = menuFolder + "特效中";
	const string StrFxLevelLow = menuFolder + "特效低";

	[MenuItem(StrShaderLevelHigh, false, 0)]
	static void ShaderLevelHigh(){
		Menu.SetChecked(StrShaderLevelHigh, true);
		Menu.SetChecked(StrShaderLevelMedium, false);
		Menu.SetChecked(StrShaderLevelLow, false);

		Shader.EnableKeyword("_SHADER_LEVEL_HIGH");
		Shader.DisableKeyword("_SHADER_LEVEL_MEDIUM");
		Shader.DisableKeyword("_SHADER_LEVEL_LOW");
	}
	[MenuItem(StrShaderLevelMedium, false, 0)]
	static void ShaderLevelMedium(){
		Menu.SetChecked(StrShaderLevelHigh, false);
		Menu.SetChecked(StrShaderLevelMedium, true);
		Menu.SetChecked(StrShaderLevelLow, false);

		Shader.DisableKeyword("_SHADER_LEVEL_HIGH");
		Shader.EnableKeyword("_SHADER_LEVEL_MEDIUM");
		Shader.DisableKeyword("_SHADER_LEVEL_LOW");
	}
	[MenuItem(StrShaderLevelLow, false, 0)]
	static void ShaderLevelLow(){
		Menu.SetChecked(StrShaderLevelHigh, false);
		Menu.SetChecked(StrShaderLevelMedium, false);
		Menu.SetChecked(StrShaderLevelLow, true);

		Shader.DisableKeyword("_SHADER_LEVEL_HIGH");
		Shader.DisableKeyword("_SHADER_LEVEL_MEDIUM");
		Shader.EnableKeyword("_SHADER_LEVEL_LOW");
	}

	[MenuItem(StrFxLevelHigh, false, 0)]
	static void FxLevelHigh(){
		Menu.SetChecked(StrFxLevelHigh, true);
		Menu.SetChecked(StrFxLevelMedium, false);
		Menu.SetChecked(StrFxLevelLow, false);

		Shader.EnableKeyword("_FX_LEVEL_HIGH");
		Shader.DisableKeyword("_FX_LEVEL_MEDIUM");
		Shader.DisableKeyword("_FX_LEVEL_LOW");
	}
	[MenuItem(StrFxLevelMedium, false, 0)]
	static void FxLevelMedium(){
		Menu.SetChecked(StrFxLevelHigh, false);
		Menu.SetChecked(StrFxLevelMedium, true);
		Menu.SetChecked(StrFxLevelLow, false);

		Shader.DisableKeyword("_FX_LEVEL_HIGH");
		Shader.EnableKeyword("_FX_LEVEL_MEDIUM");
		Shader.DisableKeyword("_FX_LEVEL_LOW");
	}
	[MenuItem(StrFxLevelLow, false, 0)]
	static void FxLevelLow(){
		Menu.SetChecked(StrFxLevelHigh, false);
		Menu.SetChecked(StrFxLevelMedium, false);
		Menu.SetChecked(StrFxLevelLow, true);

		Shader.DisableKeyword("_FX_LEVEL_HIGH");
		Shader.DisableKeyword("_FX_LEVEL_MEDIUM");
		Shader.EnableKeyword("_FX_LEVEL_LOW");
	}
}
