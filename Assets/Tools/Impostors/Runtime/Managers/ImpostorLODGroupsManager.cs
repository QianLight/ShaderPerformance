using System;
using System.Collections.Generic;
using Impostors.Attributes;
using Impostors.MemoryUsage;
using Impostors.ObjectPools;
using Impostors.TimeProvider;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Impostors.Managers
{
    [DefaultExecutionOrder(-777)]
    public class ImpostorLODGroupsManager : MonoBehaviour, IMemoryConsumer
    {
        public static ImpostorLODGroupsManager Instance { get; private set; }

        public ITimeProvider TimeProvider { get; private set; }

        [SerializeField, DisableAtRuntime]
        private bool _HDR = false;

        [SerializeField, DisableAtRuntime]
        private bool _useMipMap = false;

        [SerializeField, DisableAtRuntime]
        private float _mipMapBias = 0;

        [FormerlySerializedAs("_cutout")]
        [Range(0f, 1f)]
        [SerializeField]
        public float cutout = 0.8f;

        [FormerlySerializedAs("_cutoutTransparentFill")]
        [Range(0f, 1f)]
        [SerializeField]
        public float _cutoutTransparentFill = 0.9925f;

        [FormerlySerializedAs("_minAngleToStopLookAtCamera")]
        [Range(0f, 180f)]
        [SerializeField]
        public float minAngleToStopLookAtCamera = 30;

        [SerializeField, DisableAtRuntime]
        private Shader _shader = default;

        [SerializeField]
        private Texture _ditherTexture = default;

        [Space]
        [Header("Runtime")]
        public CompositeRenderTexturePool RenderTexturePool;

        public MaterialObjectPool MaterialObjectPool;

        [SerializeField]
        private List<ImpostorableObjectsManager> _impostorsManagers = default;

        [SerializeField]
        private List<ImpostorLODGroup> _impostorLodGroups = default;

        private Dictionary<int, ImpostorLODGroup> _dictInstanceIdToImpostorLODGroup;

        private bool _isDestroying = false;

        private void OnEnable()
        {
            _isDestroying = false;
            Instance = this;
            _impostorLodGroups = new List<ImpostorLODGroup>();
            _impostorsManagers = _impostorsManagers ?? new List<ImpostorableObjectsManager>();
            if (!_shader)
                _shader = Shader.Find("Impostors/ImpostorsShader");
            _dictInstanceIdToImpostorLODGroup = new Dictionary<int, ImpostorLODGroup>();
            TimeProvider = new UnscaledTimeProvider();
            RenderTexturePool = new CompositeRenderTexturePool(
                Enum.GetValues(typeof(AtlasResolution)) as int[], 0, 8,
                _useMipMap, _mipMapBias, GetRenderTextureFormat());
            MaterialObjectPool = new MaterialObjectPool(0, _shader);
        }

        private void OnDisable()
        {
            _isDestroying = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_ditherTexture)
                _ditherTexture = Resources.Load<Texture>("impostors-dither-pattern");
            if (!_shader)
                _shader = Shader.Find("Impostors/ImpostorsShader");
            if (!_ditherTexture)
                Debug.LogError("[IMPOSTORS] Impostors fading won't work without specifying dither pattern texture! " +
                               "Default path is 'Assets/Impostors/Runtime/Resources/impostors-dither-pattern.png'.",
                    this);
            if (!_shader)
                Debug.LogError("[IMPOSTORS] Impostors won't work without specifying right shader! " +
                               "Default path is 'Assets/Impostors/Runtime/Resources/Shaders/ImpostorsShader.shader'.",
                    this);
        }

        private void Reset()
        {
            OnValidate();
        }
#endif

        private void Update()
        {
            try
            {
                TimeProvider.Update();
                Shader.SetGlobalVector(ShaderProperties._ImpostorsTimeProvider,
                    new Vector4(TimeProvider.Time, TimeProvider.DeltaTime, 0, 0));
                Shader.SetGlobalTexture(ShaderProperties._ImpostorsNoiseTexture, _ditherTexture);
                Shader.SetGlobalFloat(ShaderProperties._ImpostorsNoiseTextureResolution, _ditherTexture.width);
                Shader.SetGlobalFloat(ShaderProperties._ImpostorsCutout, cutout);
                Shader.SetGlobalFloat(ShaderProperties._ImpostorsMinAngleToStopLookAt, minAngleToStopLookAtCamera);

                if (_lastCutoutTransparentFill != _cutoutTransparentFill)
                {
                    _lastCutoutTransparentFill = _cutoutTransparentFill;
                    UpdateCutoutTransparentFill();
                }
                //Shader.SetGlobalFloat(ShaderProperties._ImpostorAlpha, _cutoutTransparentFill);

                if (delayDo)
                {
                    delayDo = false;
                    foreach (ImpostorLODGroup lod in delayList)
                    {
                        lod.DoEnable();
                    }

                    delayList.Clear();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
                enabled = false;
            }
        }

        private float _lastCutoutTransparentFill = 0;

        public void UpdateCutoutTransparentFill()
        {
            for (int i = 0; i < newMaterialsList.Count; i++)
            {
                Material mat = newMaterialsList[i];
                if (mat == null) continue;
                mat.SetFloat(ShaderProperties._ImpostorAlpha, _cutoutTransparentFill);
            }
        }
        List<ImpostorLODGroup> delayList = new List<ImpostorLODGroup>();
        public void DelayAddImpostor(ImpostorLODGroup impostorLodGroup)
        {
            delayList.Add(impostorLodGroup);
        }
        private bool delayDo = false; 
        public void DoImpostorEnable()
        {
            delayDo = true;
        }

        public int AddImpostorLODGroup(ImpostorLODGroup impostorLodGroup)
        {
            if (_isDestroying)
                return -1;
            _impostorLodGroups.Add(impostorLodGroup);
            _dictInstanceIdToImpostorLODGroup.Add(impostorLodGroup.GetInstanceID(), impostorLodGroup);

            for (int i = 0; i < _impostorsManagers.Count; i++)
            {
                _impostorsManagers[i].AddImpostorableObject(impostorLodGroup);
            }

            return _impostorLodGroups.Count - 1;
        }

        private static string Leaf = "URP/Scene/TreeLeaf";
        private static string StylizedTreeLeaf = "URP/Scene/StylizedTreeLeaf";

        public List<Material> newMaterialsList = new List<Material>();
        
        private Dictionary<Material, Material> replaceNewMatDic = new Dictionary<Material, Material>();

        public void TryReplaceTransparentMaterials(List<Material> allMats)
        {
            for (int i =  allMats.Count-1; i >=0; i--)
            {
                Material oriMat = allMats[i];

                if (oriMat == null)
                {
                    allMats.RemoveAt(i);
                    continue;
                }


                if (replaceNewMatDic.ContainsKey(oriMat))
                {
                    allMats[i] = replaceNewMatDic[oriMat];
                }
                else
                {
                    if (oriMat.shader.name.Equals(Leaf)||oriMat.shader.name.Equals(StylizedTreeLeaf))
                    {
                        Material newMaterial = new Material(oriMat);

                        //Material.shader = TreeLeafImpostor;
                        // shader uniform 默认值是0，也就是说其他shader的默认值都是0
                        // 即fogIntensity = fogIntensity * (1 - _FogToggle)，其他shader显示的颜色才是正常的。 
                        // 所以这里需要disable的值为1。

                        if (!ImpostorableObjectsManager._instance.isFogOpen)
                        {
                            const float disable = 1.0f;
                            newMaterial.SetFloat(ShaderProperties._FogToggle, disable);
                        }

                        newMaterial.SetInt(ShaderProperties._ZWriteImpostor, 1);
                        newMaterial.SetInt(ShaderProperties._ZTestImpostor, 4);
                        newMaterial.SetFloat(ShaderProperties._ImpostorAlpha,
                            _cutoutTransparentFill);
                        newMaterial.SetFloat(ShaderProperties._ImpostorEnable, 1);
                        
                        newMaterial.EnableKeyword("_ALPHATEST_ON");
                        
                        //newMaterial.EnableKeyword("_ALPHA_TEST_IMPOSTOR");
                        allMats[i] = newMaterial;
                        newMaterialsList.Add(newMaterial);
                        replaceNewMatDic.Add(oriMat, newMaterial);
                        _lastCutoutTransparentFill = _cutoutTransparentFill;
                    }
                }
            }
        }

        public void OnDestroy()
        {
            RenderTexturePool.Destory();
            
            CollectionsPool.ClearAll();
        }

        public void RemoveImpostorLODGroup(ImpostorLODGroup impostorLodGroup)
        {
            if (_isDestroying)
                return;
            int index = impostorLodGroup.IndexInImpostorsManager;

            if(index<0 ||index>=_impostorLodGroups.Count) return;
            
            if (_impostorLodGroups[index] != impostorLodGroup) return;

            Assert.AreEqual(impostorLodGroup, _impostorLodGroups[index]);

            _impostorLodGroups[index] = _impostorLodGroups[_impostorLodGroups.Count - 1];
            _impostorLodGroups[index].IndexInImpostorsManager = index;
            _impostorLodGroups.RemoveAt(_impostorLodGroups.Count - 1);

            for (int i = 0; i < _impostorsManagers.Count; i++)
            {
                _impostorsManagers[i].RemoveImpostorableObject(impostorLodGroup, index);
            }

            _dictInstanceIdToImpostorLODGroup.Remove(impostorLodGroup.GetInstanceID());
        }

        internal void RegisterImpostorableObjectsManager(ImpostorableObjectsManager manager)
        {
            if (_impostorsManagers.Contains(manager))
                return;
            _impostorsManagers.Add(manager);

            for (int i = 0; i < _impostorLodGroups.Count; i++)
            {
                manager.AddImpostorableObject(_impostorLodGroups[i]);
            }
        }

        internal void UnregisterImpostorableObjectsManager(ImpostorableObjectsManager manager)
        {
            _impostorsManagers.Remove(manager);
        }

        public ImpostorLODGroup GetByInstanceId(int instanceId)
        {
            if (_dictInstanceIdToImpostorLODGroup.ContainsKey(instanceId))
                return _dictInstanceIdToImpostorLODGroup[instanceId];

            return null;
        }

        public void UpdateSettings(ImpostorLODGroup impostorLODGroup)
        {
            int index = impostorLODGroup.IndexInImpostorsManager;
            for (int i = 0; i < _impostorsManagers.Count; i++)
            {
                _impostorsManagers[i].UpdateSettings(index, impostorLODGroup);
            }
        }

        public void RequestImpostorTextureUpdate(ImpostorLODGroup impostorLODGroup)
        {
            for (int i = 0; i < _impostorsManagers.Count; i++)
            {
                _impostorsManagers[i].RequestImpostorTextureUpdate(impostorLODGroup);
            }
        }

        public int GetUsedBytes()
        {
            int res = 0;
            res += MemoryUsageUtility.GetMemoryUsage(_impostorsManagers);
            res += MemoryUsageUtility.GetMemoryUsage(_impostorLodGroups);
            res += _dictInstanceIdToImpostorLODGroup.Count * (8 + 4);

            foreach (var impostorableObjectsManager in _impostorsManagers)
            {
                res += impostorableObjectsManager.GetUsedBytes();
            }

            return res;
        }

        private RenderTextureFormat GetRenderTextureFormat()
        {
            if (_HDR)
            {
                if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                    return RenderTextureFormat.ARGBHalf;

                Debug.LogError(
                    $"[IMPOSTORS] Current system doesn't support '{RenderTextureFormat.ARGBHalf}' render texture format. " +
                    $"Falling back to default, non HDR textures.");
            }

            return RenderTextureFormat.Default;
        }
    }
}