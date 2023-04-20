// using System.Collections.Generic;
// using System.IO;
// using CFEngine;
// using UnityEditor;
// using UnityEngine;

// namespace CFEngine.Editor
// {
//     internal class FxAssets
//     {
//         internal class FxSaveData
//         {
//             public byte componentType = SFXComponentData.SFX_COMPONENT_NONE;
//             public LayerIndex layerIndex;
//             public byte layerCount = 0;
//             public Material material;
//             // public MaterialContext matContext;
//             // public byte matIndex = 255;
//             public ushort flag = 0;
//             public List<SFXCustomData> customData = new List<SFXCustomData> ();
//             //mesh
//             // public string meshPath;
//             // //light
//             // public LightingInfo light;
//             //projector
//             // public DecalConfig decalConfig;
//             //animation data

//             public void FillLayerIndex (byte[] layer, int index)
//             {
//                 if (index >= 0)
//                 {
//                     layerIndex.layer0 = layer[0];
//                     layerCount++;
//                 }
//                 if (index >= 1)
//                 {
//                     layerIndex.layer1 = layer[1];
//                     layerCount++;
//                 }
//                 if (index >= 2)
//                 {
//                     layerIndex.layer2 = layer[2];
//                     layerCount++;
//                 }
//                 if (index >= 3)
//                 {
//                     layerIndex.layer3 = layer[3];
//                     layerCount++;
//                 }
//                 if (index >= 4)
//                 {
//                     layerIndex.layer4 = layer[4];
//                     layerCount++;
//                 }
//                 if (index >= 5)
//                 {
//                     layerIndex.layer5 = layer[5];
//                     layerCount++;
//                 }

//             }
//             public void SaveLayerIndex (BinaryWriter bw)
//             {
//                 bw.Write (layerCount);
//                 if (layerCount >= 1)
//                 {
//                     bw.Write (layerIndex.layer0);
//                     if (layerCount >= 2)
//                     {
//                         bw.Write (layerIndex.layer1);
//                         if (layerCount >= 3)
//                         {
//                             bw.Write (layerIndex.layer2);
//                             if (layerCount >= 4)
//                             {
//                                 bw.Write (layerIndex.layer3);
//                                 if (layerCount >= 5)
//                                 {
//                                     bw.Write (layerIndex.layer4);
//                                     if (layerCount >= 6)
//                                     {
//                                         bw.Write (layerIndex.layer5);
//                                     }
//                                 }
//                             }
//                         }
//                     }
//                 }

//             }
//         }

//         internal class FxSaveContext
//         {
//             public int version = 0;
//             public uint flag = 0;
//             public AnimCurveShare animCurve = new AnimCurveShare ();
//             public List<FxSaveData> data = new List<FxSaveData> ();
//             public MatSaveData matSaveData = new MatSaveData ();
            
//             public void MatAddResName (string path, Texture tex)
//             {
//                 // if (AddResName (path))
//                 // {
//                 //     AddResReDirct (this, tex, path);
//                 // }
//             }
//         }
//         private static void AnalyzeSFX (GameObject prefab, string path) { }
//         private static void Cleanprefab (Transform trans, string path)
//         {
//             List<Component> tmpComp = ListPool<Component>.Get();
//             trans.GetComponentsInChildren<Component> (true, tmpComp);
//             for (int i = 0; i < tmpComp.Count; ++i)
//             {
//                 Component comp = tmpComp[i];

