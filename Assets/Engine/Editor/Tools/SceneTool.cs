using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class SceneTool : ToolTemplate
    {
        public SceneTool (EditorWindow editorWindow) : base (editorWindow) { }

        private ESceneTool tool = ESceneTool.None;

        public override void OnEnable ()
        {
            base.OnEnable ();
            if (toolIcons == null)
            {
                toolIcons = new GUIContent[]
                {
                new GUIContent ("Scene List"),
                new GUIContent ("Scene Edit"),
                new GUIContent ("Layer Brush"),
                new GUIContent ("Layer Object"),

                };
            }
            tools.Clear ();
            for (ESceneTool i = ESceneTool.None; i < ESceneTool.Num; ++i)
            {
                tools.Add (SceneToolUtility.GetToolInstance (i));
            }
            SetTool ((int) tool);

        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (tools == null)
            {
                return;
            }
            
            int startIndex = (int) ESceneTool.None;
            int endIndex = (int) ESceneTool.Num;
            endIndex = Mathf.Min(endIndex, tools.Count);
            for (int i = startIndex; i < endIndex; ++i)
            {
                if (tools[i] != null)
                {
                    tools[i].OnUninit();
                }
            }
        }
    }
}