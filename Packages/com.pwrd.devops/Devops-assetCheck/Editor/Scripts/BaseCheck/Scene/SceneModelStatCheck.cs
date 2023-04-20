using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace AssetCheck
{
    [CheckRuleDescription("Scene", "������FBXʹ�õ�ͳ��", "t:scene", "")]
    public class SceneModelStatCheck : RuleBase, CSVOutput
    {
        enum eSceneCheckType
        {
            Plant,
            Tree,
            Rock,
            Building,
            eSceneCheckType_Count
        }
        class SceneLineInfoResult
        {
            public string name;
            public string path;
            public string modelType;
            public int lod0Count;
            public int lod1Count;
            public int lod2Count;
            public int reuseCount;
        }

        [PublicParam("LOD0����", eGUIType.Input)]
        public int maxLod0 = 5000;
        [PublicParam("LOD1����", eGUIType.Input)]
        public int maxLod1 = 2500;
        [PublicParam("LOD2����", eGUIType.Input)]
        public int maxLod2 = 1250;

        private Dictionary<string, SceneLineInfoResult> dicAllStatInfos = new Dictionary<string, SceneLineInfoResult>();
        List<List<string>> outputDatas = new List<List<string>>();
        public List<List<string>> ResultOutput(out string fileName)
        {
            fileName = "SceneObjectCheck";
            List<string> titleLine = new List<string>();
            outputDatas.Add(titleLine);
            titleLine.Add("��Դ����");
            titleLine.Add("·��");
            titleLine.Add("ģ������");
            titleLine.Add("LOD0��������");
            titleLine.Add("LOD1��������");
            titleLine.Add("LOD2��������");
            titleLine.Add("���ô���");
            int lod0NotPassCount = 0;
            int lod1NotPassCount = 0;
            int lod2NotPassCount = 0;
            foreach (var sceneLineInfo in dicAllStatInfos)
            {
                List<string> line = new List<string>();
                AssetHelper.SplitFileRelativePathAndName(sceneLineInfo.Key, out string assetPath, out string assetName);
                line.Add(assetName);
                line.Add(assetPath);

                line.Add(sceneLineInfo.Value.modelType);
                bool bPass = true;
                if (sceneLineInfo.Value.lod0Count > maxLod0)
                {
                    ++lod0NotPassCount;
                    line.Add($"{sceneLineInfo.Value.lod0Count} > {maxLod0}");
                    bPass = false;
                }
                else
                {
                    line.Add(string.Empty);
                }

                if (sceneLineInfo.Value.lod1Count > maxLod1)
                {
                    ++lod1NotPassCount;
                    line.Add($"{sceneLineInfo.Value.lod1Count} > {maxLod1}");
                    bPass = false;
                }
                else
                {
                    line.Add(string.Empty);
                }

                if (sceneLineInfo.Value.lod2Count > maxLod2)
                {
                    ++lod2NotPassCount;
                    line.Add($"{sceneLineInfo.Value.lod2Count} > {maxLod2}");
                    bPass = false;
                }
                else
                {
                    line.Add(string.Empty);
                }
                line.Add(sceneLineInfo.Value.reuseCount.ToString());
                if (!bPass)
                {
                    outputDatas.Add(line);
                }
            }
            int totalCount = dicAllStatInfos.Count;
            List<string> totalLine = new List<string>();
            totalLine.Add("�ܼ�");
            totalLine.Add(string.Empty);
            totalLine.Add(string.Empty);
            totalLine.Add($"{lod0NotPassCount}/{totalCount}\t");
            totalLine.Add($"{lod1NotPassCount}/{totalCount}\t");
            totalLine.Add($"{lod2NotPassCount}/{totalCount}\t");
            totalLine.Add(string.Empty);
            outputDatas.Add(totalLine);
            return outputDatas;
        }

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AssetHelper.OpenScene(path);

            for(int i = 0; i < (int)eSceneCheckType.eSceneCheckType_Count; i++)
            {
                CheckSceneElement((eSceneCheckType)i);
            }
            AssetHelper.BackLastScene();
            return true;
        }

        void CheckSceneElement(eSceneCheckType checkType)
        {
            GameObject root = GameObject.Find(checkType.ToString());
            if (root == null)
                return;
            CheckSceneLodInfo(root, checkType);
        }

        string GetSceneTypeName(eSceneCheckType checkType, GameObject gObject)
        {
            switch(checkType)
            {
                case eSceneCheckType.Plant:
                    return "��ľ";
                case eSceneCheckType.Tree:
                    {
                        float maxHeight = 0.0f;
                        MeshFilter[] mfs = gObject.GetComponentsInChildren<MeshFilter>();
                        foreach (MeshFilter mf in mfs)
                        {
                            if (mf.sharedMesh == null)
                                continue;

                            if (mf.sharedMesh.bounds.size.y > maxHeight)
                                maxHeight = mf.sharedMesh.bounds.size.y;
                        }
                        if (maxHeight < 6.0f)
                            return "С����";
                        else if (maxHeight < 15.0f)
                            return "������";
                        else if (maxHeight >= 15.0f)
                            return "������";
                    }
                    break;
                case eSceneCheckType.Rock:
                    return "ʯͷ";
                case eSceneCheckType.Building:
                    return "����";
                default:
                    return "UnDefine";
            }
            return "UnDefine";
        }

        void CheckSceneLodInfo(GameObject root, eSceneCheckType checkType)
        {

            Transform[] trans = root.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; i++)
            {
                var type = PrefabUtility.GetPrefabAssetType(trans[i].gameObject);
                if (type == PrefabAssetType.Regular)
                {
                    GameObject rootGo = PrefabUtility.GetNearestPrefabInstanceRoot(trans[i].gameObject);
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(trans[i].gameObject);
                    SceneLineInfoResult info = null;
                    if (!dicAllStatInfos.TryGetValue(path, out info))
                    {
                        info = new SceneLineInfoResult();
                        dicAllStatInfos.Add(path, info);
                        if (!AssetHelper.SplitFilePathAndName(path, out info.path, out info.name))
                        {
                            info.name = path;
                            info.path = path;
                        }
                        PrefabLodCheck(rootGo, ref info);
                        info.reuseCount = 1;
                        info.modelType = GetSceneTypeName(checkType, trans[i].gameObject);
                    }
                    else
                    {
                        ++info.reuseCount;
                    }
                }
            }
        }

        void PrefabLodCheck(GameObject gObject, ref SceneLineInfoResult info)
        {
            int lod0 = 0;
            int lod1 = 0;
            int lod2 = 0;
            Renderer[] renders = gObject.GetComponentsInChildren<Renderer>();
            foreach(var render in renders)
            {
                Mesh mesh = null;
                if(render is MeshRenderer)
                {
                    MeshFilter mf = (render as MeshRenderer).gameObject.GetComponent<MeshFilter>();
                    if(mf != null)
                    {
                        mesh = mf.sharedMesh;
                    }
                }
                else if(render is SkinnedMeshRenderer)
                {
                    mesh = (render as SkinnedMeshRenderer).sharedMesh;
                }

                if(mesh != null)
                {
                    string currentObjectName = render.gameObject.name;
                    if (currentObjectName.Contains("LOD0"))
                        lod0 += mesh.triangles.Length / 3;
                    else if (currentObjectName.Contains("LOD1"))
                        lod1 += mesh.triangles.Length / 3;
                    else if (currentObjectName.Contains("LOD2"))
                        lod2 += mesh.triangles.Length / 3;
                }
            }
            lod1 += lod2;
            lod0 += lod1;

            info.lod0Count = lod0;
            info.lod1Count = lod1;
            info.lod2Count = lod2;
        }
    }
}