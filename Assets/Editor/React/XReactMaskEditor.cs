#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System;

namespace XEditor
{
    class XReactMaskEditor : EditorWindow
    {
        XReactEntranceWindow window;

        public class PathWeight
        {
            public string path;
            public int depth;
            public bool seleced;

            public string bone;
            public int lineID;

            public List<int> Childs = new List<int>();
        }

        public List<PathWeight> list = new List<PathWeight>();

        bool beginParse = false;

        //cache text.
        List<string> maskLines = new List<string>();


        ReactMaskTreeView treeView = null;
        TreeViewState treeViewState = null;
        SearchField m_SearchField;

        public void Init(XReactEntranceWindow parent)
        {
            window = parent;

            ParseMask(GetDataFileWithPath());

            ReactMaskTreeView.editor = this;

            treeViewState = new TreeViewState();
            treeView = new ReactMaskTreeView(treeViewState);            

            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }

        string GetText(string pathwithname)
        {
            string text = System.IO.File.ReadAllText(pathwithname);
            return text;
        }

        private string GetDataFileWithPath(string ex = "")
        {
            return "Assets/BundleRes/" + window.ReactDataSet.ReactData.AvatarMask + ex + ".mask";
        }

        #region Parse

        void ParseMask(string path)
        {
            beginParse = false;
            if (list == null) list = new List<PathWeight>();
            list.Clear();
            if (maskLines == null) maskLines = new List<string>();
            maskLines.Clear();
            string lastStrLine = null;
            int lineID = 0;
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string strLine = sr.ReadLine();
                     
                    while (strLine != null)
                    {
                        //Debug.Log(strLine);
                        maskLines.Add(strLine);

                        if (!beginParse && strLine.Contains("root"))
                        {
                            beginParse = true;
                        }

                        if (beginParse)
                        {
                            if (strLine.Contains("root"))
                            {
                                lastStrLine = ParsePathPre(strLine);
                            }
                            else if (strLine.Contains("Weight"))
                            {
                                PathWeight pair = ParseWeight(strLine, lastStrLine, lineID++);
                            }
                            else
                            {
                                lastStrLine = ParsePathPost(strLine, lastStrLine);
                            }
                        }

                        strLine = sr.ReadLine();
                    }
                }
            }
        }

        string ParsePathPre(string line)
        {
            int start = line.IndexOf("root");
            if (start == -1)
            {
                Debug.LogError("ParsePathPre : " + line);
                return "";
            }

            string str = line.Substring(start);

            return str;
        }

        string ParsePathPost(string line, string lastStrLine)
        {
            return lastStrLine.TrimEnd()+" " + line.TrimStart();
        }

        PathWeight ParseWeight(string line, string lastStrLine, int lineID)
        {
            int start = line.IndexOf("m_Weight:");
            if (start == -1)
            {
                Debug.LogError("ParseWeight : " + line);
                return null;
            }

            string str = line.Substring(start + 10);
            str.Trim();

            PathWeight pair = new PathWeight();
            pair.path = lastStrLine;
            pair.seleced = str.Equals("1");
            pair.lineID = lineID;

            Through(ref pair);

            return pair;
        }

        void Through(ref PathWeight pair)
        {
            //Debug.Log(pair.lineID.ToString() + "  " + pair.path);

            if (pair.lineID == 0)
            {
                pair.bone = pair.path;
                pair.depth = 0;

                list.Add(pair);
                return;
            }

            int last = pair.path.LastIndexOf('/');
            if (last == -1 )
            {
                Debug.LogError("Through err !" + pair.path);
                return;
            }

            string parent = pair.path.Substring(0, last);
            string bone = pair.path.Substring(last + 1);

            pair.bone = bone;
            bool find = false;
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].path.Equals(parent))
                {
                    pair.depth = list[i].depth + 1;
                    list[i].Childs.Add(pair.lineID);
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                Debug.LogError("Err. Not Found parent! " + parent);
            }

            list.Add(pair);

            //Debug.Log(pair.lineID.ToString() + "  "+ pair.depth.ToString()+ " " + pair.bone);
        }

        #endregion

        #region ReplaceMask

        public void ReplaceAvatar(string path, bool isCreate = true)
        {
            if (maskLines != null && maskLines.Count > 0)
            {
                int count = 0;
                bool begin = false;
                using (FileStream fs = new FileStream(path, isCreate ? FileMode.Create : FileMode.Truncate))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        for (int i = 0; i < maskLines.Count; ++i)
                        {
                            string strLine = maskLines[i];

                            if (!begin && strLine.Contains("root"))
                            {
                                begin = true;
                            }

                            if (begin)
                            {
                                int start = strLine.IndexOf("m_Weight:");

                                if (start != -1)
                                {

                                    char old = '0';
                                    int parm = strLine.IndexOf('0');

                                    if (parm != -1)
                                    {
                                        old = '0';
                                    }
                                    else
                                    {
                                        parm = strLine.IndexOf('1');
                                        old = '1';
                                    }

                                    if (parm == -1)
                                    {
                                        Debug.LogError("EEEErrror!!");
                                    }
                                    else
                                    {
                                        if (count >= list.Count)
                                        {
                                            Debug.LogError(count.ToString() + " ex ss" + list.Count.ToString());
                                        }                    
                                        var data = list[count];

                                        count++;
                                        strLine = strLine.Replace(old, data.seleced ? '1' : '0');
                                    }
                                }
                            }

                            sw.WriteLine(strLine);                           
                        }

                    }
                }
            }
        }

        #endregion

        void OnGUI()
        {

            DoToolbar(toolbarRect);
            DoTreeView(treeViewRect);
            BottomToolBar(bottomToolbarRect);
        }

        void OnSelectionChange()
        {
            if (treeView != null)
                treeView.SetSelection(Selection.instanceIDs);
            Repaint();
        }

        void DoToolbar(Rect rect)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            if(treeView != null)
                treeView.searchString = m_SearchField.OnToolbarGUI(rect, treeView.searchString);
            GUILayout.EndHorizontal();
        }

        void DoTreeView(Rect rect)
        {
            if (treeView != null)
                treeView.OnGUI(rect);

            //if (GUILayout.Button("Read"))
            //{
            //    if (text == null)
            //    {
            //        text = GetText(GetDataFileWithPath());
            //    }

            //}

            //if (text != null)
            //{
            //    GUI.color = Color.black;
            //    GUILayout.Label(text);
            //}
        }

        void BottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("Expand All", style))
                {
                    treeView.ExpandAll();
                }

                if (GUILayout.Button("Collapse All", style))
                {
                    treeView.CollapseAll();
                }

                if (GUILayout.Button("Save", style))
                {
                    ReplaceAvatar(GetDataFileWithPath(""));

                    UnityEditor.AssetDatabase.Refresh();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("Test", style))
                {
                    //Test();
                }

                scaleBone = GUILayout.HorizontalSlider(scaleBone, 0.1f, 1f);
            }

            GUILayout.EndArea();
        }

        Rect treeViewRect
        {
            get { return new Rect(20, 30, position.width - 40, position.height - 90); }
        }

        Rect toolbarRect
        {
            get { return new Rect(20f, 10f, position.width - 40f, 20f); }
        }

        Rect bottomToolbarRect
        {
            get { return new Rect(20f, position.height - 48f, position.width - 40f, 50f); }
        }

        void Test()
        {
            if (bTest) return;

            _bonePoints.Clear();
            var root = GameObject.FindObjectOfType(typeof(XReactEntity)) as XReactEntity;
            if (root != null)
            { 
                var gb = new GameObject("ReactBoneRoot");
                _bonePoints.Add(gb.transform);
                CreatTree(list[0], root.transform, gb.transform);

            }
        }

        void CreatTree(PathWeight pair ,Transform root, Transform parent)
        {
            var point = root.Find(pair.path);

            var gb = new GameObject(pair.bone);
            //var gb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gb.name = pair.bone;
            gb.transform.SetParent(parent);
            gb.transform.position = point.position;
            //gb.transform.localScale = Vector3.one * scaleBone;
            _bonePoints.Add(gb.transform);

            if (pair.Childs.Count > 0)
            {
                for (int i = 0; i < pair.Childs.Count; ++i)
                {
                    CreatTree(list[pair.Childs[i]], root, gb.transform);
                }
            }
        }

        bool bTest = false;
        static List<Transform> _bonePoints = new List<Transform>();
        static float scaleBone = 0.2f;

        [DrawGizmo(GizmoType.Active | GizmoType.Selected)]

        private static void CustomOnDrawGizmos(XReactEntity target, GizmoType gizmoType)
        {
            var color = Gizmos.color;
            Gizmos.color = Color.white;
            //target为挂载该组件的对象
            //Gizmos.DrawCube(target.transform.position, Vector3.one);

            if (_bonePoints.Count > 0)
            {
                for (int i = 0; i < _bonePoints.Count; ++i)
                {
                    Gizmos.DrawSphere(_bonePoints[i].position, scaleBone);
                }
            }

            Gizmos.color = color;
        }

        
    }


}

#endif