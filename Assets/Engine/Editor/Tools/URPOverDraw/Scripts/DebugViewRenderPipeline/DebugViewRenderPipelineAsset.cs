//
// URP Debug Views for Unity
// (c) 2019 PH Graphics
//

using UnityEngine;
using UnityEngine.Rendering;

namespace URPDebugViews
{
	//[CreateAssetMenu(menuName = "Rendering/Debug View Render Pipeline")]
	public class DebugViewRenderPipelineAsset : RenderPipelineAsset
	{
		protected override RenderPipeline CreatePipeline()
		{
			return new DebugViewRenderPipeline();
		}
	}
}