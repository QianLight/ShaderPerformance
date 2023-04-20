using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.ComponentModel;

namespace LevelEditor
{
    public enum LevelSpawnType
    {
        Spawn_Source_Monster = 0,
        Spawn_Source_Player,
        Spawn_Source_Random,
        Spawn_Source_Doodad,
        Spawn_Source_Gather,
        Spawn_Source_Item,
    }

    [Serializable]
    public class LevelEditorData
    {
        [SerializeField]
        public string DynamicRoot;

        [SerializeField]
        public int Version;

        [SerializeField]
        public int LevelType;
        
        [SerializeField]
        public List<LevelGraphData> GraphDataList = new List<LevelGraphData>();

        [SerializeField]
        public List<LevelWallData> LevelWallData = new List<LevelWallData>();

        //[SerializeField]
        //public List<LevelWallData> BossWallData = new List<LevelWallData>();
    }

    [Serializable]
    public class LevelGraphData : BluePrintData
    {
        [SerializeField]
        public List<LevelWaveData> WaveData = new List<LevelWaveData>();

        [SerializeField]
        public List<LevelMonitorData> MonitorData = new List<LevelMonitorData>();

        [SerializeField]
        public List<LevelConnectionData> ConnectionData = new List<LevelConnectionData>();

        [SerializeField]
        public List<LevelExstringData> ExstringData = new List<LevelExstringData>();

        [SerializeField]
        public List<LevelRandomNodeData> RandomData = new List<LevelRandomNodeData>();

        [SerializeField]
        public List<LevelRecordData> RecordData = new List<LevelRecordData>();

        [SerializeField]
        public List<LevelScriptData> ScriptData = new List<LevelScriptData>();

        [SerializeField]
        public List<LevelVictoryData> VictoryData = new List<LevelVictoryData>();

        [SerializeField]
        public List<LevelFailData> FailData = new List<LevelFailData>();

        [SerializeField]
        public List<LevelVarData> GetGlobalData = new List<LevelVarData>();

        [SerializeField]
        public List<LevelVarData> SetGlobalData = new List<LevelVarData>();

        [SerializeField]
        public List<LevelStartTimeData> StartTimeData = new List<LevelStartTimeData>();

        [SerializeField]
        public List<LevelEndTimeData> EndTimeData = new List<LevelEndTimeData>();

        [SerializeField]
        public List<LevelSwitchPartnerData> SwitchPartnerData = new List<LevelSwitchPartnerData>();

        [SerializeField]
        public List<LevelRunSkillScriptData> RunSkillScriptData = new List<LevelRunSkillScriptData>();

        [SerializeField]
        public List<LevelRatioData> RatioData = new List<LevelRatioData>();

        [SerializeField]
        public List<LevelTemproryPartnerData> TemproryPartnerData = new List<LevelTemproryPartnerData>();

        [SerializeField]
        public List<LevelVarientData> VarientData = new List<LevelVarientData>();

        [SerializeField]
        public List<LevelStringBuildData> StringBuildData = new List<LevelStringBuildData>();

        [SerializeField]
        public List<LevelTriggerData> TriggerData = new List<LevelTriggerData>();

        [SerializeField]
        public List<LevelMonitorValueData> MonitorValueData = new List<LevelMonitorValueData>();

        [SerializeField]
        public List<LevelRefWaveData> RefWaveData = new List<LevelRefWaveData>();

        [SerializeField]
        public List<LevelSetParamData> ParamData = new List<LevelSetParamData>();

        [SerializeField]
        public List<LevelBuffStaData> BuffStaData = new List<LevelBuffStaData>();

        [SerializeField]
        public List<LevelAppointPosData> appointPosData = new List<LevelAppointPosData>();

        [SerializeField]
        public List<LevelMonsterCountData> monsterCountData = new List<LevelMonsterCountData>();

        [SerializeField]
        public List<LevelRobotWaveData> robotWaveData = new List<LevelRobotWaveData>();

        [SerializeField]
        public List<LevelSendMessageData> sendMessageData = new List<LevelSendMessageData>();

        [SerializeField]
        public List<LevelReceiveMessageData> receiveMessageData = new List<LevelReceiveMessageData>();

        [SerializeField]
        public List<LevelDoodadData> doodadData = new List<LevelDoodadData>();

