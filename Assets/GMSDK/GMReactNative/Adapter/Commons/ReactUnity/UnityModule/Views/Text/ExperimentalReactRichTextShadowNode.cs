using System.Collections.Generic;

namespace GSDK.RNU
{
    public class ExperimentalReactRichTextShadowNode : ReactBaseTextShadowNode
    {
        private string richValue = "";
        
        public ExperimentalReactRichTextShadowNode(int tag) : base(tag)
        {
            var s = TextHelper.GetRNUDefaultGenerationSettings();
            settings.richText = true;
        }
        

        protected override string GetChildText()
        {
            return richValue;
        }
        
        public override void Insert(int index, ReactSimpleShadowNode child)
        {
            // no-op ExperimentalReactText一定是叶子节点
        }

        public override Dictionary<string, object> UpdateYogaNodePropsAndGetOtherProps(Dictionary<string, object> args)
        {
            if (args.ContainsKey("value"))
            {
                richValue = (string) args["value"];
                this.yogaNode.MarkDirty();
            }

            return base.UpdateYogaNodePropsAndGetOtherProps(args);
        }
    }

}
