using System;
using CFEngine.SRP;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using URPDebugViews;

namespace CFEngine.Editor
{ 
    public class ProfileTools : UnityEditor.Editor
    {
        private static OverdrawMonitor monitor;
        
        [MenuItem("Tools/引擎/Overdraw/开关", false, 0)]
        private static void Overdraw()
        {
            DebugViewsManager.Instance.EnableViewWithMaterialName();
        }
        
        [MenuItem("Tools/特效/Overdraw/Game视图开关 &#%g")]
        public static void ChangeGameOverdrawView()
        {
            // if (Application.isPlaying)
            // {
                OverdrawState.gameOverdrawViewMode = !OverdrawState.gameOverdrawViewMode;
                if (OverdrawState.gameOverdrawViewMode || OverdrawState.sceneOverdrawViewMode)
                {
                    SFXProfileWindow.ShowWindow();
                    OverdrawState.opaqueOverdraw = true;
                    OverdrawState.transparentOverdraw = true;
                    // OverdrawMonitor.isOn = true;
                    monitor = OverdrawMonitor.Instance;
                    OverdrawWindow.ShowWindow();
                }
                else
                {
                    OverdrawState.opaqueOverdraw = false;
                    OverdrawState.transparentOverdraw = false;
                    // OverdrawMonitor.isOn = false;
                    DestroyImmediate(OverdrawMonitor.Instance.gameObject);
                    OverdrawWindow.instance.Close();
                }
            // }
            // else
            // {
            //     OverdrawState.gameOverdrawViewMode = !OverdrawState.gameOverdrawViewMode;
            //     if (OverdrawState.gameOverdrawViewMode || OverdrawState.sceneOverdrawViewMode)
            //     {
            //         OverdrawState.opaqueOverdraw = true;
            //         OverdrawState.transparentOverdraw = true;
            //         OverdrawWindow.ShowWindow();
            //     }
            //     else
            //     {
            //         OverdrawState.opaqueOverdraw = false;
            //         OverdrawState.transparentOverdraw = false;
            //         OverdrawWindow.instance.Close();
            //     }
            // }
        }
        
        [MenuItem("Tools/特效/Overdraw/Scene视图开关 &#%s")]
        public static void ChangeSceneOverdrawView()
        {
            OverdrawState.sceneOverdrawViewMode = !OverdrawState.sceneOverdrawViewMode;
            if (OverdrawState.sceneOverdrawViewMode || OverdrawState.gameOverdrawViewMode)
            {
                OverdrawWindow.ShowWindow();
            }
        }

        [MenuItem("Tools/特效/SFXProfile &m")]
        public static void ChangeSFXProfile()
        {
            if (SFXProfileWindow.isOn)
            {
                SFXProfileWindow.CloseDialog();
                DestroyImmediate(OverdrawMonitor.Instance.gameObject);
            }
            else
            {
                SFXProfileWindow.ShowWindow();
            }
        }

        [MenuItem("Tools/引擎/技能批量分类工具")]
        public static void OpenSkillTypeSetter()
        {
            if (SkillTypeSetterAuto.isOn)
            {
                SkillTypeSetterAuto.CloseWindow();
            }
            else
            {
                SkillTypeSetterAuto.ShowWindow();
            }
        }
    }
}