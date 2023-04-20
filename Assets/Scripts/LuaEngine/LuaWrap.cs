using System.Security.Cryptography.X509Certificates;
using CFUtilPoolLib;
using System;
using UnityEngine;
using XLua;
using UnityEngine.CFUI;

public class LuaWrap : ILuaWrap
{
    public bool Deprecated { get; set; }

    private LuaFunction fixF, destroyF , preEnableF;
    private Action mEnterScene, mLeaveScene, mReconnect;

    // Don't Edit, define same in HotfixMgr.lua
    internal const string strFix = "Fix";
    internal const string strDestroy = "OnDestroy";
    internal const string strPreloaded = "PreEnable";

    public LuaWrap(LuaTable hotfixViewMgr, Action enterScene, Action leaveScene, Action reconnect)
    {
        this.mEnterScene = enterScene;
        this.mLeaveScene = leaveScene;
        this.mReconnect = reconnect;
        fixF = hotfixViewMgr.GetInPath<LuaFunction>(strFix);
        preEnableF = hotfixViewMgr.GetInPath<LuaFunction>(strPreloaded);
        destroyF = hotfixViewMgr.GetInPath<LuaFunction>(strDestroy);
        
    }

    public bool Open(uint id, XUIHeader go,XDisplayContext context)
    {
        return fixF != null? fixF.Call<bool>(LuaBehaviour.SEP, id, go,context):true;
    }

    public void PreEnable(uint id , XUIHeader go){
        preEnableF?.Call(LuaBehaviour.SEP,id,go);
    }

    public void Destroy(uint id, int hashCode)
    {
        destroyF?.Call(LuaBehaviour.SEP, id , hashCode);
    }
    public void OnLeaveScene()
    {
        mLeaveScene?.Invoke();
    }

    public void OnEnterScene()
    {
        mEnterScene?.Invoke();
    }

    public void OnReconnect()
    {
        mReconnect?.Invoke();
    }

}
