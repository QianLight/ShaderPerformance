using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class ShaderBindConfig
    {
        public Shader defaultBakeShader;
        public List<Tuple> bakeShaderList = new List<Tuple>();
        
        [Serializable]
        public class Tuple
        {
            public Shader originShader;
            public Shader bakeShader;

            public Tuple Clone()
            {
                return new Tuple()
                {
                    originShader = this.originShader,
                    bakeShader = this.bakeShader,
                };
            }
        }

        public ShaderBindConfig Clone()
        {
            var config = new ShaderBindConfig()
            {
                defaultBakeShader = this.defaultBakeShader,
            };
            foreach (var tuple in bakeShaderList)
            {
                config.bakeShaderList.Add(tuple.Clone());
            }
            return config;
        }
    }
}