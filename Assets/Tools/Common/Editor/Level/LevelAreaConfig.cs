using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace XEditor.Level
{
    [CreateAssetMenu(fileName = "LevelAreaConfig", menuName = "Level/LevelAreaConfig", order = 1)]
    public class LevelAreaConfig : ScriptableObject
    {
        public Color32[] AreaColor;
    }
    public class LevelAreaSetting
    {
        public LevelAreaConfig m_AreaConfig;
        public LevelAreaSetting()
        {
            m_AreaConfig = AssetDatabase.LoadAssetAtPath("Assets/Tools/Common/Editor/Level/Res/LevelAreaConfig.asset", typeof(LevelAreaConfig)) as LevelAreaConfig;
        }

        public Color32 GetAreaColor(int areaID)
        {
            if (areaID < 0 || areaID >= m_AreaConfig.AreaColor.Length) return Color.white;

            return m_AreaConfig.AreaColor[areaID];
        }
    }
}

