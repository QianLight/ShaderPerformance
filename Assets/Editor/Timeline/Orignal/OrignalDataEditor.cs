using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

namespace XEditor
{

    [CustomEditor(typeof(OCSData))]
    public class OrignalDataEdit : XDataEditor<OCSData>
    {
        const int MAX = 1000;
        private bool[] folder = new bool[MAX];
        internal const string timeline = "Assets/BundleRes/Timeline/";
        internal const string scene = "Assets/Scenes/Scenelib/";
       
        [MenuItem("Assets/Timeline/OriginalCSData_Create")]
        static void CreateCsData()
        {
            CreateAsset(OriginalSetting.dataPat);
        }

        private void OnEnable()
        {
            for (int i = 0; i < MAX; i++) folder[i] = false;
            base.Init(OriginalSetting.Icon, "Timeline Data");
            Undo.RegisterCompleteObjectUndo(target, "CSData");
        }

        protected override void InnerGUI()
        {
            if (odData != null)
            {
                int len = odData.data.Length;
                for (int i = 0; i < len; i++)
                {
                    if (string.IsNullOrEmpty(search) || odData.data[i].timeline.Contains(search) || odData.data[i].comment.Contains(search))
                    {
                        GUIItem(i);
                    }
                }
            }
        }

        private void GUIItem(int i)
        {
            EditorGUILayout.BeginHorizontal();
            string title = odData.data[i].comment;
            title = string.IsNullOrEmpty(title) ? odData.data[i].timeline : odData.data[i].timeline + " [" + title + "]";
            folder[i] = EditorGUILayout.Foldout(folder[i], title);
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                odData.data = XEditorUtil.Remv(odData.data, i);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            if (folder[i])
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                var d = odData.data[i];
                d.timeline = EditorGUILayout.TextField("ID", d.timeline);
                if (string.IsNullOrEmpty(odData.data[i].timeline))
                {
                    odData.data[i].timeline = GuiSelect(timeline, "playable");
                }
                else
                {
                    if (GUILayout.Button("Pin", GUILayout.Width(60)))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<PlayableAsset>(timeline + odData.data[i].timeline + ".playable");
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (!string.IsNullOrEmpty(d.timeline) && !d.timeline.StartsWith("Orignal_"))
                {
                    EditorGUILayout.HelpBox("not start with Orignal_", MessageType.Error);
                }
                d.comment = EditorGUILayout.TextField("标注", d.comment);
                if (string.IsNullOrEmpty(d.comment))
                {
                    EditorGUILayout.HelpBox("comment is null", MessageType.Warning);
                }
                EditorGUILayout.BeginHorizontal();
                d.path = EditorGUILayout.TextField("路径", d.path);

                if (string.IsNullOrEmpty(d.path))
                {
                    d.path = GuiSelect(scene, "unity");
                }
                else
                {
                    if (GUILayout.Button("Open", GUILayout.Width(60)))
                    {
                        EditorSceneManager.OpenScene(scene + odData.data[i].path + ".unity");
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(d.vcName))
                {
                    string pat = OriginalSetting.vcPath + d.vcName;
                    d.go = AssetDatabase.LoadAssetAtPath<GameObject>(pat);
                }
                d.go = (GameObject)EditorGUILayout.ObjectField("虚拟相机", d.go, typeof(GameObject), false);
                if (d.go != null)
                {
                    var vcs = d.go.GetComponentsInChildren<CinemachineVirtualCameraBase>();
                    if (vcs == null || vcs.Length == 0)
                    {
                        EditorGUILayout.HelpBox("Not found virtual camera in prefab", MessageType.Error);
                    }
                    else
                    {
                        d.vcName = d.go.name + ".prefab";
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Not special Virtual Camera for editor", MessageType.Warning);
                }
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }
        }

        private string GuiSelect(string path, string ext)
        {
            var ret = "";
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                ret = EditorUtility.OpenFilePanel("Select unity scene file", path, ext);
            }
            if (!string.IsNullOrEmpty(ret))
            {
                int start = ret.IndexOf("Assets");
                ret = ret.Substring(start);
                ret = ret.Replace(path, "");
                int last = ret.LastIndexOf(".");
                ret = ret.Substring(0, last);
                GUI.FocusControl("");
            }
            return ret;
        }

        protected override void OnAdd()
        {
            OCSNode n = new OCSNode();
            n.path = string.Empty;
            n.comment = string.Empty;
            n.timeline = string.Empty;
            XEditorUtil.Add(ref odData.data, n);
            int len = odData.data.Length;
            folder[len - 1] = true;
        }

        protected override void OnSave()
        {
            odData.OnSave();
            EditorUtility.SetDirty(odData);
            AssetDatabase.SaveAssets();
        }
    }
}