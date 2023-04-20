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
    struct MonsterInfo
    {
        public GameObject prefab;
        public float flyHeight;

    }
    class LevelWaveNode : LevelBaseNode<LevelWaveData>
    {
        private int MonsterInScene = 0;
        private Transform CacheWaveRoot;

        private LevelSpawnType cacheType = LevelSpawnType.Spawn_Source_Monster;
        private int factor = 10000;

        MonsterInfo _MonsterInfo = default(MonsterInfo);

        public override void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header7";
            nodeEditorData.Tag = "Wave";
            nodeEditorData.BackgroundText = "";
            CanbeFold = true;

            BluePrintPin pinRoundID = new BluePrintValuedPin(this, 3, "Round", PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(pinRoundID);

            BluePrintPin pinHp = new BluePrintValuedPin(this, 2, "Hp", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinHp);

            BluePrintPin pinCount = new BluePrintValuedPin(this, 1, "Alive", PinType.Data, PinStream.Out, VariantType.Var_Float);
            AddPin(pinCount);

            BluePrintPin pinID =   new BluePrintValuedPin(this, 4, "ID", PinType.Data, PinStream.Out, VariantType.Var_UINT64);
            AddPin(pinID);

            BluePrintPin pinPosIn = new BluePrintValuedPin(this, 5, "Pos", PinType.Data, PinStream.In, VariantType.Var_Vector3);
            AddPin(pinPosIn);

            BluePrintPin pinPosOut = new BluePrintValuedPin(this, 6, "Pos", PinType.Data, PinStream.Out, VariantType.Var_Vector3);
            AddPin(pinPosOut);

            BluePrintValuedPin offsetPin = new BluePrintValuedPin(this, 7, "Offset", PinType.Data, PinStream.In, VariantType.Var_Vector3);
            AddPin(offsetPin);
        }

        public override void UnInit()
        {
            DestroyIllstrationResouce();
            base.UnInit();
        }

        private void DestroyIllstrationResouce()
        {
            if (CacheWaveRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(CacheWaveRoot.gameObject);
                CacheWaveRoot = null;
            }
        }

        public override List<LevelWaveData> GetDataList(LevelGraphData data)
        {
            return data.WaveData;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("WaveID:", new GUILayoutOption[] { GUILayout.Width(150f) });
            EditorGUILayout.LabelField(HostData.WaveID.ToString(), new GUILayoutOption[] { GUILayout.Width(120f) });
            EditorGUILayout.EndHorizontal();

            cacheType = (LevelSpawnType)EditorGUILayout.EnumPopup("SpawnType", cacheType, GUILayout.Width(275f));
            HostData.SpawnType = (int)cacheType;

            if(cacheType == LevelSpawnType.Spawn_Source_Random)
            {
                HostData.SpawnID = EditorGUILayout.IntField("RandomID: ", HostData.SpawnID, GUILayout.Width(275f));
            }
            else
            {
                HostData.SpawnID = EditorGUILayout.IntField("SpawnID: ", HostData.SpawnID, GUILayout.Width(275f));
            }
            

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("TotalCount:", new GUILayoutOption[] { GUILayout.Width(150f) });
            EditorGUILayout.LabelField(MonsterInScene.ToString(), new GUILayoutOption[] { GUILayout.Width(120f) });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            HostData.Interval = EditorGUILayout.FloatField("Interval: ", HostData.Interval, GUILayout.Width(275f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            HostData.WaveRange = EditorGUILayout.FloatField("WaveRange: ", HostData.WaveRange, GUILayout.Width(275));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AIID", GUILayout.Width(100f));
            HostData.aiID = EditorGUILayout.IntField(HostData.aiID, GUILayout.Width(100f));
            //if(GUILayout.Button("Modify",GUILayout.Width(50f)))
            //{
            //    TableFullData tableData = new TableFullData();
            //    TableReader.ReadTableByFileStream(Application.dataPath + "/Table/XEntityStatistics.txt", ref tableData);
            //    int aiIndex = tableData.nameList.FindIndex(n => n == "AIID");
            //    int idIndex = tableData.nameList.FindIndex(n => n == "ID");
            //    var data = tableData.dataList.Find(d => d.valueList[idIndex] == HostData.SpawnID.ToString());
            //    data.valueList[aiIndex] = aiID.ToString();
            //    TableReader.WriteTable(Application.dataPath + "/Table/XEntityStatistics.txt", tableData);
            //}
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            //HostData.Fly = EditorGUILayout.Toggle("Fly: ", HostData.Fly, GUILayout.Width(275f));
            //EditorGUILayout.EndHorizontal();

            HostData.ignoreGrid = EditorGUILayout.Toggle("ignoreGrid", HostData.ignoreGrid, GUILayout.Width(50f));
        }

        public void Update()
        {
            if (CacheWaveRoot != null)
            {
                MonsterInScene = CacheWaveRoot.childCount;
            }
        }

        public override void OnAdded()
        {
            base.OnAdded();
            HostData.WaveID = nodeEditorData.NodeID + (Root.GraphID - 1) * 10000;
            nodeEditorData.Tag = "Wave " + HostData.WaveID;

            //HostData.SpawnsInfo = new List<LevelWaveSpawnData>();
            //MonsterInScene = HostData.SpawnsInfo.Count;

            GameObject go = new GameObject("SG" + Root.GraphID.ToString() + "_Wave" + HostData.WaveID + "_" + HostData.SpawnID);
            CacheWaveRoot = go.transform;
            go.transform.position = Vector3.zero;
        }

        public override void OnDeleted()
        {
            base.OnDeleted();
            DestroyIllstrationResouce();
        }

        public override string HeaderText()
        {
            string str = HostData.NodeID.ToString();
            if (HostData.SpawnID > 0)
            {
                XEntityStatistics.RowData monsterInfo = LevelEditorTableData.MonsterInfo.GetByID((uint)HostData.SpawnID);
                if(monsterInfo != null)
                    str += (" " + monsterInfo.Name);
            }
            else if(HostData.SpawnType == (int)LevelSpawnType.Spawn_Source_Random)
            {
                str += ("Random");
            }
 
            return str;
        }

        public void ReloadMonster()
        {
            for (var i = CacheWaveRoot.childCount-1; i>=0;i--)
            {
                GameObject.DestroyImmediate(CacheWaveRoot.GetChild(i).gameObject);
            }
            if (_MonsterInfo.prefab == null)
                _MonsterInfo = GetMonsterInfo();
            var offset = (Root.editorWindow as LevelEditor).m_Data.m_Generator.offset;
            for (var i=0;i<HostData.SpawnsInfo.Count;i++)
            {
                var pos = HostData.SpawnsInfo[i].position;
                GameObject go = GameObject.Instantiate(_MonsterInfo.prefab);

                go.transform.parent = CacheWaveRoot;

                Vector3 StickGroundPos = new Vector3(pos.x+offset.x, pos.y + _MonsterInfo.flyHeight, pos.z+offset.z);
                go.transform.position = StickGroundPos;
                go.transform.rotation= Quaternion.Euler(0, HostData.SpawnsInfo[i].rotation, 0);
            }
        }


        public void AddMonsterAtPos(Vector3 pos)
        {
            float height = pos.y;
            if (!HostData.ignoreGrid)
            {
                if(XLevel.LevelMapData.MapType==0)
                {
                    QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(pos);

                    if (element == null || !element.IsValid())
                    {
                        Debug.LogError("怪物必须种在格子上");
                        return;
                    }
                    height = element.pos.y;
                }
                else if(XLevel.LevelMapData.MapType==1)
                {
                    height = XLevel.LevelMapData.UniqueHeight;
                }
            }

            if (CacheWaveRoot != null)
            {
                if (_MonsterInfo.prefab == null)
                    _MonsterInfo = GetMonsterInfo();

                if (_MonsterInfo.prefab != null)
                {
                    GameObject go = GameObject.Instantiate(_MonsterInfo.prefab);

                    go.transform.parent = CacheWaveRoot;

                    Vector3 StickGroundPos = new Vector3(pos.x, height + _MonsterInfo.flyHeight, pos.z);
                    go.transform.position = StickGroundPos;
                }
            }
            else
            {
                Root.ShowNotification(new GUIContent("CacheWaveRoot = NULL"));
            }
        }

        public void RelocateMonster(float delta)
        {

            if (CacheWaveRoot != null)
            {
                for(int i = 0; i < CacheWaveRoot.childCount; ++i)
                {
                    Transform monster = CacheWaveRoot.GetChild(i);
                    Vector3 oldPos = monster.position;

                    QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(oldPos + new Vector3(0, delta, 0));

                    float adjustHeight = 0;

                    if (_MonsterInfo.flyHeight > 0)
                        adjustHeight = HostData.SpawnsInfo[i].height;

                    if (element != null && element.IsValid())
                        monster.position = new Vector3(oldPos.x, element.pos.y + adjustHeight, oldPos.z);
                }
            }
        }

        protected MonsterInfo GetMonsterInfo()
        {
            MonsterInfo mi = default(MonsterInfo);

            LevelSpawnType t = (LevelSpawnType)HostData.SpawnType;
            switch (t)
            {
                case LevelSpawnType.Spawn_Source_Monster:
                    if (HostData.SpawnID > 0)
                    {
                        XEntityStatistics.RowData monsterInfo = LevelEditorTableData.MonsterInfo.GetByID((uint)HostData.SpawnID);
                        if (monsterInfo != null)
                        {
                            XEntityPresentation.RowData d = LevelEditorTableData.MonsterPresent.GetByPresentID(monsterInfo.PresentID);
                            if (d != null)
                            {
                                mi.prefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Prefabs/" + d.Prefab);
                                mi.flyHeight = monsterInfo.FloatHeight == null? 0 : monsterInfo.FloatHeight[0];
                            }
                        }
                    }
                    break;
                case LevelSpawnType.Spawn_Source_Random:
                    {
                        GameObject asset = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Editor Default Resources/Tmp/dabai.prefab", typeof(GameObject));
                        mi.prefab = GameObject.Instantiate(asset);
                        mi.flyHeight = 0;

                    }
                    break;
            }

            if (mi.prefab == null)
            {
                Debug.LogError("Prefab == NULL! staticID = " + HostData.SpawnID);
            }

            //TableFullData tableData = new TableFullData();
            //TableReader.ReadTableByFileStream(Application.dataPath + "/Table/XEntityStatistics.txt", ref tableData);
            //int aiIndex = tableData.nameList.FindIndex(n => n == "AIID");
            //int idIndex = tableData.nameList.FindIndex(n => n == "ID");
            //var data = tableData.dataList.Find(d => d.valueList[idIndex] == HostData.SpawnID.ToString());
            //if (!string.IsNullOrEmpty(data.valueList[aiIndex]))
            //    this.aiID = uint.Parse(data.valueList[aiIndex]);
            //else
            //    this.aiID = 0;

            return mi;
        }

        public override void ConvenientSave()
        {
            base.BeforeSave();
        }

        public override void BeforeSave()
        {
            base.BeforeSave();
            HostData.SpawnsInfo = new List<LevelWaveSpawnData>();
            Vector3 originPoint = Vector3.zero;
            Vector3 shipOffset = (Root.editorWindow as LevelEditor).m_Data.m_Generator.offset;
            if((LevelType)LevelEditor.Instance.levelType==LevelType.SeaEvent)
            {
                for (int i = 0; i < CacheWaveRoot.childCount; ++i)
                {
                    var childPos = CacheWaveRoot.GetChild(i).transform.position;
                    originPoint += childPos;                  
                }
                originPoint.y = 0;
                originPoint /= CacheWaveRoot.childCount;
            }
            if (CacheWaveRoot != null)
            {               
                for(int i = 0; i < CacheWaveRoot.childCount; ++i)
                {
                    LevelWaveSpawnData spawnData = new LevelWaveSpawnData();

                    Vector3 editorPos = CacheWaveRoot.GetChild(i).transform.position;
                    QuadTreeElement element = (Root.editorWindow as LevelEditor).m_Data.QueryGrid(editorPos + new Vector3(0, 3.0f, 0));
                    if (!HostData.ignoreGrid&&(element == null || !element.IsValid())) continue;

                    if (_MonsterInfo.flyHeight == 0)
                    {
                        if(element!=null)
                            editorPos = new Vector3(editorPos.x, element.pos.y, editorPos.z);
                        CacheWaveRoot.GetChild(i).transform.position = editorPos;
                    }
                    spawnData.position = editorPos-originPoint-shipOffset;
                    spawnData.rotation = CacheWaveRoot.GetChild(i).transform.rotation.eulerAngles.y;
                    spawnData.height = _MonsterInfo.flyHeight;

                    HostData.SpawnsInfo.Add(spawnData);
                }
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();

            for(int i = 0; i < HostData.SpawnsInfo.Count; ++i)
            {
                if (_MonsterInfo.prefab == null)
                    _MonsterInfo = GetMonsterInfo();

                if (_MonsterInfo.prefab != null)
                {
                    GameObject go = GameObject.Instantiate(_MonsterInfo.prefab);

                    go.transform.parent = CacheWaveRoot;
                    go.transform.position = HostData.SpawnsInfo[i].position;
                    go.transform.rotation = Quaternion.Euler(0, HostData.SpawnsInfo[i].rotation, 0);
                }                    
            }
            cacheType = (LevelSpawnType)HostData.SpawnType;
        }

        public override void OnSelected()
        {
            //if (HostData.SpawnsInfo != null && HostData.SpawnsInfo.Count > 0)
            if(CacheWaveRoot != null && CacheWaveRoot.childCount > 0)
            {
                Vector3 viewCenter = Vector3.zero;

                for (int i = 0; i < CacheWaveRoot.childCount; ++i)
                {
                    viewCenter += CacheWaveRoot.GetChild(i).transform.position;
                }

                viewCenter = viewCenter / CacheWaveRoot.childCount;

                Quaternion rotation = Quaternion.Euler(45, 0, 0);

                SceneView.lastActiveSceneView.LookAtDirect(viewCenter, rotation, 20);
            }

            if (CacheWaveRoot != null)
            {
                for (int i = 0; i < CacheWaveRoot.childCount; ++i)
                {
                    GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Effects/Prefabs/Generic/Ty_jiantou.prefab", typeof(GameObject));
                    GameObject go = GameObject.Instantiate(prefab);

                    go.name = "__Arrow__";
                    go.transform.parent = CacheWaveRoot.GetChild(i);
                    go.transform.localPosition = new Vector3(0, 2, 0);
                }
                EditorGUIUtility.PingObject(CacheWaveRoot.gameObject);
                Selection.activeGameObject = CacheWaveRoot.gameObject;
            }
        }

        public override void OnUnselected()
        {
            if (CacheWaveRoot != null)
            {
                for (int i = 0; i < CacheWaveRoot.childCount; ++i)
                {
                    Transform t = CacheWaveRoot.GetChild(i);

                    if(t.childCount > 0)
                    {
                        //UnityEngine.Object.DestroyImmediate(t.GetChild(0).gameObject);
                        int index = -1;
                        for(int j = 0; j < t.childCount; ++j)
                        {
                            if(t.GetChild(j).gameObject.name == "__Arrow__")
                            {
                                index = j;
                                break;
                            }
                        }

                        if(index >= 0) UnityEngine.Object.DestroyImmediate(t.GetChild(index).gameObject);
                    }
                }
            }
        }

        #region simulation
        LevelRTWaveNode waveData;

        public override void OnEnterSimulation()
        {
            DestroyIllstrationResouce();

            base.OnEnterSimulation();
            waveData = RuntimeData as LevelRTWaveNode;

            //for (int i = 0; i < waveData.GetData().SpawnsInfo.Count; ++i)
            //    AddSimulationButton(i);

            //if (waveData.GetData().SpawnsInfo.Count == 1)
            //    AddSimulationSlider();
        }

        public override void OnEndSimulation()
        {
            foreach (BluePrintPin pin in pinList)
            {
                pin.OnEndSimulation();
            }

            waveData = null;
            base.OnEndSimulation();
        }

        public override bool DrawNodeExtra(Rect boxRect, ref Rect RectWithExtra)
        {
            if(LevelEditor.state == LEVEL_EDITOR_STATE.simulation_mode && waveData != null)
            {
                int round = GetTotalRound();

                RectWithExtra = boxRect;

                for (int r = 0; r < round; ++r)
                {
                    float startHeight = boxRect.y + boxRect.height + r * 30 + 7;
                    Rect debugRect = new Rect(boxRect.x, startHeight-7, boxRect.width, 30);
                    DrawTool.DrawStretchBox(debugRect, BlueprintStyles.NodeBackground, 12f);

                    int roundMonsterCount = waveData.GetData().SpawnsInfo.Count;

                    if (roundMonsterCount > 1)
                    {
                        for(int i = 0; i < roundMonsterCount; ++i)
                        {
                            if (r * roundMonsterCount + i == buttonList.Count)
                            {
                                AddSimulationButton(r, i);
                            }
                            buttonList[r * roundMonsterCount + i].Bounds = new Rect(boxRect.x + 8 + 16 * i, startHeight, 16, 16);
                            buttonList[r * roundMonsterCount + i].ImageName = GetButtonImageName(r, i);
                            buttonList[r * roundMonsterCount + i].Draw();
                        }
                        //RectWithExtra = new Rect(boxRect.x, boxRect.y, boxRect.width, boxRect.height + 30 *(r+1));
                    }
                    else if (roundMonsterCount == 1)
                    {
                        AddSimulationButton(r, 0);
                        buttonList[r].Bounds = new Rect(boxRect.x + 8, startHeight, 16, 16);
                        buttonList[r].ImageName = GetButtonImageName(r, 0);
                        buttonList[r].Draw();

                        AddSimulationSlider(r);
                        sliderList[r].Bounds = new Rect(boxRect.x + 28, startHeight, boxRect.width - 38, 30);
                        float hp = waveData.GetMonsterInfo(r, 0);

                        if(hp > 0)
                            hp = sliderList[r].Draw(hp);
                        else
                            sliderList[r].Draw(0);

                        waveData.SetMonsterInfo(r, 0, hp);
                    }
                }

                RectWithExtra = new Rect(boxRect.x, boxRect.y, boxRect.width, boxRect.height + 30 * round);

                return true;
            }

            return false;
        }

        protected void AddSimulationButton(int round, int index)
        {
            BlueprinButton bpButton = new BlueprinButton(this);
            AddButton(bpButton);
            bpButton.Param = round * factor + index;
            bpButton.RegisterClickEvent(ButtonClickCb);
        }

        protected void AddSimulationSlider(int round)
        {
            BlueprintSlider bpSlider = new BlueprintSlider(this);
            AddSlider(bpSlider);
        }

        private string GetButtonImageName(int round, int index)
        {
            float monsterHP = waveData.GetMonsterInfo(round, index);

            if (monsterHP > 0 && monsterHP <= 100)
                return "BluePrint/MonsterIndicateGreen";
            else if (monsterHP == 0)
                return "BluePrint/MonsterIndicateRed";
            else
                return "BluePrint/MonsterIndicateGray";
        }

        protected void ButtonClickCb(object o)
        {
            int i = (int)o;

            int round = i / factor;
            int index = i - round * factor;

            float monsterHP = waveData.GetMonsterInfo(round, index);

            if(monsterHP > 0)
                waveData.MakeMonsterDie(round, index);
        }

        private int GetTotalRound()
        {
            return waveData.GetTotalRound();
        }
        #endregion

        public override void CheckError()
        {
            base.CheckError();

            BlueprintNodeErrorInfo error = nodeErrorInfo;
            //error.nodeID = nodeEditorData.NodeID;

            if (HostData.SpawnID == 0 && HostData.SpawnType != (int)LevelSpawnType.Spawn_Source_Random)
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "怪物ID不能为0", null));
            }

            if (CacheWaveRoot != null && CacheWaveRoot.childCount == 0)
            {
                error.ErrorDataList.Add(new BlueprintErrorData(BlueprintErrorCode.NodeDataError, "怪物" + HostData.SpawnID + "个数为0", null));
            }
        }
    }
}
