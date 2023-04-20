using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BluePrint;
using CFUtilPoolLib;
using XEditor;

namespace LevelEditor
{
    class LevelRTWaveNode : BlueprintRuntimeDataNode<LevelWaveData>
    {
        public int preCount = 0;
        public int curCount = 0;

        private Transform CacheWaveRoot = null;

        private List<List<float>> monsterDynamicInfo = new List<List<float>>(); // float: 0 -> 100%

        BlueprintRuntimeValuedPin pinCount;
        BlueprintRuntimeValuedPin pinHp;
        BlueprintRuntimeValuedPin pinRound;

        BlueprintRuntimeValuedPin pinID;

        BlueprintRuntimeValuedPin pinPosIn;
        BlueprintRuntimeValuedPin pinPosOut;

        GameObject _Prefab = null;

        //public LevelRTWaveNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(LevelWaveData data, bool AutoStreamPin = true)
        {
            base.Init(data);

            pinCount = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, VariantType.Var_Float, false);
            pinCount.SetValueSource(GetCount);
            AddPin(pinCount);

            pinHp = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.Out, VariantType.Var_Float, false);
            pinHp.SetValueSource(GetHp);
            AddPin(pinHp);

            pinRound = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.In, VariantType.Var_Float);
            AddPin(pinRound);

            pinID = new BlueprintRuntimeValuedPin(this, 4, PinType.Data, PinStream.Out, VariantType.Var_UINT64, false);
            pinID.SetValueSource(GetID);
            AddPin(pinID);

            pinPosIn = new BlueprintRuntimeValuedPin(this, 5, PinType.Data, PinStream.In, VariantType.Var_Vector3);
            AddPin(pinPosIn);

            pinPosOut = new BlueprintRuntimeValuedPin(this, 6, PinType.Data, PinStream.Out, VariantType.Var_Vector3);
            pinPosOut.SetValueSource(GetPosition);
            AddPin(pinPosOut);

            monsterDynamicInfo.Clear();
        }

        public int GetTotalRound()
        {
            return monsterDynamicInfo.Count;
        }

        protected void ClearMonsterDynamicInfo()
        {
            for (int i = 0; i < monsterDynamicInfo.Count; ++i)
                monsterDynamicInfo[i].Clear();

            monsterDynamicInfo.Clear();
        }

        public override void UnInit()
        {
            ClearMonsterDynamicInfo();

            if (CacheWaveRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(CacheWaveRoot.gameObject);
                CacheWaveRoot = null;
            }

            base.UnInit();

        }

        public BPVariant GetPosition()
        {
            BPVariant position = new BPVariant();
            position.type = VariantType.Var_Vector3;

            BPVariant fRound = new BPVariant();
            pinRound.GetPinValue(ref fRound);

            if (fRound.type == VariantType.Var_Float)
            {
                int round = (int)fRound.val._float;

                if (round < monsterDynamicInfo.Count && round >= 0)
                {
                    if (monsterDynamicInfo[round].Count > 0)
                    {
                        position.val._vec3 = new Vector3(0, 0 ,0);
                    }
                }
            }


            return position;
        }

        public BPVariant GetID()
        {
            BPVariant monsterID = new BPVariant();
            monsterID.type = VariantType.Var_UINT64;
            monsterID.val._uint64 = 0;

            if(HostData.SpawnType == (int)LevelSpawnType.Spawn_Source_Monster)
                monsterID.val._uint64 =  (UInt64)HostData.SpawnID;
            else if(HostData.SpawnType == (int)LevelSpawnType.Spawn_Source_Random)
                 monsterID.val._uint64 = 0;

            return monsterID;
        } 
        public BPVariant GetCount()
        {
            BPVariant alive = new BPVariant();
            alive.type = VariantType.Var_Float;
            alive.val._float = float.MinValue;

            BPVariant fRound = new BPVariant();
            pinRound.GetPinValue(ref fRound);

            if(fRound.type == VariantType.Var_Float)
            {
                int round = (int)fRound.val._float;

                if (round < monsterDynamicInfo.Count && round >= 0)
                {
                    alive.val._float = 0;
                    for (int i = 0; i < monsterDynamicInfo[round].Count; ++i)
                    {
                        if (monsterDynamicInfo[round][i] > 0) alive.val._float++;
                    }

                    return alive;
                }
            }
 
            return alive;
        }

        public BPVariant GetHp()
        {
            BPVariant ret = new BPVariant();
            ret.type = VariantType.Var_Float;
            ret.val._float = float.MinValue;

            BPVariant fRound = new BPVariant();
            pinRound.GetPinValue(ref fRound);

            if (fRound.type == VariantType.Var_Float)
            {
                int round = (int)fRound.val._float;

                if (round < monsterDynamicInfo.Count && round >= 0)
                {
                    if (monsterDynamicInfo[round].Count > 0)
                    {
                        ret.val._float = monsterDynamicInfo[round][0];
                    }
                }
            }

            return ret;
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            monsterDynamicInfo.Add(new List<float>());

            Vector3 dynamicPosition = Vector3.zero;
            BPVariant inPos = new BPVariant();
            if(pinPosIn.GetPinValue(ref inPos))
                dynamicPosition = inPos.val._vec3;

            for (int i = 0; i < HostData.SpawnsInfo.Count; ++i)
            {
                Vector3 pos = dynamicPosition == Vector3.zero ? HostData.SpawnsInfo[i].position : dynamicPosition;
                SpawnMonsterAt(HostData.SpawnID, HostData.SpawnType, pos, HostData.SpawnsInfo[i].rotation, monsterDynamicInfo.Count - 1, i);
            }

            pinCount.dataOpen = true;
            pinHp.dataOpen = true;
            pinID.dataOpen = true;

            if (pinOut != null)
                pinOut.Active();
        }

        protected void SpawnMonsterAt(int monsterID, int type, Vector3 position, float rotation, int round, int index)
        {
            if(CacheWaveRoot == null)
            {
                GameObject root = new GameObject("Wave" + HostData.WaveID);
                CacheWaveRoot = root.transform;
                root.transform.position = Vector3.zero;
            }

            if (_Prefab == null)
                _Prefab = GetPrefab();

            if (_Prefab != null)
            {
                GameObject go = GameObject.Instantiate(_Prefab);

                go.name = "Monster" + monsterID + "_" + round + "_" + index;
                go.transform.parent = CacheWaveRoot;
                go.transform.position = position;
                go.transform.rotation = Quaternion.Euler(0, rotation, 0);

                monsterDynamicInfo[round].Add(100);
            }
                
        }

        protected GameObject GetPrefab()
        {
            GameObject prefab = null;
            if (HostData.SpawnID > 0)
            {
                LevelSpawnType t = (LevelSpawnType)HostData.SpawnType;
                switch (t)
                {
                    case LevelSpawnType.Spawn_Source_Monster:
                        XEntityStatistics.RowData monsterInfo = LevelEditorTableData.MonsterInfo.GetByID((uint)HostData.SpawnID);
                        if (monsterInfo != null)
                        {
                            prefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Prefabs/" + XAnimationLibrary.AssociatedAnimations(monsterInfo.PresentID).Prefab);
                        }
                        break;
                    case LevelSpawnType.Spawn_Source_Doodad:
                        prefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Effects/Perfab/Public/story_task_02_Clip02");
                        break;
                    //case LevelSpawnType.Spawn_Source_Gather:
                    //    uint presentID = XItemLibrary.GetGatherInfo(HostData.SpawnID).GoodsModelID;
                    //    prefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>("Prefabs/" + XAnimationLibrary.AssociatedAnimations(presentID).Prefab);
                    //    break;
                    case LevelSpawnType.Spawn_Source_Item:
                        prefab = XResourceHelper.LoadEditorResourceAtBundleRes<GameObject>(XItemLibrary.GetItemInfo((uint)HostData.SpawnID).DoodaFx);
                        break;
                }
            }

            return prefab;
        }

        public void MakeMonsterDie(int round, int index)
        {
            if (round >= 0 && round < monsterDynamicInfo.Count)
            {
                if (index >= 0 && index < monsterDynamicInfo[round].Count)
                {
                    monsterDynamicInfo[round][index] = 0;

                    GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Editor Default Resources/Tmp/death.prefab", typeof(GameObject));
                    GameObject go = GameObject.Instantiate(prefab);

                    go.transform.parent = CacheWaveRoot.GetChild(index);
                    go.transform.localPosition = Vector3.zero;
                }
            }
        }

        public float GetMonsterInfo(int round, int index)
        {
            if (round >= 0 && round < monsterDynamicInfo.Count)
            {
                if (index >= 0 && index < monsterDynamicInfo[round].Count)
                {
                    return monsterDynamicInfo[round][index];
                }
            }

            return float.MinValue;
        }

        public void SetMonsterInfo(int round, int index, float hp)
        {
            if (round >= 0 && round < monsterDynamicInfo.Count)
            {
                if (index >= 0 && index < monsterDynamicInfo[round].Count)
                {
                    monsterDynamicInfo[round][index] = hp;
                }
            }
        }

        public override void OnEndSimulation()
        {
            UnInit();
        }



    }
}
