using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace LevelEditor
{
 
    class LevelRTScriptNode : BlueprintRuntimeDataNode<LevelScriptData>
    {
        public LevelScriptCmd Cmd = LevelScriptCmd.Level_Cmd_Invalid;
        protected LSRTBase InnerExecutor = null;

        //public LevelRTScriptNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(LevelScriptData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);
            Cmd = data.Cmd;

            switch (Cmd)
            {
                case LevelScriptCmd.Level_Cmd_Addbuff:
                    InnerExecutor = new LSRTAddbuff(this);
                    break;
                case LevelScriptCmd.Level_Cmd_Notice:
                    InnerExecutor = new LSRTNotice(this);
                    break;
                case LevelScriptCmd.Level_Cmd_CameraControl:
                    InnerExecutor = new LSRTControlCamera(this);
                    break;
                default:
                    InnerExecutor = new LSRTBase(this);
                    break;
                // ... other level script run time node
            }

            if (InnerExecutor != null) InnerExecutor.AddPin();
        }
        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            if (InnerExecutor != null) InnerExecutor.Execute();
            //Debug.Log("Level script " + HostData.script + " executed");

            if (pinOut != null)
                pinOut.Active();
        }
    }

    class LSRTBase
    {
        protected LevelRTScriptNode node;

        public LSRTBase(LevelRTScriptNode _node) { node = _node; }

        public virtual void AddPin() { }

        public virtual void Execute()
        {
            Debug.Log(node.Cmd.ToString() + " executed");
        }
    }

    class LSRTAddbuff : LSRTBase
    {
        BlueprintRuntimeValuedPin pinEnemyID;
        BlueprintRuntimeValuedPin pinBuffID;
        BlueprintRuntimeValuedPin pinBuffLevel;
        public LSRTAddbuff(LevelRTScriptNode _node) : base(_node){}
  
        public override void Execute()
        {
            float enemyID = node.HostData.valueParam[0];
            float buffID = node.HostData.valueParam[1];
            float buffLevel = node.HostData.valueParam[2];
           
            Debug.Log("LSRTAddbuff executed: " + enemyID + " " + buffID + " " + buffLevel); 
        }
    }

    class LSRTNotice : LSRTBase
    {
        public LSRTNotice(LevelRTScriptNode _node) : base(_node) { }

        public override void Execute()
        {
            string notcie = node.HostData.stringParam[0];

            Debug.Log("LSRTNotice executed: " + notcie);
        }
    }

    class LSRTControlCamera : LSRTBase
    {
        BlueprintRuntimeValuedPin pinMonsterID;
        public LSRTControlCamera(LevelRTScriptNode _node) : base(_node) { }

        public override void AddPin() 
        {
            pinMonsterID = new BlueprintRuntimeValuedPin(node, 1, PinType.Data, PinStream.In, VariantType.Var_UINT64);
            node.AddPin(pinMonsterID);
        }

        public override void Execute()
        {
            BPVariant monsterID = new BPVariant();
            if(pinMonsterID.GetPinValue(ref monsterID))
            {
                Debug.Log("LSRTControlCamera executed: " + monsterID.val._uint64);
            }
        }
    }
}
