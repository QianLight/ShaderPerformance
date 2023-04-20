using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XEditor;
using TableEditor;
using System.Linq;
using System.Text;

namespace LevelEditor
{
    [Flags]
    public enum InputSlot
    {
        普通攻击 = 1,
        闪避 = 1<<1,
        技能1 = 1<<2,
        技能2 = 1<<3,
        技能3 = 1<<4,
        技能4 = 1<<5,
        Dash_Virtual_Slot = 1<<6,
        Skill_6_Slot = 1<<7,
        Skill_7_Slot = 1<<8,
        Normal_Virtual_Slot = 1<<9,
    }

    public enum ApplyEntityType
    {
        Monster=0
    }

    public enum CheckBuffType
    {
        Monster=0,
        Player=1,
        MultiPlayer=2
    }

    public enum State
    {        
        Off=0,
        On = 1
    }

    public enum EntityType
    {
        Normal=0,
        Partner=1
    }

    public enum WallType
    {
        normal = 0,
        boss = 1,
        player = 2,
        sTod = 3,
    }

    public enum LevelType
    {
        NormalLevel=0,
        SeaEvent=1,
        IslandEvent=2
    }

    class LevelHelper
    {
        /// <summary>
        /// 以第一个选中的墙为基准对齐两个墙
        /// </summary>
        [MenuItem("GameObject/Level/CombineWall",priority =0)]       
        public static void CombineWall()
        {
            var walls = Selection.gameObjects;
            if(walls.Length!=2)
            {
                EditorUtility.DisplayDialog("LevelHint", "需要选中两个dummywall","确定");
                return;
            }
            if(!walls[0].TryGetComponent<XDummyWall>(out var wall1)||!walls[1].TryGetComponent<XDummyWall>(out var wall2))
            {
                EditorUtility.DisplayDialog("LevelHint", "选中物体需要带有XDummyWall脚本", "确定");
                return;
            }
            if(!wall1.TryGetComponent<BoxCollider>(out var box1)||!wall2.TryGetComponent<BoxCollider>(out var box2))
            {
                EditorUtility.DisplayDialog("LevelHint", "选中的墙没有boxcollider组件", "确定");
                return;
            }
            var editor = LevelWallEditor.InitWallEditor();
            editor.InitData(wall1, wall2);
        }

        private static string[] nodeConfig1 = new string[] { "NodeCmd", "CustomNodeName",
            "NodeType", "Condition1", "Condition2", "Var_Float","Var_String","Var_Vector" };
        private static string[] nodeConfig2 = new string[] {"节点Cmd","自定义节点名字",
                "节点类型（0=客户端节点 1=服务端节点 2=双端节点）","小重连重发条件(0=总是不重发 1=总是重发 其他编辑根据变量名和值编辑条件)",
                "大重连重发条件(0=总是不重发 1=总是重发 其他编辑根据变量名和值编辑条件)",
                "该节点float变量名（自动生成 勿修改 新增或修改节点时此配置需要重新生成）",
                "该节点string变量名",
                "该节点vector变量名"};

