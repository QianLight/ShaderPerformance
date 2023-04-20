using Devops.Performance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;


public enum ScreenType
{
    None,
    SceenShot,
    ObjectShot
}

public class BPTestNode : MonoBehaviour
{


    public void StartProfiler(string profilerTag)
    {
        string[] tags = profilerTag.Split(',');
        PTestManager.Instance.StartProfiler(tags);
    }

    public void StartProfiler(string profilerTag,int frame, bool snap)
    {
        string[] tags = profilerTag.Split(',');
        PTestManager.Instance.StartProfiler(tags, frame, snap);
    }

    public void StopProfiler()
    {
        PTestManager.Instance.StopProfiler();
    }

    public GameObject[] GetChildButton(string gameObject)
    {
        GameObject parent = GetGameObject(gameObject);
        Button[] btns = parent.GetComponentsInChildren<Button>();
        List<GameObject> childs = new List<GameObject>();
        foreach (Button btn in btns)
        {
            childs.Add(btn.gameObject);
        }
        return childs.ToArray();
    }

    public void PointClick(string gameObject)
    {
        GameObject obj = GetGameObject(gameObject);
        if (obj != null)
            ExecuteEvents.Execute(obj, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    public void PointClick(GameObject gameObject)
    {
        ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    public void Drag(GameObject gameObject, Vector2 startpos, Vector2 endpos)
    {
        StartCoroutine(Draging(gameObject, startpos, endpos));
    }

    private IEnumerator Draging(GameObject gameObject, Vector2 startpos, Vector2 endpos)
    {
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = startpos;

        while (Vector3.Distance(pointer.position, endpos) > 0.1f)
        {
            pointer.position = Vector3.Lerp(pointer.position, endpos, Time.deltaTime * 3);
            ExecuteEvents.Execute<IDragHandler>(gameObject, pointer, ExecuteEvents.dragHandler);
            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }


    /// <summary>
    /// 获取物体
    /// </summary>
    /// <param name="objname">物体名</param>
    /// <returns></returns>
    //public GameObject GetGameObject(string objname)
    //{
    //    GameObject Target = GameObject.Find(objname).transform.gameObject;

    //    return Target;
    //}


    public GameObject GetGameObject(string objpath)
    {
        string[] paths = objpath.Split('/');
        bool root = true;
        GameObject Target = null;
        foreach (string name in paths)
        {
            if (root)
            {
                Target = GameObject.Find(name).gameObject;
                root = false;
            }
            else if (Target != null)
                Target = Target.transform.Find(name).gameObject;

            if (Target == null)
                return null;
        }


        //GameObject Target = GameObject.Find(objpath).transform.gameObject;

        return Target;
    }

    /// <summary>
    /// 获取对应脚本
    /// </summary>
    /// <typeparam name="C">脚本类型</typeparam>
    /// <param name="objname">物体名</param>
    /// <returns></returns>
    public C GetGameObject<C>(string objname) where C : Component
    {
        GameObject Target = GameObject.Find(objname).transform.gameObject;

        return Target.GetComponent<C>();
    }


    /// <summary>
    /// 判断方法
    /// </summary>
    /// <param name="result">执行结果</param>
    /// <param name="successed">成功回调</param>
    /// <param name="failed">失败回调</param>
    /// <returns></returns>
    public bool CaseDecide(bool result, Action successed, Action failed)
    {
        if (result)
            successed?.Invoke();
        else
            failed?.Invoke();

        return result;
    }
    /// <summary>
    /// True 断言输出
    /// </summary>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool CaseAssertTrue(bool result, string msg)
    {
        Assert.IsTrue(result, msg);
        return result;
    }
    /// <summary>
    /// False 断言输出
    /// </summary>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public bool CaseAssertFalse(bool result, string msg)
    {
        Assert.IsFalse(result, msg);
        return result;
    }

    /// <summary>
    /// 结果保存
    /// </summary>
    /// <param name="key">测试Case名</param>
    /// <param name="des">测试结果描述</param>
    /// <param name="successed">是否成功</param>
    public void CaseSteps(string key, string des, string tag, bool successed, ScreenType screenType)
    {
        PTestUtils.AddCaseSteps(key, des, tag, successed, screenType);
    }
    /// <summary>
    /// 成功结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public void CaseStepsSuccessed(string key, string des, string tag)
    {
        PTestUtils.AddCaseSteps(key, des, tag, true, ScreenType.None);
    }

    public void CaseStepsSuccessedShot(string key, string des, string tag)
    {
        PTestUtils.AddCaseSteps(key, des, tag, true, ScreenType.SceenShot);
    }


    public void CaseStepsFailed(string key, string des, string tag)
    {
        PTestUtils.AddCaseSteps(key, des, tag, false, ScreenType.SceenShot);
    }


    /// <summary>
    /// 结果保存
    /// </summary>
    /// <param name="key">测试Case名</param>
    /// <param name="des">测试结果描述</param>
    /// <param name="successed">是否成功</param>
    public void _CaseSteps(string key, string des, string tag, bool successed, ScreenType screenType)
    {
        PTestUtils.AddCaseSteps(key, des, tag, successed, screenType);
    }
    /// <summary>
    /// 成功结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public void _CaseStepsSuccessed(string key, string des, string tag)
    {
        PTestUtils.AddCaseSteps(key, des, tag, true, ScreenType.None);
    }

    /// <summary>
    /// 成功结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public void _CaseStepsSuccessedShot(string key, string des, string tag)
    {
        PTestUtils.AddCaseSteps(key, des, tag, true, ScreenType.SceenShot);
    }

    /// <summary>
    /// 失败结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public void _CaseStepsFailed(string key, string des, string tag)
    {
        PTestUtils.AddCaseSteps(key, des, tag, false, ScreenType.SceenShot);
    }


    //public IEnumerator StartCase(CaseItemData caseItem)
    //{

    //    PTestData.key = caseItem.type.Name + "." + caseItem.method.Name;
    //    long start = PTestUtils.GetTimeStamp();
    //    PTestData.start = start;
    //    PTestDataManager.Instance.CreateCaseData(PTestData.key);
    //    IEnumerator enumerator = (IEnumerator)caseItem.method.Invoke(caseItem.obj, new object[] { });
    //    yield return enumerator;
    //}

    public bool isEditorPlatform()
    {
#if UNITY_EDITOR
        return true;
#endif
        return false;
    }

    public bool isPhonePlatform()
    {
#if UNITY_EDITOR
        return false;
#endif
        return true;
    }


    // Public function to emulate a mouse button click (left button)
    public void MouseClick()
    {
        PTestUtils.MouseClick();
    }

    // Public function to emulate a mouse drag event (left button)
    public void MouseDrag()
    {
        PTestUtils.MouseDrag();
    }

    // Public function to emulate a mouse release event (left button)
    public void MouseRelease()
    {
        PTestUtils.MouseRelease();
    }

    public void MouseMove(int x, int y)
    {
        PTestUtils.MouseMove(x, y);
    }

    public void MouseMove(Vector3 screenCoordinates)
    {
        PTestUtils.MouseMove(screenCoordinates);
    }


    public void AddTag(string tag)
    {
        PTestManager.Instance.SendTag(tag);

    }
    public void DownCSVToJson(string url, string contentName,Action<string> action)
    {
        StartCoroutine(DownloadFileToJson(url, contentName, action));
    }


    public void DownCSV(string url, string contentName)
    {
        StartCoroutine(DownloadFile(url, contentName));
    }

    public string FilePath(string contentName)
    {
        
        string downloadFileName = Application.dataPath+"/"+ contentName;
#if UNITY_EDITOR
        downloadFileName = Path.Combine(Application.dataPath, contentName);
#elif UNITY_ANDROID
        downloadFileName = Path.Combine(Application.persistentDataPath, contentName);
#elif UNITY_IOS
        downloadFileName = Path.Combine(Application.persistentDataPath, contentName);
#endif
            return downloadFileName;
    }

    public IEnumerator DownloadFile(string url, string contentName)
    {
        string downloadFileName = Application.dataPath + "/" + contentName;
#if UNITY_EDITOR
        downloadFileName = Path.Combine(Application.dataPath, contentName);
#elif UNITY_ANDROID
        downloadFileName = Path.Combine(Application.persistentDataPath, contentName);
#elif UNITY_IOS
        downloadFileName = Path.Combine(Application.persistentDataPath, contentName);
#endif
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.LogError("PTest :file Failed down :" + webRequest.error);
            }
            else
            {
                DownloadHandler fileHandler = webRequest.downloadHandler;
                using (MemoryStream memory = new MemoryStream(fileHandler.data))
                {
                    byte[] buffer = new byte[1024 * 1024];
                    FileStream file = File.Open(downloadFileName, FileMode.OpenOrCreate);
                    int readBytes;
                    while ((readBytes = memory.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        file.Write(buffer, 0, readBytes);
                    }
                    file.Close();
                    Debug.LogError("PTest :file down successed");
                }
            }
        }
    }

    public IEnumerator DownloadFileToJson(string url, string contentName,Action<string> action)
    {
        string downloadFileName = Application.dataPath + "/" + contentName;
#if UNITY_EDITOR
        downloadFileName = Path.Combine(Application.dataPath, contentName);
#elif UNITY_ANDROID
        downloadFileName = Path.Combine(Application.persistentDataPath, contentName);
#elif UNITY_IOS
        downloadFileName = Path.Combine(Application.persistentDataPath, contentName);
#endif
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.LogError("PTest :file Failed down :" + webRequest.error);
            }
            else
            {
                DownloadHandler fileHandler = webRequest.downloadHandler;
                using (MemoryStream memory = new MemoryStream(fileHandler.data))
                {

                    if (File.Exists(downloadFileName))
                    {
                        File.Delete(downloadFileName);
                    }

                    byte[] buffer = new byte[1024 * 1024];
                    FileStream file = File.Open(downloadFileName, FileMode.OpenOrCreate);
                    int readBytes;
                    while ((readBytes = memory.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        file.Write(buffer, 0, readBytes);
                    }
                    file.Close();

                    string json = ReadCSVToJson(downloadFileName);
                    action?.Invoke(json);
                    Debug.LogError("PTest :file down successed");
                }
            }
        }
    }


    public List<object> ReadCSV(string path, object t)
    {
        List<object> list = new List<object>();
        Encoding utf = Encoding.GetEncoding("UTF-8");
        string InfoConfig = File.ReadAllText(path, utf);

        InfoConfig = InfoConfig.Replace("\r", "");
        InfoConfig = InfoConfig.Replace("\"", "");
        string[] CSVDatas = InfoConfig.Split('\n');

        for (int i = 1; i < CSVDatas.Length; i++)
        {
            if (CSVDatas[i] != "")
            {
                string[] infos = CSVDatas[i].Split(',');
                object obj = System.Activator.CreateInstance(t.GetType());
                obj = SetCSVValue(obj, infos);
                list.Add(obj);
            }
        }

        return list;
    }


    public object SetCSVValue(object data, string[] infos)
    {
        Type type = data.GetType();
        FieldInfo[] fields = type.GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
           
            string value = infos[i];
            field.SetValue(data, Convert.ChangeType(value, field.FieldType));
        }
        return data;
    }



    public string ReadCSVToJson(string path)
    {

        StringBuilder builder = new StringBuilder();
        Encoding utf = Encoding.GetEncoding("UTF-8");
        string InfoConfig = File.ReadAllText(path, utf);

        InfoConfig = InfoConfig.Replace("\r", "");
        InfoConfig = InfoConfig.Replace("\"", "");
        string[] CSVDatas = InfoConfig.Split('\n');
        string[] keys = CSVDatas[0].Split(',');
        string[] types = CSVDatas[1].Split(',');
        List<string> _datas = new List<string>();
        for (int i = 3; i < CSVDatas.Length; i++)
        {
            if (CSVDatas[i] != "")
            {
                _datas.Add(CSVDatas[i]);
            }
         }
        builder.Append("[");
        for (int i = 0; i < _datas.Count; i++)
        {
            if (_datas[i] != "")
            {
                string[] infos = _datas[i].Split(',');

                List<string> items = new List<string>();

                bool isSpecial = false;

                string special = "";

                for (int j = 0; j < infos.Length; j++)
                {
                    string _item = infos[j];
                    if (_item.Contains("[]")){

                    }else if (_item.Contains("[") && _item.Contains("]"))
                    {
                        //items.Add(_item);
                    }
                    else if(_item.Contains("[")|| _item.Contains("{"))
                    {
                        isSpecial = true;
                        special += _item + ",";
                    }
                    else if(_item.Contains("]") || _item.Contains("}"))
                    {
                        isSpecial = false;
                        special += _item;
                        _item = special;
                        special = "";
                    }else if (isSpecial)
                    {
                        special += _item + ",";
                    }
                    if(!isSpecial)
                        items.Add(_item);


                }

                string item = SetCSVValueToJson(keys, items.ToArray(), types, i == _datas.Count - 1);
                builder.AppendLine(item);

            }
          

        }
        builder.Append("]");
        return builder.ToString();
    }


    public string ReadCSVStringToJson(string content)
    {

        StringBuilder builder = new StringBuilder();
        Encoding utf = Encoding.GetEncoding("UTF-8");
        string InfoConfig = content;

        InfoConfig = InfoConfig.Replace("\r", "");
        InfoConfig = InfoConfig.Replace("\"", "");
        string[] CSVDatas = InfoConfig.Split('\n');
        string[] keys = CSVDatas[0].Split(',');
        string[] types = CSVDatas[1].Split(',');
        List<string> _datas = new List<string>();
        for (int i = 3; i < CSVDatas.Length; i++)
        {
            if (CSVDatas[i] != "")
            {
                _datas.Add(CSVDatas[i]);
            }
        }
        builder.Append("[");
        for (int i = 0; i < _datas.Count; i++)
        {
            if (_datas[i] != "")
            {
                string[] infos = _datas[i].Split(',');

                List<string> items = new List<string>();

                bool isSpecial = false;

                string special = "";

                for (int j = 0; j < infos.Length; j++)
                {
                    string _item = infos[j];
                    if (_item.Contains("[]"))
                    {

                    }
                    else if (_item.Contains("[") && _item.Contains("]"))
                    {
                        //items.Add(_item);
                    }
                    else if (_item.Contains("[") || _item.Contains("{"))
                    {
                        isSpecial = true;
                        special += _item + ",";
                    }
                    else if (_item.Contains("]") || _item.Contains("}"))
                    {
                        isSpecial = false;
                        special += _item;
                        _item = special;
                        special = "";
                    }
                    else if (isSpecial)
                    {
                        special += _item + ",";
                    }
                    if (!isSpecial)
                        items.Add(_item);


                }

                string item = SetCSVValueToJson(keys, items.ToArray(), types, i == _datas.Count - 1);
                builder.AppendLine(item);

            }


        }
        builder.Append("]");
        return builder.ToString();
    }

    public string SetCSVValueToJson(string[] data, string[] infos,string[] types,bool last)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("{");
        for (int i = 0; i < infos.Length; i++)
        {
            string key = data[i];
            string value = infos[i];
            string type = types[i];
            if (value == ""&& !type.Contains("string"))
            {
                value = "null";
            }
            if (type.Contains("[")|| type.Contains("{"))
            {
                if (value.Contains("[]"))
                {
                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":" + value + "\"");

                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":" + value + ",");

                    }
                }
                else if (value.Contains("["))
                {
                    string text = value.Replace("[", "").Replace("]", "");
                    string[] items = text.Split(',');

                    string newText = "[";

                    for (int j = 0; j < items.Length; j++)
                    {
                        string item = items[j];
                        if (!type.Contains("string"))
                        {
                            if (j == items.Length - 1)
                            {
                                newText += item;
                            }
                            else
                            {
                                newText += item + ",";

                            }
                        }
                        else
                        {
                            if (j == items.Length - 1)
                            {
                                newText += "\"" + item + "\"";
                            }
                            else
                            {
                                newText += "\"" + item + "\"" + ",";

                            }
                        }
                    }
                    newText += "]";


                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":" + newText);
                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":" + newText + ",");
                    }

                }
                else if (value.Contains("{"))
                {


                    string text = value.Replace("{", "").Replace("}", "");
                    string[] items = text.Split(',');

                    string newText = "{";

                    for (int j = 0; j < items.Length; j++)
                    {
                        string item = items[j];
                        string[] itemkeyvalue = item.Split(':');
                        string itemkey = itemkeyvalue[0];
                        string itemvalue = itemkeyvalue[1];

                        if (!type.Contains("string"))
                        {
                            if (j == items.Length - 1)
                            {
                                newText += "\"" + itemkey + "\"" + ":" + itemvalue;
                            }
                            else
                            {
                                newText += "\"" + itemkey + "\"" + ":" + itemvalue + ",";

                            }
                        }
                        else
                        {
                            if (j == items.Length - 1)
                            {
                                newText += "\"" + itemkey + "\"" + ":" + "\"" + itemvalue + "\"";
                            }
                            else
                            {
                                newText += "\"" + itemkey + "\"" + ":" + "\"" + itemvalue + "\"" + ",";

                            }
                        }
                    }
                    newText += "}";


                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":" + newText);
                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":" + newText + ",");
                    }

                }else if(value == "")
                {
                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":null"  + "");

                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":null" + ",");

                    }
                }
            }
            else
            {
                if (type.Contains("string"))
                {
                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":" + "\"" + value + "\"");
                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":" + "\"" + value + "\",");
                    }
                }else if (type.Contains("bool"))
                {
                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":" + value.ToLower() + "");

                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":" + value.ToLower() + ",");

                    }
                }
                else
                {
                    
                    if (i == infos.Length - 1)
                    {
                        builder.Append("\"" + key + "\"" + ":" + value + "");

                    }
                    else
                    {
                        builder.Append("\"" + key + "\"" + ":" + value + ",");

                    }
                }
            }

        }
        if (last)
            builder.Append("}");
        else
            builder.Append("},");
        return builder.ToString(); 
    }



    public  string GetCaseParams(int index, string defaultValue)
    {

#if UNITY_EDITOR
        return defaultValue;
#else
        switch (index)
        {
            case 0:
                if (PTestManager.Instance.InTestCaseItem != null)
                {
                    if (!string.IsNullOrEmpty(PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam0))
                        return PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam0;
                    else
                        return defaultValue;
                }
                else
                {
                    return defaultValue;
                }
                break;
            case 1:
                if (PTestManager.Instance.InTestCaseItem != null)
                {
                    if (!string.IsNullOrEmpty(PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam1))
                        return PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam1;
                    else
                        return defaultValue;
                }
                else
                {
                    return defaultValue;
                }
                break;
            case 2:
                if (PTestManager.Instance.InTestCaseItem != null)
                {
                    if (!string.IsNullOrEmpty(PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam2))
                        return PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam2;
                    else
                        return defaultValue;
                }
                else
                {
                    return defaultValue;
                }
                break;
            case 3:
                if (PTestManager.Instance.InTestCaseItem != null)
                {
                    if (!string.IsNullOrEmpty(PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam3))
                        return PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam3;
                    else
                        return defaultValue;
                }
                else
                {
                    return defaultValue;
                }
                break;
            case 4:
                if (PTestManager.Instance.InTestCaseItem != null)
                {
                    if (!string.IsNullOrEmpty(PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam4))
                        return PTestManager.Instance.InTestCaseItem.caseItemData.CaseParam4;
                    else
                        return defaultValue;
                }
                else
                {
                    return defaultValue;
                }
                break;
            default:
                return defaultValue;
                break;
        }
#endif
        return defaultValue;


       
    }

    public  static bool isRunning()
    {
        return PTestManager.Instance.Testing;
    }


    public  void CaseFinished()
    {

    }
}
