#if UNITY_EDITOR || UNITY_ANDROID
namespace UNBridgeLib
{
    public interface IBaseStaticCallAgent
    {

        ReturnType CallStatic<ReturnType>(string methodName, params object[] args);

        void CallStatic(string methodName, params object[] args);
    }
}
#endif
