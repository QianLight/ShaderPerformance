using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blueprint
{

    public class BlueprintSettings : ScriptableObject
    {
        public string BlueprintResourcePath = string.Empty;
        public string GetBlueprintResourceFullPath()
        {
            if (string.IsNullOrEmpty(BlueprintResourcePath))
                return string.Empty;
            if (System.IO.Path.IsPathRooted(BlueprintResourcePath))
                return BlueprintResourcePath;
            //相对路径转绝对路径
            return System.IO.Path.GetFullPath(BlueprintResourcePath);
        }
    }
}