using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Text;

namespace com.pwrd.hlod.editor
{
    public interface IHLODProviderEntitiyCollector
    {
        List<Entity> CollectEntity(SceneNode sceneNode, int useLODIndex = 0);
    }

    public class HLODProviderEntityColector : IHLODProviderEntitiyCollector
    {
        private SceneNode sceneNode;

        public List<Entity> CollectEntity(SceneNode sceneNode, int useLODIndex = 0)
        {
            this.sceneNode = sceneNode;
            List<Entity> entities = new List<Entity>();
            if (sceneNode == null || sceneNode.roots == null)
                return entities;

            List<(GameObject, LODGroup)> manifests = new List<(GameObject, LODGroup)>();
            HashSet<GameObject> set = new HashSet<GameObject>();
            foreach (var root in sceneNode.roots)
            {
                if (set.Contains(root))
                    continue;
                set.Add(root);
                //CollectPrefabInstance(root, manifests);        
                CollectRenders(root, manifests, true, useLODIndex);
            }

            for (int i = 0; i < manifests.Count; i++)
            {
                var tuple = manifests[i];
                entities.Add(new Entity(tuple.Item1));
                EditorUtility.DisplayProgressBar("CollectEntity", i + "/" + manifests.Count,
                    (float) i / manifests.Count);
            }

            return entities;
        }

        private List<Cluster> CollectClusterNode(int useLODIndex = 0)
        {
            var list = new List<Cluster>();
            if (sceneNode == null || sceneNode.roots == null)
                return list;

            int index = 0;
            HashSet<GameObject> set = new HashSet<GameObject>();
            foreach (var root in sceneNode.roots)
            {
                if (root == null || set.Contains(root))
                    continue;
                ;
                set.Add(root);

                var nodes = root.GetComponentsInChildren<HLODClusterNode>();
                var nodeList = new List<HLODClusterNode>(nodes);
                var selfNode = root.GetComponent<HLODClusterNode>();
                if (selfNode != null)
                    nodeList.Add(selfNode);
                if (nodeList.Count == 0)
                    continue;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    EditorUtility.DisplayProgressBar("CollectClusterNode", i + "/" + nodes.Length,
                        (float) i / nodes.Length);

                    var renders = new List<(GameObject, LODGroup)>();
                    CollectRenders(node.gameObject, renders, false, useLODIndex);
                    var entites = new List<Entity>();
                    foreach (var tuple in renders)
                    {
                        entites.Add(new Entity(tuple.Item1));
                    }

                    var cluster = new Cluster(entites)
                    {
                        name = "Cluster-Node-" + index + "_" + node.name,
                    };
                    index++;

                    list.Add(cluster);
                }
            }

            return list;
        }

        private List<Cluster> CollectClusterByVolume(List<Entity> entities)
        {
            var volumeList = GameObject.FindObjectsOfType<HLODClusterVolume>();
            var clusterList = new List<Cluster>();
            var newList = entities;
            for (int t = 0; t < volumeList.Length; t++)
            {
                EditorUtility.DisplayProgressBar("CollectClusterNode", t + "/" + volumeList.Length,
                    (float) t / volumeList.Length);

                var volume = volumeList[t];
                var volumeEntities = new List<Entity>();
                for (int i = newList.Count - 1; i >= 0; i--)
                {
                    var entity = newList[i];

                    bool contain = true;
                    var renderers = entity.FetchRenderer();
                    foreach (var renderer in renderers)
                    {
                        if (!volume.Contains(renderer))
                        {
                            contain = false;
                            break;
                        }
                    }
                    
                    if (contain)
                    {
                        volumeEntities.Add(entity);
                    }
                }

                if (volumeEntities.Count >= 2)
                {
                    var cluster = new Cluster(volumeEntities);
                    cluster.name = volume.name;

                    foreach (var volumeEntity in volumeEntities)
                    {
                        newList.Remove(volumeEntity);
                    }

                    clusterList.Add(cluster);
                }
            }


            return clusterList;
        }

        private void FliterAlphaTestEntity(List<Entity> entities, List<Cluster> clusterNodes,
            List<Cluster> clusterVolumes, HLODProvider.AlphaTestType filterType)
        {
            if (filterType == HLODProvider.AlphaTestType.All)
                return;
            bool onlyAlphaTest = filterType == HLODProvider.AlphaTestType.OnlyAlphaTest;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                bool hasAlphaTest = HLODProviderUtils.HasAlphaTest(entities[i].FetchRenderer());
                if (onlyAlphaTest && !hasAlphaTest || !onlyAlphaTest && hasAlphaTest)
                {
                    entities.RemoveAt(i);
                }
            }

            foreach (var cluster in clusterNodes)
            {
                for (int i = cluster.entities.Count - 1; i >= 0; i--)
                {
                    bool hasAlphaTest = HLODProviderUtils.HasAlphaTest(cluster.entities[i].FetchRenderer());
                    if (onlyAlphaTest && !hasAlphaTest || !onlyAlphaTest && hasAlphaTest)
                    {
                        cluster.entities.RemoveAt(i);
                    }
                }
            }

            foreach (var cluster in clusterVolumes)
            {
                for (int i = cluster.entities.Count - 1; i >= 0; i--)
                {
                    bool hasAlphaTest = HLODProviderUtils.HasAlphaTest(cluster.entities[i].FetchRenderer());
                    if (onlyAlphaTest && !hasAlphaTest || !onlyAlphaTest && hasAlphaTest)
                    {
                        cluster.entities.RemoveAt(i);
                    }
                }
            }
        }

        private void CollectRenders(GameObject curGo, List<(GameObject, LODGroup)> entities, bool CheckSelfNode = true, int useLODIndex = 0)
        {
            if (curGo == null)
                return;

            var clusterNode = curGo.GetComponent<HLODClusterNode>();
            if (CheckSelfNode && clusterNode != null)
            {
                return;
            }

            var renderer = curGo.GetComponent<Renderer>();
            var lodGroup = curGo.GetComponentInParent<LODGroup>();
            if (renderer != null)
            {
                if (lodGroup == null)
                {
                    if (curGo.activeInHierarchy && !IsIgnoreNode(curGo))
                        entities.Add((curGo, lodGroup));
                }
                else
                {
                    var lods = lodGroup.GetLODs();
                    if (lods.Length > useLODIndex && lods[useLODIndex].renderers.Contains(renderer) && !IsIgnoreNode(lodGroup.gameObject))
                        entities.Add((curGo, lodGroup));
                }
            }

            for (var i = 0; i < curGo.transform.childCount; ++i)
            {
                var go = curGo.transform.GetChild(i).gameObject;

                if (go != null)
                    CollectRenders(go, entities, true, useLODIndex);
            }
        }

        private bool IsIgnoreNode(GameObject gameObject)
        {
            return HLODProviderUtils.IsIgnoreNode(gameObject);
        }
    }
}