using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class PTestNetManager : Singleton<PTestNetManager>
{

    private string reuslt = "result";
    private string upload = "upload";
    private string config = "config";
    private string getConfig = "getConfig";

    private float fps = 0;
    private float totalTimer, last_1_frame_cost, last_2_frame_cost, last_3_frame_cost;

    private int jank;
    private int bigJank;
    private float timer = 0;
    private FPSData fPSData = null;


    public Action JankEvent;
    public void Init()
    {
        //clientSocket = new ClientSocket("127.0.0.1", 5020);
        SocketHelper.GetInstance().SetCallBack(SocketCallBack);
        SocketHelper.GetInstance().StartServer();
        //s.SendMessage("Test");
    }


    private float m_time = 0.0f;
    private float game_timer = 0.0f;

    void Update()
    {

        if (!PTestManager.Instance.Testing)
            return;
        m_time += (Time.unscaledDeltaTime - m_time) * 0.1f;
        fps = 1.0f / m_time;

        float this_frame_cost = Time.unscaledDeltaTime;
        float movie = ((float)1 / (float)24);
        if (last_3_frame_cost != 0 && last_2_frame_cost != 0 && last_1_frame_cost != 0)
        {
            float rev_frame = (last_1_frame_cost + last_2_frame_cost + last_3_frame_cost) / 3;

            if (this_frame_cost > rev_frame * 2 && this_frame_cost > (movie * 2))
            {
                jank++;
                if (this_frame_cost > rev_frame * 2 && this_frame_cost > (movie * 3))
                {
                    bigJank++;
                }
            }

        }

        last_3_frame_cost = last_2_frame_cost;
        last_2_frame_cost = last_1_frame_cost;
        last_1_frame_cost = this_frame_cost;

        totalTimer += Time.unscaledDeltaTime;
        // 每帧累加的时间如果大于等于1就可以修改时间
        if (totalTimer >= 1)
        {
            if (jank > 0|| bigJank > 0){
                JankEvent?.Invoke();
            }

            fPSData = new FPSData();
            fPSData.FPS = fps;
            fPSData.jank = jank;
            fPSData.bigJank = bigJank;
            fPSData.gameTimer = Convert.ToInt64(Time.realtimeSinceStartup);
            string data = "DATAFPS$" + JsonUtility.ToJson(fPSData);
            //Debug.Log(data);
            SocketHelper.GetInstance().SendMessage(data);
            totalTimer = 0f;
            jank = 0;
            bigJank = 0;
            timer++;
        }


    }
    public void SetCSVDatas(string content)
    {
        CSVDatas datas = JsonUtility.FromJson<CSVDatas>(content);
        foreach (CSVData data in datas.content)
        {
            PTestNodeManager.Instance.extend_list.Add(data.name, data.content);
        }
    }

    public void SocketCallBack(string result)
    {
        if (result == "Status")
        {
            if (PTestManager.Instance.Testing)
                SocketHelper.GetInstance().SendMessage(SocketHelper.SocketStatus.TestStart.ToString());
            else
                SocketHelper.GetInstance().SendMessage(SocketHelper.SocketStatus.TestWait.ToString());
        }
        else if (result.Contains("pwrd_Start"))
        {
            string json = result.Split('$')[1];
            CaseNetList caseNetItemDatas = JsonUtility.FromJson<CaseNetList>(json);
            PTestManager.Instance.outTimer = caseNetItemDatas.caseTime;
            PTestManager.Instance.planName = caseNetItemDatas.planName;
            PTestNodeManager.Instance.extend_config = caseNetItemDatas.extend_config;

            try
            {
                Debug.Log("CaseList ：" + JsonUtility.ToJson(caseNetItemDatas));
                SetCSVDatas(caseNetItemDatas.extend_config);
            }
            catch (Exception e)
            {

            }
                
            
            if (caseNetItemDatas.startprofiler)
            {
                PTestManager.Instance.startprofiler = caseNetItemDatas.startprofiler;
                //PTestManager.Instance.ProfilerSnapPremises();

            }

            PTestManager.Instance.LoadCaseList(caseNetItemDatas);
            PTestManager.Instance.StartCases(true);
        }
        Debug.Log("SocketHelper 结果：" + result);

    }




    IEnumerator Post<T>(string api, Action<RequestData<T>, bool> action)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(PTestNetConfig.url + api, "");
        yield return request.SendWebRequest();


        if (request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
            action?.Invoke(null, false);
        }
        else
        { 
            string receiveContent = request.downloadHandler.text;
            Debug.Log(receiveContent);
            RequestData<T> result = JsonUtility.FromJson<RequestData<T>>(receiveContent);
            action?.Invoke(result, true);
        }
    }

    IEnumerator PostFile(string api, string form)
    {
        byte[] databyte = Encoding.UTF8.GetBytes(form);
        UnityWebRequest request = new UnityWebRequest(PTestNetConfig.url + api, UnityWebRequest.kHttpVerbPOST);
        request.useHttpContinue = true;
        request.uploadHandler = new UploadHandlerRaw(databyte);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        yield return request.SendWebRequest();


        if (request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string receiveContent = request.downloadHandler.text;
            Debug.Log(receiveContent);
        }
    }

    IEnumerator PostFile<T>(string api, string form, Action<RequestData<T>, bool> action = null)
    {
        byte[] databyte = Encoding.UTF8.GetBytes(form);
        UnityWebRequest request = new UnityWebRequest(PTestNetConfig.url + api, UnityWebRequest.kHttpVerbPOST);
        request.useHttpContinue = true;
        request.uploadHandler = new UploadHandlerRaw(databyte);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        yield return request.SendWebRequest();


        if (request.isHttpError || request.isNetworkError)
        {
            Debug.LogError(request.error);
            action?.Invoke(null, false);
        }
        else
        {
            string receiveContent = request.downloadHandler.text;
            Debug.Log(receiveContent);
            RequestData<T> result = JsonUtility.FromJson<RequestData<T>>(receiveContent);
            action?.Invoke(result, true);
        }
    }

    IEnumerator UpLoadTexture(Attachments imageData)
    {
        FileStream fs = new FileStream(Application.persistentDataPath + "/" + imageData.source, FileMode.Open, FileAccess.Read);
        byte[] fileByte = new byte[fs.Length];
        fs.Read(fileByte, 0, fileByte.Length);
        fs.Close();

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileByte, imageData.source);
        using (UnityWebRequest www = UnityWebRequest.Post(PTestNetConfig.url + upload, form))
        {

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("上传失败:" + www.error);
            }
            else
            {
                string text = www.downloadHandler.text;
                Debug.Log("服务器返回值" + text);//正确打印服务器返回值
                Debug.Log("上传成功！");
            }
        }
    }



    public void SendUpLoadTexture(List<Attachments> attachments, Action callback)
    {
        StartCoroutine(SendTexture(attachments, callback));
    }

    public IEnumerator SendTexture(List<Attachments> attachments, Action callback)
    {
        foreach (Attachments imageData in attachments)
        {
            yield return UpLoadTexture(imageData);
        }
        callback?.Invoke();
    }


    public void SendPostResult(string json)
    {
        StartCoroutine(PostFile(reuslt, json));
    }

    public void SendPostConfig<T>(T response)
    {
        string datasJson = JsonUtility.ToJson(response);
        StartCoroutine(PostFile(config, datasJson));
    }

    public void GetConfigPost<T>(T response, Action<RequestData<CaseNetList>, bool> action)
    {
        string datasJson = JsonUtility.ToJson(response);
        StartCoroutine(PostFile(getConfig, datasJson, action));
    }

    public static string Result(string container, string datas)
    {
        string result = "{" + "\"container\":" + container + "," + "\"datas\":" + datas + "}";
        return result;
    }

    public float GetTimer()
    {
        return timer;
    }
}
