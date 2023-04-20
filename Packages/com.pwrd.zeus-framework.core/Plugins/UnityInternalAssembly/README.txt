DynamicProxyGenAssembly2命名的Assembly可作为 UnityEngine.dll 和 UnityEditor.dll的友元程序集，
即，可调用其中的

Internal 的类，如：
	UnityEngine.GUIClip
	UnityEditor.CurveEditor
Internal 的方法，如：
	HandleUtility.ApplyWireMaterial(Handles.zTest);
Internal 的委托，如：
	UnityEditor.SceneView.onPreSceneGUIDelegate
Internal 的字段，如：
	UnityEditor.EditorGUI.s_AllowedCharactersForInt


配合反编译软件食用更佳，如dnSPY