/*
 * @author yankang.nj
 * yoga 处理工具类，负责处理JS测传递过来的yoga属性
 */

using System;
using System.Collections.Generic;
using Facebook.Yoga;
using UnityEngine;

namespace GSDK.RNU
{
    public class YogaHelper
    {
        private static Dictionary<string, YogaAlign> ALIGN = new Dictionary<string, YogaAlign>()
        {
            {"auto", YogaAlign.Auto},
            {"flex-end", YogaAlign.FlexEnd},
            {"flex-start", YogaAlign.FlexStart},
            {"center", YogaAlign.Center},
            {"space-between", YogaAlign.SpaceBetween},
            {"space-around", YogaAlign.SpaceAround},
            {"stretch", YogaAlign.Stretch},
            {"baseline", YogaAlign.Baseline}
        };

        private static Dictionary<string, YogaOverflow> OVERFLOW = new Dictionary<string, YogaOverflow>()
        {
            {"hidden", YogaOverflow.Hidden},
            {"scroll", YogaOverflow.Scroll},
            {"visible", YogaOverflow.Visible}
        };

        private static Dictionary<string, YogaJustify> JUSTIFY_CONTENT = new Dictionary<string, YogaJustify>()
        {
            {"center", YogaJustify.Center},
            {"flex-end", YogaJustify.FlexEnd},
            {"flex-start", YogaJustify.FlexStart},
            {"space-around", YogaJustify.SpaceAround},
            {"space-between", YogaJustify.SpaceBetween},
            {"space-evenly", YogaJustify.SpaceEvenly}
        };

        private static Dictionary<string, YogaPositionType> POSITION = new Dictionary<string, YogaPositionType>()
        {
            {"static", YogaPositionType.Static},
            {"absolute", YogaPositionType.Absolute},
            {"relative", YogaPositionType.Relative},
        };

        private static Dictionary<string, YogaDisplay> DISPLAY = new Dictionary<string, YogaDisplay>()
        {
            {"none", YogaDisplay.None},
            {"flex", YogaDisplay.Flex}
        };

        private static Dictionary<string, YogaFlexDirection> FLEX_DIRECTION =
            new Dictionary<string, YogaFlexDirection>()
            {
                {"column", YogaFlexDirection.Column},
                {"row", YogaFlexDirection.Row},
                {"column-reverse", YogaFlexDirection.ColumnReverse},
                {"row-reverse", YogaFlexDirection.RowReverse},
            };

        private static Dictionary<string, YogaWrap> WRAP = new Dictionary<string, YogaWrap>()
        {
            {"nowrap", YogaWrap.NoWrap},
            {"wrap-reverse", YogaWrap.WrapReverse},
            {"wrap", YogaWrap.Wrap},
        };

        private static Dictionary<string, YogaDirection> DIRECTION = new Dictionary<string, YogaDirection>()
        {
            {"inherit", YogaDirection.Inherit},
            {"ltr", YogaDirection.LTR},
            {"rtl", YogaDirection.RTL}
        };


        public static YogaConfig config = new YogaConfig();

        // 根据JS测的值，获取YogaValue
        private static YogaValue GetYogaValue(object val)
        {
            if (val == null)
            {
                return YogaValue.Undefined();
            }
            
            string typeName = val.GetType().Name;

            if (typeName == "String")
            {
                string sv = (string) val;
                if (sv == "auto")
                {
                    return YogaValue.Auto();
                }
                else if (sv.EndsWith("%"))
                {
                    float f2;
                    if (!float.TryParse(sv.Substring(0, sv.Length - 1), out f2))
                    {
                        Util.Log("yoga value not valid format {0}", val);
                        return YogaValue.Undefined();
                    }

                    return f2.Percent();
                }
                else
                {
                    Util.Log("yoga value not valid format {0}", val);
                    return YogaValue.Undefined();
                }
            }
            else
            {
                return Convert.ToSingle(val);
            }
        }

        private static float GetYogaFloat(object val)
        {
            if (val == null)
            {
                return 0f;
            }

            return Convert.ToSingle(val);
        }
        
