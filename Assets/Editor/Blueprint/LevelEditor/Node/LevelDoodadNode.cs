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
    class LevelDoodadNode:LevelBaseNode<LevelDoodadData>
    {
        private GameObject root;
        private GameObject doodadPrefab;

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header7";
            nodeEditorData.Tag = "DoodadWave";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;
        }

        public override List<LevelDoodadData> GetDataList(LevelGraphData data)
        {
            return data.doodadData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DoodadID", GUILayout.Width(80f));
            HostData.doodadID = (uint)EditorGUILayout.FloatField(HostData.doodadID, GUILayout.Width(150f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("TotalCount:   {0}", root.transform.childCount), GUILayout.Width(150f));
            EditorGUILayout.EndHorizontal();
        }

        public override void OnAdded()
        {
            base.OnAdded();
            GameObject go = new GameObject("RW" + Root.GraphID.ToString() + "_DoodadWave_" + HostData.doodadID);
            go.transform.position = Vector3.zero;
            root = go;
        }

        public override void UnInit()
        {
            base.UnInit();
            DestroyRoot();
        }

        public override void OnDeleted()
        {
            base.OnDeleted();
            DestroyRoot();
        }

        private void DestroyRoot()
        {
            if(root!=null)
            {
                GameObject.DestroyImmediate(root);
                root = null;
            }
        }

        private void GetDoodadInfo()
        {
            var info = LevelEditorTableData.DropInfo.GetByID((int)HostData.doodadID);
            doodadPrefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>(info.DropFx[0]);
        }

        public void AddDoodadAtPos(Vector3 pos)
        {
            float height = pos.y;
            if (XLevel.LevelMapData.MapType == 0)
            {
                QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(pos);

                if (element == null || !element.IsValid())
                {
                    Debug.LogError("掉落物必须种在格子上");
                    return;
                }
                height = element.pos.y;
            }
            else if (XLevel.LevelMapData.MapType == 1)
            {
                height = XLevel.LevelMapData.UniqueHeight;
            }
            if (root != null)
            {
                GetDoodadInfo();

                if (doodadPrefab != null)
                {
                    GameObject go = GameObject.Instantiate(doodadPrefab);

                    go.transform.parent = root.transform;

                    Vector3 StickGroundPos = new Vector3(pos.x, height, pos.z);
                    go.transform.position = StickGroundPos;
                }
            }
            else
            {
                Root.ShowNotification(new GUIContent("CacheWaveRoot = NULL"));
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            GetDoodadInfo();
            for(var i=0;i<HostData.spawnDatas.Count;i++)
            {
                GameObject go = GameObject.Instantiate(doodadPrefab, root.transform);
                go.transform.position = HostData.spawnDatas[i].position;
                go.transform.rotation = Quaternion.Euler(0, HostData.spawnDatas[i].rotation, 0);
            }
        }

        public override void ConvenientSave()
        {
            base.BeforeSave();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            HostData.spawnDatas.Clear();
            if(root!=null)
            {
                for(var i=0;i<root.transform.childCount;i++)
                {
                    var child = root.transform.GetChild(i);
                    LevelWaveSpawnData spawnData = new LevelWaveSpawnData();

                    Vector3 editorPos = child.position;
                    QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(editorPos + new Vector3(0, 3.0f, 0));
                    if (element == null || !element.IsValid()) continue;
                    editorPos = new Vector3(editorPos.x, element.pos.y, editorPos.z);
                    child.position = editorPos;
                    spawnData.position = editorPos;
                    spawnData.rotation = child.rotation.eulerAngles.y;
                    spawnData.height = 0;

                    HostData.spawnDatas.Add(spawnData);
                }
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (root != null && root.transform.childCount > 0)
            {
                Vector3 viewCenter = Vector3.zero;

                for (int i = 0; i < root.transform.childCount; ++i)
                {
                    viewCenter += root.transform.GetChild(i).transform.position;
                }

                viewCenter = viewCenter / root.transform.childCount;

                Quaternion rotation = Quaternion.Euler(45, 0, 0);

                SceneView.lastActiveSceneView.LookAtDirect(viewCenter, rotation, 20);
            }
        }
    }
}
