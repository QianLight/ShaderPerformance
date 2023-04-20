//
// URP Debug Views for Unity
// (c) 2019 PH Graphics
// 

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace URPDebugViews
{
	public partial class DebugViewRenderer
	{
		private const string BufferName = "Debug View";

		private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();

		private readonly CommandBuffer _buffer = new CommandBuffer
		{
			name = BufferName
		};

		private ScriptableRenderContext _context;
		private Camera _camera;
		private CullingResults _cullingResults;
		private readonly Color ClearColor = new Color(0.1f, 0.1f, 0.1f);
		
		// copied from URP MainLightShadowCasterPass.cs
		private static readonly int CascadeShadowSplitSpheres0 = Shader.PropertyToID("_CascadeShadowSplitSpheres0");
		private static readonly int CascadeShadowSplitSpheres1 = Shader.PropertyToID("_CascadeShadowSplitSpheres1");
		private static readonly int CascadeShadowSplitSpheres2 = Shader.PropertyToID("_CascadeShadowSplitSpheres2");
		private static readonly int CascadeShadowSplitSpheres3 = Shader.PropertyToID("_CascadeShadowSplitSpheres3");
		private static readonly int CascadeShadowSplitSphereRadii = Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
		
		private static readonly int DebugViewShadowDistances = Shader.PropertyToID("_DebugViewShadowDistances");

		public DebugViewRenderer()
		{
			_shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			_shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
			_shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
		}

		public void Render(ScriptableRenderContext context, Camera camera)
		{
			_context = context;
			_camera = camera;

			PrepareBuffer();
			PrepareForSceneWindow();
			if (!Cull())
			{
				return;
			}

			Setup();
			UpdateGlobalProperties();
			DrawVisibleGeometry();
			DrawUnsupportedShaders();
			DrawGizmos();
			Submit();
		}

		private bool Cull()
		{
			if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
			{
				_cullingResults = _context.Cull(ref p);
				return true;
			}

			return false;
		}

		private void Setup()
		{
			_context.SetupCameraProperties(_camera);
			_buffer.ClearRenderTarget(true,true, ClearColor);
			_buffer.BeginSample(SampleName);
			ExecuteBuffer();
		}
		
		private void UpdateGlobalProperties()
		{
			Vector4 shadowDistances;
			DebugViewsManager.Instance.GetShadowsDistances(out shadowDistances);
			_buffer.SetGlobalVector(DebugViewShadowDistances, shadowDistances);
		}

		private void Submit()
		{
			_buffer.EndSample(SampleName);
			ExecuteBuffer();
			_context.Submit();
		}

		private void ExecuteBuffer()
		{
			_context.ExecuteCommandBuffer(_buffer);
			_buffer.Clear();
		}

		private void DrawVisibleGeometry()
		{
			//不透明物体
			var sortingSettings = new SortingSettings(_camera)
			{
				criteria = SortingCriteria.CommonOpaque
			};
			var drawingSettings = new DrawingSettings();
			drawingSettings.sortingSettings = sortingSettings;
			for (int i = 0; i < _shaderTagIdList.Count; i++)
				drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);

			var viewData = DebugViewsManager.Instance.CurrentViewData;
			drawingSettings.overrideMaterial = viewData ? viewData.MatQueue : null;
			//drawingSettings.overrideMaterial = viewData ? viewData.Material : null;
			var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
			_context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);

			//透明物体
			sortingSettings = new SortingSettings(_camera)
			{
				criteria = SortingCriteria.CommonTransparent
			};
			//sortingSettings.criteria = SortingCriteria.CommonTransparent;
			drawingSettings = new DrawingSettings();
			drawingSettings.sortingSettings = sortingSettings;
			for (int i = 0; i < _shaderTagIdList.Count; i++)
				drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);
			//filteringSettings.renderQueueRange = RenderQueueRange.transparent;
			filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
			drawingSettings.overrideMaterial = viewData ? viewData.MatTransparent : null;
			_context.DrawRenderers(
				_cullingResults, ref drawingSettings, ref filteringSettings
			);
		}
	}

	partial class DebugViewRenderer
	{
		partial void DrawGizmos();

		partial void DrawUnsupportedShaders();

		partial void PrepareForSceneWindow();

		partial void PrepareBuffer();

#if UNITY_EDITOR

		private static readonly ShaderTagId[] _legacyShaderTagIds =
		{
			new ShaderTagId("Always"),
			new ShaderTagId("ForwardBase"),
			new ShaderTagId("PrepassBase"),
			new ShaderTagId("Vertex"),
			new ShaderTagId("VertexLMRGBM"),
			new ShaderTagId("VertexLM")
		};

		private static Material _errorMaterial;

		private string SampleName { get; set; }

		partial void DrawGizmos()
		{
			if (UnityEditor.Handles.ShouldRenderGizmos())
			{
				_context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
				_context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
			}
		}

		partial void DrawUnsupportedShaders()
		{
			if (_errorMaterial == null)
			{
				_errorMaterial =
					new Material(Shader.Find("Hidden/InternalErrorShader"));
			}

			var drawingSettings = new DrawingSettings(
				_legacyShaderTagIds[0], new SortingSettings(_camera)
			)
			{
				overrideMaterial = _errorMaterial
			};
			for (int i = 1; i < _legacyShaderTagIds.Length; i++)
			{
				drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
			}

			var filteringSettings = FilteringSettings.defaultValue;
			_context.DrawRenderers(
				_cullingResults, ref drawingSettings, ref filteringSettings
			);
		}

		partial void PrepareForSceneWindow()
		{
			if (_camera.cameraType == CameraType.SceneView)
			{
				ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
			}
		}

		partial void PrepareBuffer()
		{
			Profiler.BeginSample("Editor Only");
			_buffer.name = SampleName = _camera.name;
			Profiler.EndSample();
		}

#else

	const string SampleName = BufferName;

#endif
	}
}