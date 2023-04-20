namespace GSDK.RNU
{
    public class ExperimentalReactRichTextManager : ReactBaseTextManager
    {
        private const string ReactRichTextName = "RNUExperimentalRichText";

        public override string GetName()
        {
            return ReactRichTextName;
        }

        protected override BaseView createViewInstance()
        {
            return new ReactText(ReactRichTextName, true);
        }
        
        [ReactProp(name = "value")]
        public void SetValue(ReactText view, string value)
        {
            view.SetText(value);
        }

        public override ReactSimpleShadowNode CreateShadowNodeInstance(int tag)
        {
            return new ExperimentalReactRichTextShadowNode(tag);
        }
        
    }
}