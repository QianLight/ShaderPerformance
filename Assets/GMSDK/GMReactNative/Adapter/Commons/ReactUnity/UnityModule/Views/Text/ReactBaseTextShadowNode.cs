using Facebook.Yoga;
using System.Collections.Generic;
using UnityEngine;



namespace GSDK.RNU
{
    public abstract class ReactBaseTextShadowNode : ReactSimpleShadowNode
    {
        private static TextGenerator commonTextGenerator = new TextGenerator();
        protected TextGenerationSettings settings = TextHelper.GetRNUDefaultGenerationSettings();
        

        protected ReactBaseTextShadowNode(int tag) : base(tag)
        {
            this.yogaNode.SetMeasureFunction(TextMeasureFunction);
        }

        protected abstract string GetChildText();

        private YogaSize TextMeasureFunction(YogaNode node, float width, YogaMeasureMode widthMode, float height, YogaMeasureMode heightMode)
        {
            var text = GetChildText();
            settings.generationExtents = new Vector2(width, height);
            
            var pixelsPerUnit = 1f;
            if (!(bool) (UnityEngine.Object) settings.font || settings.font.dynamic)
            {
                pixelsPerUnit = RNUMainCore.GetCanvasScaleFactor();
            }
            else
            {
                pixelsPerUnit = settings.fontSize <= 0 || settings.font.fontSize <= 0
                    ? 1f
                    : (float) settings.font.fontSize / (float) settings.fontSize;
            }
            settings.scaleFactor = pixelsPerUnit;
            
            
            var finalWidth = 0f;
            var finalHeight = 0f;

            if (widthMode == YogaMeasureMode.Exactly)
            {
                finalWidth = width;
            } else if (widthMode == YogaMeasureMode.AtMost)
            {
                var preferredWidth = commonTextGenerator.GetPreferredWidth(text, settings) / pixelsPerUnit;
                finalWidth = preferredWidth <= width ? preferredWidth : width;
            }
            else
            {
                finalWidth = commonTextGenerator.GetPreferredWidth(text, settings) / pixelsPerUnit;
            }

            if (heightMode == YogaMeasureMode.Exactly)
            {
                finalHeight = height;
            } else if (heightMode == YogaMeasureMode.AtMost)
            {
                var preferredHeight = commonTextGenerator.GetPreferredHeight(text, settings) / pixelsPerUnit;
                finalHeight = preferredHeight <= height ? preferredHeight : height;
            }
            else
            {
                finalHeight =  commonTextGenerator.GetPreferredHeight(text, settings) / pixelsPerUnit;
            }
            
            return MeasureOutput.Make(finalWidth, finalHeight);
        }
        
        public override Dictionary<string, object> UpdateYogaNodePropsAndGetOtherProps(Dictionary<string, object> args)
        {
            Dictionary<string, object> r = base.UpdateYogaNodePropsAndGetOtherProps(args);

            bool markDirtyFlag = false;
            Dictionary<string, object> rr = UnityUtils.FontStyleHelp(ref markDirtyFlag, r, ref settings);
            if (markDirtyFlag)
            {
                yogaNode.MarkDirty();
            }

            return rr;
        }
    }

}
