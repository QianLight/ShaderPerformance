using System.Collections.Generic;

namespace GSDK.RNU
{
    public class ReactTextManager : ReactBaseTextManager
    {

        public static string ReactTextName = "RCTText";

        public override string GetName()
        {
            return ReactTextName;
        }

        protected override BaseView createViewInstance()
        {
            return new ReactText(ReactTextName, false);
        }
        
        public override ReactSimpleShadowNode CreateShadowNodeInstance(int tag)
        {
            return new ReactTextShadowNode(tag);
        }
        
        public override void UpdateProperties(BaseView viewToUpdate, Dictionary<string, object> props)
        {
            ReactText textToUpdate = (ReactText)viewToUpdate;
            if (props.ContainsKey(ReactTextShadowNode.sExtraUpdateFlag))
            {
                string text = (string)props[ReactTextShadowNode.sExtraUpdateFlag];
                textToUpdate.SetText(text);
                return;
            }
            
            base.UpdateProperties(viewToUpdate, props);
        }
        
    }
}