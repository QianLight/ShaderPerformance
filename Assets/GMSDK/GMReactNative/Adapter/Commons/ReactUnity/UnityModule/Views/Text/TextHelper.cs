using System.Collections.Generic;
using UnityEngine;


namespace GSDK.RNU
{
    public struct TextAlignRaw
    {
        public string textAlign;
        public string textAlignVertical;

        public TextAlignRaw(string tav, string ta)
        {
            textAlignVertical = tav;
            textAlign = ta;
        }
    }

    public class TextHelper
    {

        public static int DEFAULT_FONT_SIZE = 14;
        private static Dictionary<TextAnchor, TextAlignRaw> textAlignMap = new Dictionary<TextAnchor, TextAlignRaw>
        {
            {
                TextAnchor.UpperLeft, new TextAlignRaw("top", "left")
            },
            {
                TextAnchor.UpperCenter, new TextAlignRaw("top", "center")
            },
            {
                TextAnchor.UpperRight, new TextAlignRaw("top", "right")
            },


            {
                TextAnchor.MiddleLeft, new TextAlignRaw("center", "left")
            },
            {
                TextAnchor.MiddleCenter, new TextAlignRaw("center", "center")
            },
            {
                TextAnchor.MiddleRight, new TextAlignRaw("center", "right")
            },


            {
                TextAnchor.LowerLeft, new TextAlignRaw("bottom", "left")
            },
            {
                TextAnchor.LowerCenter, new TextAlignRaw("bottom", "center")
            },
            {
                TextAnchor.LowerRight, new TextAlignRaw("bottom", "right")
            },
        };

        private static Dictionary<string, Dictionary<string, TextAnchor>> textAlignRawMap = new Dictionary<string, Dictionary<string, TextAnchor>>
        {
            {
                "top", new Dictionary<string, TextAnchor>
                {
                    {"left", TextAnchor.UpperLeft},
                    {"center", TextAnchor.UpperCenter},
                    {"right", TextAnchor.UpperRight}
                }
            },
            {
                "center", new Dictionary<string, TextAnchor>
                {
                    {"left", TextAnchor.MiddleLeft},
                    {"center", TextAnchor.MiddleCenter},
                    {"right", TextAnchor.MiddleRight}
                }
            },
            {
                "bottom", new Dictionary<string, TextAnchor>
                {
                    {"left", TextAnchor.LowerLeft},
                    {"center", TextAnchor.LowerCenter},
                    {"right", TextAnchor.LowerRight}
                }
            }
        };

        private static Dictionary<string, FontStyle> fontStyleMap = new Dictionary<string, FontStyle>
        {
            {"normal", FontStyle.Normal},
            {"bold", FontStyle.Bold},
            {"italic", FontStyle.Italic},
            {"BoldAndItalic", FontStyle.BoldAndItalic},
        };


        public static TextGenerationSettings GetRNUDefaultGenerationSettings()
        {
            TextGenerationSettings settings = new TextGenerationSettings();

            settings.fontSize = DEFAULT_FONT_SIZE;
            Font arialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

            // font load
            // Font arialFont = Resources.Load<Font>("Fonts/NORMAL_FONT");
            // Util.Log("----------load font");
            // if (arialFont == null){
            //     Util.Log("arialFont is nil");
            // }
            

            settings.font = arialFont;
            settings.color = Color.black;
            settings.fontStyle = FontStyle.Normal;

            settings.verticalOverflow = VerticalWrapMode.Overflow;
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;

            settings.pivot = Vector2.zero;
            settings.textAnchor = TextAnchor.MiddleLeft;
            //TODO 根据canvas 获取
            settings.scaleFactor = 1;

            settings.lineSpacing = 1;
            settings.updateBounds = false;

            settings.richText = false;

            // 保持Text组件一致
            settings.resizeTextForBestFit = false;
            settings.resizeTextMaxSize = 40;
            settings.resizeTextMinSize = 10;

            return settings;
        }



        public static TextAnchor GetTextAnchor(string textAlign, string textAlignVertical)
        {
            return textAlignRawMap[textAlignVertical][textAlign];
        }

        public static TextAlignRaw GetTextAlignRaw(TextAnchor ta)
        {
            return textAlignMap[ta];
        }


        public static FontStyle GetFontStyleFromStr(string style)
        {
            return fontStyleMap[style];
        }
    }
}