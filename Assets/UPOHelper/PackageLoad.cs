#if ENABLE_UPO
using UnityEngine;

namespace UPOHelper
{
    using UPOHelper.Network;

    public class PackageLoad : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnStartGame()
        {
            GameObject upoGameObject = new GameObject("UPOGameObject");
            upoGameObject.name = "UPOProfiler";
            upoGameObject.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(upoGameObject);
            NetworkServer.ConnectTcpPort(57000); // 使用配置文件中的变量
            // unity 2017.4版本没有wantsToQuit的方法，添加宏定义后editor死循环无法启动
// #if UNITY_2018_1_OR_NERER
            // Application.wantsToQuit += Quit;
// #endif
        }

        private void OnApplicationQuit()
        {
            NetworkServer.Close();
        }

        static bool Quit()
        {
            NetworkServer.Close();
            return true;
        }
    }
}
#endif