using System;
using System.Collections;

/*
 * @Author: hexiaonuo
 * @Date: 2021-11-01
 * @Description: input component
 * @FilePath: ReactUnity/UnityModule/Views/InputField/ReactInputFieldShadowNode.cs
 */
using Facebook.Yoga;
using System.Collections.Generic;
using UnityEngine;



namespace GSDK.RNU
{
    public class ReactInputFieldShadowNode : ReactSimpleShadowNode
    {
        private static TextGenerator commonTextGenerator = new TextGenerator();
        private TextGenerationSettings settings = TextHelper.GetRNUDefaultGenerationSettings();

        private static float defaultWidth = 200;
        public ReactInputFieldShadowNode(int tag) : base(tag)
        {
            yogaNode.SetMeasureFunction(TextMeasureFunction);
        }

        private YogaSize TextMeasureFunction(YogaNode node, float width, YogaMeasureMode widthMode, float height, YogaMeasureMode heightMode)
        {
            // 不同字符内容的高度计算结果值，默认字体大小时高度一致
            var text = "";
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
            }
            else
            {
                finalWidth = defaultWidth;
            }

            if (heightMode == YogaMeasureMode.Exactly)
            {
                finalHeight = height;
            } else if (heightMode == YogaMeasureMode.AtMost)
            {
                float preferredHeight = commonTextGenerator.GetPreferredHeight(text, settings) / pixelsPerUnit;
                finalHeight = preferredHeight >= height ? height : preferredHeight;
            }
            else
            {
                finalHeight = commonTextGenerator.GetPreferredHeight(text, settings) / pixelsPerUnit;
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
