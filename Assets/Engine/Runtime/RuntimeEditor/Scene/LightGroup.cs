#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class LightGroup : MonoBehaviour
    {
        public List<LightRender> lights = new List<LightRender>();
        public List<SceneLightRender> staticLights = new List<SceneLightRender> ();

        public Vector2 debugScroll = Vector2.zero;

        public static LightGroup lightGroup;

        private void OnDestroy()
        {
            lightGroup = null;
        }
        public static void SortLight (Light light, Dictionary<string, int> objMap)
        {
            if (light.name != Lighting.mainLightName &&
                light.name != Lighting.addLightName)
            {
                string path = "PointLight";
                if (light.type == LightType.Directional)
                {
                    path = "DirectionalLight";
                }
                else if (light.type == LightType.Spot)
                {
                    path = "SpotLight";
                }
                else if (light.type == LightType.Area)
                {
                    path = "AreaLight";
                }
                SceneAssets.SortSceneObjectName (light, path, objMap);
            }
        }

        private void CollectLights (Transform t)
        {
            if (t.TryGetComponent<SceneLightRender> (out var lr))
            {
                staticLights.Add (lr);
            }

            for (int i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild (i);
                CollectLights (child);
            }
        }
        public void CollectLights ()
        {
            staticLights.Clear ();
            CollectLights (this.transform);
        }

        private void Update()
        {
            lightGroup = this;
        }
        // public void SortLight ()
        // {
        //     EditorCommon.EnumTransform funLight = null;
        //     funLight = (trans, param) =>
        //     {
        //         if (trans.gameObject.activeSelf && trans.gameObject.activeInHierarchy)
        //         {
        //             Light light = trans.GetComponent<Light> ();
        //             LightRender lr = trans.GetComponent<LightRender> ();
        //             if (light != null && light.enabled && light.type == LightType.Point && lr != null)
        //             {
        //                 LightGroup lg = param as LightGroup;
        //                 lg.lights.Add (lr);

        //                 // Vector3 pos = trans.position;
        //                 // int xGird = (int) (pos.x / EngineContext.LightGridSize);
        //                 // int zGird = (int) (pos.z / EngineContext.LightGridSize);
        //                 // if (xGird < 0)
        //                 //     xGird = 0;
        //                 // if (xGird >= lightData.maxXGrid)
        //                 //     xGird = lightData.maxXGrid - 1;
        //                 // if (zGird >= lightData.maxZGrid)
        //                 //     zGird = lightData.maxZGrid - 1;
        //                 // int key = xGird + zGird * lightData.maxXGrid;
        //                 // string path = EditorCommon.GetSceneObjectPath (trans);
        //                 // lightData.lights.Add (new LightingData ()
        //                 // {
        //                 //     pos = pos,
        //                 //         range = light.range,
        //                 //         c = light.color,
        //                 //         intensity = light.intensity,
        //                 //         key = key,
        //                 //         path = path,
        //                 //         priority = lr.priority,
        //                 //         rangeBias = lr.rangeBias
        //                 // });
        //             }
        //             EditorCommon.EnumChildObject (trans, param, funLight);
        //         }

        //     };

        //     if (cld == null)
        //     {
        //         cld = ChunkLightData.CreateInstance<ChunkLightData> ();
        //     }
        //     cld.lights.Clear ();
        //     cld.dir = sceneContext.terrainDir;
        //     cld.maxXGrid = (int) (context.ChunkWidth * context.xChunkCount / EngineContext.LightGridSize);
        //     cld.maxZGrid = (int) (context.ChunkHeight * context.zChunkCount / EngineContext.LightGridSize);
        //     if (cld.chunkLightIndex == null ||
        //         cld.chunkLightIndex.Length != context.xChunkCount * context.zChunkCount)
        //     {
        //         cld.chunkLightIndex = new LightingChunkIndex[context.xChunkCount * context.zChunkCount];
        //     }

        //     EditorCommon.EnumChildObject (transform, cld, (trans, param) => { funLight (trans, cld); });
        //     //sort light
        //     cld.lights.Sort ((x, y) =>
        //     {
        //         if (x.priority == y.priority)
        //         {
        //             if (x.key == y.key)
        //             {
        //                 return (int) (x.pos.y * 100 - x.pos.y * 100);

        //             }
        //             else
        //             {
        //                 return x.key - y.key;
        //             }
        //         }
        //         else
        //         {
        //             return y.priority - x.priority;
        //         }

        //     });

        //     //set light chunk index
        //     for (int z = 0; z < context.zChunkCount; ++z)
        //     {
        //         for (int x = 0; x < context.zChunkCount; ++x)
        //         {
        //             int chunkIndex = x + z * context.xChunkCount;
        //             if (chunkIndex >= 0 && chunkIndex < cld.chunkLightIndex.Length)
        //             {
        //                 var lci = cld.chunkLightIndex[chunkIndex];
        //                 if (lci == null)
        //                 {
        //                     lci = new LightingChunkIndex ();
        //                     cld.chunkLightIndex[chunkIndex] = lci;
        //                 }
        //                 lci.lightIndex.Clear ();
        //                 Vector2 min = new Vector2 (x * EngineContext.ChunkSize, z * EngineContext.ChunkSize);
        //                 Vector2 max = new Vector2 (min.x + EngineContext.ChunkSize, min.y + EngineContext.ChunkSize);

        //                 for (int l = 0; l < cld.lights.Count; ++l)
        //                 {
        //                     var light = cld.lights[l];
        //                     if (RuntimeUtilities.TestCircleRect (light.pos.x, light.pos.z, light.range, ref min, ref max))
        //                     {
        //                         lci.lightIndex.Add (l);
        //                     }
        //                 }
        //             }

        //         }
        //     }
        // }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (LightGroup))]
    public class LightGroupEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI ()
        {
            LightGroup lg = target as LightGroup;
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80)))
            {
                lg.CollectLights();
            }
            //EditorGUILayout.LabelField(string.Format("GridSize:{0}x{1}", lg.cld.maxXGrid, lg.cld.maxZGrid));

            int count = lg.staticLights.Count > 10 ? 10 : lg.staticLights.Count;
            lg.debugScroll = GUILayout.BeginScrollView(lg.debugScroll, GUILayout.MinHeight(count * 20 + 10));
            for (int i = 0; i < lg.staticLights.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                var ld = lg.staticLights[i];
                EditorGUILayout.ObjectField(ld, typeof(LightRender), true);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif