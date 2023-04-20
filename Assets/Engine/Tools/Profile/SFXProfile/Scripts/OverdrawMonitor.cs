#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

/// <summary> This is a singleton component that is responsible for measuring overdraw information
/// on the main camera. You shouldn't add this compoenent manually, but use the Instance getter to
/// access it.
/// 
/// The measurements process is done in two passes. First a new camera is created that will render
/// the scene into a texture with high precission, the texture is called overdrawTexture. This texture
/// contains the information how many times a pixel has been overdrawn. After this step a compute shader
/// is used to add up all the pixels in the overdrawTexture and stores the information into this component.
/// 
/// We say this tool measures exactly the amount of overdraw, but it does so only in certain cases. In other
/// cases the error margin is very small. This is because of the nature of the compute shaders. Compute
/// shaders should operate in batches in order for them to be efficient. In our case the compute shader 
/// batch that sums up the results of the first render pass has a size of 32x32. This means that if the
/// pixel size of the camera is not divisible by 32, then the edge pixels that don't fit in range won't
/// be processed. But since we usually have huge render targets (in comparison to 32x32 pixel blocks) and
/// the error comes from the part of the image that is not important, this is acceptable. 
/// </summary>

// [ExecuteInEditMode]
public delegate void MonitorRecording();
public class OverdrawMonitor : MonoBehaviour
{
	private static OverdrawMonitor instance;
	public static bool isOn;
	public static OverdrawMonitor Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<OverdrawMonitor>();
				if (instance == null)
				{
					var go = new GameObject("OverdrawMonitor");
					instance = go.AddComponent<OverdrawMonitor>();
				}
			}

			return instance;
		}
	}

	// private new Camera camera;
	public RenderTexture overdrawTexture;

	private ComputeShader computeShader;

	private const int dataSize = 128 * 128;
	private int[] inputData = new int[dataSize];
	private int[] resultData = new int[dataSize];
	private ComputeBuffer resultBuffer;
	public string currentName = "";
	public List<string> errorList = new List<string>();
	
	// ========= Results ========
	// Last measurement
	/// <summary> The number of shaded fragments in the last frame. </summary>
	public long TotalShadedFragments { get; private set; }

	/// <summary> The overdraw ration in the last frame. </summary>
	public float OverdrawRatio { get; private set; }

	// Sampled measurement
	/// <summary> Number of shaded fragments in the measured time span. </summary>
	public long IntervalShadedFragments { get; private set; }

	/// <summary> The average number of shaded fragments in the measured time span. </summary>
	public float IntervalAverageShadedFragments { get; private set; }

	/// <summary> The average overdraw in the measured time span. </summary>
	public float IntervalAverageOverdraw { get; private set; }

	public float AccumulatedAverageOverdraw
	{
		get { return accumulatedIntervalOverdraw / intervalFrames; }
	}

	// Extreems
	/// <summary> The maximum overdraw measured. </summary>
	public float MaxOverdraw { get; private set; }

	private long accumulatedIntervalFragments;
	private float accumulatedIntervalOverdraw;
	private long intervalFrames;

	private float intervalTime = 0;
	public float SampleTime = 1;

	[SerializeField]public SingleSkillSFXProfileData RuntimeProfileData;
	[SerializeField]public ProfileMonitorData StartProfileData;
	public bool isObserving = false;
	private float _starttime;
	public List<SingleSkillSFXProfileData> PassedSkillProfileDatas;

	// public bool isOn = false;
	private MethodInfo info;
	public int ObservingType;
	private int sfxCount;

	public MonitorRecording OnSkillStart;
	public MonitorRecording OnSkillEnd;

	/// <summary> An empty method that can be used to initialize the singleton. </summary>
	public void Touch()
	{
	}

	#region Measurement magic

	public void Awake()
	{

		// Since this emulation always turns on by default if on mobile platform. With the emulation
		// turned on the tool won't work.
		// UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Graphics Emulation/No Emulation");
		// SubscribeToPlayStateChanged();


		if (Application.isPlaying) DontDestroyOnLoad(gameObject);
		gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;

		// RecreateTexture(Camera.main);
		RecreateComputeBuffer();
		computeShader = AssetsConfig.instance.OverdrawCompute;
		for (int i = 0; i < inputData.Length; i++) inputData[i] = 0;
		PassedSkillProfileDatas = new List<SingleSkillSFXProfileData>();

	}
	
	// #if UNITY_EDITOR
	// 	public void SubscribeToPlayStateChanged()
	// 	{
	// 		UnityEditor.EditorApplication.playmodeStateChanged -= OnPlayStateChanged;
	// 		UnityEditor.EditorApplication.playmodeStateChanged += OnPlayStateChanged;
	// 	}
	//
	// 	private static void OnPlayStateChanged()
	// 	{
	// 		if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && UnityEditor.EditorApplication.isPlaying)
	// 		{
	// 			if (instance != null) instance.OnDisable();
	// 		}
	// 	}
	// #endif
	public void InitSFXCount()
	{
		sfxCount = 0;
	}
	private void LateUpdate()
	{
		if (isObserving && isOn)
		{
			UpdateProfileData();
		}
	}
	
	public void OnEnable()
	{
		disabled = false;
		isOn = true;
		isObserving = false;
		ForwardRenderer.OverdrawProcess += CaptureOverdraw;
		// RenderLayer.Instance.prePostProcessing += CaptureOverdraw;
	}

	public void OnDisable()
	{
		ForwardRenderer.OverdrawProcess -= CaptureOverdraw;
		// RenderLayer.Instance.prePostProcessing -= CaptureOverdraw;
		disabled = true;
		isOn = false;
		OnDestroy();
	}

	public void ResetSceneStaticProfile()
	{
		StartProfileData = new ProfileMonitorData();
		StartProfileData.currentBatches = UnityStats.batches;
		StartProfileData.currentFillrate = Instance.AccumulatedAverageOverdraw;
		StartProfileData.currentParticleSystemCount = SFXMgr.singleton.sfxPool.Count + SFXMgr.singleton.sfxOwnerPool.Count;
		StartProfileData.currentParticleCount = 0;
		_starttime = Time.time;
		List<ParticleSystem> psList = new List<ParticleSystem>();
		foreach (var vpSfxe in SFXMgr.singleton.sfxPool)
		{
			var root = vpSfxe.Value.GetTrans();
			ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();
			foreach (var system in particleSystems) psList.Add(system);
		}
		foreach (var vpSfxe in SFXMgr.singleton.sfxOwnerPool)
		{
			var root = vpSfxe.GetTrans();
			ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();
			foreach (var system in particleSystems) psList.Add(system);
		}
		foreach (var ps in psList)
		{
			StartProfileData.currentParticleCount += ps.particleCount;
		}
		info = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.Instance | BindingFlags.NonPublic);
	}
	
	public void StartObserveProfile(int type, string name)//0空1技能2子弹3单独特效
	{
		
		if(type==1)
		{
			ObservingType = 1;
			ResetSceneStaticProfile();
			RuntimeProfileData = new SingleSkillSFXProfileData(name);
			isObserving = true;
			OnSkillStart?.Invoke();
			Debug.Log("Skill start recording:    "+RuntimeProfileData.skillname);
			sfxCount = 0;
			RuntimeProfileData.separateTime = 0;
			return;
		}
		if (type==2)
		{
			if (ObservingType == 0)
			{
				ResetSceneStaticProfile();
				RuntimeProfileData = new SingleSkillSFXProfileData(name);
				isObserving = true;
				OnSkillStart?.Invoke();
				Debug.Log("Bullet Skill start recording:    "+RuntimeProfileData.skillname);
				sfxCount = 0;
				RuntimeProfileData.separateTime = 0;
			}
			this.ObservingType = 2;
			Debug.Log("Bullet is triggered from skill:    "+RuntimeProfileData.skillname);
			return;
		}
		if (type == 3)
		{
			sfxCount++;
			if (ObservingType == 0)
			{
				ObservingType = 3;
				ResetSceneStaticProfile();
				RuntimeProfileData = new SingleSkillSFXProfileData(name);
				isObserving = true;
				OnSkillStart?.Invoke();
				Debug.Log("Single SFX start recording:    "+name);
				return;
			}
			
			if (ObservingType == 1)
			{
				DebugLog.AddLog2("SFX is triggered from skill:    "+name);
				return;
			}

			if (ObservingType == 2)
			{
				DebugLog.AddLog2("SFX is triggered from bullet skill:    "+name);
				return;
			}
			DebugLog.AddLog2("Single SFX is triggered:    "+RuntimeProfileData.skillname);
			return;
		}
		
	}

	public void EndObserveProfile( int type, string name = "")//0空1技能2子弹3单独特效
	{
		if (isObserving)
		{
			// if (ObservingType == 2&&type==2)
			// {
			// 	isObserving = false;
			// 	SaveData();
			// 	ObservingType = 0;
			// 	Debug.Log("子弹技能"+RuntimeProfileData.skillname+"已记录");
			// 	return;
			// }
			if (type == 1)
			{
				if (ObservingType == 1)
				{
					if (sfxCount == 0)
					{
						isObserving = false;
						SaveData();
						OnSkillEnd?.Invoke();
						ObservingType = 0;
						DebugLog.AddLog2("(Ending Record)Skill:	"+RuntimeProfileData.skillname);
						return;
					}
					else
					{
						ObservingType = 2;
						RuntimeProfileData.separateTime = Time.time - _starttime;
						DebugLog.AddLog2("Skill ended but not all sfx released:	"+RuntimeProfileData.skillname+"    separateTime:"+RuntimeProfileData.separateTime);
						return;
					}
				}
		
				if(ObservingType==2)
				{
					RuntimeProfileData.separateTime = Time.time - _starttime;
 					DebugLog.AddLog2("Bullet skill released(wait for all sfx release):    "+RuntimeProfileData.skillname+"    separateTime:"+RuntimeProfileData.separateTime);
					return;
				}
			}
			else if (type == 3)
			{
				sfxCount--;
				if (ObservingType == 1)
				{
					// RuntimeProfileData.totalduration = Time.time - _starttime;
					DebugLog.AddLog2("sfx released from skill:	"+name);
					// if (sfxCount == 0)
					// {
					// 	isObserving = false;
					// 	SaveData();
					// 	ObservingType = 0;
					// 	DebugLog.AddLog2("(Ending Record) All SFX from skill is released:	"+name);
					// 	return;
					// }
					return;
				}
				
				if (ObservingType ==2)
				{
					DebugLog.AddLog2("sfx released from bullet skill:	"+name);
					if (sfxCount == 0)
					{
						SaveData();
						OnSkillEnd?.Invoke();
						isObserving = false;
						ObservingType = 0;
						DebugLog.AddLog2("(Ending Record) All SFX from bullet skill is released:	"+name);
						return;
					}
					return;
				}
		
				if (ObservingType == 3 && sfxCount == 0)
				{
					SaveData();
					OnSkillEnd?.Invoke();
					isObserving = false;
					ObservingType = 0;
					Debug.Log("(Ending Record)Single SFX:	"+name);
					return;
				}

				return;
			}
			Debug.LogWarning("Mismatching SFX type:	Observing - "+ObservingType.ToString()+" Being Observed - "+type.ToString());
		}
		Debug.LogWarning("Unobserving state but observed:	"+type.ToString());
	}

	public void SaveData()
	{
		RuntimeProfileData.totalduration = Time.time - _starttime;
		PassedSkillProfileDatas.Add(new SingleSkillSFXProfileData(RuntimeProfileData));
	}
	private void UpdateProfileData()
	{
		// intervalFrames++;
		var value = AccumulatedAverageOverdraw - StartProfileData.currentFillrate;
		if (float.IsNaN(value)) return;
		Keyframe key = new Keyframe(Time.time - _starttime,
			AccumulatedAverageOverdraw - StartProfileData.currentFillrate, 0, 0);
		RuntimeProfileData.fillrate.AddKey(key);
		
		key = new Keyframe(Time.time - _starttime, 
			UnityStats.batches - StartProfileData.currentBatches, 0, 0);
		RuntimeProfileData.batchesCount.AddKey(key);

		int psCount = EngineProfiler.context.psCount;
		key = new Keyframe(Time.time - _starttime,
			psCount, 0, 0);
		RuntimeProfileData.particleSystemCount.AddKey(key);

		int pCount = 0;
		List<ParticleSystem> psList = new List<ParticleSystem>();
		foreach (var vpSfxe in SFXMgr.singleton.sfxPool)
		{
			var root = vpSfxe.Value.GetTrans();
			if (root)
			{
				ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();
				foreach (var system in particleSystems) psList.Add(system);	
			}
		}
		foreach (var vpSfxe in SFXMgr.singleton.sfxOwnerPool)
		{
			var root = vpSfxe.GetTrans();
			if(root)
			{
				ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>();
				foreach (var system in particleSystems) psList.Add(system);
			}
			else
			{
				errorList.Add(Path.GetFileName(currentName));
			}
			
		}
		foreach (var ps in psList)
		{
			// int count = 0;
			// object[] invokeArgs = {count, 0.0f, Mathf.Infinity};
			// info.Invoke(ps, invokeArgs);
			// count = (int) invokeArgs[0];
			pCount += ps.particleCount;
		}
		key = new Keyframe(Time.time - _starttime, 
		pCount, 0, 0);
		RuntimeProfileData.particlesCount.AddKey(key);

	}
	private bool disabled = true;



	public void SetCam(Texture source, int pixelWidth, int pixelHeight)
	{
		if (disabled) return;

		// RecreateTexture(main);
		SetTexture(source, pixelWidth, pixelHeight);
		intervalTime += Time.deltaTime;


		if (intervalTime > SampleTime)
		{
			IntervalShadedFragments = accumulatedIntervalFragments;
			IntervalAverageShadedFragments = (float) accumulatedIntervalFragments / intervalFrames;
			IntervalAverageOverdraw = (float) accumulatedIntervalOverdraw / intervalFrames;

			intervalTime -= SampleTime;

			accumulatedIntervalFragments = 0;
			accumulatedIntervalOverdraw = 0;
			intervalFrames = 0;
		}
	}

	/// <summary> Checks if the overdraw texture should be updated. This needs to happen if the main camera
	/// configuration changes. </summary>
	private void RecreateTexture(int pixelWidth, int pixelHeight /*Camera main*/)
	{
		if (overdrawTexture == null)
		{
			overdrawTexture = new RenderTexture(pixelWidth, pixelHeight, 24, RenderTextureFormat.ARGBFloat);
			overdrawTexture.enableRandomWrite = true;
			// camera.targetTexture = overdrawTexture;
		}

		if (pixelWidth != overdrawTexture.width || pixelHeight != overdrawTexture.height)
		{
			overdrawTexture.Release();
			overdrawTexture.width = pixelWidth;
			overdrawTexture.height = pixelHeight;
		}
	}

	public void SetTexture(Texture source, int pixelWidth, int pixelHeight)
	{
		RecreateTexture(pixelWidth, pixelHeight);
		Graphics.CopyTexture(source, overdrawTexture);
	}

	private void RecreateComputeBuffer()
	{
		if (resultBuffer != null) return;
		resultBuffer = new ComputeBuffer(resultData.Length, 4);
	}

	public int GetSfxCount()
    {
		return sfxCount;
    }

	public void OnDestroy()
	{
		// if (camera != null)
		// {
		// 	camera.targetTexture = null;
		// }
		if (resultBuffer != null) resultBuffer.Release();
		if (overdrawTexture) overdrawTexture.Release();
	}


	public void ComputeOverdrawFillrate( /*CommandBuffer cmd, RenderTargetIdentifier rtID*/)
	{
		if (disabled || !overdrawTexture) return;
		int kernel = computeShader.FindKernel("OverdrawCalculation");

		RecreateComputeBuffer();
		// Setting up the data
		resultBuffer.SetData(inputData);
		// cmd.SetComputeTextureParam(computeShader,kernel, "Overdraw", rtID);
		// cmd.SetComputeBufferParam(computeShader, kernel, "Output", resultBuffer);
		// Graphics.CopyTexture(Shader.GetGlobalTexture(RenderContext._OverdrawTex),overdrawTexture);

		computeShader.SetTexture(kernel, "Overdraw", overdrawTexture);
		computeShader.SetBuffer(kernel, "Output", resultBuffer);

		int xGroups = (Camera.main.pixelWidth / 32);
		int yGroups = (Camera.main.pixelHeight / 32);

		// Summing up the fragments
		// cmd.DispatchCompute(computeShader, kernel, xGroups, yGroups, 1);
		// Graphics.ExecuteCommandBuffer(cmd);
		computeShader.Dispatch(kernel, xGroups, yGroups, 1);

		resultBuffer.GetData(resultData);
		// Graphics.ExecuteCommandBuffer(cmd);
		// RuntimeUtilities.EndProfile(cmd, "OverdrawMonitor");
		TotalShadedFragments = 0;
		for (int i = 0; i < resultData.Length; i++)
		{
			TotalShadedFragments += resultData[i];
		}

		OverdrawRatio = (float) TotalShadedFragments / (xGroups * 32 * yGroups * 32);

		accumulatedIntervalFragments += TotalShadedFragments;
		accumulatedIntervalOverdraw += OverdrawRatio;
		intervalFrames++;
		if (OverdrawRatio > MaxOverdraw) MaxOverdraw = OverdrawRatio;
	}

	#endregion

	#region Measurement control methods

	public void StartMeasurement()
	{
		enabled = true;
		// camera.enabled = true;
	}

	public void Stop()
	{
		enabled = false;
		// camera.enabled = false;
	}

	public void SetSampleTime(float time)
	{
		SampleTime = time;
	}

	public void ResetSampling()
	{
		accumulatedIntervalOverdraw = 0;
		accumulatedIntervalFragments = 0;
		intervalTime = 0;
		intervalFrames = 0;
	}

	public void ResetExtreemes()
	{
		MaxOverdraw = 0;
	}

	#endregion
	void CaptureOverdraw()
	{
		EngineContext engineContex = EngineContext.instance;
		if (engineContex != null)
		{
			var camera = engineContex.CameraRef;
			Camera[] cam = Camera.allCameras;
			for (int i = 0; i < cam.Length; i++)
			{
				if (cam[i].cameraType.Equals(CameraType.Game))
				{
					camera = cam[i];
					break;
				}
			}
			if (camera != null)
			{
        
				int w = camera.pixelWidth;
				int h = camera.pixelHeight;
        
				RenderTexture targetRT = RenderTexture.GetTemporary (w, h, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				targetRT.name = "GrabForOverdraw";
				var lastActive = RenderTexture.active;
				lastActive = camera.targetTexture;
				CommandBuffer cmd = RenderContext.singleton.prePPCommand;
				
				Graphics.Blit(lastActive, targetRT);
				OverdrawMonitor.Instance.SetCam(targetRT, w, h);
        
				RenderTexture.ReleaseTemporary (targetRT);
			}
        
		}
	}
}
[Serializable]
public class SingleSkillSFXProfileData
{
	[SerializeField]public int SFXLevel = 0;
	[SerializeField]public string skillname;
	// [SerializeField]public AnimationCurve memoryUse; 
	// public int texCount;
	[SerializeField]public float separateTime;
	[SerializeField]public float totalduration;
	[SerializeField]public bool decreaseWarning;
	[SerializeField]public List<HighestCurrentProfile> highestProfiles;
	[SerializeField]public AnimationCurve particleSystemCount;
	[SerializeField]public AnimationCurve particlesCount;
	[SerializeField]public AnimationCurve batchesCount;
	[SerializeField]public AnimationCurve fillrate;

