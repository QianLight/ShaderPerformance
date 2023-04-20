using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{
    public class Help
    {

        public static Hashtable GetInjectObj(List<ModuleHolder> moduleHolders)
        {
            ArrayList r = new ArrayList();

            foreach (ModuleHolder mh in moduleHolders)
            {
                r.Add(new ArrayList
                    {
                        mh.GetName(),
                        mh.GetConstants(),
                        mh.GetMethods(),
                        mh.GetPromiseMethods(),
                        mh.GetSyncMethods(),
                    }
                );
            }

            Hashtable remoteModuleConfig = new Hashtable{
                {"remoteModuleConfig", r}
            };
            return remoteModuleConfig;
        }

    }
}