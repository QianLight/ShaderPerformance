using System.IO;
using System.Text;
using GSDK;
using UNBridgeLib.LitJson;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#pragma warning disable 0618
class TransformComponentInfoProcessor : IPreprocessBuild
{
#pragma warning restore 0618
    public int callbackOrder
    {
        get { return 10; }
    }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        return;
        
        Debug.Log("begin transform gsdk.json content to android platform content.");
        
        string gsdkJsonPath = Path.Combine(GMSDKEnv.Instance.PATH_ASSETS_GSDK, "gsdk.json");
        if (!File.Exists(gsdkJsonPath))
        {
            // gsdk.json文件不存在
            GLog.LogDebug("gsdk.json文件不存在，因此无法进行转换并移动文件。文件路径：" + gsdkJsonPath);
        }
        else
        {
            string sourceContent = File.ReadAllText(gsdkJsonPath);
            string targetContent = TransformContent2AndroidPlatform(sourceContent);
            if (!Directory.Exists(Path.Combine(GMSDKEnv.Instance.PATH_PUGLIN_ANDROID, "assets")))
            {
                Directory.CreateDirectory(Path.Combine(GMSDKEnv.Instance.PATH_PUGLIN_ANDROID, "assets"));
            }
            string newJsonPath = Path.Combine(GMSDKEnv.Instance.PATH_PUGLIN_ANDROID, @"assets/gsdk.json");
            File.WriteAllText(newJsonPath, targetContent);
        }
    }

    /// <summary>
    /// 转换gsdk.json的内容为android平台的配置内容
    /// </summary>
    /// <param name="sourceContent">源配置内容</param>
    /// <returns></returns>
    private string TransformContent2AndroidPlatform(string sourceContent)
    {
        JsonData json = JsonMapper.ToObject(sourceContent);
        if (!json.ContainsKey("dsl"))
        {
            return sourceContent;
        }
        if (json.ContainsKey("region"))
        {
            string region = json["region"].ToString() == "domestic" ? "cn" : "i18n";
            json["region"] = region;
        }

        JsonData dslJson = json["dsl"];
        if (dslJson.ContainsKey("region"))
        {
            string region = dslJson["region"].ToString() == "domestic" ? "cn" : "i18n";
            dslJson["region"] = region;
        }

        if (!dslJson.ContainsKey("components"))
        {
            return sourceContent;
        }

        JsonData componentsJson = dslJson["components"];

        foreach (JsonData component in componentsJson)
        {
            string name = component.ContainsKey("name") ? (string) component["name"] : "";
            ComponentInfo target = ComponentManager.ComponentInfos.ContainsKey(name)
                ? ComponentManager.ComponentInfos[name] : null;
            if (target != null)
            {
                component["android_name"] = target.androidName;
                component["unity_name"] = target.unityName;
                string version = component.ContainsKey("version") ? (string) component["version"] : "";
                if (version == "" && target.name == ComponentName.Main)
                {
                    version = dslJson.ContainsKey("baseline") ? (string) dslJson["baseline"] : "";
                    component["version"] = version;
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);
        writer.PrettyPrint = true;
        writer.IndentValue = 4;
        JsonMapper.ToJson(json, writer);
        return sb.ToString();
    }
}