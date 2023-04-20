using System;
using System.Collections.Generic;
using GSDK;
using UNBridgeLib.LitJson;
using IServiceProvider = GSDK.IServiceProvider;

namespace GSDK
{
#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
    internal class MobileServiceProvider : IServiceProvider
    {
        private readonly object _thisLock = new object();

        //todo after upgrade to .net4.0, we can use ConcurrentDictionary to optimize
        private readonly Dictionary<string, IService> _serviceProviderMap = new Dictionary<string, IService>();

        private IService GetService(string serviceName, string moduleName = "")
        {
            lock (_thisLock)
            {
                var interfaceName = serviceName + moduleName;
                if (!_serviceProviderMap.ContainsKey(interfaceName))
                {
                    String serviceClassName = "GSDK." + interfaceName + "Service";
                    Type t = Type.GetType(serviceClassName);
                    if (t == null) return null;
                    IService service = (IService) Activator.CreateInstance(t);
                    _serviceProviderMap.Add(interfaceName, service);
                    var eventData = new JsonData();
                    eventData["service_name"] = interfaceName;
                    if (!interfaceName.Equals("Agreement") && !interfaceName.Equals("Privacy"))
                    {
                        //当初始化AgreementService时，不进行埋点上报，支持AgreementService的子线程使用
                        InnerTools.SdkMonitorEvent("gsdk_unity_service_init", eventData);
                    }
                }

                return _serviceProviderMap[interfaceName];
            }
        }

        public IService GetService(ServiceType service, string moduleName = "")
        {
            return GetService(service.ToString(), moduleName);
        }
    }
#endif
}