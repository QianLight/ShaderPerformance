using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CFEngine;
using CFUtilPoolLib;
using Unity.Collections;
using UnityEngine.Rendering;
[ExecuteInEditMode]
public class MFLensFlare : MonoBehaviour
{
    public static bool TimelineAvoid = false;
    public bool DebugMode;
    [Space(10)] 
    public Material material;
    public float fadeoutTime;

    public static MFLensFlare singleton;
    private Camera _camera;
    private Vector2 _halfScreen;
    private MaterialPropertyBlock _propertyBlock;
    private Dictionary<MFFlareLauncher, FlareStatusData> _lightToDataDict;
    private Queue<Mesh> _flareMeshPool;

    private Vector3[] _tempVertList;
    private Vector2[] _tempUvList;
    private int[] _tempTriangleList;
    private Color[] _tempVertColors;

    private static readonly int STATIC_FLARESCREENPOS = Shader.PropertyToID("_FlareScreenPos");
    private static readonly int STATIC_BaseMap = Shader.PropertyToID("_MainTex");
    private static readonly float DISTANCE = 1f;
    private bool hasInit = false;
    private float _timer;
    private RTimeline _timeline;
    private void OnEnable()
    {
    #if UNITY_EDITOR
        singleton = this;
        if (Application.isPlaying)
        {
    #endif
            if (!hasInit)
            {
                singleton = this;
                Init();
                
            }
    #if UNITY_EDITOR
        }
    #endif
    }

    public void Init()
    {
        _lightToDataDict = new Dictionary<MFFlareLauncher, FlareStatusData>();
        _flareMeshPool = new Queue<Mesh>();
        _camera = GetComponent<Camera>();
        _propertyBlock = new MaterialPropertyBlock();
        _timeline = RTimeline.singleton;
        hasInit = true;
    }
    private FlareStatusData InitFlareData(int count)
    {
        if (!hasInit) Init();
        return new FlareStatusData
        {
            sourceCoordinate = Vector3.zero,
            flareWorldPosCenter = new Vector3[count],
            // edgeScale = 1,
            fadeoutScale = 0,
            fadeState = 1,
            screenPos = Vector4.zero,
            meshData = _flareMeshPool.Count == 0 ? new Mesh() : _flareMeshPool.Dequeue()
        };
    }

    public void AddLight(MFFlareLauncher mfFlareLauncher)
    {
        Shader.EnableKeyword("_FAKELIGHT");
        if (DebugMode)
        {
            DebugLog.AddLog("Add Light " + mfFlareLauncher.gameObject.name + " to FlareList");
        }

        if (mfFlareLauncher.assetModel == null)
        {
            DebugLog.AddErrorLog($"光源{mfFlareLauncher.gameObject.name} 未配置光晕asset");
            return;
        }

        mfFlareLauncher.statusData = InitFlareData(mfFlareLauncher.assetModel.spriteBlocks.Count);
        _lightToDataDict.Add(mfFlareLauncher, mfFlareLauncher.statusData);
    }

