using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine.Editor
{
    public class MaterialReplaceGroup : ScriptableObject
    {
        public List<MaterialReplaceConfig> configs = new List<MaterialReplaceConfig>();
    }
}
