#if UNITY_EDITOR
namespace Ender
{
    public interface IEnderRemoteCallback
    {
        void HandleEnderRemoteMsgFromNative(string message);

        void HandleEnderRemoteAlohaMsgFromNative(string message);

        void HandleConnectionChange(int count);
    }
}
#endif