    public void RemoveLight(MFFlareLauncher flareLauncher)
    {
        if(DebugMode)DebugLog.AddLog("Remove Light " + flareLauncher.gameObject.name + " from FlareList");
        if (_lightToDataDict.TryGetValue(flareLauncher, out FlareStatusData flareState))
        {
            flareState.meshData.Clear();
            _flareMeshPool.Enqueue(flareState.meshData);
            _lightToDataDict.Remove(flareLauncher);
        }
        else
        {
            DebugLog.AddLog($"Light {flareLauncher.gameObject.name} 光晕信息不在列表中");
        }

        if (_lightToDataDict.Count == 0)
        {
            Shader.DisableKeyword("_FAKELIGHT");
        }
    }
    private void LateUpdate()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying && !DebugMode) return;
        #endif
        if (!hasInit) return;
        if (!UnityEngine.Rendering.Universal.GameQualitySetting.LensFlares) return;
        _timer += Time.deltaTime;
        bool updateData = _timer >= 0.033f;
        if (updateData)
        {
            _timer = 0;
            _halfScreen = new Vector2(_camera.scaledPixelWidth / 2 + _camera.pixelRect.xMin,
                _camera.scaledPixelHeight / 2 + _camera.pixelRect.yMin);
            foreach (var pair in _lightToDataDict)
            {
                if (TimelineSkipCheck(pair)) continue;
                FlareStatusData data = pair.Value;
                GetSourceCoordinate(pair);
                bool isIn = CheckIn(pair);
                if (data.fadeoutScale > 0)
                {
                    if (!isIn)
                    {
                        data.fadeState = 2;
                    }
                }

                if (data.fadeoutScale < 1)
                {
                    if (isIn)
                    {
                        data.fadeState = 1;
                    }
                }

                if (!isIn && data.fadeoutScale <= 0)
                {
                    if (data.fadeState != 3)
                    {
                        data.fadeState = 3;
                    }
                }
                else
                {
                    CalculateMeshData(pair);
                }

                switch (data.fadeState)
                {
                    case 1:
                        data.fadeoutScale += Time.deltaTime / fadeoutTime;
                        data.fadeoutScale = Mathf.Clamp(data.fadeoutScale, 0, 1);
                        break;
                    case 2:
                        data.fadeoutScale -= Time.deltaTime / fadeoutTime;
                        data.fadeoutScale = Mathf.Clamp(data.fadeoutScale, 0, 1);
                        break;
                    case 3:
                        // RemoveLight(lightSource[i]);
                        break;
                    default:
                        break;
                }
                // _lightToDataDict[pair.Key] = data;
            }

        }

        var cameraTransform = _camera.transform;
        var center = cameraTransform.position + cameraTransform.forward * 0.1f;
        CreateMesh(center, updateData);

        if (DebugMode)
        {
            Debug.Log("Lens Flare : " + _lightToDataDict.Count + " lights");
            foreach (var pair in _lightToDataDict)
            {
                DebugDrawMeshPos(pair);
            }
        }
        
    }

    private bool TimelineSkipCheck(KeyValuePair<MFFlareLauncher, FlareStatusData> pair)
    {
        if (TimelineAvoid)
        {
            if ((int)pair.Key.priority < (int)MFFlareLauncher.FlarePriority.Timeline)
            {
                return true;
            }
        }

        return false;
    }

    bool CheckIn(KeyValuePair<MFFlareLauncher, FlareStatusData> pair)
    {
        var lightSource = pair.Key;
        var statusData = pair.Value;
        //范围修正减少太阳角度过高造成可视范围小的问题
        var dir = lightSource.transform.forward;
        float realOffset = (lightSource.fadeOffset + 0.5f);
        if (statusData.sourceCoordinate.x <  _camera.pixelRect.xMin - realOffset
            || statusData.sourceCoordinate.y < _camera.pixelRect.yMin - realOffset
            || statusData.sourceCoordinate.x > _camera.pixelRect.xMax + realOffset
            || statusData.sourceCoordinate.y > _camera.pixelRect.yMax + realOffset
            || statusData.sourceCoordinate.z < 0
            /*|| Vector3.Dot(lightSource.directionalLight ? -dir : lightSource.transform.position - _camera.transform.position, _camera.transform.forward) < 0*//*0.25f*/)
        {
            statusData.screenPos = Vector4.zero;
            return false;
        }
        else
        {
            // ******旧算法：使用射线检测*******
            // var camPos = _camera.transform.position;
            // var targetPos = lightSource[lightIndex].directionalLight
            //     ? -lightSource[lightIndex].transform.forward * 10000f
            //     : lightSource[lightIndex].transform.position;
            // Ray ray = new Ray(camPos, targetPos - camPos );
            // RaycastHit hit;
            // Physics.Raycast(ray, out hit);
            // if (Vector3.Distance(hit.point, camPos) < Vector3.Distance(targetPos, camPos))
            // {
            //     if (hit.point == Vector3.zero) return true;
            //     return false;
            // }
            // ******新算法：传入光源屏幕坐标在shader里用深度图算遮挡***************
            Vector4 screenUV = statusData.sourceCoordinate;
            screenUV.x = screenUV.x / _camera.pixelWidth;
            screenUV.y = screenUV.y / _camera.pixelHeight;
            screenUV.w =lightSource.directionalLight ? 1 : 0;
            statusData.screenPos = screenUV;
            // _lightToDataDict[lightSource] = statusData;
            return true;
        }
    }

    void GetSourceCoordinate(KeyValuePair<MFFlareLauncher, FlareStatusData> pair)
    {
        var launcher = pair.Key;
        var statusData = pair.Value;
        statusData.sourceCoordinate = _camera.WorldToScreenPoint(
            launcher.directionalLight 
            ?_camera.transform.position - launcher.transform.forward * 10000
            : launcher.transform.position
            );
    }

    void CalculateMeshData(KeyValuePair<MFFlareLauncher, FlareStatusData> pair)
    {
        var lightSource = pair.Key;
        var statusData = pair.Value;
        for (int i = 0; i < lightSource.assetModel.spriteBlocks.Count; i++)
        {
            Vector2 realSourceCoordinateOffset = new Vector2(statusData.sourceCoordinate.x - _halfScreen.x, statusData.sourceCoordinate.y - _halfScreen.y);
            Vector2 realOffset = realSourceCoordinateOffset * lightSource.assetModel.spriteBlocks[i].offset;
            statusData.flareWorldPosCenter[i] = new Vector3(_halfScreen.x + realOffset.x, _halfScreen.y + realOffset.y, DISTANCE);
        }
    }

    void CreateMesh(Vector3 center, bool UpdateData)
    {
        foreach (var pair in _lightToDataDict)
        {
            if (TimelineSkipCheck(pair)) continue;
            var lightSource = pair.Key;
            var flareData = pair.Value;

            if (flareData.fadeoutScale > 0)
            {
                if(UpdateData)lightSource.UpdateMesh(center, _halfScreen, _camera);
                _propertyBlock.SetTexture(STATIC_BaseMap, lightSource.assetModel.flareSprite);
                _propertyBlock.SetVector(STATIC_FLARESCREENPOS, flareData.screenPos);
                Graphics.DrawMesh(flareData.meshData, center, Quaternion.identity, material, 0, _camera, 0, _propertyBlock);
            }
        }
    }
    void DebugDrawMeshPos(KeyValuePair<MFFlareLauncher, FlareStatusData> pair)
    {
        for (int i = 0; i < pair.Key.assetModel.spriteBlocks.Count; i++)
        {
            Debug.DrawLine(_camera.transform.position, _camera.ScreenToWorldPoint(pair.Value.flareWorldPosCenter[i]));
        }
    }

    private void OnDestroy()
    {
        #if UNITY_EDITOR
        if (!hasInit) return;
        #endif
        Shader.DisableKeyword("_FAKELIGHT");
        foreach (var pair in _lightToDataDict)
        {
            Destroy(pair.Value.meshData);
        }
        while (_flareMeshPool.Count > 0)
        {
            Mesh mesh = _flareMeshPool.Dequeue();
            Destroy(mesh);
        }

        hasInit = false;
    }
}
