using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using CFUtilPoolLib;

namespace XEditor.Level
{
    class LevelDisplayPatrolMode : LevelDisplayBaseMode
    {
        LevelPatrol Patrol = null;

        public LevelDisplayPatrolMode(LevelGridTool tool) : base(tool)
        {
            Patrol = new LevelPatrol(this);
        }

        public override void OnEnterMode()
        {
            base.OnEnterMode();

            Patrol.LoadFromTable();
        }

        public override void OnDisable()
        {
            Patrol.Clear();
        }

        public override void OnLeaveMode()
        {
            Patrol.Clear();

            base.OnLeaveMode();
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("生成路线", new GUILayoutOption[] { GUILayout.Width(150) }))
            {
                Patrol.LinkPatrolPoint();
            }

            if (!Patrol.IsPatrolInfoDirty())
            {
                if (GUILayout.Button("保存到巡逻表格", new GUILayoutOption[] { GUILayout.Width(150) }))
                {
                    Patrol.SaveToTable();
                }
            }
        }

        public override void OnUpdate()
        {
            Patrol.OnUpdate();

            if (Selection.activeGameObject != null)
            {
                string name = Selection.activeGameObject.name;

                if(name.StartsWith("PatrolPath_"))
                {
                    int patrolID = int.Parse(name.Substring(11));
                    Patrol.SetOperationPath(patrolID);
                    return;
                }

                if(name.StartsWith("pp_"))
                {
                    int patrolID = int.Parse(Selection.activeGameObject.transform.parent.name.Substring(11));
                    Patrol.SetOperationPath(patrolID);
                    Patrol.UpdateCurrentPath();
                    return;
                }
                
            }
            Patrol.SetOperationPath(0);

        }

        public override bool OnMouseDoubleClick(SceneView sceneView)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            RaycastHit hitInfo;

            bool bHit = Physics.Raycast(r, out hitInfo, 10000.0f, layer_mask);

            if (bHit)
            {
                Patrol.AddPatrolPoint(hitInfo.point);
            }

            return false;
        }

        public override bool OnKeyDown(SceneView sceneView)
        {
            return false;
        }
    }
}