        [SerializeField]
        public List<LevelRandomStreamData> randomStreamData = new List<LevelRandomStreamData>();
    }

    [Serializable]
    public class LevelWaveData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int WaveID;
        [SerializeField]
        public int SpawnType;
        [SerializeField]
        public int SpawnID;
        [SerializeField]
        public List<LevelWaveSpawnData> SpawnsInfo;
        [SerializeField]
        public float Interval;
        [SerializeField]
        public float WaveRange;
        [SerializeField]
        public int aiID;
        [SerializeField]
        public bool ignoreGrid;
    }

    [Serializable]
    public class LevelWaveSpawnData
    {
        [SerializeField]
        public Vector3 position;

        [SerializeField]
        public float rotation;

        [SerializeField]
        public float height;
    }

    [Serializable]
    public class LevelMonitorData : BluePrintNodeBaseData
    {
        [SerializeField]
        public float TargetValue;

        [SerializeField]
        public float MonitorValue;

        [SerializeField]
        public string TargetExString;
    }

    [Serializable]
    public class LevelConnectionData
    {
        [SerializeField]
        public int StartNode;
        [SerializeField]
        public int StartPin;
        [SerializeField]
        public int EndNode;
        [SerializeField]
        public int EndPin;
    }

    [Serializable]
    public class LevelExstringData : BluePrintNodeBaseData
    {
        [SerializeField]
        public string exString;

        [SerializeField]
        public int maxTriggerTime;
    }

    [Serializable]
    public class LevelRandomNodeData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int randomID;
    }

    [Serializable]
    public class LevelRecordData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int recordID;
    }

    [Serializable]
    public class LevelScriptData : BluePrintNodeBaseData
    {
        [SerializeField]
        public LevelScriptCmd Cmd;

        [SerializeField]
        public List<float> valueParam = new List<float>();

        [SerializeField]
        public List<Vector4> vecParam = new List<Vector4>();

        [SerializeField]
        public List<string> stringParam = new List<string>();
    }

    [Serializable]
    public class LevelWallPassFlag
    {
        [SerializeField]
        public bool Forward;
        [SerializeField]
        public bool Backward;
    }

    [Serializable]
    public class LevelWallData
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public Vector3 position;

        [SerializeField]
        public float rotation;

        [SerializeField]
        public Vector3 leftdown;

        [SerializeField]
        public Vector3 rightup;

        [SerializeField]
        public bool on;

        [SerializeField]
        public int type;

        [SerializeField]
        public Vector3 size;

        [SerializeField]
        public int wallType;

        [SerializeField]
        public Vector3 forward;

        [SerializeField]
        public bool SideLimit;

        [SerializeField]
        public List<LevelWallPassFlag> PassFlag = new List<LevelWallPassFlag>();

        [SerializeField]
        public bool isTrigger=false;

        [SerializeField]
        public bool once;

        [SerializeField]
        public string exString = string.Empty;

        [SerializeField]
        public int target;

        [SerializeField]
        public uint aiID;

        [SerializeField]
        public bool permanentFx=false;

        [SerializeField]
        public List<uint> buffIDList = new List<uint>();

        [SerializeField]
        public List<uint> typeList = new List<uint>();

        public LevelWallData ShallowCopy()
        {
            return this.MemberwiseClone() as LevelWallData;
        }
    }

    [Serializable]
    public class LevelVictoryData : BluePrintNodeBaseData
    { }

    [Serializable]
    public class LevelFailData : BluePrintNodeBaseData
    {
        [SerializeField]
        public bool PlayerAbandon;
    }

    [Serializable]
    public class LevelVarData : BluePrintNodeBaseData
    {
        [SerializeField]
        public string VarName;
    }

    [Serializable]
    public class LevelCustomNodeDefineData
    {
        [SerializeField]
        public string name;

        [SerializeField]
        public List<string> desc = new List<string>();

        [SerializeField]
        public List<string> type = new List<string>();

        [SerializeField]
        public List<string> layout = new List<string>();

        [SerializeField]
        public List<LevelCustomPinData> pinList = new List<LevelCustomPinData>();

        [SerializeField]
        public bool useDefaultPin = false;
    }

    [SerializeField]
    public class LevelCustomPinData
    {
        [SerializeField]
        public string pinName;

        [SerializeField]
        public int pinID;

        [SerializeField]
        public VariantType pinDatatType;

        [SerializeField]
        public PinStream pinStream;

        [SerializeField]
        public PinType pinType;
    }

    [Serializable]
    public class LevelCustomDefineData
    {
        [SerializeField]
        public List<LevelCustomNodeDefineData> CustomDefineNodes = new List<LevelCustomNodeDefineData>();
    }

    [Serializable]
    public class LevelStartTimeData: BluePrintNodeBaseData
    {
        [SerializeField]
        public bool CountDown;

        [SerializeField]
        public float CountDownLimit;

        [SerializeField]
        public bool Sync;

        [SerializeField]
        public string Tag;
    }

    [Serializable]
    public class LevelEndTimeData: BluePrintNodeBaseData
    {
        [SerializeField]
        public string Tag;
    }

    [Serializable]
    public class LevelSwitchPartnerData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int PartnerID;

        [SerializeField]
        public int ForcePartnerID;

        [SerializeField]
        public int ForceBack;

        [SerializeField]
        public int AddPartnerID;
    }


    [Serializable]
    public class LevelRunSkillScriptData : BluePrintNodeBaseData
    {
        [SerializeField]
        public string script;
    }

    [Serializable]
    public class LevelRatioData : BluePrintNodeBaseData
    {
        [SerializeField]
        public float rate;
    }

    [Serializable]
    public class LevelTemproryPartnerData:BluePrintNodeBaseData
    {
        [SerializeField]
        public int partnerID;

        [SerializeField]
        public bool add;
    }

    [Serializable]
    public class LevelVarientData:BluePrintNodeBaseData
    {
        [SerializeField]
        public VariantType varType;
        [SerializeField]
        public string value;
        [SerializeField]
        public bool boolParam;
        [SerializeField]
        public float floatParam;
        [SerializeField]
        public int intParam;
        [SerializeField]
        public Vector3 vec=Vector3.zero;
    }

    public class LevelStringBuildData:BluePrintNodeBaseData
    {
        [SerializeField]
        public string str;
    }

    public class LevelTriggerData:BluePrintNodeBaseData
    {
        [SerializeField]
        public float interval=1;

        [SerializeField]
        public bool running = true;

        [SerializeField]
        public float totalTime = -1;

        [SerializeField]
        public bool excuteImmediatly = true;
    }

    public class LevelMonitorValueData:BluePrintNodeBaseData
    {
        [SerializeField]
        public float value;
    }

    public class LevelRefWaveData:BluePrintNodeBaseData
    {
        [SerializeField]
        public int waveNodeID;

        [SerializeField]
        public int graphID;
    }

    public class LevelSetParamData:BluePrintNodeBaseData
    {
        [SerializeField]
        public string data;

        [SerializeField]
        public uint nodeID;

        [SerializeField]
        public uint graphID;
    }

    public class LevelBuffStaData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint staType;

        [SerializeField]
        public uint buffID;
    }

    public class LevelAppointPosData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint posType;

        [SerializeField]
        public List<string> objNameList = new List<string>();

        [SerializeField]
        public List<Vector4> posList = new List<Vector4>();

    }

    public class LevelMonsterCountData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint countType;

        [SerializeField]
        public List<uint> waveNodeIDList = new List<uint>();

        [SerializeField]
        public List<uint> waveGraphIDList = new List<uint>();
    }

    public class LevelRobotWaveData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint camp;

        [SerializeField]
        public uint robotID;

        [SerializeField]
        public LevelWaveSpawnData spawnData = new LevelWaveSpawnData();
    }

    public class LevelSendMessageData:BluePrintNodeBaseData
    {
        [SerializeField]
        public string message;

        [SerializeField]
        public bool sendToAll;

        [SerializeField]
        public bool cacheMessage;

        [SerializeField]
        public uint targetPhase;

        [SerializeField]
        public List<uint> mapIDList = new List<uint>();
    }

    public class LevelReceiveMessageData:BluePrintNodeBaseData
    {
        [SerializeField]
        public string message;
    }

    public class LevelDoodadData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint doodadID;

        [SerializeField]
        public List<LevelWaveSpawnData> spawnDatas = new List<LevelWaveSpawnData>();
    }

    public class LevelRandomStreamData:BluePrintNodeBaseData
    {
        [SerializeField]
        public List<float> weightList = new List<float>();

        [SerializeField]
        public int streamCount;
    }

}
