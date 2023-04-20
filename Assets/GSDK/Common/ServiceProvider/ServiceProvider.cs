using System;

namespace GSDK
{
    public class ServiceProvider
    {
        private ServiceProvider()
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
            _serviceProvider = new MobileServiceProvider();
            //_serviceProvider = new StandAloneServiceProvider();
#else
            throw new Exception("GSDK Unsupport current OS system");
#endif
        }

        public static readonly ServiceProvider Instance = new ServiceProvider();

        readonly IServiceProvider _serviceProvider;

        public IService GetService(ServiceType service, string moduleName = "")
        {
            if (_serviceProvider != null)
            {
                return _serviceProvider.GetService(service, moduleName);
            }

            return null;
        }
    }
}