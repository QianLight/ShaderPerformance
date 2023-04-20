using System;
using CFEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using LightInfo = UnityEngine.Rendering.Universal.LightInfo;


[ExecuteInEditMode]
[RequireComponent(typeof(Volume))] 
public abstract class VolumeAnimation : MonoBehaviour
{
    protected Volume _volume;
    private URPTimelineVolumnCenter _instance;
    protected virtual void OnEnable()
    {
        FindTimelineVolume();
       
        // StartCoroutine(Clear());
        UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering -= SetUp;
        UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering += SetUp;
    }

    private void SetUp(ScriptableRenderContext arg1, Camera[] arg2)
    {
        RefreshData();
    }


    public void FindTimelineVolume()
    {
        TryGetComponent(out _volume);
        if (_volume == null)
        {
            var volumeInScene = GameObject.FindObjectsOfType(typeof(Volume), true);
            for (int i = 0; i < volumeInScene.Length; i++)
            {
                Volume v = volumeInScene[i] as Volume;
                if (v.isTimeline)
                {
                    DebugLog.AddErrorLog($"场景已有Timeline用Volume且不是本对象，请勿重复挂载");
                #if UNITY_EDITOR
                    if(!Application.isPlaying)
                        DestroyImmediate(this);
                #else
                    Destroy(this);
                #endif
                    return;
                }
            }
            if (URPTimelineVolumnCenter.instance.TimelineVolume == null)
            {
                Debug.LogError("由于无法代码挂载Asset，请手动添加Volume组件及CommonTimelineVolume配置");
            }
        }

        if (URPTimelineVolumnCenter.instance != null)
        {
            _instance = URPTimelineVolumnCenter.instance;
            _instance.EnableComponent(_volume);
        }
        

    }
    protected void Search<T>(ref T volumeComp) where T:VolumeComponent
    {
        bool find = false;
        VolumeProfile sp = null;
#if UNITY_EDITOR
        if (!Application.isPlaying || !EngineContext.IsRunning)
        {
            sp = _instance.TimelineVolume.sharedProfile;
        }
        else
        {
#endif
            sp = _instance.TimelineVolume.profile;
#if UNITY_EDITOR
            string name = _instance.TimelineVolume.sharedProfile.name;
            sp.name = name + "(ins)";
        }
#endif

        for (int i = 0; i < sp.components.Count; i++)
        {
            if (sp.components[i] is T)
            {
                volumeComp = sp.components[i] as T;
                find = true;
                break;
            }
        }

        if (!find)
        {
            volumeComp = ScriptableObject.CreateInstance<T>();
            var oldProfile = sp;
            oldProfile.components.Add(volumeComp);
            _instance.TimelineVolume.profile = oldProfile;
            Debug.LogError($"需要开启{typeof(T)}组件, 已自动添加");
        }
    }
    // private void Update()
    // {
    //     RefreshData();
    // }

    private void OnDisable()
    {
        var insProfile = _instance.TimelineVolume.profile;
        if (insProfile != null && !ReferenceEquals(insProfile, _instance.TimelineVolume.sharedProfile))
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                Destroy(insProfile);
#if UNITY_EDITOR
            }
            else
            {
                DestroyImmediate(insProfile);
            }
#endif
        }
        _instance?.DisableComponent();
        
        UnityEngine.Rendering.RenderPipelineManager.beginFrameRendering -= SetUp;
    }

    public abstract void RefreshData();
}

#if UNITY_EDITOR
public abstract class VolumeAnimationEditor : Editor
{
    protected GUIStyle style;

