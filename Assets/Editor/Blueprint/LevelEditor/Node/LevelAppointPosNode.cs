using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using TableEditor;
using System.Text;

namespace LevelEditor
{
    class LevelAppointPosNode : LevelBaseNode<LevelAppointPosData>
    {
        private List<GameObject> objList = new List<GameObject>();

        enum AppointPosType
        {
            Birth,
            Settle
        }

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            nodeEditorData.Tag = "AppointPos";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("指定位置类型", new GUILayoutOption[] { GUILayout.Width(80f) });
            HostData.posType = (uint)(AppointPosType)EditorGUILayout.EnumPopup((AppointPosType)HostData.posType,
                new GUILayoutOption[] { GUILayout.Width(100f) });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("位置列表", new GUILayoutOption[] { GUILayout.Width(80f) });
            if (GUILayout.Button(new GUIContent("Add"), new GUILayoutOption[] { GUILayout.Width(50f) }))
            {
                HostData.objNameList.Add(string.Empty);
                HostData.posList.Add(Vector3.zero);
                objList.Add(null);
            }
            EditorGUILayout.EndHorizontal();

            int temp = -1;
            for (var i = 0; i < objList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                objList[i] = (GameObject)EditorGUILayout.ObjectField(objList[i], typeof(GameObject), true, new GUILayoutOption[] { GUILayout.Width(150f) });
                if (GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(50f) }))
                {
                    temp = i;
                }
                EditorGUILayout.EndHorizontal();
                if (objList[i] != null)
                    EditorGUILayout.LabelField(string.Format("pos:{0}   face:{1}",
                        objList[i].transform.position.ToString(), objList[i].transform.eulerAngles.y));
            }
            if (temp >= 0)
            {
                objList.RemoveAt(temp);
                HostData.objNameList.RemoveAt(temp);
            }
        }

        public override List<LevelAppointPosData> GetDataList(LevelGraphData data)
        {
            return data.appointPosData;
        }

        public override void CheckError()
        {
            base.CheckError();
            foreach (var obj in objList)
            {
                if (obj == null)
                {
                    nodeErrorInfo.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "指定位置的物体不能为空", null));
                    break;
                }
            }
        }

        public void Reload()
        {
            objList.Clear();
            for (var i = 0; i < HostData.objNameList.Count; i++)
            {
                objList.Add(GameObject.Find(HostData.objNameList[i]));
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            objList.Clear();
            for (var i = 0; i < HostData.objNameList.Count; i++)
            {
                objList.Add(GameObject.Find(HostData.objNameList[i]));
            }
        }

        public override void ConvenientSave()
        {
            base.BeforeSave();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            Transform objTran = null;
            for (var i = 0; i < objList.Count; i++)
            {
                HostData.objNameList[i] = objList[i].name;
                objTran = objList[i].transform;
                // StartPos
                if(HostData.posType == 0)
                    HostData.posList[i] = new Vector4(objTran.position.x, objTran.position.y, objTran.position.z, objTran.eulerAngles.y);
                else if(HostData.posType == 1) // SettlePos
                {
                    SaveSettlePosToTable();
                }
            }
        }

        private void SaveSettlePosToTable()
        {
            try
            {
                TableFullData tableData = new TableFullData();
                TableReader.ReadTableByFileStream(Application.dataPath + "/Table/MapList.txt", ref tableData);
                int posIndex = tableData.nameList.FindIndex(n => n == "EndPostion");
                int levelIndex = tableData.nameList.FindIndex(n => n == "LevelConfigFile");
                int s = LevelEditor.Instance.CachedOpenFile.IndexOf("Level/");
                int e = LevelEditor.Instance.CachedOpenFile.IndexOf(".cfg");
                string level = LevelEditor.Instance.CachedOpenFile.Substring(s, e - s);
                var data = tableData.dataList.Find(d =>
                d.valueList[levelIndex] == level);
                if (data == null)
                {
                    Debug.LogError("maplist表里找不到对应当前关卡的配置行");
                    return;
                }
                StringBuilder posSB = new StringBuilder();
                for (var i = 0; i < objList.Count; i++)
                {
                    Transform t = objList[i].transform;
                    posSB.Append(string.Format("{0}={1}={2}", t.position.x, t.position.y, t.position.z));
                    if (i < objList.Count - 1)
                        posSB.Append('|');
                }
                data.valueList[posIndex] = posSB.ToString();
                TableReader.WriteTable(Application.dataPath + "/Table/MapList.txt", tableData);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError("其他节点已保存，appointpos未生效，关闭表格后再尝试保存。");
            }
        }

        //    public override void BeforeSave()
        //    {
        //        base.BeforeSave();
        //        for(var i=0;i<objList.Count;i++)
        //        {
        //            HostData.objNameList[i] = objList[i].name;
        //        }
        //        try
        //        {
        //            TableFullData tableData = new TableFullData();
        //            TableReader.ReadTableByFileStream(Application.dataPath + "/Table/MapList.txt", ref tableData);
        //            bool birth = (AppointPosType)HostData.posType == AppointPosType.Birth;
        //            int posIndex = tableData.nameList.FindIndex(n => n == (birth ? "StartPos" : "EndPosition"));
        //            int levelIndex = tableData.nameList.FindIndex(n => n == "LevelConfigFile");
        //            int s = LevelEditor.Instance.CachedOpenFile.IndexOf("Level/");
        //            int e = LevelEditor.Instance.CachedOpenFile.IndexOf(".cfg");
        //            string level = LevelEditor.Instance.CachedOpenFile.Substring(s, e - s);
        //            var data = tableData.dataList.Find(d =>
        //            d.valueList[levelIndex] == level);
        //            if (data == null)
        //            {
        //                Debug.LogError("maplist表里找不到对应当前关卡的配置行");
        //                return;
        //            }
        //            StringBuilder posSB = new StringBuilder();
        //            for (var i = 0; i < objList.Count; i++)
        //            {
        //                Transform t = objList[i].transform;
        //                posSB.Append(string.Format("{0}={1}={2}", t.position.x, t.position.y, t.position.z));
        //                if (i < objList.Count - 1)
        //                    posSB.Append('|');
        //            }
        //            data.valueList[posIndex] = posSB.ToString();
        //            if (birth)
        //            {
        //                int faceIndex = tableData.nameList.FindIndex(n => n == "StartFace");
        //                StringBuilder faceSB = new StringBuilder();
        //                for (var i = 0; i < objList.Count; i++)
        //                {
        //                    Transform t = objList[i].transform;
        //                    faceSB.Append(t.eulerAngles.y);
        //                    if (i < objList.Count - 1)
        //                        faceSB.Append('|');
        //                }
        //                data.valueList[faceIndex] = faceSB.ToString();
        //            }
        //            TableReader.WriteTable(Application.dataPath + "/Table/MapList.txt", tableData);
        //        }
        //        catch(Exception e)
        //        {
        //            Debug.LogError(e);
        //            Debug.LogError("其他节点已保存，appointpos未生效，关闭表格后再尝试保存。");
        //        }
        //    }
        //}
    }
}