        // 预热yoga代码。 其中文本布局的预热尤其重要，TextGenerator在第一次计算文本宽度的调用中耗时较多，
        // 其后的调用 这代价很低
        public static void PreHotCode()
        {
            YogaNode root = new YogaNode();
            root.Width = 100;
            root.Height = 100;
            
            Dictionary<string, object> preMap = new Dictionary<string, object>() 
            {
                {"alignContent", "center"},
                {"alignItems", "center"},
                {"alignSelf", "center"},
                {"borderBottomWidth", 1},
                {"borderEndWidth", 1},
                {"borderLeftWidth", 1},
                {"borderRightWidth", 1},
                {"borderStartWidth", 1},
                {"borderTopWidth", 1},
                {"borderWidth", 1},
                {"bottom", 1},
                {"display", "flex"},
                {"end", 1},
                {"flex", 1},
                {"flexBasis", 100},
                {"flexDirection", "column"},
                {"flexGrow", 1},
                {"flexShrink", 0},
                {"flexWrap", "nowrap"},
                {"height", 100},
                {"justifyContent", "center"},
                {"left", 1},
                {"margin", 1},
                {"marginBottom", 1},
                {"marginEnd", 1},
                {"marginHorizontal", 1},
                {"marginLeft", 1},
                {"marginRight", 1},
                {"marginStart", 1},
                {"marginTop", 1},
                {"marginVertical", 1},
                {"maxHeight", 100},
                {"maxWidth", 100},
                {"minHeight", 100},
                {"minWidth", 100},
                {"overflow", "visible"},
                {"padding", 1},
                {"paddingBottom", 1},
                {"paddingEnd", 1},
                {"paddingHorizontal", 1},
                {"paddingLeft", 1},
                {"paddingRight", 1},
                {"paddingStart", 1},
                {"paddingTop", 1},
                {"paddingVertical", 1},
                {"position", "relative"},
                {"right", 1},
                {"start", 1},
                {"top", 1},
                {"width", 100},
            };

            YogaNode child = new YogaNode();
            UpdateYogaNodeAndGetOtherProps(preMap, child, new Dictionary<string, object>());
            root.AddChild(child);

            YogaNode childText = new YogaNode();
            childText.SetMeasureFunction((YogaNode node, float width, YogaMeasureMode widthMode, float height, YogaMeasureMode heightMode) =>
            {
                TextGenerator commonTextGenerator = new TextGenerator();
                TextGenerationSettings settings = TextHelper.GetRNUDefaultGenerationSettings();
                string text = "PRETEXT";
                settings.generationExtents = new Vector2(width, height);
                
                float preferredWidth = commonTextGenerator.GetPreferredWidth(text, settings);
                float preferredHeight = commonTextGenerator.GetPreferredHeight(text, settings);
                
                return MeasureOutput.Make(preferredWidth, preferredHeight);
            });
            root.AddChild(childText);
            
            root.CalculateLayout();
            
            //
            root.Clear();
            child.Destory();
            childText.Destory();
            root.Destory();
        }
        public static void UpdateYogaNodeAndGetOtherProps(Dictionary<string, object> props, YogaNode yogaNode,
            Dictionary<string, object> finalProps)
        {
            if (props == null)
            {
                return;
            }

            foreach (var item in props)
            {
                var key = item.Key;
                var val = item.Value;


                switch (key)
                {
                    case "alignContent":
                    {
                        if (val == null)
                        {
                            //TODO AlignContent是否有默认值
                            yogaNode.AlignContent = YogaAlign.Stretch;
                            break;
                        }
                        
                        yogaNode.AlignContent = ALIGN[(string) val];
                        break;
                    }
                    case "alignItems":
                    {
                        if (val == null)
                        {
                            yogaNode.AlignItems = YogaAlign.Stretch;
                            break;
                        }
                        
                        yogaNode.AlignItems = ALIGN[(string) val];
                        break;
                    }
                    case "alignSelf":
                    {
                        if (val == null)
                        {
                            yogaNode.AlignSelf = YogaAlign.Auto;
                            break;
                        }

                        
                        yogaNode.AlignSelf = ALIGN[(string) val];
                        break;
                    }
                    case "aspectRatio":
                    {
                        yogaNode.AspectRatio = GetYogaFloat(val);
                        break;
                    }
                    case "borderBottomWidth":
                    {
                        yogaNode.BorderBottomWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "borderEndWidth":
                    {
                        yogaNode.BorderEndWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "borderLeftWidth":
                    {
                        yogaNode.BorderLeftWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "borderRightWidth":
                    {
                        yogaNode.BorderRightWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "borderStartWidth":
                    {
                        yogaNode.BorderStartWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "borderTopWidth":
                    {
                        yogaNode.BorderTopWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "borderWidth":
                    {
                        yogaNode.BorderWidth = GetYogaFloat(val);
                        finalProps.Add(key, val);
                        break;
                    }
                    case "bottom":
                    {
                        yogaNode.Bottom = GetYogaValue(val);
                        break;
                    }
                    case "direction":
                    {
                        //TODO
                        //yogaNode.LayoutDirection = DIRECTION[(string)val];
                        break;
                    }
                    case "display":
                    {
                        if (val == null)
                        {
                            yogaNode.Display = YogaDisplay.Flex;
                            break;
                        }
                        
                        yogaNode.Display = DISPLAY[(string) val];
                        break;
                    }
                    case "end":
                    {
                        yogaNode.End = GetYogaValue(val);
                        break;
                    }
                    case "flex":
                    {
                        yogaNode.Flex = GetYogaFloat(val);
                        break;
                    }
                    case "flexBasis":
                    {
                        yogaNode.FlexBasis = GetYogaValue(val);
                        break;
                    }
                    case "flexDirection":
                    {
                        if (val == null)
                        {
                            yogaNode.FlexDirection = YogaFlexDirection.Column;
                            break;
                        }
                        
                        yogaNode.FlexDirection = FLEX_DIRECTION[(string) val];
                        break;
                    }
                    case "flexGrow":
                    {
                        yogaNode.FlexGrow = GetYogaFloat(val);
                        break;
                    }
                    case "flexShrink":
                    {
                        yogaNode.FlexShrink = GetYogaFloat(val);
                        break;
                    }
                    case "flexWrap":
                    {
                        if (val == null)
                        {
                            yogaNode.Wrap = YogaWrap.NoWrap;
                            break;
                        }
                        
                        yogaNode.Wrap = WRAP[(string) val];
                        break;
                    }
                    case "height":
                    {
                        yogaNode.Height = GetYogaValue(val);
                        break;
                    }
                    case "justifyContent":
                    {
                        if (val == null)
                        {
                            yogaNode.JustifyContent = YogaJustify.FlexStart;
                            break;
                        }
                        
                        yogaNode.JustifyContent = JUSTIFY_CONTENT[(string) val];
                        break;
                    }
                    case "left":
                    {
                        yogaNode.Left = GetYogaValue(val);
                        break;
                    }
                    case "margin":
                    {
                        yogaNode.Margin = GetYogaValue(val);
                        break;
                    }
                    case "marginBottom":
                    {
                        yogaNode.MarginBottom = GetYogaValue(val);
                        break;
                    }
                    case "marginEnd":
                    {
                        yogaNode.MarginEnd = GetYogaValue(val);
                        break;
                    }
                    case "marginHorizontal":
                    {
                        yogaNode.MarginHorizontal = GetYogaValue(val);
                        break;
                    }
                    case "marginLeft":
                    {
                        yogaNode.MarginLeft = GetYogaValue(val);
                        break;
                    }
                    case "marginRight":
                    {
                        yogaNode.MarginRight = GetYogaValue(val);
                        break;
                    }
                    case "marginStart":
                    {
                        yogaNode.MarginStart = GetYogaValue(val);
                        break;
                    }
                    case "marginTop":
                    {
                        yogaNode.MarginTop = GetYogaValue(val);
                        break;
                    }
                    case "marginVertical":
                    {
                        yogaNode.MarginVertical = GetYogaValue(val);
                        break;
                    }
                    case "maxHeight":
                    {
                        yogaNode.MaxHeight = GetYogaValue(val);
                        break;
                    }
                    case "maxWidth":
                    {
                        yogaNode.MaxWidth = GetYogaValue(val);
                        break;
                    }
                    case "minHeight":
                    {
                        yogaNode.MinHeight = GetYogaValue(val);
                        break;
                    }
                    case "minWidth":
                    {
                        yogaNode.MinWidth = GetYogaValue(val);
                        break;
                    }
                    case "overflow":
                    {
                        if (val == null)
                        {
                            yogaNode.Overflow = YogaOverflow.Visible;
                            break;
                        }

                        yogaNode.Overflow = OVERFLOW[(string) val];
                        finalProps.Add(key, val);
                        break;
                    }
                    case "padding":
                    {
                        yogaNode.Padding = GetYogaValue(val);
                        break;
                    }
                    case "paddingBottom":
                    {
                        yogaNode.PaddingBottom = GetYogaValue(val);
                        break;
                    }
                    case "paddingEnd":
                    {
                        yogaNode.PaddingEnd = GetYogaValue(val);
                        break;
                    }
                    case "paddingHorizontal":
                    {
                        yogaNode.PaddingHorizontal = GetYogaValue(val);
                        break;
                    }
                    case "paddingLeft":
                    {
                        yogaNode.PaddingLeft = GetYogaValue(val);
                        break;
                    }
                    case "paddingRight":
                    {
                        yogaNode.PaddingRight = GetYogaValue(val);
                        break;
                    }
                    case "paddingStart":
                    {
                        yogaNode.PaddingStart = GetYogaValue(val);
                        break;
                    }
                    case "paddingTop":
                    {
                        yogaNode.PaddingTop = GetYogaValue(val);
                        break;
                    }
                    case "paddingVertical":
                    {
                        yogaNode.PaddingVertical = GetYogaValue(val);
                        break;
                    }
                    case "position":
                    {
                        if (val == null)
                        {
                            yogaNode.PositionType = YogaPositionType.Relative;
                            break;
                        }
                        
                        yogaNode.PositionType = POSITION[(string) val];
                        break;
                    }
                    case "right":
                    {
                        yogaNode.Right = GetYogaValue(val);
                        break;
                    }
                    case "start":
                    {
                        yogaNode.Start = GetYogaValue(val);
                        break;
                    }
                    case "top":
                    {
                        yogaNode.Top = GetYogaValue(val);
                        break;
                    }
                    case "width":
                    {
                        yogaNode.Width = GetYogaValue(val);
                        break;
                    }
                    default:
                    {
                        finalProps.Add(key, val);
                        break;
                    }
                }
            }
        }
    }
}