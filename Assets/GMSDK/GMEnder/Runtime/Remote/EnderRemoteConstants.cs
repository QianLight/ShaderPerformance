#if UNITY_EDITOR
namespace Ender
{
    public class EnderRemoteConstants
    {
        public static class DeployErrorStatus
        {
            public const int UnavailableInstallPackage = -1;
            public const int OccupyDeviceFailed = -2;
            public const int InstallFailed = -3;
            public const int NoError = -4;
        }
        
        public static class DeployStatus
        {
            public const int Init = 0;
            public const int GetInstallPackage = 1;
            public const int OccupyDevice = 2;
            public const int InstallPackage = 3;
            public const int Done = 4;
        }
        
        public static class PlatformRequestConstants
        {
            public const int PlatformAndroid = 1;
            public const int PlatformIOS = 2;
        }
        
        public static class SelfState
        {
            public const int Ready = 1;
            public const int Retry = 2;
        }
        
        public enum EnderType
        {
            EnderLocal,
        
            EnderAnywhere
        }
        
        public enum EnderPlatform
        {
            Android
        }

    }
}
#endif