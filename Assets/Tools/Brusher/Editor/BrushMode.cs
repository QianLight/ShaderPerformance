using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PainterEditor
{	
	[System.Serializable]
	public abstract class BrushMode : ScriptableObject 
	{
		public virtual void OnDrag() 
		{
			
		}
		public virtual void OnMove() 
		{

		}
	}
}

