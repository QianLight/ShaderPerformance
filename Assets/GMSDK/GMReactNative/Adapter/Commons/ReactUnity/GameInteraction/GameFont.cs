/*
 * @Author: hexiaonuo
 * @Date: 2021-10-15
 * @Description: Game set Font, for ReactUnity action panel
 * @FilePath: ReactUnity/GameInteraction/GameFont.cs
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    /*
     * 游戏设置共享字体
     * 游戏可单次加载单个字体，总共可加载多个字体
     */
    partial class GameInteraction
    {
        private static Dictionary<string, Font> gameFontDic = new Dictionary<string, Font>();
        
        public static void SetGameFont(string fontName, Font font)
        {
            if (String.IsNullOrEmpty(fontName) || font == null)
            {
                Util.LogAndReport("game set font failed, for name is null or font is null");
                return;
            }

            if (gameFontDic.Count != 0 && gameFontDic.ContainsKey(fontName))
            {
                Util.LogAndReport("game set font failed, for duplicated {0}", fontName);
                return;
            }
            Util.Log("game set font: {0}", fontName);
            gameFontDic.Add(fontName, font);
        }
        
        public static ArrayList GetGameFontDic()
        {
            ArrayList res = new ArrayList();
            foreach (var item in gameFontDic)
            {
                res.Add(item.Key);
            }
            return res;
        }
        public static Font GetGameFont(string name)
        {
            if (gameFontDic != null && gameFontDic.ContainsKey(name))
            {
                return gameFontDic[name];
            }

            return null;
        }
        
    }
}