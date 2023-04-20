using System.Collections;

namespace GSDK.RNU
{
    public class DevSettingsModule : SimpleUnityModule
    {
        
        private static string NAME = "DevSettings";
        private readonly RNUMainCore rnuContext;

        public DevSettingsModule(RNUMainCore rnuContext)
        {
            this.rnuContext = rnuContext;
        }
        public override string GetName()
        {
            return NAME;
        }
        
        [ReactMethod]
        public void reload()
        {
            StaticCommonScript.StaticStartCoroutine(InnerReload());
        }
        
        private IEnumerator InnerReload()
        {
            yield return null;
            rnuContext.debugPanel.ReloadHandle();
        }
        
    }
}