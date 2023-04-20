using System.Collections;

namespace GSDK.RNU
{
    public class ReactScrollViewContentManager: BaseViewManager
    {
        public static string viewName = "RNUScrollViewContent";
        
        
        public static string sContentSizeChange = "onContentSizeChange";

        public override string GetName()
        {
            return viewName;
        }

        protected override BaseView createViewInstance( )
        {
            return new ReactScrollViewContent(viewName);
        }

        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {sContentSizeChange, new Hashtable{{sRegistration, sContentSizeChange}}},
            };
        }
    }
}