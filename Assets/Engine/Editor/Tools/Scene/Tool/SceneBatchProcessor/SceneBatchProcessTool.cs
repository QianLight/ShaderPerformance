using System.Collections.Generic;
using UnityEditor;

namespace CFEngine.Editor
{
    public class SceneBatchContext : BatchToolContext
    {
        public List<OnProcessRawScene> processRawScene = new List<OnProcessRawScene>();
        public List<OnProcessCopiedScene> processCopiedScene = new List<OnProcessCopiedScene>();
    }

    public class SceneBatchTool : BatchTool<SceneBatchToolConfig, SceneBatchProcessor, SceneBatchContext>
    {
        protected override void OnPostProcess(SceneBatchContext context)
        {
            OnProcessRawScene GetRawSceneAction(List<OnProcessRawScene> funcList)
            {
                if (funcList.Count == 0)
                    return null;
                
                return (out bool resultDirty, out bool resultError) =>
                {
                    resultDirty = false;
                    resultError = false;
                    foreach (OnProcessRawScene func in funcList)
                    {
                        if (func != null)
                        {
                            func(out bool dirty, out bool error);
                            resultDirty |= dirty;
                            resultError |= error;
                        }    
                    }
                };
            }
            
            OnProcessCopiedScene GetCopiedSceneAction(List<OnProcessCopiedScene> funcList)
            {
                if (funcList.Count == 0)
                    return null;
                
                return (out bool resultError) =>
                {
                    resultError = false;
                    foreach (OnProcessCopiedScene func in funcList)
                    {
                        if (func != null)
                        {
                            func(out bool error);
                            resultError |= error;
                        }    
                    }
                };
            }
            string sceneListGuid = AssetDatabase.FindAssets("t:SceneList")[0];
            string sceneListAssetPath = AssetDatabase.GUIDToAssetPath(sceneListGuid);
            SceneList sceneList = AssetDatabase.LoadAssetAtPath<SceneList>(sceneListAssetPath);
            
            if (EditorUtility.DisplayDialog("批量刷场景", $"确定刷新{sceneList.sceneList.Count}个场景？", "确定", "取消"))
            {
                SceneListTool.ConvertScene(sceneList, GetRawSceneAction(context.processRawScene),
                    GetCopiedSceneAction(context.processCopiedScene));       
            }
        }
    }
}