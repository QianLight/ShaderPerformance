using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UsingTheirs.ShaderHotSwap
{

    public class EditorWindowBase : EditorWindow
    {

        #region Link
        protected void ShowLink()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Kinds", EditorStyles.miniButton))
                Application.OpenURL( Consts.kKindsAssetStoreUrlLite );
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label( string.Format("Ver {0}",Consts.kVersion), EditorStyles.miniLabel);
            
            if (GUILayout.Button("Online Doc", EditorStyles.miniButton))
                Application.OpenURL( Consts.kOnlineDocUrl);
            
            if (GUILayout.Button("Review", EditorStyles.miniButton))
                Application.OpenURL( Consts.kAssetStoreReviewUrl);
            
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}