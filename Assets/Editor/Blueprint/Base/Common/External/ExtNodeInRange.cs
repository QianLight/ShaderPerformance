using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CFEngine;

namespace BluePrint
{
    class ExtNodeInRange : BluePrintBaseDataNode<ExtInRangeData>
    {
        public GameObject center;

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, false);

            DrawHead = false;
            nodeEditorData.BackgroundText = "";
            nodeEditorData.Tag = "InRange";

            BluePrintPin pinInPos = new BluePrintValuedPin(this, 1, "", PinType.Data, PinStream.In, VariantType.Var_Vector3);
            AddPin(pinInPos);

            BluePrintPin pinOut = new BluePrintValuedPin(this, 2, "", PinType.Data, PinStream.Out, VariantType.Var_Bool);
            AddPin(pinOut);
        }

        public override List<ExtInRangeData> GetCommonDataList(BluePrintData data)
        {
            return data.InRangeData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("目标点", new GUILayoutOption[] { GUILayout.Width(150f) });
            center = (GameObject)EditorGUILayout.ObjectField(center, typeof(GameObject), true, new GUILayoutOption[] { GUILayout.Width(120f) });
            EditorGUILayout.EndHorizontal();

            HostData.Radius = EditorGUILayout.FloatField("半径", HostData.Radius, new GUILayoutOption[] { GUILayout.Width(270f) });
        }

        public override void DrawGizmo()
        {
            if (center != null && HostData.Radius > 0)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(center.transform.position, new Vector3(0, 1, 0), HostData.Radius);
            }
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            if (center != null)
            {
                nodeEditorData.CustomData = EditorCommon.GetSceneObjectPath(center.transform);
                HostData.Center = center.transform.position;
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            if (!string.IsNullOrEmpty(nodeEditorData.CustomData))
            {
                center = GameObject.Find(nodeEditorData.CustomData);
            }
        }
    }
}