    protected virtual void OnEnable()
    {
        style = new GUIStyle() {fontStyle = FontStyle.Bold, normal = new GUIStyleState() {textColor = Color.white}};
    }
    public void DrawFloatAnimation(ref SerializedProperty floatAnimation, string name)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(floatAnimation.FindPropertyRelative("overrideState"), new GUIContent(), GUILayout.Width(10));
            EditorGUILayout.PropertyField(floatAnimation.FindPropertyRelative("value"), new GUIContent(name));
        }
    }
    public void DrawMinFloatAnimation(ref SerializedProperty floatAnimation, string name, float min)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(floatAnimation.FindPropertyRelative("overrideState"), new GUIContent(), GUILayout.Width(10));
            EditorGUILayout.PropertyField(floatAnimation.FindPropertyRelative("value"), new GUIContent(name));
            float value = floatAnimation.FindPropertyRelative("value").floatValue;
            value = Mathf.Max(min, value);
            floatAnimation.FindPropertyRelative("value").floatValue = value;
        }
    }
    public void DrawFloatAnimation(ref SerializedProperty floatAnimation, string name, float min, float max)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(floatAnimation.FindPropertyRelative("overrideState"), new GUIContent(), GUILayout.Width(10));
            EditorGUILayout.PropertyField(floatAnimation.FindPropertyRelative("value"), new GUIContent(name));
            float value = floatAnimation.FindPropertyRelative("value").floatValue;
            value = Mathf.Max(min, value);
            value = Mathf.Min(max, value);
            floatAnimation.FindPropertyRelative("value").floatValue = value;
        }
    }
    
    public void SetFloatAnimation(ref FloatAnimation data, ref SerializedProperty spData)
    {
        data.overrideState = spData.FindPropertyRelative("overrideState").boolValue;
        data.value = spData.FindPropertyRelative("value").floatValue;
    }

    public void DrawColorAnimation(ref SerializedProperty colorAnimation, string name)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(colorAnimation.FindPropertyRelative("overrideState"), new GUIContent(), GUILayout.Width(10));
            EditorGUILayout.PropertyField(colorAnimation.FindPropertyRelative("color"), new GUIContent(name));
        }
    }
    public void DrawHDRColorAnimation(ref SerializedProperty colorAnimation, string name)
    {
        using (new EditorGUILayout.VerticalScope())
        {
            Color color;
            using (new EditorGUILayout.HorizontalScope())
            {
                // colorAnimation.overrideState =
                EditorGUILayout.PropertyField(colorAnimation.FindPropertyRelative("overrideState"), new GUIContent(), GUILayout.Width(10));
                color = EditorGUILayout.ColorField(new GUIContent(name+" Color"), 
                    new Color(
                        colorAnimation.FindPropertyRelative("color").colorValue.r, 
                        colorAnimation.FindPropertyRelative("color").colorValue.g,
                        colorAnimation.FindPropertyRelative("color").colorValue.b, 
                        colorAnimation.FindPropertyRelative("color").colorValue.a),
                    true, false, true);
            }
            // using (new EditorGUILayout.HorizontalScope())
            // {
            //     EditorGUILayout.LabelField("", GUILayout.Width(10));
            //     float intensity =
            //         EditorGUILayout.FloatField(name+" Intensity", colorAnimation.FindPropertyRelative("color").colorValue.a);
            // }
            colorAnimation.FindPropertyRelative("color").colorValue = color;
        }
    }

    public void SetColorAnimation(ref ColorAnimation data, ref SerializedProperty spData)
    {
        data.overrideState = spData.FindPropertyRelative("overrideState").boolValue;
        data.color = spData.FindPropertyRelative("color").colorValue;
    }
    public void SetHDRColorAnimation(ref HDRColorAnimation data, ref SerializedProperty spData)
    {
        data.overrideState = spData.FindPropertyRelative("overrideState").boolValue;
        data.color = spData.FindPropertyRelative("color").colorValue;
    }

    public void DrawBoolAnimation(ref SerializedProperty boolAnimation, string name)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PropertyField(boolAnimation.FindPropertyRelative("overrideState"), new GUIContent(), GUILayout.Width(10));
            EditorGUILayout.PropertyField(boolAnimation.FindPropertyRelative("value"), new GUIContent(name));
        }
    }
    public void SetBoolAnimation(ref BoolAnimation data, ref SerializedProperty spData)
    {
        data.overrideState = spData.FindPropertyRelative("overrideState").boolValue;
        data.value = spData.FindPropertyRelative("value").boolValue;
    }

}
#endif
[Serializable]
public struct FloatAnimation
{
    public bool overrideState;
    public float value;
}

[Serializable]
public struct ColorAnimation
{
    public bool overrideState;
    public Color color;
}
[Serializable]
public struct HDRColorAnimation
{
    public bool overrideState;
    [ColorUsage(false, true)]public Color color;
}
[Serializable]
public struct BoolAnimation
{
    public bool overrideState;
    public bool value;
}

[Serializable]
public struct LightAnimation
{
    public bool overrideState;
    public Light light;
    public LightInfo info;
}
