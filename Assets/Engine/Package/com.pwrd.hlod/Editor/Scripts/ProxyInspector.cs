using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace com.pwrd.hlod.editor
{
    [CustomEditor(typeof(Proxy))]
    public class ProxyInspector : Editor
    {
        private bool displayVoxel = false;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            Proxy myScript = target as Proxy;

            if (myScript == null)
                return;

            EditorGUI.BeginChangeCheck();
            displayVoxel = EditorGUILayout.Toggle("显示体素数据：", displayVoxel);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeDisplay();
            }
        }

        private void ChangeDisplay()
        {
            var cluster = findCluster();
            if (cluster == null)
            {
                return;
            }

            if (displayVoxel)
            {
                cluster.DisplayVoxel();
            }
            else
            {
                cluster.UnDisplayVoxel();
            }
        }

        private Cluster findCluster()
        {
            Proxy myScript = target as Proxy;
            GameObject go = myScript.gameObject;

            var parent = go.transform.parent;
            var coordStr = Regex.Match(parent.name, @"HLODRoot(_\d_\d)").Groups[1]?.Value;
            if (string.IsNullOrEmpty(coordStr))
            {
                return null;
            }

            var scene = go.scene;
            var rootGOs = scene.GetRootGameObjects();

            GameObject dataGO = null;
            foreach (var rootGO in rootGOs)
            {
                if (rootGO.name == $"HLODEditorData{coordStr}")
                {
                    dataGO = rootGO;
                    break;
                }
            }

            if (dataGO == null)
            {
                return null;
            }

            var editorHLODData = dataGO.GetComponent<HLODSceneEditorData>();
            if (editorHLODData == null)
            {
                return null;
            }

            var sceneNode = editorHLODData.scenes.Find(s => s.scenePath.Equals(dataGO.scene.path));
            Cluster cluster = findCluster(sceneNode, go);
            // if (sceneNode != null)
            // {
            //     var resultList = sceneNode.resultList;
            //     foreach (var result in resultList)
            //     {
            //         if (result.instance == go)
            //         {
            //             cluster = result.cluster;
            //             break;
            //         }
            //     }
            // }

            return cluster;
        }

        private Cluster findCluster(SceneNode sceneNode, GameObject instance)
        {
            Cluster m_cluster = null; 
            foreach (var layer in sceneNode.layers)
            {
                foreach (var layerCluster in layer.clusters)
                {
                    m_cluster = findCluster(layerCluster, instance);
                    if (m_cluster != null)
                    {
                        return m_cluster;
                    }
                }
            }
            return m_cluster;
        }

        private Cluster findCluster(Cluster cluster, GameObject instance)
        {
            if (cluster.hlodResult.instance == instance)
            {
                return cluster;
            }

            Cluster m_cluster = null;
            if (cluster.clusters != null)
            {
                foreach (var clusterCluster in cluster.clusters)
                {
                    m_cluster = findCluster(clusterCluster, instance);
                    if (m_cluster != null)
                    {
                        break;
                    }
                }
            }
            return m_cluster;
        }
    }
}