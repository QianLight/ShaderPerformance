using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ShaderInstructAnalyze
{
    [Serializable]
    public class AnalyzeResult
    {
        public Shaders[] shaders;

        public string filePath;
        public string GetShortestPathBound()
        {
            Shaders shader = shaders[0];
            return shaders[0].variants[0].performance.shortest_path_cycles.bound_pipelines[0];
        }

        public float GetTotalCycle(CyclePathType cyclePathType)
        {
            float result = 0;
            
            if (shaders[0].shader.type == "Fragment")
            {
                Cycles fragmentCycles = shaders[0].variants[0].performance.total_cycles;
                switch (cyclePathType)
                {
                    case CyclePathType.total:
                        break;
                    case CyclePathType.longest:
                        fragmentCycles = shaders[0].variants[0].performance.longest_path_cycles;
                        break;
                    case CyclePathType.shortest:
                        fragmentCycles = shaders[0].variants[0].performance.shortest_path_cycles;
                        break;
                }
                for (int i = 0; i < fragmentCycles.cycle_count.Length; i++)
                {
                    result += fragmentCycles.cycle_count[i];
                }
                return result;
            }
            else if (shaders[0].shader.type == "Vertex")
            {
                Cycles[] vertexCyclesArray = new Cycles[shaders[0].variants.Length];

                for (int i = 0;i < vertexCyclesArray.Length;i++)
                {
                    vertexCyclesArray[i] = shaders[0].variants[i].performance.total_cycles;
                }
                switch (cyclePathType)
                {
                    case CyclePathType.total:
                        break;
                    case CyclePathType.longest:
                        for (int i = 0; i < vertexCyclesArray.Length; i++)
                        {
                            vertexCyclesArray[i] = shaders[0].variants[i].performance.longest_path_cycles;
                        }
                        break;
                    case CyclePathType.shortest:
                        for (int i = 0; i < vertexCyclesArray.Length; i++)
                        {
                            vertexCyclesArray[i] = shaders[0].variants[i].performance.shortest_path_cycles;
                        }
                        break;
                }
                for(int i = 0; i < vertexCyclesArray.Length; i++)
                {
                    for (int j = 0; j < vertexCyclesArray[i].cycle_count.Length; j++)
                    {
                        result += vertexCyclesArray[i].cycle_count[j] + vertexCyclesArray[i].cycle_count[j];
                    }
                }
                return result;
            }
            else
            {
                Debug.LogError("glsl type is error!");
                return -1;
            }
        }

        public string Show()
        {
            if(shaders[0].shader.type == "Fragment")
            {
                string properties = "";
                foreach (var propertie in shaders[0].properties)
                {
                    properties += (propertie.name + " : " + propertie.value + "\n");
                }
                foreach (var propertie in shaders[0].variants[0].properties)
                {
                    properties += (propertie.name + " : " + propertie.value + "\n");
                }
                /*
                Debug.Log("glsl properties is :\n " + properties);
                Debug.Log("total cycles is :\n" + OnePathInfo(CyclePathType.total));
                Debug.Log("longest cycles is :\n" + OnePathInfo(CyclePathType.longest));
                Debug.Log("shortest cycles is :\n" + OnePathInfo(CyclePathType.shortest));
                */
                string result = "#glsl properties is :\n\n" + properties +
                    "\n\ntotal cycles is :\n" + OnePathInfo(ShaderType.fragment, CyclePathType.total) +
                    "\nlongest cycles is :\n" + OnePathInfo(ShaderType.fragment, CyclePathType.longest) +
                    "\nshortest cycles is :\n" + OnePathInfo(ShaderType.fragment, CyclePathType.shortest) + "\n";
                return result;
            }
            else if (shaders[0].shader.type == "Vertex")
            {
                string properties0 = "";
                foreach (var propertie in shaders[0].properties)
                {
                    properties0 += (propertie.name + " : " + propertie.value + "\n");
                }
                for(int i = 0; i < shaders[0].variants.Length; i++)
                {
                    foreach (var propertie in shaders[0].variants[i].properties)
                    {
                        properties0 += (propertie.name + " : " + propertie.value + "\n");
                    }
                }

                string result0 = "#glsl properties is :\n\n" + properties0 +
                    "\n\ntotal cycles is :\n" + OnePathInfo(ShaderType.vertex, CyclePathType.total) +
                    "\nlongest cycles is :\n" + OnePathInfo(ShaderType.vertex, CyclePathType.longest) +
                    "\nshortest cycles is :\n" + OnePathInfo(ShaderType.vertex, CyclePathType.shortest) + "\n\n";

                return result0;
            }
            else
            {
                Debug.LogError("glsl type is error!");
                return "glsl type is error!";
            }
        }

        // TODO:优化vertex shader没有varying时对variants[1]的处理情况
        private String OnePathInfo(ShaderType shaderType, CyclePathType cyclePathType)
        {
            if(shaderType == ShaderType.fragment)
            {
                string[] piplines = shaders[0].variants[0].performance.pipelines;
                Cycles cycles = shaders[0].variants[0].performance.total_cycles;
                switch (cyclePathType)
                {
                    case CyclePathType.longest:
                        cycles = shaders[0].variants[0].performance.longest_path_cycles;
                        break;
                    case CyclePathType.shortest:
                        cycles = shaders[0].variants[0].performance.shortest_path_cycles;
                        break;
                }

                string onePathInfo = "";
                for (int i = 0; i < piplines.Length; i++)
                {
                    onePathInfo += piplines[i] + ":" + cycles.cycle_count[i] + "    ";
                }
                onePathInfo += "bound_pipline is :" + cycles.bound_pipelines[0];
                return onePathInfo;
            }
            else
            {
                string[] piplinesPostion = shaders[0].variants[0].performance.pipelines;
                Cycles cyclesPostion = shaders[0].variants[0].performance.total_cycles;
                switch (cyclePathType)
                {
                    case CyclePathType.longest:
                        cyclesPostion = shaders[0].variants[0].performance.longest_path_cycles;
                        break;
                    case CyclePathType.shortest:
                        cyclesPostion = shaders[0].variants[0].performance.shortest_path_cycles;
                        break;
                }

                string postionOnePathInfo = shaders[0].variants[0].name + ":\n";
                for (int i = 0; i < piplinesPostion.Length; i++)
                {
                    postionOnePathInfo += piplinesPostion[i] + ":" + cyclesPostion.cycle_count[i] + "    ";
                }
                postionOnePathInfo += "bound_pipline is :" + cyclesPostion.bound_pipelines[0];
                if(shaders[0].variants.Length < 2)
                {
                    return postionOnePathInfo + '\n';
                }

                string[] piplinesVaring = shaders[0].variants[1].performance.pipelines;
                Cycles cyclesVaring = shaders[0].variants[1].performance.total_cycles;
                switch (cyclePathType)
                {
                    case CyclePathType.longest:
                        cyclesPostion = shaders[0].variants[1].performance.longest_path_cycles;
                        break;
                    case CyclePathType.shortest:
                        cyclesPostion = shaders[0].variants[1].performance.shortest_path_cycles;
                        break;
                }

                string varyingOnePathInfo = shaders[0].variants[1].name + ":\n"; ;
                for (int i = 0; i < piplinesVaring.Length; i++)
                {
                    varyingOnePathInfo += piplinesVaring[i] + ":" + cyclesVaring.cycle_count[i] + "    ";
                }
                varyingOnePathInfo += "bound_pipline is :" + cyclesVaring.bound_pipelines[0];

                return postionOnePathInfo + '\n' + varyingOnePathInfo;
            }
        }

    }
    [Serializable]
    public class Shaders
    {
        public Properties[] properties;
        public ShaderInJson shader;
        public Variants[] variants;
    }
    [Serializable]
    public class Properties
    {
        public string name;
        public bool value;
    }
    [Serializable]
    public class ShaderInJson
    {
        public string type;
    }
    [Serializable]
    public class Variants
    {
        public string name;
        public Performance performance;
        public Properties_2[] properties;
    }
    [Serializable]
    public class Performance
    {
        public string[] pipelines;
        public Cycles longest_path_cycles;
        public Cycles shortest_path_cycles;
        public Cycles total_cycles;
    }
    [Serializable]
    public class Cycles
    {
        public string[] bound_pipelines;
        public float[] cycle_count;
    }
    [Serializable]
    public class Properties_2
    {
        public string name;
        public long value;
    }
    [Serializable]
    public enum ShaderType
    {
        vertex,
        fragment
    }
    [Serializable]
    public enum CyclePathType
    {
        longest,
        shortest,
        total
    }
}

