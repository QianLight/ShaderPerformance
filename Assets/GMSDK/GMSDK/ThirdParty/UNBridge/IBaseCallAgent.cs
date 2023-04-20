#if UNITY_EDITOR || UNITY_ANDROID
namespace UNBridgeLib
{
    public interface IBaseCallAgent
    {
        void Call(string methodName, params object[] args);

        ReturnType Call<ReturnType>(string methodName, params object[] args);
    }
}
#endif
