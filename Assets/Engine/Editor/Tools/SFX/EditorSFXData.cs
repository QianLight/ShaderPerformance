using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine.Editor
{
	public class EditorSFXData : AssetBaseConifg<EditorSFXData>
	{
		public List<string> dirs = new List<string>();
		public int profileLevels = 4;
		public List<SFXProfileSettings> settingType;
		public List<SkillProfileType> skillTypeByFolder;
		public List<SkillProfileType> skillTypeByDoc;

		public EditorSFXData()
		{
			settingType = new List<SFXProfileSettings>();
            for (var index = 0; index < profileLevels; index++)
            {
	            SFXProfileSettings st = new SFXProfileSettings();
	            st.profileLevels = new SFXProfileProperties[4];
                for (int i = 0; i < 3; i++)
                {
	                st.profileLevels[i] = new SFXProfileProperties();
                }
                settingType.Add(st);
            }

            skillTypeByFolder = new List<SkillProfileType>();
            skillTypeByDoc = new List<SkillProfileType>();
		}
	}
	[System.Serializable]
	public class SFXProfileProperties
	{
		public int pCount;
		// public float delay;
		public int psCount;
		public float fillrate;
		public int batches;
		public float fillrateArea;
		public SFXProfileProperties()
		{
			pCount = 0;
			psCount = 0;
			fillrate = 0;
			batches = 0;
		}
	}

	[System.Serializable]
	public class SFXProfileSettings
	{
		public string countInfo;
		public string exampleInfo;
		public SFXProfileProperties[] profileLevels;

		public SFXProfileSettings()
		{
			countInfo = "";
			exampleInfo = "新增";
			profileLevels = new []
			{
				new SFXProfileProperties(), 
				new SFXProfileProperties(), 
				new SFXProfileProperties()
			};
		}
	}
	// [System.Serializable]
	// public enum SFXProfileLevel
	// {
	// 	Not_Setted = 0,
	// 	GiantSingle = 1,
	// 	Normal = 2,
	// 	Hit = 3,
	// }

	[System.Serializable]
	public struct SkillProfileType
	{
		public string skillName;
		public int skillType;
	}
}