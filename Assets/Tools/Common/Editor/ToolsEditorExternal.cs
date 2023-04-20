using System.Collections;
using System.Collections.Generic;
using CFEngine.Editor;
using UnityEngine;
public partial class ToolsEditorExternal
{
    public static void Init (ToolsEditor te)
    {
        ToolsEditor.externalTools.Clear();
        ToolsEditor.externalTools.Add(new XEditor.Level.LevelTool (te));
    }
}