using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.ComponentModel;
using System.Xml.Serialization;

namespace BluePrint
{
    [Serializable]
    public class BluePrintData
    {
        [SerializeField]
        public int graphID;
        [SerializeField]
        public string name;

        [SerializeField]
        public List<BluePrintControllerData> ControllerData = new List<BluePrintControllerData>();

        [SerializeField]
        public List<BluePrintTimerData> TimerData = new List<BluePrintTimerData>();

        [SerializeField]
        public List<BluePrintPinData> pinData = new List<BluePrintPinData>();

        [SerializeField]
        public List<BluePrintVariantData> VarSetData = new List<BluePrintVariantData>();

        [SerializeField]
        public List<BluePrintVariantData> VarGetData = new List<BluePrintVariantData>();

        [SerializeField]
        public List<BluePrintVariant> VariantDefine = new List<BluePrintVariant>();

        [SerializeField]
        public List<ExtGetPlayerPositionData> GetPlayerPositionData = new List<ExtGetPlayerPositionData>();

        [SerializeField]
        public List<BluePrintNodeBaseData> GetPlayerHpData = new List<BluePrintNodeBaseData>();

        [SerializeField]
        public List<ExtInRangeData> InRangeData = new List<ExtInRangeData>();

        [SerializeField]
        public List<ExtGetPartnerAttrData> GetPartnerAttrData = new List<ExtGetPartnerAttrData>();

        [SerializeField]
        public List<ExtSetPartnerAttrData> SetPartnerAttrData = new List<ExtSetPartnerAttrData>();

        [SerializeField]
        public List<ExtGetPlayerIDData> GetPlayerIDData = new List<ExtGetPlayerIDData>();

        [SerializeField]
        public List<ExtLevelProgressData> LevelProgressData = new List<ExtLevelProgressData>();

        [SerializeField]
        public List<ExtGetScoreData> PlayerScoreData = new List<ExtGetScoreData>();

        [SerializeField]
        public List<ExtGetRandomNumData> GetRandomNumData = new List<ExtGetRandomNumData>();

        [SerializeField]
        public List<ExtEventCompleteData> EventCompleteData = new List<ExtEventCompleteData>();

        [SerializeField]
        public List<BluePrintSubGraphData> SubGraphData = new List<BluePrintSubGraphData>();
    }

    [Serializable]
    public class BluePrintSubGraphData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int GraphID;

        [SerializeField]
        public string GraphName;
    }

    [Serializable]
    public class BluePrintPinData
    {
        [SerializeField]
        public int nodeID;

        [SerializeField]
        public int pinID;

        [SerializeField, DefaultValueAttribute(float.MinValue)]
        public float defaultValue;
    }

    [Serializable]
    public class BluePrintNodeBaseData
    {
        [SerializeField]
        public int NodeID;

        [SerializeField]
        public bool enabled = true;

        [SerializeField]
        public uint condition1 = 0;//小重连是否重发 为兼容 1代表重发 2代表不重发

        [SerializeField]
        public uint condition2 = 0;//大重连是否重发 为兼容 1代表重发 2代表不重发
    }

    public enum ArithmeticType
    {
        Add = 0,
        Sub,
        Mul,
        Div,
    }

    [Serializable]
    public class BluePrintControllerData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int TypeID;

        [SerializeField]
        public int ArithmeticID;

        [SerializeField]
        public int nodeID;
    }

    [Serializable]
    public class BluePrintTimerData : BluePrintNodeBaseData
    {
        [SerializeField]
        public float Interval;

        [SerializeField]
        public bool Sync;

        [SerializeField]
        public int timerType;
    }

    public enum VariantType
    {
        Var_Float,
        Var_Bool,
        Var_Vector3,
        Var_Custom,
        Var_UINT64,
        Var_Unknow,
        Var_String
    }

    [Serializable]
    public class BluePrintVariant
    {
        [SerializeField]
        public VariantType VarType;

        [SerializeField]
        public string VariableName;

        [SerializeField]
        public string InitValue;
    }

    [Serializable]
    public class BluePrintVariantData : BluePrintNodeBaseData
    {
        [SerializeField]
        public string VariableName;
    }

    [Serializable]
    public class ExtGetPlayerPositionData : BluePrintNodeBaseData {}

    [Serializable]
    public class ExtInRangeData : BluePrintNodeBaseData
    {
        [SerializeField]
        public Vector3 Center;

        [SerializeField]
        public float Radius;
    }

    [Serializable]
    public class ExtGetPartnerAttrData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int PartnerID;

        [SerializeField]
        public int AttrID;
    }

    [Serializable]
    public class ExtSetPartnerAttrData : BluePrintNodeBaseData
    {
        [SerializeField]
        public int PartnerID;

        [SerializeField]
        public int AttrID;

        [SerializeField]
        public float AttrValue;
    }

    [Serializable]
    public class ExtGetPlayerIDData:BluePrintNodeBaseData
    {
        [SerializeField]
        public int PlayerSlot;
    }

    [Serializable]
    public class ExtLevelProgressData:BluePrintNodeBaseData
    {
        [SerializeField]
        public float maxProgress;
    }

    [Serializable]
    public class ExtGetScoreData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint slot = 1;
    }

    [Serializable]
    public class ExtGetRandomNumData:BluePrintNodeBaseData
    {
        [SerializeField]
        public float min;

        [SerializeField]
        public float max;
    }

    [Serializable]
    public class ExtEventCompleteData:BluePrintNodeBaseData
    {
        [SerializeField]
        public uint eventID;
    }

}
