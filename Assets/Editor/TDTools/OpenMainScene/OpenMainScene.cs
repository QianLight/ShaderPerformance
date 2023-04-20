using BluePrint;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TDTools
{
    class OpenMainScene
    {
        [MenuItem("Tools/TDTools/通用工具/WYJ专属SSVIP特供抖音极速版打开主场景 #&X")]
        private static void Open()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/entrance.unity"); 
        }
    }
}
