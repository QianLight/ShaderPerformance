using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PartEditor = CFEngine.Editor.CommonListEditor<CFEngine.PartList>;
using PartContext = CFEngine.Editor.AssetListContext<CFEngine.PartList>;

namespace CFEngine.Editor
{
    public partial class EffectConfigTool : BaseConfigTool<EffectConfig>
    {
        private MatEffectGraph meGraph;
        private EffectPreviewContext previewContext = new EffectPreviewContext ();
        private Vector2 debugScroll = Vector2.zero;
        private TestGameObject currentGo;
        public override void OnInit ()
        {
            base.OnInit ();
            config = EffectConfig.instance;

            RenderEffectSystem.Init (EngineContext.instance);
            meGraph = AssetDatabase.LoadAssetAtPath<MatEffectGraph> (AssetsConfig.instance.MatEffectPath);
            BindEffectData ();

        }

        private void BindEffectData ()
        {
            meGraph.Init ();
            config.effectMap.Clear ();
            Dictionary<string, MatEffectConfigList> tmpMap = new Dictionary<string, MatEffectConfigList> ();
            for (int i = 0; i < meGraph.templates.Count; ++i)
            {
                var t = meGraph.templates[i];
                if (t.effectIndex >= 0 && t.effectIndex < meGraph.effectTemplate.Count)
                {
                    var et = meGraph.effectTemplate[t.effectIndex];
                    if (et.valid)
                    {
                        MatEffectConfigList meConfig = new MatEffectConfigList();
                        config.effectMap.Add (meConfig);
                        tmpMap.Add (et.effectName, meConfig);
                        meConfig.nodeList = t.nodeList;
                        meConfig.et = et;
                        for (int j = 0; j < t.nodeList.Count; ++j)
                        {
                            var node = t.nodeList[j];
                            if (node.index < meGraph.nodes.Count)
                            {
                                node.node = meGraph.nodes[node.index] as MatEffectNode;
                            }
                        }
                    }
                }

            }

            for (int i = 0; i < config.matEffects.Count; ++i)
            {
                var me = config.matEffects[i];
                if (tmpMap.TryGetValue (me.templateID, out var meConfig))
                {
                    meConfig.effects.Add (me);
                    for (int j = me.meData.Count - 1; j >= 0; --j)
                    {
                        var med = me.meData[j];
                        if (j < meConfig.nodeList.Count)
                        {
                            med.nodeIndex = meConfig.nodeList[j];
                            med.flag.SetFlag(MatEffectTemplate.Flag_ApplyOnReplaceMat, meConfig.et.applyOnReplaceMat);
                            med.flag.SetFlag(MatEffectTemplate.Flag_ApplyOnAddMat, meConfig.et.applyOnAddMat);
                            med.flag.SetFlag(MatEffectTemplate.Flag_HideShadow, meConfig.et.hideShadow);
                        }
                        else
                        {
                            me.meData.RemoveAt (j);
                        }

                    }
                }
            }

            config.effectMap.Sort ((x, y) => x.et.groupID.CompareTo (y.et.groupID));
        }

