using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{
	[System.Serializable]
	public class ShaderData
	{
		public string name;

		public Shader shader;

		//public Material refMaterial;
		public bool enable;
	}
}