	public SingleSkillSFXProfileData(string skillName)
	{
		skillname = skillName;
		// memoryUse = new AnimationCurve();
		// RuntimeProfileData.texCount = tex;
		decreaseWarning = false;
		particleSystemCount = new AnimationCurve();
		fillrate = new AnimationCurve();
		batchesCount = new AnimationCurve();
		particlesCount = new AnimationCurve();
	}

	public SingleSkillSFXProfileData(SingleSkillSFXProfileData other)
	{
		skillname = other.skillname;
		// memoryUse = other.memoryUse;
		separateTime = other.separateTime;
		totalduration = other.totalduration;
		highestProfiles = other.highestProfiles;
		decreaseWarning = other.decreaseWarning;
		particleSystemCount = other.particleSystemCount;
		fillrate = other.fillrate;
		batchesCount = other.batchesCount;
		particlesCount = other.particlesCount;
		SFXLevel = SFXMgr.performanceLevel;
	}
}
[Serializable]
public class ProfileMonitorData
{
	[SerializeField]public int currentBatches;
	// [SerializeField]public int currentSavedByBatching;
	[SerializeField]public float currentFillrate;
	[SerializeField]public int currentParticleSystemCount;
	[SerializeField]public int currentParticleCount;
}

[Serializable]
public class HighestCurrentProfile
{
	public HighestProfilePair Batches;
	public HighestProfilePair Fillrate;
	public HighestProfilePair ParticleSystemCount;
	public HighestProfilePair ParticleCount;
    public float AvgBatches;
    public float AvgFillrate;
    public float AvgParticleSystemCount;
    public float AvgParticleCount;
    public float AreaFillrate;
	public HighestCurrentProfile()
	{
		Batches = new HighestProfilePair(){time = 0, value = 0};
		Fillrate = new HighestProfilePair(){time = 0, value = 0};
		ParticleSystemCount = new HighestProfilePair(){time = 0, value = 0};
		ParticleCount = new HighestProfilePair(){time = 0, value = 0};
	}
}
[Serializable]
public struct HighestProfilePair
{
	public float time;
	public float value;
}
#endif