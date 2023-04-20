using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class PTestNode
{

    public static void StartProfiler(string profilerTag)
    {
        PTestNodeManager.Instance.testNode.StartProfiler(profilerTag);
    }

    public static void StartProfiler(string profilerTag, int frame, bool snap)
    {
        PTestNodeManager.Instance.testNode.StartProfiler(profilerTag, frame,snap);

    }

    public static void StopProfiler()
    {
        PTestNodeManager.Instance.testNode.StopProfiler();
    }

    public static GameObject[] GetChildButton(string gameObject)
    {
      return   PTestNodeManager.Instance.testNode.GetChildButton(gameObject);
    }

    public static void PointClick(string gameObject)
    {
        PTestNodeManager.Instance.testNode.PointClick(gameObject);
    }

    public static void PointClick(GameObject gameObject)
    {
        PTestNodeManager.Instance.testNode.PointClick(gameObject);
    }

    public static void Drag(GameObject gameObject, Vector2 startpos, Vector2 endpos)
    {
        PTestNodeManager.Instance.testNode.Drag(gameObject, startpos, endpos);
    }

    public static void BindJankEvent(Action action)
    {
        PTestNetManager.Instance.JankEvent = action;
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


    public static GameObject GetGameObject(string objpath)
    {


        return PTestNodeManager.Instance.testNode.GetGameObject(objpath);
    }

    /// <summary>
    /// 获取对应脚本
    /// </summary>
    /// <typeparam name="C">脚本类型</typeparam>
    /// <param name="objname">物体名</param>
    /// <returns></returns>
    public static C GetGameObject<C>(string objname) where C : Component
    {

        return PTestNodeManager.Instance.testNode.GetGameObject<C>(objname);
    }


    /// <summary>
    /// 判断方法
    /// </summary>
    /// <param name="result">执行结果</param>
    /// <param name="successed">成功回调</param>
    /// <param name="failed">失败回调</param>
    /// <returns></returns>
    public static bool CaseDecide(bool result, Action successed, Action failed)
    {

        return PTestNodeManager.Instance.testNode.CaseDecide(result,successed,failed);
    }
    /// <summary>
    /// True 断言输出
    /// </summary>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static bool CaseAssertTrue(bool result, string msg)
    {
        return PTestNodeManager.Instance.testNode.CaseAssertTrue(result, msg);
    }
    /// <summary>
    /// False 断言输出
    /// </summary>
    /// <param name="result"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public static bool CaseAssertFalse(bool result, string msg)
    {
        return PTestNodeManager.Instance.testNode.CaseAssertFalse(result, msg);
    }

    /// <summary>
    /// 结果保存
    /// </summary>
    /// <param name="key">测试Case名</param>
    /// <param name="des">测试结果描述</param>
    /// <param name="successed">是否成功</param>
    public static void CaseSteps(string key, string des, string tag, bool successed, ScreenType screenType)
    {
        PTestNodeManager.Instance.testNode.CaseSteps( key,  des,  tag,  successed,  screenType);
    }
    /// <summary>
    /// 成功结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public static void CaseStepsSuccessed(string key, string des, string tag)
    {
        PTestNodeManager.Instance.testNode.CaseStepsSuccessed( key,  des,  tag);
    }

    public static void CaseStepsSuccessedShot(string key, string des, string tag)
    {
        PTestNodeManager.Instance.testNode.CaseStepsSuccessedShot( key,  des,  tag);
    }


    public static void CaseStepsFailed(string key, string des, string tag)
    {
        PTestNodeManager.Instance.testNode.CaseStepsFailed( key,  des,  tag);
    }


    /// <summary>
    /// 结果保存
    /// </summary>
    /// <param name="key">测试Case名</param>
    /// <param name="des">测试结果描述</param>
    /// <param name="successed">是否成功</param>
    public static void _CaseSteps(string key, string des, string tag, bool successed, ScreenType screenType)
    {
        PTestNodeManager.Instance.testNode._CaseSteps(key, des, tag, successed, screenType);
    }
    /// <summary>
    /// 成功结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public static void _CaseStepsSuccessed(string key, string des, string tag)
    {
        PTestNodeManager.Instance.testNode._CaseStepsSuccessed(key, des, tag);

    }

    /// <summary>
    /// 成功结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public static void _CaseStepsSuccessedShot(string key, string des, string tag)
    {
        PTestNodeManager.Instance.testNode._CaseStepsSuccessedShot(key, des, tag);

    }

    /// <summary>
    /// 失败结果保存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="des"></param>
    public static void _CaseStepsFailed(string key, string des, string tag)
    {
        PTestNodeManager.Instance.testNode._CaseStepsFailed(key, des, tag);

    }


    public static bool isEditorPlatform()
    {
#if UNITY_EDITOR
        return true;
#endif
        return false;
    }

    public static bool isPhonePlatform()
    {
#if UNITY_EDITOR
        return false;
#endif
        return true;
    }

    public static void KeyBoardPush(KBEnum bEnum)
    {
        KeyBoardIn(bEnum);
        KeyBoardOut(bEnum);
    }

    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void keybd_event(
       byte bVk,            //虚拟键值 对应按键的ascll码十进制值  
       byte bScan,          //0
       int dwFlags,         //0 为按下，1按住，2为释放 
       int dwExtraInfo      //0
   );


    public static void KeyBoardIn(KBEnum bEnum)
    {
        keybd_event(((byte)bEnum), 0, 0, 0);
    }


    public static void KeyBoardHold(KBEnum bEnum)
    {
        keybd_event(((byte)bEnum), 0, 1, 0);
    }

    public static void KeyBoardOut(KBEnum bEnum)
    {
        keybd_event(((byte)bEnum), 0, 2, 0);
    }

    // Public function to emulate a mouse button click (left button)
    public static void MouseClick()
    {
        PTestUtils.MouseClick();
    }

    // Public function to emulate a mouse drag event (left button)
    public static void MouseDrag()
    {
        PTestUtils.MouseDrag();
    }

    // Public function to emulate a mouse release event (left button)
    public static void MouseRelease()
    {
        PTestUtils.MouseRelease();
    }

    public static void MouseMove(int x, int y)
    {
        PTestUtils.MouseMove(x, y);
    }

    public static void MouseMove(Vector3 screenCoordinates)
    {
        PTestUtils.MouseMove(screenCoordinates);
    }


    public static void AddTag(string tag)
    {
        PTestManager.Instance.SendTag(tag);

    }
    public static void DownCSVToJson(string url, string contentName,Action<string> action)
    {
        PTestNodeManager.Instance.testNode.DownCSVToJson(url, contentName, action);
    }

    public static void DownCSV(string url, string contentName)
    {
        PTestNodeManager.Instance.testNode.DownCSV(url, contentName);
    }

    public static string FilePath(string contentName)
    {

        return PTestNodeManager.Instance.testNode.FilePath(contentName);
    }


    public static List<object> ReadCSV(string path, object t)
    {

        return PTestNodeManager.Instance.testNode.ReadCSV(path,t);
    }


    public static object SetCSVValue(object data, string[] infos)
    {

        return PTestNodeManager.Instance.testNode.SetCSVValue(data, infos);
    }

    public static string ReadCSVToJson(string path)
    {

        return PTestNodeManager.Instance.testNode.ReadCSVToJson(path);
    }

    public static string ReadCSVStringToJson(string content)
    {

        return PTestNodeManager.Instance.testNode.ReadCSVStringToJson(content);
    }

    public static string GetCaseParams(int index, string defaultValue)
    {


        return PTestNodeManager.Instance.testNode.GetCaseParams(index, defaultValue);
    }

    public static bool isRunning()
    {
        return PTestManager.Instance.Testing;
    }


    public static void CaseFinished()
    {
        PTestManager.Instance.CaseFinished();
    }

    public static string GetCSVDatas()
    {
        return PTestNodeManager.Instance.extend_config;
    }

    public static string GetCSVDataWithName(string name)
    {
        if (PTestNodeManager.Instance.extend_list.ContainsKey(name))
            return PTestNodeManager.Instance.extend_list[name];
        else
            return "";
    }


    public static string GetCSVDataJsonWithName(string name)
    {
        string csvData = GetCSVDataWithName(name);

        if(csvData == "")
        {
            return "";
        }
        else
        {
            return ReadCSVStringToJson(csvData);
        }
    }


    public static bool IsInEditor()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif
        return false;
    }

}
