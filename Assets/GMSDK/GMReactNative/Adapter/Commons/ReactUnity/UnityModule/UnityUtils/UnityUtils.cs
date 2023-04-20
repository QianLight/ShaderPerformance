/*
 * @Author: hexiaonuo
 * @Date: 2021-11-04
 * @Description: Unity help file
 * @FilePath: ReactUnity/UnityModule/UnityUtils/InputField/ReactinputField.cs
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class UnityUtils
    {
        public static Color GetColor( long backgroundColor) {
            uint int32Color = (uint)backgroundColor;
            byte a = (byte)((int32Color & 0xff000000) >> 24);
            byte r = (byte)((int32Color & 0x00ff0000) >> 16);
            byte g = (byte)((int32Color & 0x0000ff00) >> 8);
            byte b = (byte)(int32Color & 0x000000ff);
            return new Color32(r, g, b, a);
        }

        public static Dictionary<string, object> FontStyleHelp(ref bool markDirtyFlag, Dictionary<string, object> fontProps,
            ref TextGenerationSettings settings)
        {
            Dictionary<string, object> fontRes = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> item in fontProps)
            {
                string key = item.Key;
                object val = item.Value;

                switch (key)
                {
                    case "fontFamily":
                    {
                        // game font load
                        Font gameFont = GameInteraction.GetGameFont((string) val);
                        if (gameFont != null)
                        {
                            Util.Log("load font from gameFontDic, {0}", (string) val);
                            settings.font = gameFont;
                        }
                        else
                        {
                            // ab font load
                            Font abFont = RNUMainCore.LoadFontAsset((string) val);
                            if (abFont != null)
                            {
                                Util.Log("load font from assetbundle, {0}", (string) val);
                                settings.font = abFont;
                            }
                        }

                        markDirtyFlag = true;
                        break;
                    }
                    case "fontSize":
                    {
                        settings.fontSize = (int) Math.Ceiling(Convert.ToDouble(val));
                        markDirtyFlag = true;
                        break;
                    }
                    case "fontStyle":
                    {
                        settings.fontStyle = TextHelper.GetFontStyleFromStr((string) val);
                        markDirtyFlag = true;
                        break;
                    }
                    case "fontWeight":
                    {
                        //TODO do nothing now
                        break;
                    }
                    case "lineHeight":
                    {
                        int fontSize = fontProps.ContainsKey("fontSize")
                            ? (int) Math.Ceiling(Convert.ToDouble(fontProps["fontSize"]))
                            : settings.fontSize;
                        settings.lineSpacing = Convert.ToSingle(Convert.ToDouble(val) / fontSize);
                        markDirtyFlag = true;
                        break;
                    }
                    case "color":
                    {
                        settings.color = GetColor(Convert.ToInt64(val));
                        break;
                    }
                    case "textAlign":
                    case "textAlignVertical":
                    {
                        // no-op, 下面会处理
                        break;
                    }
                    case "bestFit":
                    {
                        settings.resizeTextForBestFit = (bool) val;
                        markDirtyFlag = true;
                        break;
                    }
                    case "bestFitMaxSize":
                    {
                        settings.resizeTextMaxSize = (int) Math.Ceiling(Convert.ToDouble(val));
                        markDirtyFlag = true;
                        break;
                    }
                    case "bestFitMinSize":
                    {
                        settings.resizeTextMinSize = (int) Math.Ceiling(Convert.ToDouble(val));
                        markDirtyFlag = true;
                        break;
                    }
                    default:
                    {
                        fontRes.Add(key, val);
                        break;
                    }
                }
            }

            if (!fontProps.ContainsKey("textAlign") && !fontProps.ContainsKey("textAlignVertical"))
            {
                // do nothing;
            }
            else
            {
                TextAlignRaw oldRaw = TextHelper.GetTextAlignRaw(settings.textAnchor);

                string textAlign = fontProps.ContainsKey("textAlign") ? (string)fontProps["textAlign"] : oldRaw.textAlign;
                string textAlignVertical = fontProps.ContainsKey("textAlignVertical") ? (string)fontProps["textAlignVertical"] : oldRaw.textAlignVertical;

                TextAnchor newAnchor = TextHelper.GetTextAnchor(textAlign, textAlignVertical);
                settings.textAnchor = newAnchor;
            }

            fontRes.Add("textGenerationSettings", settings);

            return fontRes;
        }
    }
}
