using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
// using Polybrush;
using UnityEngine.CFUI;
namespace CFEngine.Editor
{
    public class PrefabUIFix : CommonToolTemplate
    {
        private string ShowLog = "";
        GUIStyle style = new GUIStyle();
        public override void OnInit()
        {
            base.OnInit();
        }

        public override void OnUninit()
        {
            base.OnUninit();
        }

        public override void DrawGUI(ref Rect rect)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run", GUILayout.Width(100), GUILayout.Height(50)))
            {
                Process();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            style.normal.textColor = Color.red;
            style.fontSize = 12;
            GUILayout.Label(ShowLog, style);
            GUILayout.EndHorizontal();
        }

        private void Process()
        {
            ShowLog = "";

            string[] ids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/BundleRes/UI" });
            for (int i = 0; i < ids.Length; i++)
            {
                bool change = false;
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);

                GameObject oldPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                GameObject go = PrefabUtility.InstantiatePrefab(oldPrefab) as GameObject;

                DealWith(go, out change);
                if (change)
                {
                    ShowLog += "\n " + path.Substring(path.LastIndexOf("/") + 1);
                    Debug.Log(path.Substring(path.LastIndexOf("/") + 1));

                    // PrefabUtility.ReplacePrefab(go, oldPrefab, ReplacePrefabOptions.Default);
                }

                DestroyImmediate(go);
            }

            Debug.Log("修改成功！！！");
            ShowLog += "\n 修改成功！！！";
        }

        ///
        ///下面是可以瞎改的地方
        ///change表示该prefab是否有改动，有改动的话才会apply保存
        ///

        public void DealWith(GameObject go, out bool change)
        {
            FixCFAnimation(go, out change);
        }

        public void FixCFAnimation(GameObject go, out bool change)
        {
            change = false;
            CFAnimation[] animList = go.GetComponentsInChildren<CFAnimation>();
            foreach (CFAnimation anim in animList)
            {
                for (int i = 0; i < anim.AnimList.Count; i++)
                {
                    if (anim.AnimList[i].Target == null || anim.AnimList[i].m_Type != CFAnimType.Alpha)
                        continue;
                    CanvasGroup cg = anim.AnimList[i].Target.GetComponent<CanvasGroup>();
                    if (cg == null)
                    {
                        anim.AnimList[i].Target.AddComponent<CanvasGroup>();
                        change = true;
                    }
                }
            }
        }
    }
}