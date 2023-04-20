#if UNITY_EDITOR
using CFEngine;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Ambient = UnityEngine.Rendering.Universal.Ambient;
using LightInfo = UnityEngine.Rendering.Universal.LightInfo;
using Lighting = UnityEngine.Rendering.Universal.Lighting;


public class LightingAnimation : VolumeAnimation
{
    public BoolAnimation cameraSpaceLight;
    // public LightAnimation mainLight;
    // public LightAnimation addLight;
    public HDRColorAnimation roleLightColor;
    // public FloatAnimation roleHeightSHAlpha;
    // public FloatAnimation roleHeightSHFadeout;
    [Space(10)]
    // public bool lightParamOverride;
    // public BoolAnimation useNoneLight;
    public FloatAnimation minRoleHAngle;
    public FloatAnimation maxRoleHAngle;
    public BoolAnimation addRoleLight;
    public FloatAnimation roleLightRotOffset;
    
    // public bool roleLightDirParamOverride;
    // public FloatAnimation leftRightControl;
    // public FloatAnimation upDownControl;
    //param:RimLightColor
    
    // public bool lightParam1Override;.
    // param:SimpleLightIntensity
    // param:AddLightSpecScope
    // public FloatAnimation roleFaceHAngle;
    [Space(10)]
    public HDRColorAnimation roleShadowColor;
    // public LightAnimation waterLight;
    // public FloatAnimation shadowRimIntensity;
    
    
    private Lighting _volumeLighting;
    private Ambient _volumeAmbient;

#if UNITY_EDITOR
#endif
    protected override void OnEnable()
    {
        base.OnEnable();
        Search(ref _volumeLighting);
        Search(ref _volumeAmbient);
        // StartCoroutine(Clear());
    }

    public override void RefreshData()
    {
        _volumeLighting.cameraSpaceLighting.overrideState = cameraSpaceLight.overrideState;
        if (cameraSpaceLight.overrideState) _volumeLighting.cameraSpaceLighting.value = cameraSpaceLight.value;
        // _volumeLighting.usingNoneLight.overrideState = useNoneLight.overrideState;
        // if (useNoneLight.overrideState)
        // {
        //     _volumeLighting.usingNoneLight.value = useNoneLight.value;
        // }
        _volumeLighting.roleLightColor.overrideState = roleLightColor.overrideState;
        if (roleLightColor.overrideState)
        {
            // float intensity = Mathf.Pow(2, roleLightColor.color.w);
            // _volumeLighting.roleLightColor.value = new Color(roleLightColor.color.x * intensity,
            //     roleLightColor.color.y * intensity, roleLightColor.color.z * intensity,
            //     1);
            _volumeLighting.roleLightColor.value = roleLightColor.color;
        }
        _volumeAmbient.roleShadowColor.overrideState = roleShadowColor.overrideState;
        if (roleShadowColor.overrideState)
        {
            _volumeAmbient.roleShadowColor.value = roleShadowColor.color;
            // float intensity = Mathf.Pow(2, roleShadowColor.color.w);
            // _volumeAmbient.roleShadowColor.value = new Color(roleShadowColor.color.x * intensity,
            //     roleShadowColor.color.y * intensity, roleShadowColor.color.z * intensity);
        }
        _volumeLighting.addRoleLight.overrideState = addRoleLight.overrideState;
        if(addRoleLight.overrideState)_volumeLighting.addRoleLight.value = addRoleLight.value;
        _volumeLighting.minRoleHAngle.overrideState = minRoleHAngle.overrideState;
        if(minRoleHAngle.overrideState)_volumeLighting.minRoleHAngle.value = minRoleHAngle.value;
        _volumeLighting.maxRoleHAngle.overrideState = maxRoleHAngle.overrideState;
        if(maxRoleHAngle.overrideState)_volumeLighting.maxRoleHAngle.value = maxRoleHAngle.value;
        _volumeLighting.roleLightRotOffset.overrideState = roleLightRotOffset.overrideState;
        if(roleLightRotOffset.overrideState)_volumeLighting.roleLightRotOffset.value = roleLightRotOffset.value;
        _volumeLighting.OnOverrideFinish();
        // URPTimelineVolumnCenter.instance.TimelineVolume.weight = 1;
        
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(LightingAnimation))]
public class LightingAnimationEditor : VolumeAnimationEditor
{
    public LightingAnimation _target;
    private SerializedProperty _cameraSpaceLight;
    private SerializedProperty _useNoneLight;
    private SerializedProperty _maxRoleHAngle;
    private SerializedProperty _roleLightColor;
    private SerializedProperty _minRoleHAngle;
    private SerializedProperty _roleLightRotOffset;
    private SerializedProperty _roleShadowColor;
    private SerializedProperty _addRoleLight;
    // private SerializedObject _so;
    protected override void OnEnable()
    {
        base.OnEnable();
        _target = target as LightingAnimation;
        // _so = new SerializedObject(target);
        _cameraSpaceLight = serializedObject.FindProperty("cameraSpaceLight");
        _useNoneLight = serializedObject.FindProperty("useNoneLight");
        _maxRoleHAngle = serializedObject.FindProperty("maxRoleHAngle");
        _roleLightColor = serializedObject.FindProperty("roleLightColor");
        _minRoleHAngle = serializedObject.FindProperty("minRoleHAngle");
        _roleLightRotOffset = serializedObject.FindProperty("roleLightRotOffset");
        _roleShadowColor = serializedObject.FindProperty("roleShadowColor");
        _addRoleLight = serializedObject.FindProperty("addRoleLight");
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawBoolAnimation(ref _cameraSpaceLight, "Camera Space Light");
        // DrawBoolAnimation(ref _useNoneLight, "Using None Light");
        DrawHDRColorAnimation(ref _roleLightColor, "Role Light");
        DrawHDRColorAnimation(ref _roleShadowColor, "Role Shadow Light");
        
        EditorGUILayout.LabelField("Light Param", style);
        DrawFloatAnimation(ref _minRoleHAngle, "MinRoleHAngle", -45, 0);
        DrawFloatAnimation(ref _maxRoleHAngle, "MaxRoleHAngle", -80, 45);
        DrawBoolAnimation(ref _addRoleLight, "AddRoleLight");
        DrawFloatAnimation(ref _roleLightRotOffset, "RoleLightRotOffset", 0, 360);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed LightingAnimation");
            SetBoolAnimation(ref _target.cameraSpaceLight, ref _cameraSpaceLight);
            // SetBoolAnimation(ref _target.useNoneLight, ref _useNoneLight);
            SetHDRColorAnimation(ref _target.roleLightColor, ref _roleLightColor);
            SetHDRColorAnimation(ref _target.roleShadowColor, ref _roleShadowColor);
            SetFloatAnimation(ref _target.minRoleHAngle, ref _minRoleHAngle);
            SetFloatAnimation(ref _target.maxRoleHAngle, ref _maxRoleHAngle);
            SetBoolAnimation(ref _target.addRoleLight, ref _addRoleLight);
            SetFloatAnimation(ref _target.roleLightRotOffset, ref _roleLightRotOffset);
        }
        
    }

    public void DrawLightAnimation(ref LightAnimation lightAnimation)
    {
        lightAnimation.light =
            EditorGUILayout.ObjectField("Source", lightAnimation.light, typeof(Light), true) as Light;
        lightAnimation.info.color = EditorGUILayout.ColorField(new GUIContent("Color"), lightAnimation.info.color,true, false,true);
    }
}

#endif
