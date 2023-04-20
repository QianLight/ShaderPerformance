using AssetCheck.AssetCheckToDevops;
using Devops.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    public class DevopsRulesCheck
    {
        public static void CheckRuleConfig()
        {
            AssetCheckPathConfig checkPathConfig = AssetDatabase.LoadAssetAtPath<AssetCheckPathConfig>($"{Defines.CheckPathConfigPath}/{Defines.CheckPathConfigName}");

        }

        public static async void SyncRuleAndPaths(ToDevopsRules toDevopsRules)
        {
            string ruleJsonMsg = JsonUtility.ToJson(toDevopsRules);
            Debug.Log(ruleJsonMsg);
            AssetHelper.WriteAllText($"{Defines.OutputDir}/Rules.txt", ruleJsonMsg);
            DevopsCoreInfo coreInfo = EditorDevopsInfoSettings.GetDevopsInfo();
            LoginTokenInfo loginTokenInfo = await Login.GetTokenInfo();
            if (loginTokenInfo == null)
                return;
            bool bResult = await PostAsync($"{coreInfo.devopsIpPort}/devops/versionAssetCheckRule/config", loginTokenInfo.token, ruleJsonMsg);
            if(bResult)
            {
                Debug.Log("规则提交成功！");
            }
            else
            {
                Debug.Log("规则提交失败！");
            }
            RuleSyncGUI.HideWindow();
        }

        public static void CheckFromServer()
        {
            string rulePath = CommandLineTool.GetEnvironmentVariable("-ruleFilePath");
            CheckFromServer(rulePath);
        }

        public static void CheckFromServer(string path)
        {
            if(!File.Exists(path))
            {
                Debug.LogError($"Can not find file {path}");
                return;
            }
            string checkFromServerJson = File.ReadAllText(path);
            ToDevopsRules devopsRules = JsonUtility.FromJson<ToDevopsRules>(checkFromServerJson);
            AssetCheckPathConfig config = DevopsReports.DevopsRulesToConfig(devopsRules);
            if(AssetCheckMain.IsBusy())
            {
                AssetCheckMain.CheckEndCloseApp = false;
                AssetCheckMain.ForceEnd();
            }
            AssetCheckMain.CheckEndCloseApp = true;
            AssetCheckMain.CheckWithConfig(config);
        }

        class PostResult
        {
            public int code;
            public string message;
            public string data;
        }


        public static async Task<bool> PostAsync(string url, string token, string info)
        {
            Debug.Log($"url:{url},token:{token}");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    using (var request = new HttpRequestMessage(new HttpMethod("Post"), url))
                    {
                        request.Content = new StringContent(info);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var resultStr = await response.Content.ReadAsStringAsync();
                            Debug.Log($"resultStr:{resultStr}");
                            PostResult result = JsonUtility.FromJson<PostResult>(resultStr);
                            if(result != null && result.code == 400)
                            {
                                return false;
                            }
                            return true;
                        }
                        else
                        {
                            Debug.LogError($"SendAsync error {response.IsSuccessStatusCode} {response.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("PostUrl error:" + ex.ToString());
            }
            return false;
        }
    }
}
