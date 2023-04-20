using System;
using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;

/// <summary>
/// GSDK接口——九尾接口
/// </summary>
public partial class GSDKSystem
{
    private Action<CFUtilPoolLib.GSDK.updateGameConfigRet> m_initRNCallback;
    private Action<CFUtilPoolLib.GSDK.openFaceVerifyRet> m_openFaceVerifyCallback;
    private Action<CFUtilPoolLib.GSDK.openFaceVerifyRet> m_openActivityVerifyCallback;
    private Action<CFUtilPoolLib.GSDK.openPageRet> m_openPageRetCallback;
    private Action<CFUtilPoolLib.GSDK.PageCloseResult> m_closeCallBack;
    private Action<CFUtilPoolLib.GSDK.queryActivityNotifyDataRet> m_queryActivityNotifyDataRetCallback;
    private Action<CFUtilPoolLib.GSDK.GetRNDebugRet> m_getRNDebugCallback;

    #region 九尾接口
    /// <summary>
    /// RN注册消息监听
    /// </summary>
    /// <param name="callback"></param>
    public void ListenNativeNotification(Action<string> callback)
    {
        XDebug.singleton.AddGreenLog("ListenNativeNotification");
        GMReactNativeMgr.instance.SDK.listenNativeNotification(callback);
    }

    /// <summary>
    /// RN初始化角色信息
    /// </summary>
    /// <param name="roleID"></param>
    /// <param name="roleName"></param>
    /// <param name="serverID"></param>
    /// <param name="callback"></param>
    public void UpdateGameConfig(string roleID, string roleName, string serverID, Action<CFUtilPoolLib.GSDK.updateGameConfigRet> callback)
    {
        XDebug.singleton.AddGreenLog("UpdateGameConfig roleID=" + roleID + ", roleName=" + roleName + ", serverID=" + serverID);
        m_initRNCallback = callback;
        GMReactNativeMgr.instance.SDK.updateGameConfig(roleID, roleName, serverID, ReactNativeInitCallback);
    }

    public void ReactNativeInitCallback(GMSDK.updateGameConfigRet result)
    {
        XDebug.singleton.AddGreenLog("ReactNativeInitCallback result=" + result.ToString());
        if (m_initRNCallback != null)
        {
            CFUtilPoolLib.GSDK.updateGameConfigRet gResult = GetUpdateGameConfigRet(result);
            m_initRNCallback(gResult);
        }
    }

    /// <summary>
    /// 九尾游戏可设置九尾活动面板挂载的父节点信息
    /// </summary>
    /// <param name="gameObject"></param>
    public void SetGameGoParent(GameObject gameObject)
    {
        GMReactNativeMgr.instance.SDK.SetGameGoParent(gameObject);
    }

    /// <summary>
    /// 九尾请求拍脸
    /// </summary>
    public void OpenFaceVerify(string type, Action<CFUtilPoolLib.GSDK.openFaceVerifyRet> callback)
    {
        XDebug.singleton.AddGreenLog("resOpenFaceVerify");
        m_openFaceVerifyCallback = callback;
        GMReactNativeMgr.instance.SDK.openFaceVerify(type, OpenFaceVerifyCallback);
    }

    public void OpenFaceVerifyCallback(GMSDK.openFaceVerifyRet result)
    {
        if (m_openFaceVerifyCallback != null)
        {
            XDebug.singleton.AddGreenLog("resOpenFaceVerifyCallback");
            CFUtilPoolLib.GSDK.openFaceVerifyRet gResult = GetOpenFaceVerifyRet(result);
            for (int i = 0; i < gResult.list.Count; ++i)
            {
                XDebug.singleton.AddGreenLog("OpenFaceVerifyCallback&" + gResult.list[i].activityId + "&" + gResult.list[i].activityUrl);
            }
            m_openFaceVerifyCallback(gResult);
        }
    }

    /// <summary>
    /// 九尾请求活动
    /// </summary>
    public void OpenActvityVerify(string type, Action<CFUtilPoolLib.GSDK.openFaceVerifyRet> callback)
    {
        XDebug.singleton.AddGreenLog("resOpenActvityVerify type=" + type);
        m_openActivityVerifyCallback = callback;
        GMReactNativeMgr.instance.SDK.openFaceVerify(type, OpenActivityVerifyCallback);
    }

    public void OpenActivityVerifyCallback(GMSDK.openFaceVerifyRet result)
    {
        if (m_openActivityVerifyCallback != null)
        {
            XDebug.singleton.AddGreenLog("resOpenActivityVerifyCallback");
            CFUtilPoolLib.GSDK.openFaceVerifyRet gResult = GetOpenFaceVerifyRet(result);
            for (int i = 0; i < gResult.list.Count; ++i)
            {
                XDebug.singleton.AddGreenLog("OpenActivityVerifyCallback&" + gResult.list[i].activityId + "&" + gResult.list[i].activityUrl);
            }
            m_openActivityVerifyCallback(gResult);
        }
    }