        public static void GenerateConfig()
        {
            TableFullData tableData = new TableFullData();
            TableReader.ReadTable(Application.dataPath + "/Editor/Blueprint/LevelEditor/LevelNodeConfig.txt", ref tableData);
            tableData.nameList = nodeConfig1.ToList();
            tableData.noteList = nodeConfig2.ToList();
            var cmds = Enum.GetValues(typeof(LevelScriptCmd));
            var it = cmds.GetEnumerator();
            while(it.MoveNext())
            {
                var cmd = (LevelScriptCmd)it.Current;
                if(cmd!=LevelScriptCmd.Level_Cmd_Custom)
                {
                    if (LSNodeDescBase.CreateNodeDesc(cmd, null) == null)
                        continue;
                    TableData dataLine=tableData.dataList.Find(d => d.valueList[0] == cmd.ToString());
                    if (dataLine==null)
                    {
                        dataLine = new TableData();
                        dataLine.valueList.Add(cmd.ToString());
                        dataLine.valueList.Add(string.Empty);
                        dataLine.valueList.Add("0");
                        dataLine.valueList.Add("0");
                        dataLine.valueList.Add("0");
                        dataLine.valueList.Add(string.Empty);
                        dataLine.valueList.Add(string.Empty);
                        dataLine.valueList.Add(string.Empty);
                        tableData.dataList.Add(dataLine);
                    }
                    var dummy = LSNodeDescBase.CreateNodeDesc(cmd, null);
                    if (dummy == null)
                        continue;
                    var type = dummy.GetType();
                    var f_f = type.GetField("desc_f", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if(f_f!=null)
                    {
                        var desc_f = (string[])f_f.GetValue(null);
                        StringBuilder fSb = new StringBuilder();
                        foreach(var s in desc_f)
                        {
                            fSb.Append(s);
                            fSb.Append('|');
                        }
                        dataLine.valueList[5] = fSb.ToString().TrimEnd('|');
                    }
                    var f_s = type.GetField("desc_s", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (f_s != null)
                    {
                        var desc_s = (string[])f_s.GetValue(null);
                        StringBuilder sSb = new StringBuilder();
                        foreach (var s in desc_s)
                        {
                            sSb.Append(s);
                            sSb.Append('|');
                        }
                        dataLine.valueList[6] = sSb.ToString().TrimEnd('|');
                    }
                    var f_v = type.GetField("desc_v", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (f_v != null)
                    {
                        var desc_v = (string[])f_v.GetValue(null);
                        StringBuilder vSb = new StringBuilder();
                        foreach (var s in desc_v)
                        {
                            vSb.Append(s);
                            vSb.Append('|');
                        }
                        dataLine.valueList[7] = vSb.ToString().TrimEnd('|');
                    }                    
                }
            }
            foreach(var node in dList.CustomDefineNodes)
            {
                TableData dataLine = tableData.dataList.Find(d => d.valueList[1] == node.name);
                if(dataLine==null)
                {
                    dataLine = new TableData();
                    dataLine.valueList.Add(LevelScriptCmd.Level_Cmd_Custom.ToString());
                    dataLine.valueList.Add(node.name);
                    dataLine.valueList.Add("0");
                    dataLine.valueList.Add("0");
                    dataLine.valueList.Add("0");
                    dataLine.valueList.Add(string.Empty);
                    dataLine.valueList.Add(string.Empty);
                    dataLine.valueList.Add(string.Empty);
                    tableData.dataList.Add(dataLine);
                }
                StringBuilder fSb = new StringBuilder(), sSb = new StringBuilder(), vSb = new StringBuilder();
                int idx_f = 0, idx_s = 0, idx_v = 0;
                for(var i=0;i<node.type.Count;i++)
                {
                    switch(node.type[i])
                    {
                        case "float":
                        case "enumflag":
                            fSb.Append(node.desc[idx_f]);
                            fSb.Append('|');
                            idx_f++;
                            break;
                        case "vector":
                            vSb.Append(node.desc[idx_v]);
                            vSb.Append('|');
                            idx_v++;
                            break;
                        case "string":
                        case "enumstring":
                            sSb.Append(node.desc[idx_s]);
                            sSb.Append('|');
                            idx_s++;
                            break;
                    }
                }
                dataLine.valueList[5] = fSb.ToString().TrimEnd('|');
                dataLine.valueList[6] = sSb.ToString().TrimEnd('|');
                dataLine.valueList[7] = vSb.ToString().TrimEnd('|');
            }
            TableReader.WriteTable(Application.dataPath+"/Editor/Blueprint/LevelEditor/LevelNodeConfig.txt", tableData);
        }

        public static Vector4 InValidVector = new Vector4(-1, -1, -1, -1);
        public static bool GetWallColliderData(GameObject go, WallType wallType, List<LevelWallData> levelWallDatas)
        {
            if (go == null) return false;

            LevelWallData data = new LevelWallData();
            Transform t = go.transform;
            XSpawnWall spawnWall = t.GetComponent<XSpawnWall>();
            XTransferWall transferWall = t.GetComponent<XTransferWall>();
            XDummyWall dummyWall = t.GetComponent<XDummyWall>();
            IAmDynamicWall staticWall = t.GetComponent<IAmDynamicWall>();

            if ((spawnWall == null&& dummyWall == null&&staticWall==null) || transferWall != null ) return false;

            Vector3 pos = t.position;
            float r = t.rotation.eulerAngles.y;

            data.name = go.name;
            data.position = pos;
            data.rotation = r;
            data.forward = t.forward;
            data.on = go.activeInHierarchy;
            data.wallType = (int)wallType;

            if(spawnWall!=null)
            {
                data.isTrigger = true;
                data.exString = spawnWall.exString;
                data.once = spawnWall.TriggerType == XSpawnWall.etrigger_type.once;
                data.PassFlag.Clear();
                //触发墙玩家和非玩家都是true
                data.PassFlag.Add(new LevelWallPassFlag() { Forward = true, Backward = true });
                data.PassFlag.Add(new LevelWallPassFlag() { Forward = true, Backward = true });
                data.target = spawnWall.target;
                data.aiID = spawnWall.aiID;
            }
            else if(dummyWall!=null)
            {
                data.SideLimit = dummyWall.sideLimit;
                data.PassFlag.Clear();
                data.PassFlag.Add(new LevelWallPassFlag() { Forward = dummyWall.playerFlag.Forward, Backward = dummyWall.playerFlag.Backward });
                data.PassFlag.Add(new LevelWallPassFlag() { Forward = dummyWall.monsterFlag.Forward, Backward = dummyWall.monsterFlag.Backward });
                data.permanentFx = dummyWall.permanentFx;
                if(!string.IsNullOrEmpty(dummyWall.buffIDList))
                {
                    var buffStr = dummyWall.buffIDList.Split('|');
                    for (var i = 0; i < buffStr.Length; i++)
                    {
                        data.buffIDList.Add(uint.Parse(buffStr[i]));
                    }
                }
                if(!string.IsNullOrEmpty(dummyWall.typeList))
                {
                    var typeStr = dummyWall.typeList.Split('|');
                    for (var i = 0; i < typeStr.Length; i++)
                    {
                        data.typeList.Add(uint.Parse(typeStr[i]));
                    }
                }               
            }
            else
            {
                data.PassFlag.Clear();
                data.PassFlag.Add(new LevelWallPassFlag() { Forward = false, Backward = false });
                data.PassFlag.Add(new LevelWallPassFlag() { Forward = false, Backward = false });
                data.SideLimit = false;
                data.permanentFx = false;
            }
            

            BoxCollider c = t.GetComponent<BoxCollider>();
            if (c != null)
            {
                data.type = 0;
                data.size = new Vector3(c.size.x * t.localScale.x, t.position.y + c.size.y / 2 * t.localScale.y, c.size.z * t.localScale.z);

                if (staticWall != null && c.size.z * t.localScale.z > 2f)
                {
                    Vector3 leftdown = new Vector3(-c.size.x / 2, -c.size.y / 2, -c.size.z / 2);
                    Vector3 rightup = new Vector3(c.size.x / 2, c.size.y / 2, c.size.z / 2);
                    data.leftdown = t.localToWorldMatrix * new Vector4(leftdown.x, leftdown.y, leftdown.z, 1);
                    data.rightup = t.localToWorldMatrix * new Vector4(rightup.x, rightup.y, rightup.z, 1);

                    levelWallDatas.Add(data);

                    LevelWallData anotherWall = data.ShallowCopy();
                    anotherWall.name = anotherWall.name + "(x)";
                    leftdown = new Vector3(-c.size.x / 2, -c.size.y / 2, c.size.z / 2);
                    rightup = new Vector3(c.size.x / 2, c.size.y / 2, -c.size.z / 2);
                    anotherWall.leftdown = t.localToWorldMatrix * new Vector4(leftdown.x, leftdown.y, leftdown.z, 1);
                    anotherWall.rightup = t.localToWorldMatrix * new Vector4(rightup.x, rightup.y, rightup.z, 1);

                    levelWallDatas.Add(anotherWall);

                    return true;
                }


                {
                    float h = c.size.y * 0.5f * t.localScale.y;
                    Vector3 half = Vector3.right * c.size.x * 0.5f;
                    Vector3 leftdown = c.center - half;
                    Vector3 rightup = c.center + half;
                    

                    data.leftdown = t.localToWorldMatrix * leftdown;
                    data.leftdown += t.position;
                    data.leftdown.y = t.position.y - h;
                    data.rightup = t.localToWorldMatrix * rightup;
                    data.rightup += t.position;
                    data.rightup.y = t.position.y + h;

                    //data.leftdown = t.localToWorldMatrix * new Vector4(leftdown.x, leftdown.y, leftdown.z, 1);
                    //data.rightup = t.localToWorldMatrix * new Vector4(rightup.x, rightup.y, rightup.z, 1);

                    levelWallDatas.Add(data);
                    return true;
                }
            }
            else
            {
                SphereCollider sc = t.GetComponent<SphereCollider>();
                if(sc != null)
                {
                    data.type = 1;
                    data.size = new Vector3(sc.radius, 0, 0);
                    levelWallDatas.Add(data);
                    return true;
                }
            }

            return false;
        }

        public static LevelCustomDefineData dList;
        public static void ReadLevelCustomConfig()
        {
            dList = new LevelCustomDefineData();

            // "E:/OP/res/OPProject/Assets/Editor/Blueprint/LevelEditor/Data/CustomNode.cfg"
            dList = DataIO.DeserializeData<LevelCustomDefineData>(Application.dataPath + "/Editor/Blueprint/LevelEditor/Data/CustomNode.cfg");
        }

        public static TableFullData nodeConfig = new TableFullData();

        public static void ReadLevelNodeConfig()
        {
            TableReader.ReadTable(Application.dataPath + "/Editor/Blueprint/LevelEditor/LevelNodeConfig.txt", ref nodeConfig);
        }

        public static LevelCustomNodeDefineData GetCustomNodeDefineData(string name)
        {
            for(int i = 0; i < dList.CustomDefineNodes.Count; ++i)
            {
                if (dList.CustomDefineNodes[i].name == name)
                    return dList.CustomDefineNodes[i];
            }

            return null;
        }
    }
}