        private void TestGameobjectGUI ()
        {
            if (config.folder.Folder ("RenderEffectTestPrefab", "TestPrefab"))
            {
                int deleteIndex = -1;
                for (int i = 0; i < config.testGO.Count; ++i)
                {
                    TestGameObject testGameObject = config.testGO[i];
                    EditorGUILayout.BeginHorizontal (); 
                    testGameObject.gameObject = EditorGUILayout.ObjectField ("", testGameObject.gameObject, typeof (GameObject), false, GUILayout.MaxWidth (160)) as GameObject;
                    EditorGUILayout.LabelField("Height", GUILayout.MaxWidth(50));
                    testGameObject.height = EditorGUILayout.FloatField(testGameObject.height, GUILayout.MaxWidth(50));
                    if (testGameObject.height < 0)
                    {
                        testGameObject.height = 0;
                    }
                    //testGameObject.localPos = EditorGUILayout.Vector3Field("LocalPos", testGameObject.localPos, GUILayout.MaxWidth(300));
                    EditorGUILayout.LabelField("PartTag", GUILayout.MaxWidth(50));
                    testGameObject.partTag = EditorGUILayout.TextField(testGameObject.partTag, GUILayout.MaxWidth(100));
                    if (GUILayout.Button ("Delete", GUILayout.MaxWidth (60)))
                    {
                        deleteIndex = i;
                    }
                    if (GUILayout.Button ("Test", GUILayout.MaxWidth (60)))
                    {
                        if (testGameObject.gameObject != null)
                        {
                            currentGo = testGameObject;
                            Vector3 pos = Vector3.zero;
                            Quaternion rot = Quaternion.identity;
                            Vector3 scale = Vector3.one;
                            GameObject testGo = GameObject.Find ("EffectTestPrefab");
                            if (testGo != null)
                            {
                                pos = testGo.transform.position;
                                rot = testGo.transform.rotation;
                                scale = testGo.transform.localScale;
                                GameObject.DestroyImmediate (testGo);
                            }
                            testGo = PrefabUtility.InstantiatePrefab (testGameObject.gameObject) as GameObject;
                            testGo.name = "EffectTestPrefab";
                            testGo.transform.position = pos;
                            testGo.transform.rotation = rot;
                            testGo.transform.localScale = scale;
                            EffectConfig.InitEffect (previewContext, testGo, config, testGameObject.height);
                            EffectConfig.PostInit (previewContext, testGameObject.partTag);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (deleteIndex >= 0)
                {
                    config.testGO.RemoveAt (deleteIndex);
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button ("AddGameObject", GUILayout.MaxWidth (160)))
                {
                    config.testGO.Add (new TestGameObject ());
                }
                if (GUILayout.Button("ReBind", GUILayout.MaxWidth(160)))
                {
                    BindEffectData();
                }
                EditorGUILayout.EndHorizontal ();
            }
        }
        private Vector2 totalConfigScroll;
        private void MatEffectGUI (ref Rect rect)
        {
            totalConfigScroll = EditorGUILayout.BeginScrollView(totalConfigScroll, false, true);
            if (meGraph != null)
            {
                string meName = string.Format ("MatEffect({0})", meGraph.templates.Count);
                if (config.folder.Folder ("MatEffect", meName))
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < config.effectMap.Count; ++i)
                    {
                        var matEffects = config.effectMap[i];
                        var et = matEffects.et;
                        var nodeList = matEffects.nodeList;
                        string path = string.Format ("MatEffect_{0}", et.effectName);
                        string name = string.Format ("{0}({1})", et.effectName, matEffects.effects.Count.ToString ());
                        bool effectFolder = config.folder.FolderGroup (path, name, rect.width - 10, 30);
                        if (effectFolder)
                        {
                            if (GUILayout.Button("Add", GUILayout.Width(100)))
                            {
                                if (matEffects.effects.Count == 99)
                                {
                                    DebugLog.AddErrorLog ("too many mat config > 99");
                                }
                                else
                                {
                                    var mec = new MatEffectConfig ()
                                    {
                                        templateID = et.effectName,
                                    };
                                    for (int j = 0; j < nodeList.Count; ++j)
                                    {
                                        var med = new MatEffectData ();
                                        med.partMask = 0xffffffff;
                                        med.nodeIndex = nodeList[j];

                                        med.nodeIndex.node.InitData (ref med.x,
                                            ref med.y,
                                            ref med.z,
                                            ref med.w,
                                            ref med.path,
                                            ref med.param);
                                        med.nodeIndex.node.InitRes(ref med.asset);
                                        mec.meData.Add (med);
                                    }
                                    HashSet<int> uniqueIDs = new HashSet<int> ();
                                    for (int j = 0; j < matEffects.effects.Count; ++j)
                                    {
                                        var me = matEffects.effects[j];
                                        uniqueIDs.Add (me.uniqueID);
                                    }
                                    int id = et.groupID;
                                    for (; id < et.groupID + 99; id++)
                                    {
                                        if (!uniqueIDs.Contains (id))
                                        {
                                            break;
                                        }
                                    }
                                    mec.uniqueID = id;
                                    config.matEffects.Add (mec);
                                    matEffects.effects.Add (mec);
                                }
                            }
                            //EditorCommon.BeginScroll (ref matEffects.scroll, 21, 40, -1, rect.width - 20);
                            int deleteIndex = ToolsUtility.BeginDelete ();
                            for (int j = 0; j < matEffects.effects.Count; ++j)
                            {
                                var me = matEffects.effects[j];
                                string folderPath = me.GetHash ();

                                EditorGUILayout.BeginHorizontal ();
                                string desc = string.IsNullOrEmpty (me.desc) ? "empty" : me.desc;
                                desc = string.Format ("{0}(ID {1})", desc, me.uniqueID.ToString ());
                                bool meFolder = config.folder.Folder (
                                    folderPath,
                                    desc);
                                if (previewContext.entityInit)
                                {
                                    if (GUILayout.Button ("Test", GUILayout.Width (60)))
                                    {
                                        previewContext.BeginEffect (me);
                                    }
                                    if (GUILayout.Button ("EndTest", GUILayout.Width (60)))
                                    {
                                        previewContext.EndEffect ();
                                    }
                                }

                                ToolsUtility.DeleteButton (ref deleteIndex, j);
                                EditorGUILayout.EndHorizontal ();

                                if (meFolder)
                                {
                                    EditorCommon.BeginGroup ("", true, rect.width - 20, 100, 30);

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("Name", GUILayout.MaxWidth (200));
                                    me.desc = EditorGUILayout.TextField (me.desc, GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("PartTag", GUILayout.MaxWidth (200));
                                    me.partTags = EditorGUILayout.TextField (me.partTags, GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("UniqueID", GUILayout.MaxWidth (200));
                                    EditorGUILayout.LabelField (me.uniqueID.ToString (), GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("Priority", GUILayout.MaxWidth (200));
                                    me.priority = EditorGUILayout.IntField (me.priority, GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("FadeIn", GUILayout.MaxWidth (200));
                                    me.fadeIn = EditorGUILayout.Slider (me.fadeIn, 0.0f, 2, GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("FadeOut", GUILayout.MaxWidth (200));
                                    me.fadeOut = EditorGUILayout.Slider (me.fadeOut, 0.0f, 2, GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUILayout.BeginHorizontal ();
                                    EditorGUILayout.LabelField ("Time (Test Only)", GUILayout.MaxWidth (200));
                                    me.effectTime = EditorGUILayout.Slider (me.effectTime, -1, 10, GUILayout.MaxWidth (300));
                                    EditorGUILayout.EndHorizontal ();

                                    EditorGUI.indentLevel++;
                                    for (int k = 0; k < nodeList.Count; ++k)
                                    {
                                        var n = nodeList[k];
                                        if (n.node != null)
                                        {
                                            if (k >= me.meData.Count)
                                            {
                                                me.meData.Add (new MatEffectData ());
                                            }
                                            var med = me.meData[k];
                                            PartConfig.instance.OnPartGUI (me.partTags, ref med.partMask);
                                            n.node.OnGUI (med);
                                            EditorGUILayout.Space ();
                                        }
                                    }
                                    EditorGUI.indentLevel--;
                                    EditorCommon.EndGroup ();
                                }
                            }
                            ToolsUtility.EndDelete (deleteIndex, matEffects.effects);
                            //EditorCommon.EndScroll ();
                            EditorCommon.EndFolderGroup();
                        }
                    }
                    EditorGUI.indentLevel--;
                    
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void EffectDebugGui(ref Rect rect)
        {
            if (EngineContext.IsRunning)
            {
                if (config.folder.Folder("EffectDebug", "EffectDebug"))
                {
                    EditorCommon.BeginScroll(ref debugScroll, 21, 40, -1, rect.width - 20);
                    for (int i = 0; i < RenderEffectSystem.effectLog.Count; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(RenderEffectSystem.effectLog[i]);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorCommon.EndScroll();
                }
            }
        }

        protected override void OnConfigGui (ref Rect rect)
        {
            TestGameobjectGUI ();
            MatEffectGUI (ref rect);
            EffectDebugGui(ref rect);
        }

        protected override void OnConfigUpdate ()
        {
            if (currentGo != null)
            {
                previewContext.height = currentGo.height;
            }
            previewContext.Update();
        }

        private void SaveDummyCamera (BinaryWriter bw, DummyCameraController dcc, int index)
        {
            ref var ec = ref dcc.editorCameras[index];
            bw.Write (ec.fov);
            EditorCommon.WriteVector (bw, ec.color);
            EditorCommon.WriteVector (bw, ec.dir);
            EditorCommon.WriteVector (bw, ec.lightColor);
            bw.Write (ec.outlineAdd);
        }

        private void SaveSplitParam(BinaryWriter bw, Vector2 splitPoint)
        {
            splitPoint.y = Mathf.Tan(splitPoint.y * Mathf.Deg2Rad);
            EditorCommon.WriteVector(bw, splitPoint);
        }
        protected override void OnSave ()
        {
            try
            {
                config.matEffects.Clear ();
                var it = config.effectMap.GetEnumerator ();
                while (it.MoveNext ())
                {
                    var v = it.Current;
                    for (int i = 0; i < v.effects.Count; ++i)
                    {
                        config.matEffects.Add (v.effects[i]);
                    }
                }
                ResRedirectConfig rrConfig = null;
                string configPath = string.Format ("{0}Config/EffectRes.asset", LoadMgr.singleton.editorResPath);
                if (File.Exists (configPath))
                {
                    rrConfig = AssetDatabase.LoadAssetAtPath<ResRedirectConfig> (configPath);
                }
                else
                {
                    rrConfig = ResRedirectConfig.CreateInstance<ResRedirectConfig> ();
                    rrConfig.name = "EffectRes";
                    rrConfig = CommonAssets.CreateAsset<ResRedirectConfig> (configPath, ".asset", rrConfig);
                }
                rrConfig.resPath.Clear ();

                string path = string.Format ("{0}Config/EffectData.bytes",
                    LoadMgr.singleton.EngineResPath);
                using (FileStream fs = new FileStream (path, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter (fs);
                    List<MatEffectTemplate> templateNodes = new List<MatEffectTemplate> ();
                    for (int i = 0; i < meGraph.templates.Count; ++i)
                    {
                        var et = meGraph.effectTemplate[i];
                        if (et.valid)
                        {
                            var t = meGraph.templates[i];
                            for (int j = 0; j < t.nodeList.Count; ++j)
                            {
                                var node = t.nodeList[j];
                                if (node.node != null)
                                {
                                    var met = node.node.GetEffectTemplate();
                                    if (met != null)
                                    {
                                        met.uniqueID = et.groupID + j;
                                        met.flag.SetFlag(MatEffectTemplate.Flag_ApplyOnReplaceMat, et.applyOnReplaceMat);
                                        met.flag.SetFlag(MatEffectTemplate.Flag_ApplyOnAddMat, et.applyOnAddMat);
                                        met.flag.SetFlag(MatEffectTemplate.Flag_HideShadow, et.hideShadow);
                                        templateNodes.Add(met);
                                    }
                                }
                            }
                        }

                    }

                    bw.Write ((ushort) templateNodes.Count);
                    for (int i = 0; i < templateNodes.Count; ++i)
                    {
                        var met = templateNodes[i];
                        bw.Write ((ushort) met.uniqueID);
                        bw.Write ((byte) met.effectType);
                        string key = string.IsNullOrEmpty (met.shaderKey) ? "" : met.shaderKey;
                        bw.Write (key);
                        bw.Write (met.param);
                        bw.Write (met.lerpMask);
                        EditorCommon.WriteVector (bw, met.v);
                        bw.Write (met.flag.flag);
                    }

                    //pre define
                    List<MatEffectData> effectDataList = new List<MatEffectData> ();

                    for (int i = 0; i < config.matEffects.Count; ++i)
                    {
                        var matConfig = config.matEffects[i];

                        for (int j = 0; j < matConfig.meData.Count; ++j)
                        {
                            effectDataList.Add (matConfig.meData[j]);
                        }
                    }

                    bw.Write ((ushort) effectDataList.Count);
                    for (int i = 0; i < effectDataList.Count; ++i)
                    {
                        var med = effectDataList[i];
                        if (med.nodeIndex != null)
                        {
                            bw.Write (med.nodeIndex.uniqueID);
                        }
                        else
                        {
                            bw.Write ((short) (-1));
                        }
                        string assetName = med.asset != null ? med.asset.name.ToLower () : "";                        
                        if (med.asset == null)
                        {
                            if(string.IsNullOrEmpty(med.path))
                            {
                                bw.Write(assetName);
                                bw.Write(med.x);
                                bw.Write(med.y);
                                bw.Write(med.z);
                                bw.Write(med.w);
                            }
                            else
                            {
                                bw.Write(med.path);
                                bw.Write((byte)255);
                            }    
                        }
                        else
                        {
                            bw.Write(assetName);
                            bw.Write ((byte) med.x);
                            string ext = ResObject.GetExt ((byte) med.x);
                            string dir = med.path;
                            rrConfig.resPath.Add (new ResPathRedirect ()
                            {
                                name = assetName,
                                    physicPath = dir,
                                    ext = ext
                            });
                        }
                        bw.Write(med.partMask);
                        bw.Write(med.param);
                    }

                    bw.Write ((ushort) config.matEffects.Count);
                    ushort startIndex = 0;
                    for (int i = 0; i < config.matEffects.Count; ++i)
                    {
                        var matConfig = config.matEffects[i];
                        bw.Write ((short) matConfig.uniqueID);
                        bw.Write (matConfig.fadeIn);
                        bw.Write (matConfig.fadeOut);
                        bw.Write (matConfig.priority);
                        bw.Write (startIndex);
                        startIndex += (ushort) matConfig.meData.Count;
                        bw.Write (startIndex);
                    }

                    DummyCameraController dcc = null;
                    string splitScreenPath = string.Format ("{0}EditorConfig/SplitScreen.prefab", LoadMgr.singleton.EngineResPath);
                    var splitScreen = AssetDatabase.LoadAssetAtPath<GameObject> (splitScreenPath);
                    if (splitScreen != null)
                    {
                        splitScreen.TryGetComponent<DummyCameraController> (out dcc);
                    }
                    if (dcc != null)
                    {
                        bw.Write(dcc.editorCameras.Length);
                        for (int i = 0; i < dcc.editorCameras.Length; ++i)
                        {
                            SaveDummyCamera(bw, dcc, i);
                        }
                        SaveSplitParam(bw, dcc.splitPoint3[0]);
                        SaveSplitParam(bw, dcc.splitPoint3[1]);
                        SaveSplitParam(bw, dcc.splitPoint2[0]);
                        SaveSplitParam(bw, dcc.splitPoint2[1]);
                        SaveSplitParam(bw, dcc.splitPoint2[2]);
                    }
                    else
                    {
                        bw.Write(0);
                    }
                }
                CommonAssets.SaveAsset (rrConfig);
            }
            catch (Exception ex)
            {
                DebugLog.AddErrorLog (ex.StackTrace);
            }

        }
    }
}