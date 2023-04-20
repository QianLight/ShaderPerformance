using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace XEditor
{

    [System.Serializable]
    public class OCSNode
    {
        public string timeline;
        public string path;
        public string comment;
        public Vector3 pos;
        public Vector3 rot;

        // Editor virtual camera
        public string vcName;
        public GameObject go;
    }


    public class OCSData : ScriptableObject
    {
        public OCSNode[] data;
        
        public void OnSave()
        {
            List<OCSNode> list = new List<OCSNode>(data);
            list.RemoveAll(x => (string.IsNullOrEmpty(x.timeline) || string.IsNullOrEmpty(x.path) || x.timeline.Equals("Orignal_")));
            list.Sort((x, y) => x.timeline.CompareTo(y.timeline));
            data = list.ToArray();
        }

        public bool Exist(string cs)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    return true;
                }
            }
            return false;
        }

        public string SearchPath(string cs)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    return data[i].path;
                }
            }
            return string.Empty;
        }

        public string SearchComment(string cs)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    return data[i].comment;
                }
            }
            return string.Empty;
        }

        public string SearchVirtualCam(string cs)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    return data[i].vcName;
                }
            }
            return string.Empty;
        }

        public bool SearchVCM(string cs, out Vector3 pos, out Vector3 rot)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    pos = data[i].pos;
                    rot = data[i].rot;
                    return true;
                }
            }
            pos = Vector3.zero;
            rot = Vector3.zero;
            return false;
        }

        public bool AddorUpdate(string cs, string pat, string commt)
        {
            bool find = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    find = true;
                    data[i].path = pat;
                    data[i].comment = commt;
                    break;
                }
            }
            if (!find)
            {
                OCSNode n = new OCSNode();
                n.timeline = cs;
                n.path = pat;
                n.comment = commt;
                XEditorUtil.Add(ref data, n);
            }
            EditorUtility.SetDirty(this);
            return find;
        }

        public bool UpdateVCM(string cs, Vector3 pos, Vector3 rot)
        {
            bool find = false;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].timeline == cs)
                {
                    find = true;
                    data[i].pos = pos;
                    data[i].rot = rot;
                    break;
                }
            }
            EditorUtility.SetDirty(this);
            return find;
        }


        public void OpenScene(string path)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string npath = path;
                if (!path.EndsWith(".unity"))
                {
                    npath = path + ".unity";
                }
                if (!path.StartsWith("Assets"))
                {
                    npath = "Assets/Scenes/Scenelib/" + npath;
                }
                EditorSceneManager.OpenScene(npath);
                NullComponentAndMissingPrefabSearchTool.Clear();
            }
        }

    }
}