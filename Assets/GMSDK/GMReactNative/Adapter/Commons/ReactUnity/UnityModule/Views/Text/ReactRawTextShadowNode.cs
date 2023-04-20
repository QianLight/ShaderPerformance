using System.Collections.Generic;

namespace GSDK.RNU
{

    public class ReactRawTextShadowNode : ReactSimpleShadowNode
    {

        public string text;

        public ReactRawTextShadowNode(int tag) : base(tag)
        {
        }


        public override Dictionary<string, object> UpdateYogaNodePropsAndGetOtherProps(Dictionary<string, object> args)
        {
            text = (string)args["text"];

            if (this.parent != null)
            {
                ExtraUpdaterShadowNodeQueue.Add(parent);
            }

            return null;
        }

        public override bool isVirtual()
        {
            return true;
        }
    }

}
