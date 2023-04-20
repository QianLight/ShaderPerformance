using UnityEditor;
#if UNITY_2021_2_OR_NEWER
    using UnityEditor.SceneManagement;
#else 
    using UnityEditor.Experimental.SceneManagement;
    using UnityEditor.SceneManagement;
#endif

namespace Blueprint.ActorEditor
{

    using Blueprint.Actor;

    [InitializeOnLoadAttribute]
    public static class ActorHierarchyMonitor
    {

        public static bool IsInPrefabMode = false;

        public static PrefabStage CurrentPrefabStage;

        static ActorHierarchyMonitor()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosed;
            PrefabStage.prefabStageDirtied += OnPrefabStageDirtied;
        }

        static void OnHierarchyChanged()
        {
            if (IsInPrefabMode)
            {

                return ;
            }
        }


        static void OnPrefabStageOpened(PrefabStage stage)
        {
            IsInPrefabMode = true;
            CurrentPrefabStage = stage;

            BlueprintActor actor = stage.prefabContentsRoot.GetComponent<BlueprintActor>();

            if (actor != null)
            {
                actor.RefreshActorParam();
#if UNITY_2020_1_OR_NEWER
                ActorEditor.SendAcotrRefresh(stage.assetPath, actor);
#else
                ActorEditor.SendAcotrRefresh(stage.prefabAssetPath, actor);
#endif
                EditorSceneManager.MarkSceneDirty(stage.scene);
            }
        }

        static void OnPrefabStageClosed(PrefabStage stage)
        {
            IsInPrefabMode = false;
            CurrentPrefabStage = null;
        }

        static void OnPrefabStageDirtied(PrefabStage stage)
        {
            BlueprintActor actor = stage.prefabContentsRoot.GetComponent<BlueprintActor>();

            if (actor != null)
            {
                actor.RefreshActorParam();
#if UNITY_2020_1_OR_NEWER
                ActorEditor.SendAcotrRefresh(stage.assetPath, actor);
#else
                ActorEditor.SendAcotrRefresh(stage.prefabAssetPath, actor);
#endif
            }
        }
    }
}
