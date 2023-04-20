using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XEditor;
using TableEditor;

namespace LevelEditor
{
    public enum CampType
    {
        Friend,
        Opposite
    }

    class LevelRobotWaveNode:LevelBaseNode<LevelRobotWaveData>
    {
        public GameObject bornPoint;
        public GameObject robotPrefab;

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header7";
            nodeEditorData.Tag = "RobotWave";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinHp = new BluePrintValuedPin(this, 2, "Hp", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinHp);

            BluePrintPin pinCount = new BluePrintValuedPin(this, 1, "Alive", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinCount);

            BluePrintPin pinID = new BluePrintValuedPin(this, 3, "ID", PinType.Data, PinStream.Out, VariantType.Var_UINT64);
            AddPin(pinID);

            BluePrintPin pinPosIn = new BluePrintValuedPin(this, 4, "Pos", PinType.Data, PinStream.In, VariantType.Var_Vector3);
            AddPin(pinPosIn);

            BluePrintPin pinPosOut = new BluePrintValuedPin(this, 5, "Pos", PinType.Data, PinStream.Out, VariantType.Var_Vector3);
            AddPin(pinPosOut);

        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("阵营", GUILayout.Width(80f));
            HostData.camp = (uint)(CampType)EditorGUILayout.EnumPopup((CampType)HostData.camp, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RobotID", GUILayout.Width(80f));
            HostData.robotID = (uint)EditorGUILayout.FloatField(HostData.robotID, GUILayout.Width(150f));
            EditorGUILayout.EndHorizontal();
        }

        public override List<LevelRobotWaveData> GetDataList(LevelGraphData data)
        {
            return data.robotWaveData;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            GameObject go = new GameObject("RW" + Root.GraphID.ToString() + "_RobotWave_"  + HostData.robotID);
            go.transform.position = Vector3.zero;
            bornPoint = go;
        }

        public override void UnInit()
        {
            DestroyRoot();
            base.UnInit();
        }

        public override void OnDeleted()
        {
            base.OnDeleted();
            DestroyRoot();
        }

        private void DestroyRoot()
        {
            if(bornPoint!=null)
            {
                GameObject.DestroyImmediate(bornPoint);
                bornPoint = null;
            }
        }

        public override void ConvenientSave()
        {
            base.BeforeSave();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            if(bornPoint!=null&&bornPoint.transform.childCount>0)
            {
                var child = bornPoint.transform.GetChild(0);
                var editorPos = child.position;
                QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(editorPos + new Vector3(0, 3.0f, 0));
                editorPos.y = element.pos.y;
                child.position = editorPos;
                HostData.spawnData.height = element.pos.y;
                HostData.spawnData.position = editorPos;
                HostData.spawnData.rotation = child.rotation.eulerAngles.y;
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if(bornPoint!=null&&bornPoint.transform.childCount>0)
            {
                Quaternion rotation = Quaternion.Euler(45, 0, 0);
                SceneView.lastActiveSceneView.LookAtDirect(bornPoint.transform.GetChild(0).position, rotation, 20);
                EditorGUIUtility.PingObject(bornPoint);
                Selection.activeGameObject = bornPoint;
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            if (robotPrefab == null)
                robotPrefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Prefabs/Role_luffy");
            GameObject go = GameObject.Instantiate(robotPrefab);

            go.transform.parent = bornPoint.transform;
            go.transform.position = HostData.spawnData.position;
            go.transform.rotation = Quaternion.Euler(0, HostData.spawnData.rotation, 0);
        }

        public void AddRobotAtPos(Vector3 pos)
        {
            float height = pos.y;
            QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(pos);

            if (element == null || !element.IsValid())
            {
                Debug.LogError("机器人必须种在格子上");
                return;
            }
            height = element.pos.y;

            if (bornPoint != null)
            {
                if (robotPrefab == null)
                    robotPrefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Prefabs/Role_luffy");

                GameObject go = GameObject.Instantiate(robotPrefab);

                go.transform.parent = bornPoint.transform;

                Vector3 StickGroundPos = new Vector3(pos.x, height, pos.z);
                go.transform.position = StickGroundPos;
            }
            else
            {
                Root.ShowNotification(new GUIContent("BornPoint = NULL"));
            }
        }

        public override void CheckError()
        {
            base.CheckError();
            BlueprintNodeErrorInfo error = nodeErrorInfo;
            if (HostData.robotID == 0)
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "机器人ID不能为0", null));
            }

            if (bornPoint != null && bornPoint.transform.childCount != 1)
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "机器人" + HostData.NodeID + "个数必须为1", null));
            }
        }

    }
}
