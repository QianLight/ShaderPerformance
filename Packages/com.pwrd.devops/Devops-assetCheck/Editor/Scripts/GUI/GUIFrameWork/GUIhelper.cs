using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GUIFrameWork
{
    public class GUIhelper
    {
        private static Font m_DefaultFont;
        public static Font DefaultFont
        {
            get
            {
                if (m_DefaultFont == null) m_DefaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
                return m_DefaultFont;
            }
            set { m_DefaultFont = value; }
        }

        public static int GetGUIStringWidth(string message)
        {
            return GetGUIStringWidth(message, DefaultFont);
        }
        // 获取GUI文本宽度
        public static int GetGUIStringWidth(string message, Font font)
        {
            int totalLength = 0;
            font.RequestCharactersInTexture(message, font.fontSize);
            char[] arr = message.ToCharArray();
            foreach(char c in arr)
            {
                font.GetCharacterInfo(c, out var characterInfo, font.fontSize);
                totalLength += characterInfo.advance;
            }
            return totalLength;
        }

        public static int GetGUIStringsMaxWidth(string[] messages)
        {
            int max = 0;
            foreach(var msg in messages)
            {
                int len = GetGUIStringWidth(msg);
                if (len > max)
                    max = len;
            }
            return max;
        }
    }
}
