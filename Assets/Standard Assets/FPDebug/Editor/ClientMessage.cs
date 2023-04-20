using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Text;

public class ClientMessage
{
    public static void GetObjectList(Action<SCList> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=getobject", basePath));
        if (result != null)
        {
            //Debug.Log(result);
            SCList obj = JsonUtility.FromJson<SCList>(result);
            callback(obj);
        }
        else
        {
            callback(null);
        }
    }
    public static void GetModelInfo(Action<string> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=info", basePath));
        if (result != null)
        {
            callback(result);
        }
        else
        {
            callback(null);
        }
    }
    public static void GetShadertList(bool share, Action<FPDebugShaderList> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=shaderlist&share={1}", basePath, share ? "1" : "0"));
        if (result != null)
        {
            FPDebugShaderList obj = JsonUtility.FromJson<FPDebugShaderList>(result);
            callback(obj);
        }
        else
        {
            callback(null);
        }
    }
    public static void ShowShader(int id, int show, bool other, Action<string> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=showshader&id={1}&state={2}&other={3}", basePath, id, show, other ? "1" : "0"));
        if (result != null)
        {
            callback(result);
        }
        else
        {
            callback(null);
        }
    }
    public static void ReplaceShader(string id, byte[] ab, Action<object> callback)
    {
        string url = string.Format("http://{0}:8820/?cmd=replaceshader&id={1}", basePath, id);
        string result = PostRequestResult(url, ab);
        callback(result);
    }

    public static void ReplaceColorShader(int id, byte[] ab, Action<object> callback)
    {
        string url = string.Format("http://{0}:8820/?cmd=replacecolorshader&id={1}", basePath, id);
        string result = PostRequestResult(url, ab);
        callback(result);
    }

    public static void GetFPS(bool clear, Action<string[]> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=fps&clear={1}", basePath, clear));
        if (result != null)
        {
            callback(result.Split(new char[] { ',' }));
        }
        else
        {
            callback(null);
        }
    }
    public static void GetRenderInfo(Action<RenderInfo> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=getrender", basePath));
        if (result != null)
        {
            //Debug.Log(result);
            RenderInfo obj = JsonUtility.FromJson<RenderInfo>(result);
            callback(obj);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetRenderInfo(RenderInfo info, Action<object> callback)
    {
        string objStr = JsonUtility.ToJson(info);
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=setrender&render={1}", basePath, objStr));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void PauseGame(string p, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=pause&state={1}", basePath, p));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetLog(string p, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=log&state={1}", basePath, p));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetCamera(string p, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=camera&state={1}", basePath, p));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetSrpBatch(string p, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=srpbatch&state={1}", basePath, p));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void Start(Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=start", basePath));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetObject(int id, bool enable, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=object&id={1}&action={2}", basePath, id, enable ? "enable" : "disable"));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetPost(int id, string post, bool enable, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=post&id={1}&name={2}&action={3}", basePath, id, post, enable ? "enable" : "disable"));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void GetPostPara(int id, string post, Action<PVP> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=getpostpara&id={1}&name={2}", basePath, id, post));
        if (result != null && result != "fail")
        {
            //Debug.Log(result);
            PVP obj = JsonUtility.FromJson<PVP>(result);
            callback(obj);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetPostPara(int id, string post, string para, bool enable, string value, Action<object> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=setpostpara&id={1}&name={2}&para={3}&action={4}&value={5}", basePath, id, post, para, enable ? "enable" : "disable", value));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void GetMaterialPara(int id, Action<RenderMat> callback)
    {
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=getmaterialpara&id={1}", basePath, id));
        if (result != null && result != "fail")
        {
            RenderMat obj = JsonUtility.FromJson<RenderMat>(result);
            callback(obj);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetMaterialPara(RenderMat rm, Action<object> callback)
    {
        string objStr = JsonUtility.ToJson(rm);
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=setmaterialpara&id={1}&mat={2}", basePath, rm.ID, objStr));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    public static void SetGlobalPara(GlobalPara rm, Action<object> callback)
    {
        string objStr = JsonUtility.ToJson(rm);
        string result = GetRequestResult(string.Format("http://{0}:8820/?cmd=setglobalpara&para={1}", basePath, objStr));
        if (result != null && result == "ok")
        {
            callback(true);
        }
        else
        {
            callback(null);
        }
    }
    private static string basePath;
    public static void Init(string path)
    {
        basePath = path;
    }
    static HttpWebRequest GetRequest(string url, string method)
    {
        HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
        req.Timeout = 20000;
        req.Method = method;
        return req;
    }
    static string GetRequestResult(string url)
    {
        HttpWebRequest req = GetRequest(url, "GET");
        string result = null;
        try
        {
            using (WebResponse wr = req.GetResponse())
            {
                using (Stream resStream = wr.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(resStream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        return result;
    }
    static string PostRequestResult(string url, byte[] dat)
    {
        string result = null;
        HttpWebRequest req = GetRequest(url, "POST");
        req.ContentType = "application/json";
        req.ContentLength = dat.Length;
        try
        {
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(dat, 0, dat.Length);
                reqStream.Close();
            }

            using (WebResponse wr = req.GetResponse())
            {
                using (Stream resStream = wr.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(resStream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                    resStream.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        return result;
    }
}
