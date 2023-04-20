#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    public class EffectTemplateConfig : AssetBaseConifg<EffectTemplateConfig>
    {
        public List<TestGameObject> testGO = new List<TestGameObject> ();

    }

}
#endif