    /// <summary>
    /// 九尾打开活动界面
    /// </summary>
    /// <param name="url"></param>
    /// <param name="inGameId"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    public void OpenPage(string url, string inGameId, Dictionary<string, object> parameters, Action<CFUtilPoolLib.GSDK.openPageRet> callback)
    {
        m_openPageRetCallback = callback;
        GMReactNativeMgr.instance.SDK.openPage(url, inGameId, parameters, OpenPageCallback);
    }

    /// <summary>
    /// 九尾打开活动界面
    /// </summary>
    /// <param name="url"></param>
    /// <param name="inGameId"></param>
    /// <param name="parameters"></param>
    /// <param name="callback"></param>
    public void OpenPage(string url, GameObject parentGo, string inGameId, Dictionary<string, object> parameters, Action<CFUtilPoolLib.GSDK.openPageRet> callback, Action<CFUtilPoolLib.GSDK.PageCloseResult> closeCallBack)
    {
        m_openPageRetCallback = callback;
        m_closeCallBack = closeCallBack;
        GMReactNativeMgr.instance.SDK.openPage(url, parentGo, inGameId, parameters, OpenPageCallback, ClosePageCallback);
    }

    public void OpenPageCallback(GMSDK.openPageRet result)
    {
        if (m_openPageRetCallback != null)
        {
            CFUtilPoolLib.GSDK.openPageRet gResult = GetOpenPageRet(result);
            m_openPageRetCallback(gResult);
        }
    }

    public void ClosePageCallback(GMSDK.PageCloseResult result)
    {
        if (m_closeCallBack != null)
        {
            CFUtilPoolLib.GSDK.PageCloseResult gResult = GetClosePageRet(result);
            m_closeCallBack(gResult);
        }
    }

    /// <summary>
    /// 九尾获取活动红点信息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void QueryActivityNotifyDataByType(string type, Action<CFUtilPoolLib.GSDK.queryActivityNotifyDataRet> callback)
    {
        XDebug.singleton.AddGreenLog("QueryActivityNotifyDataByType");
        m_queryActivityNotifyDataRetCallback = callback;
        GMReactNativeMgr.instance.SDK.queryActivityNotifyDataByType(type, QueryActivityNotifyDataByTypeCallback);
    }

    private void QueryActivityNotifyDataByTypeCallback(GMSDK.queryActivityNotifyDataRet result)
    {
        XDebug.singleton.AddGreenLog("QueryActivityNotifyDataByTypeCallback");
        if (m_queryActivityNotifyDataRetCallback != null)
        {
            CFUtilPoolLib.GSDK.queryActivityNotifyDataRet gResult = GetQueryActivityNotifyDataRet(result);
            m_queryActivityNotifyDataRetCallback(gResult);
        }
    }

    /// <summary>
    /// 关闭所有打开的九尾页面
    /// </summary>
    public void CloseAllPages()
    {
        XDebug.singleton.AddGreenLog("resCloseAllPages");
        GMReactNativeMgr.instance.SDK.closeAllPages();
    }

    /// <summary>
    /// 游戏内发送消息给九尾界面
    /// </summary>
    public void SendMessageToGumiho(string message)
    {
        XDebug.singleton.AddGreenLog("resSendMessageToGumiho:" + message);
        GMReactNativeMgr.instance.SDK.sendMessageToGumiho(message);
    }

    /// <summary>
    /// 九尾获取调试模式
    /// </summary>
    /// <param name="callback"></param>
    public void GetRNDebug(Action<CFUtilPoolLib.GSDK.GetRNDebugRet> callback)
    {
#if UNITY_ANDROID
        m_getRNDebugCallback = callback;
        GMReactNativeMgr.instance.SDK.getRNDebug(GetRNDebugCallback);
#endif
    }

    public void GetRNDebugCallback(GMSDK.GetRNDebugRet result)
    {
#if UNITY_ANDROID
        if (m_getRNDebugCallback != null)
        {
            CFUtilPoolLib.GSDK.GetRNDebugRet gResult = GetRNDebugRet(result);
            m_getRNDebugCallback(gResult);
        }
#endif
    }

    public void SetRNDebug(bool isEnabled)
    {
#if UNITY_ANDROID
        GMReactNativeMgr.instance.SDK.setRNDebug(isEnabled);
#endif
    }

    public void SetRuGameAdvancedInjection()
    {
        XDebug.singleton.AddGreenLog("SetRuGameAdvancedInjection");
        GSDK.RNU.RNUMain.SetRuGameAdvancedInjection(GSDKIRuGameAdvancedInjection.Instance);
    }
    #endregion
}
