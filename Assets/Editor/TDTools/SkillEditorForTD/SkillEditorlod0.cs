using BluePrint;
using UnityEditor;


namespace TDTools
{
    class SkillEditorlod0 : BlueprintEditor
    {
        [MenuItem("Tools/TDTools/关卡相关工具/技能编辑器lod0 #%[")]
        private static void OpenSkillEditor()
        {
            SkillEditor.InitEmpty();
            SkillEditor.Instance.SetcurrentLod(SkillEditor.EnumLodDes.lod0);
        }
    }
}
