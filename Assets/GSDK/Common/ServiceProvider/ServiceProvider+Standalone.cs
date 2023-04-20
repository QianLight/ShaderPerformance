using System;

namespace GSDK
{
#if UNITY_STANDALONE || UNITY_EDITOR
    internal class StandAloneServiceProvider : IServiceProvider
    {
        public IService GetService(ServiceType service, string moduleName = "")
        {
            switch (service)
            {
                case ServiceType.Account:
                {
                    return null;
                }
                default:
                    return null;
            }

            //return null;
        }
    }
#endif
}