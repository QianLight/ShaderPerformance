using UnityEngine;

public abstract class SDKComponent
{
    /// <summary>
    /// 开关
    /// </summary>
    public abstract bool on { get; }

    /// <summary>
    /// 版本号
    /// </summary>
    public abstract string version { get; }

    public abstract int type { get; }

    protected string userid, account;

    public Transform transform;

    /// <summary>
    /// 初始化函数
    /// </summary>
    public abstract void Init();

    public virtual void OnPause(bool pause) { }

    public virtual void OnQuit() { }


    public void Login(string account, string userid)
    {
        this.account = account;
        this.userid = userid;
        OnLogin();
    }

    protected virtual void OnLogin() { }

    public void Logout()
    {
        OnLogout();
    }

    protected virtual void OnLogout() { }

}