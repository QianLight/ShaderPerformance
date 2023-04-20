using System.Collections;

namespace GSDK.RNU
{
    public class SourceCodeModule : SimpleUnityModule
    {
        private static string NAME = "SourceCode";
        private readonly RNUMainCore rnuContext;

        public SourceCodeModule(RNUMainCore context)
        {
            rnuContext = context;
        }
        
        public override string GetName()
        {
            return NAME;
        }
        
        public override Hashtable GetConstants()
        {
            Hashtable constants = new Hashtable()
            {
                {"scriptURL", "http://"+ rnuContext.GetDebugIp() +":" + rnuContext.GetDebugPort() +  "/index.bundle?platform=android&dev=true&minify=false&app=com.rnlearn2&modulesOnly=false&runModule=true"}
            };
            return constants;
        }
        
    }
}