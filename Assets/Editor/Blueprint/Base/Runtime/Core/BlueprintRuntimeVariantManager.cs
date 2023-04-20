using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BluePrint
{
     public class BlueprintRuntimeVariantManager
    {
        public Dictionary<string, BPVariant> VariantList = new Dictionary<string, BPVariant>();

        public BlueprintRuntimeGraph hostGraph;

        public BlueprintRuntimeVariantManager(BlueprintRuntimeGraph hostGraph)
        {
            this.hostGraph = hostGraph;
        }

        public void Clear()
        {
            VariantList.Clear();
        }
        public BPVariant GetValue(string name)
        {
            if(VariantList.ContainsKey(name))
            {
                return VariantList[name];
            }
            else if (hostGraph.GraphID != 1)
            {
                return hostGraph.Engine.GetMainGraph().VarManager.GetValue(name);
            }
            else
            {
                throw new Exception("Variant name not exists: " + name);
            }
        }

        public void SetValue(string name, BPVariant val)
        {
            if (VariantList.ContainsKey(name))
            {
                VariantList[name] = val;
            }
            else if (hostGraph.GraphID != 1)
            {
                hostGraph.Engine.GetMainGraph().VarManager.SetValue(name, val);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Variant name not exists: " + name);
            }

        }

        public void AddVariant(string name, VariantType t)
        {
            BPVariant var = new BPVariant
            {
                type = t,
            };

            var.val._float = 0.0f;
            var.val._bool = false;

            VariantList.Add(name, var);
        }

        public VariantType GetVariantType(string name)
        {
            if (VariantList.ContainsKey(name))
            {
                return VariantList[name].type;
            }
            else if(hostGraph.GraphID != 1)
            {
                return hostGraph.Engine.GetMainGraph().VarManager.GetVariantType(name);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Variant name not exists: " + name);
            }
        }
    }
}
