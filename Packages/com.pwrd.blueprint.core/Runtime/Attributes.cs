using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blueprint
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ParamTypeAttribute : Attribute
    {
        public string type;
        public ParamTypeAttribute(string t)
        {
            type = t;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MethodDescriptionAttribute : Attribute
    {
        public string desc;
        public MethodDescriptionAttribute(string t)
        {
            desc = t;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter| AttributeTargets.ReturnValue | AttributeTargets.Method)]
    public sealed class CommentAttribute : Attribute
    {
        public string comment;
        public CommentAttribute(string t)
        {
            comment = t;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CodeAttribute : Attribute
    {
        public string code;
        public CodeAttribute(string t)
        {
            code = t;
        }
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class ProtoEnumAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.All)]
    public class NotReflectAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ClassPlatformAttribute : Attribute
    {
        public int platform = 1;
        public ClassPlatformAttribute(int _platform)
        {
            platform = _platform;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NotReflectConstructorAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class LoadPathAttribute : Attribute
    {
        public string path;
        public LoadPathAttribute(string _path)
        {
            path = _path;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DisplayNameAttribute : Attribute
    {
        public string displayName;
        public DisplayNameAttribute(string t)
        {
            displayName = t;
        }
    }
}
