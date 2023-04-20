using UnityEngine;

namespace PainterEditor
{
	/**
	 *	Collection of settings for a sculpting brush.
	 */
	[CreateAssetMenuAttribute(menuName = "Polybrush/Brush Settings Preset", fileName = "Brush Settings", order = 800)]
	[System.Serializable]
	public class BrushSettings : ScriptableObject
	{
		public Color innerColor = Color.blue;
		public Color outterColor= Color.red;
		public float minSize = 1f;
		public float maxSize = 1f;
		/// The total affected radius of this brush.
		[SerializeField] private float _radius = 1f;

		/// At what point the strength of the brush begins to taper off.
		[SerializeField] float _falloff = .5f;

		/// How may times per-second a mouse click will apply a brush stroke.
		[SerializeField] float _strength = 10f;

		
		[SerializeField] AnimationCurve _curve = new AnimationCurve(
			new Keyframe(0f, 1f),
			new Keyframe(1f, 0f, -3f, -3f)
			);

		public AnimationCurve falloffCurve
		{
			get
			{
				return _curve;
			}
			set
			{
				_curve = value;
			}
		}

		/// If true, the falloff curve won't be clamped to keyframes at 0,0 and 1,1.
		public bool allowNonNormalizedFalloff = false;

		/// The total affected radius of this brush.
		public float radius
		{
			get
			{
				return _radius;
			}

			set
			{
				_radius = value;
			}
		}

		/// At what point the strength of the brush begins to taper off.
		/// 0 means the strength tapers from the center of the brush to the edge.
		/// 1 means the strength is 100% all the way through the brush.
		/// .5 means the strength is 100% through 1/2 the radius then tapers to the edge.
		public float falloff
		{
			get
			{
				return _falloff;
			}

			set
			{
				_falloff = Mathf.Clamp(value, 0f, 1f);
			}
		}

		public float strength
		{
			get
			{
				return _strength;
			}

			set
			{
				_strength = Mathf.Clamp(value, 0f, 1f);
			}
		}

	
		/**
		 *	Set the object's default values.
		 */
		public void SetDefaultValues()
		{
			radius = 1f;
			falloff = .5f;
			strength = 1f;

			innerColor = new Color(0.2f,0.3f,0.7f,0.9f);
			outterColor = new Color(1.0f,0.3f,0f,0.35f);

			minSize = 1f;
			maxSize = 1f;
		}

		public BrushSettings DeepCopy()
		{
			BrushSettings copy = ScriptableObject.CreateInstance<BrushSettings>();
			this.CopyTo(copy);
			return copy;
		}

		/**
		 * Copy all properties to target
		 */
		public void CopyTo(BrushSettings target)
		{
			target.name 							= this.name;
			target._radius							= this._radius;
			target.maxSize							= this.maxSize;
			target.minSize							= this.minSize;
			target._falloff							= this._falloff;
			target._strength						= this._strength;
			target._curve							= new AnimationCurve(this._curve.keys);
			target.allowNonNormalizedFalloff		= this.allowNonNormalizedFalloff;
		}
	}
}
