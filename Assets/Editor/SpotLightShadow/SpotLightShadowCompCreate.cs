using UnityEditor;
using UnityEngine;


public static class SpotLightShadowCompCreate
{
    [MenuItem("GameObject/Light/SpotLightShadow", false, 999)]
    static void CreateCustomGameObject(MenuCommand menuCommadn)
    {
        GameObject go = new GameObject("SpotLightShadow");
        go.AddComponent<SpotLightAsShadow>();
        GameObjectUtility.SetParentAndAlign(go, menuCommadn.context as GameObject);
        Undo.RegisterCompleteObjectUndo(go, "Create" + go.name);
        Selection.activeObject = go;
    }
}