//                 if (comp is Renderer)
//                 {
//                     var render = comp as Renderer;
//                     if (!CommonAssets.IsPhysicAsset (render.sharedMaterial))
//                     {
//                         DebugLog.AddErrorLog2 ("[SFX]use internal mat sfx:{0} path:{1}", path, EditorCommon.GetSceneObjectPath (comp.transform));
//                         if (comp.transform.childCount == 0)
//                         {
//                             UnityEngine.Object.DestroyImmediate (comp.gameObject);
//                         }
//                         else
//                         {
//                             if (comp is ParticleSystemRenderer)
//                             {
//                                 var ps = comp.gameObject.GetComponent<ParticleSystem> ();
//                                 if (ps != null)
//                                 {
//                                     UnityEngine.Object.DestroyImmediate (ps);
//                                 }
//                             }
//                             else if (comp is MeshRenderer)
//                             {
//                                 var mf = comp.gameObject.GetComponent<MeshFilter> ();
//                                 if (mf != null)
//                                 {
//                                     UnityEngine.Object.DestroyImmediate (mf);
//                                 }
//                                 UnityEngine.Object.DestroyImmediate (comp);
//                             }
//                             else if (comp is SkinnedMeshRenderer)
//                             {
//                                 UnityEngine.Object.DestroyImmediate (comp);
//                             }
//                         }
//                     }
//                 }
//             }
//             ListPool<Component>.Release(tmpComp);
//         }
//         static byte[] layerIndex = new byte[6];
//         static AnimationCurve[] curves = new AnimationCurve[1];
//         private static void CollectSFXData (Transform trans, FxSaveContext fsc, int index)
//         {
//             if (index >= 6)
//             {
//                 DebugLog.AddErrorLog2 ("[SFX]fatal error too deep layer:{0}", index.ToString());
//                 return;
//             }
//             for (int i = 0; i < trans.childCount; ++i)
//             {
//                 var child = trans.GetChild (i);
//                 layerIndex[index] = (byte)i;
//                 ParticleSystemRenderer psr = child.GetComponent<ParticleSystemRenderer> ();
//                 ParticleSystem ps = child.GetComponent<ParticleSystem> ();
//                 MeshRenderer mr = child.GetComponent<MeshRenderer> ();
//                 DynamicLightRender dlr = child.GetComponent<DynamicLightRender> ();
//                 FxSaveData fsd = null;
//                 Material mat = null;
//                 Mesh mesh = null;
//                 if (psr != null && ps != null)
//                 {
//                     fsd = new FxSaveData ();
//                     fsd.componentType = SFXComponentData.SFX_COMPONENT_PARTICLE;
//                     mat = psr.sharedMaterial;
//                     var customData = ps.customData;
//                     customData.SetMode (ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Disabled);
//                     customData.SetMode (ParticleSystemCustomData.Custom2, ParticleSystemCustomDataMode.Disabled);
//                     customData.enabled = false;
//                 }
//                 else if (mr != null)
//                 {
//                     var mf = child.GetComponent<MeshFilter> ();
//                     if (mf != null && mf.sharedMesh != null)
//                     {
//                         mesh = mf.sharedMesh;
//                         fsd = new FxSaveData ();
//                         fsd.componentType = SFXComponentData.SFX_COMPONENT_MESH;
//                         mat = mr.sharedMaterial;
//                     }

//                 }
//                 else if (dlr != null)
//                 {
//                     var l = child.GetComponent<Light> ();
//                     fsd = new FxSaveData ();
//                     fsd.componentType = SFXComponentData.SFX_COMPONENT_POINTLIGHT;

//                     // Vector4 posWithBias = child.position;
//                     // posWithBias.w = dlr.rangeBias;
//                     // Vector4 color = new Vector4 (
//                     //     Mathf.Pow (l.color.r * l.intensity, 2.2f),
//                     //     Mathf.Pow (l.color.g * l.intensity, 2.2f),
//                     //     Mathf.Pow (l.color.b * l.intensity, 2.2f), 1.0f / l.range);
//                     // fsd.light.posRange = posWithBias;
//                     // fsd.light.color = color;
//                 }
//                 if (fsd != null)
//                 {
//                     fsd.layerIndex.Reset ();
//                     fsd.FillLayerIndex (layerIndex, index);
//                     SFXControl control = child.GetComponent<SFXControl> ();
//                     if (control != null)
//                     {
//                         fsd.flag |= (ushort) control.expType;
//                         var group = control.customDataGroup;
//                         for (int j = 0; j < group.customData.Length; ++j)
//                         {
//                             var cd = group.customData[j];
//                             if (cd.dataType == CustomDataType.Curve)
//                             {
//                                 CurveContext cc = new CurveContext ();
//                                 curves[0] = cd.curve;
//                                 cd.curveIndex = (short)fsc.animCurve.AddCurve (curves, ref cc);
//                                 fsc.flag |= SFXData.Flag_HasAnim;
//                             }
//                             fsd.customData.Add (cd);
//                         }
//                     }

//                     if (mat != null)
//                     {
//                         fsd.material = mat;
//                         fsc.matSaveData.FindOrAddMatInfo (mat);
//                     }
//                     fsc.data.Add (fsd);
//                 }
//                 CollectSFXData (child, fsc, index + 1);
//             }
//         }
//         private static void ExportSFXPrefab (GameObject prefab, string path)
//         {
//             GameObject go = GameObject.Instantiate (prefab) as GameObject;
//             go.name = prefab.name;

