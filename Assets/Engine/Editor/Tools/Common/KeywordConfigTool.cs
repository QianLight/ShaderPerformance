using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public class KeywordConfigGUI
    {
        public static void OnGUI (ref ListElementContext lec, ref bool show, List<KeywordGroup> groups, List<string> keywords)
        {
            if (ToolsUtility.Button (ref lec, "keyword", 80))
            {
                show = !show;

            }
            if (show)
            {
                ToolsUtility.NewLine(ref lec, 20);
                if (ToolsUtility.Button(ref lec, "Reset", 80,true))
                {
                    keywords.Clear();
                }
                int lineCount = ToolsUtility.FixLineCount (ref lec);                
                for (int i = 0; i < groups.Count; ++i)
                {
                    var kwg = groups[i];
                    int perLineCount = lineCount;
                    ToolsUtility.NewLine (ref lec, 20);
                    ToolsUtility.Label (ref lec, kwg.name, 160, true);
                    for (int j = 0; j < kwg.keywords.Count; ++j)
                    {
                        var kwi = kwg.keywords[j];
                        bool reset = ToolsUtility.FixPerLine (ref lec, lineCount, ref perLineCount);
                        var isEnable = keywords.Contains (kwi.str);
                        if (ToolsUtility.Toggle (ref lec, kwi.str, 140, ref isEnable, reset))
                        {
                            if (isEnable)
                            {
                                keywords.Add (kwi.str);
                            }
                            else
                            {
                                keywords.Remove (kwi.str);
                            }
                        }
                    }
                }
            }
        }
    }
}