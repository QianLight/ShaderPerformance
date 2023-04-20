//
// URP Debug Views for Unity
// (c) 2019 PH Graphics
//

using UnityEngine;
using UnityEngine.Rendering;

namespace URPDebugViews
{

	public class DebugViewRenderPipeline : RenderPipeline
	{
		private readonly DebugViewRenderer _renderer = new DebugViewRenderer();

		public DebugViewRenderPipeline()
		{
			// For compatibility reasons we also match old LightweightPipeline tag.
			Shader.globalRenderPipeline = "UniversalPipeline,LightweightPipeline";
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			
			Shader.globalRenderPipeline = "";
		}

		protected override void Render(
			ScriptableRenderContext context, Camera[] cameras
		)
		{
			foreach (var camera in cameras)
			{
				_renderer.Render(context, camera);
			}
		}
	}
}