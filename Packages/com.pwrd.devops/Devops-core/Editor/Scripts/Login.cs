using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Devops.Core
{
    public class Login : EditorWindow
    {
        static LoginTokenInfo loginTokenInfo = null;
        static DateTime beginLoginTime;
        static EditorWindow loginWindow = null;

        public static async Task<LoginTokenInfo> GetTokenInfo()
        {
            if(loginTokenInfo == null)
            {
                await OpenLogin();
            }
            return loginTokenInfo;
        }

        [MenuItem("Devops/Login")]
        static async Task OpenLogin()
        {
            loginTokenInfo = null;
            LoginInfo loginInfo = await EditorDevopsInfoSettings.GetLoginInfo();
            if(loginInfo == null)
            {
                Debug.LogError("Get can get loginInfo from devops-core, please check config");
                return;
            }
            if(loginWindow != null)
            {
                return;
            }
#if UNITY_2020_1_OR_NEWER
            Application.OpenURL(loginInfo.loginUrl);
#else
            string typeName = "UnityEditor.Web.WebViewEditorWindowTabs";
            Type type = Assembly.Load("UnityEditor.dll").GetType(typeName);
            BindingFlags Flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            MethodInfo methodInfo = type.GetMethod("Create", Flags);
            methodInfo = methodInfo.MakeGenericMethod(type);
            loginWindow = (EditorWindow)methodInfo.Invoke(null, new object[] {"login", loginInfo.loginUrl, 400, 300, 800, 600 });
#endif
            beginLoginTime = DateTime.Now;
            await CheckToken(loginInfo);
            if(loginWindow != null)
            {
                loginWindow.Close();
                loginWindow = null;
            }
        }
        static async Task CheckToken(LoginInfo loginInfo)
        {
            while((DateTime.Now - beginLoginTime).TotalSeconds < 30.0f)
            {
                string result = await CoreTool.GetUrlData(loginInfo.getTokenUrl);
                if(result != string.Empty)
                {
                    LoginTokenInfo tempLoginTokenInfo = JsonUtility.FromJson<LoginTokenInfo>(result);
                    if (tempLoginTokenInfo.status == 1)
                    {
                        loginTokenInfo = tempLoginTokenInfo;
                        return;
                    }
                }
            }
            Debug.LogError("Get Token Error");
        }
    }
}