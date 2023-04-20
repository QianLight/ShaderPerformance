using System;
#if UNITY_EDITOR
namespace Ender
{
    public interface IEnderBaseCallBack
    {
        string GetCallBackClassName();
    }
}
#endif