//             // go.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
//             var trans = go.transform;
//             Animator ator = prefab.GetComponent<Animator> ();
//             if (ator != null)
//             {

//             }
//             Cleanprefab (trans,path);
//             FxSaveContext fsc = new FxSaveContext ();
//             fsc.matSaveData.addResCb = fsc.MatAddResName;
//             fsc.animCurve.BeginAddCurves ();
//             CollectSFXData (trans, fsc, 0);
//             if (fsc.data.Count > 50)
//             {
//                 DebugLog.AddErrorLog2 ("[SFX]fatal error sfx component count:{0} path:{1} what f*******ck", fsc.data.Count.ToString(), path);
//             }
//             else
//             {
//                 using (FileStream fs = new FileStream (string.Format ("{0}/RuntimePrefab/SFX/{1}.bytes",
//                     AssetsConfig.instance.ResourcePath,
//                     go.name), FileMode.Create))
//                 {
//                     BinaryWriter bw = new BinaryWriter (fs);
//                     bw.Write (fsc.version);
//                     bw.Write (fsc.flag);
//                     byte count = (byte) fsc.data.Count;
//                     if (fsc.data.Count > 16)
//                     {
//                         DebugLog.AddErrorLog2 ("[SFX]too many sfx component count:{0} path:{1} performance is pool", fsc.data.Count.ToString(), path);
//                     }
//                     if ((fsc.flag & SFXData.Flag_HasAnim) != 0)
//                         fsc.animCurve.FlushCurves (bw);
//                     bw.Write (count);
//                     for (int i = 0; i < count; ++i)
//                     {
//                         FxSaveData fsd = fsc.data[i];
//                         bw.Write (fsd.componentType);
//                         fsd.SaveLayerIndex (bw);
//                         bw.Write (fsd.flag);
//                         if (fsd.flag > 0)
//                         {
//                             var customDatas = fsd.customData;
//                             for (int j = 0; j < customDatas.Count; ++j)
//                             {
//                                 var cd = customDatas[j];
//                                 bw.Write ((byte) cd.dataType);
//                                 if (cd.dataType == CustomDataType.Curve)
//                                 {
//                                     bw.Write (cd.curveIndex);
//                                     bw.Write (cd.scale);
//                                 }
//                                 else if (cd.dataType == CustomDataType.Value)
//                                 {
//                                     bw.Write (cd.value);
//                                 }
//                             }
//                         }
//                     }
//                 }
//             }

//             string targetPath = string.Format ("{0}/RuntimePrefab/SFX/{1}.prefab",
//                 AssetsConfig.instance.ResourcePath,
//                 go.name);
//             PrefabUtility.SaveAsPrefabAsset (go, targetPath);
//             GameObject.DestroyImmediate (go);
//         }

//         [MenuItem (@"Assets/Tool/SFX_ExportPrefab")]
//         private static void SFX_ExportPrefab ()
//         {
//             CommonAssets.enumPrefab.cb = (prefab, path) =>
//             {
//                 ExportSFXPrefab (prefab, path);
//             };
//             CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "ExportPrefab");

//         }
        
//         [MenuItem (@"Assets/Tool/SFX_AddControlScript")]
//         private static void SFX_AddControlScript ()
//         {
//             CommonAssets.enumPrefab.cb = (prefab, path) =>
//             {
//                 List<Component> tmpComp = ListPool<Component>.Get();
//                 prefab.transform.GetComponentsInChildren<Component> (true, tmpComp);
//                 for (int i = 0; i < tmpComp.Count; ++i)
//                 {
//                     Component comp = tmpComp[i];

//                 if (comp is Renderer)
//                 {
//                     var render = comp as Renderer;
//                     if (CommonAssets.IsPhysicAsset (render.sharedMaterial))
//                     {
//                         if (comp is ParticleSystemRenderer)
//                         {
//                             var psr = comp as ParticleSystemRenderer;
//                             var mat = psr.sharedMaterial;
//                             if(mat.shader.name=="Effect/UVEffect_Particle")
//                             {
//                                 SFXControl control = comp.GetComponent<SFXControl> ();
//                                 if (control == null)
//                                 {
//                                     control = comp.gameObject.AddComponent<SFXControl> ();
//                                 }
//                                 control.RefreshData();
//                             }
//                         }
//                     }
//                 }
//                 }
//                 ListPool<Component>.Release(tmpComp);
//             };
//             CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "AddControlScript");

//         }
//     }
// }