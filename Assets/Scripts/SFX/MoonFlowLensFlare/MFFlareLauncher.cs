using System;
using CFEngine;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Light))]

public class MFFlareLauncher : MonoBehaviour
{
    public bool directionalLight;
    public bool useLightIntensity;
    public FlarePriority priority = FlarePriority.Normal;
    public MFFlareAsset assetModel;
    public float fadeOffset;
    [HideInInspector] public FlareStatusData statusData;
    [HideInInspector] public Light lightSource;
    [HideInInspector, NonSerialized] public Vector3[] vertList;
    [HideInInspector, NonSerialized] public Color[] vertColorList;
    [HideInInspector, NonSerialized] public Vector2[] uvList;
    [HideInInspector, NonSerialized] public int[] triList;

    private Camera cam;
    private bool hasInit = false;
    private static readonly int FakeLightDir = Shader.PropertyToID("_FakeLightDir");
    private Transform _transform;
    private bool errorConfig = false;

    // [HideInInspector]public Texture2D tex;
    private void OnEnable()
    {
        _transform = transform;
    #if UNITY_EDITOR
        if (Application.isPlaying)
        {
    #endif
            AddFlare();
    #if UNITY_EDITOR
        }
    #endif
    }

    private void OnDestroy()
    {
        if (vertList != null)
        {
            ArrayPool<Vector3>.Release(vertList);
            vertList = null;
        }
        if (vertColorList != null)
        {
            ArrayPool<Color>.Release(vertColorList);
            vertColorList = null;
        }
        if (uvList != null)
        {
            ArrayPool<Vector2>.Release(uvList);
            uvList = null;
        }
        if (triList != null)
        {
            ArrayPool<int>.Release(triList);
            triList = null;
        }

    }
    public void AddFlare()
    {
        if (errorConfig) return;
        if(assetModel == null){
            DebugLog.AddEngineLog2("镜头光晕光源{0}没有配置图集或图集未能正确加载", gameObject.name);
            errorConfig = true;
            return;
        }
    #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            cam = Camera.main;
        }else
    #endif
        {
            cam = EngineUtility.GetMainCamera();
        }
        if (cam != null)
        {
            lightSource = GetComponent<Light>();
            // Add self to awake function: AddLight in URPLensFlare.cs on camera in render;
            if (vertList == null)
                vertList = ArrayPool<Vector3>.Get(assetModel.spriteBlocks.Count * 4);
            if (vertColorList == null)
                vertColorList = ArrayPool<Color>.Get(assetModel.spriteBlocks.Count * 4);
            if (uvList == null)
                uvList = ArrayPool<Vector2>.Get(assetModel.spriteBlocks.Count * 4);
            if (triList == null)
                triList = ArrayPool<int>.Get(assetModel.spriteBlocks.Count * 6);
            
            cam.GetComponent<MFLensFlare>().AddLight(this);
            hasInit = true;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
#endif
            if(!hasInit)AddFlare();
#if UNITY_EDITOR
        }
#endif
        Shader.SetGlobalVector(FakeLightDir, -_transform.forward);
    }

    public void UpdateMesh(Vector3 center, Vector2 halfScreen, Camera camera)
    {
        Texture2D tex = assetModel.flareSprite;//asset.flareSprite;
        float angle = (45 +Vector2.SignedAngle(Vector2.up, new Vector2(statusData.sourceCoordinate.x - halfScreen.x, statusData.sourceCoordinate.y - halfScreen.y))) / 180 * Mathf.PI;
        for (int i = 0; i < assetModel.spriteBlocks.Count; i++)
        {
            Rect rect = assetModel.spriteBlocks[i].block;
            Vector2 halfSize = new Vector2(
                tex.width * rect.width / 2 * assetModel.spriteBlocks[i].scale * (assetModel.fadeWithScale ? ( statusData.fadeoutScale * 0.5f + 0.5f) : 1), 
                tex.height * rect.height / 2 * assetModel.spriteBlocks[i].scale * (assetModel.fadeWithScale ? ( statusData.fadeoutScale * 0.5f + 0.5f) : 1));
            Vector3 flarePos = statusData.flareWorldPosCenter[i];
            if (assetModel.spriteBlocks[i].useRotation)
            {
                float magnitude = Mathf.Sqrt(halfSize.x * halfSize.x + halfSize.y * halfSize.y);
                float cos = magnitude * Mathf.Cos(angle);
                float sin = magnitude * Mathf.Sin(angle);
                vertList[i * 4] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x - sin, flarePos.y + cos, flarePos.z)) - center;
                vertList[i * 4 + 1] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x - cos, flarePos.y - sin, flarePos.z)) - center;
                vertList[i * 4 + 2] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x + cos, flarePos.y + sin, flarePos.z)) - center;
                vertList[i * 4 + 3] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x + sin, flarePos.y - cos, flarePos.z)) - center;
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x - sin, flarePos.y + cos, flarePos.z)) - center);
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x - cos, flarePos.y - sin, flarePos.z)) - center);
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x + cos, flarePos.y + sin, flarePos.z)) - center);
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x + sin, flarePos.y - cos, flarePos.z)) - center);
            }
            else
            {
                vertList[i * 4] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y + halfSize.y,
                        flarePos.z)) - center;
                vertList[i * 4 + 1] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y - halfSize.y,
                        flarePos.z)) - center;
                vertList[i * 4 + 2] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y + halfSize.y,
                        flarePos.z)) - center;
                vertList[i * 4 + 3] =
                    camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y - halfSize.y,
                        flarePos.z)) - center;
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y + halfSize.y, flarePos.z)) - center);
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x - halfSize.x, flarePos.y - halfSize.y, flarePos.z)) - center);
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y + halfSize.y, flarePos.z)) - center);
                // vertList.Add(camera.ScreenToWorldPoint(new Vector3(flarePos.x + halfSize.x, flarePos.y - halfSize.y, flarePos.z)) - center);

            }
            uvList[i * 4] = rect.position;
            uvList[i * 4 + 1] = rect.position + new Vector2(rect.width, 0);
            uvList[i * 4 + 2] = rect.position + new Vector2(0, rect.height);
            uvList[i * 4 + 3] = rect.position + rect.size;
            
            triList[i * 6] = i * 4;
            triList[i * 6 + 1] = i * 4 + 3;
            triList[i * 6 + 2] = i * 4 + 1;
            triList[i * 6 + 3] = i * 4;
            triList[i * 6 + 4] = i * 4 + 2;
            triList[i * 6 + 5] = i * 4 + 3;

            // uvList.Add(rect.position);
            // uvList.Add(rect.position + new Vector2(rect.width, 0));
            // uvList.Add(rect.position + new Vector2(0, rect.height));
            // uvList.Add(rect.position + rect.size);
            //
            // triList.Add(i * 4);
            // triList.Add(i * 4 + 3);
            // triList.Add(i * 4 + 1);
            // triList.Add(i * 4);
            // triList.Add(i * 4 + 2);
            // triList.Add(i * 4 + 3);
            
            Color vertexAddColor = assetModel.spriteBlocks[i].color;
            Color lightColor = default;
            Light source = lightSource;
            lightColor.r = Mathf.Lerp(1, source.color.r,
                assetModel.spriteBlocks[i].useLightColor);
            lightColor.g = Mathf.Lerp(1, source.color.g,
                assetModel.spriteBlocks[i].useLightColor);
            lightColor.b = Mathf.Lerp(1, source.color.b,
                assetModel.spriteBlocks[i].useLightColor);
            lightColor.a = 1;
            lightColor *= useLightIntensity ? source.intensity : 1;
            
            vertexAddColor *= new Vector4(lightColor.r, lightColor.g, lightColor.b, 
                (1.5f - Mathf.Abs(assetModel.spriteBlocks[i].offset)) / 1.5f
                * (1 - Mathf.Min(1, new Vector2(flarePos.x - halfScreen.x, flarePos.y - halfScreen.y).magnitude / new Vector2(halfScreen.x, halfScreen.y).magnitude))
            ) * ((assetModel.fadeWithAlpha ? statusData.fadeoutScale: 1));
            vertexAddColor = vertexAddColor.linear;
            vertColorList[i * 4] = vertexAddColor;
            vertColorList[i * 4 + 1] = vertexAddColor;
            vertColorList[i * 4 + 2] = vertexAddColor;
            vertColorList[i * 4 + 3] = vertexAddColor;
            
            // vertColorList.Add(vertexAddColor);
            // vertColorList.Add(vertexAddColor);
            // vertColorList.Add(vertexAddColor);
            // vertColorList.Add(vertexAddColor);
        }
        statusData.meshData.vertices = vertList;
        statusData.meshData.uv = uvList;
        statusData.meshData.triangles = triList;
        statusData.meshData.colors = vertColorList;
    }

    private void Reset()
    {
        lightSource = GetComponent<Light>();
    }
 
    private void OnDisable()
    {
        RemoveFlare();
    }

    public void RemoveFlare()
    {
        // Add self to awake function: RemoveLight in URPLensFlare.cs on camera in render;
        try
        {
            EngineUtility.GetMainCamera().GetComponent<MFLensFlare>().RemoveLight(this);
        }
        catch (Exception e)
        {
            DebugLog.AddLog("Main camera has been removed");
        }
    }
    
    public enum FlarePriority
    {
        Normal = 0,
        Timeline = 1
    }
    
}
//[Serializable]
public class FlareStatusData
{
    public Vector3 sourceCoordinate;
    public Vector3[] flareWorldPosCenter;
    public float fadeoutScale;
    public int fadeState;    //0:normal, 1:fade in, 2: fade out, 3:unrendered
    public Vector4 screenPos;
    public Mesh meshData;
}

#if UNITY_EDITOR
[CustomEditor(typeof(MFFlareLauncher))]
public class MFFlareLauncherEditor : Editor
{
    public MFFlareLauncher _target;

    private void OnEnable()
    {
        _target = target as MFFlareLauncher;
    }

    public override void OnInspectorGUI()
    {
        bool needforce = Application.isPlaying;
        
        if (GUILayout.Button(needforce? "手动初始化光晕":"初始化光晕"))
        {
            if(!needforce)MFLensFlare.singleton.Init();
            MFLensFlare.singleton.DebugMode = true;
            _target.AddFlare();
        }

        if (GUILayout.Button(needforce ? "手动清除光晕" : "清除光晕"))
        {
            _target.RemoveFlare();
            MFLensFlare.singleton.DebugMode = false;
        }
        
        base.OnInspectorGUI();
    }
}
#endif