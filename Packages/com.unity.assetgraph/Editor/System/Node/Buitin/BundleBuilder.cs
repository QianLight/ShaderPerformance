using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using V1 = AssetBundleGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using System.Xml;
#if ZEUS_ASSET_ENABLE_SBP
using UnityEditor.Build.Pipeline;
using UnityEngine.Build.Pipeline;
#endif

namespace UnityEngine.AssetGraph {
	[CustomNode("Build/Build Asset Bundles", 90)]
	public class BundleBuilder : Node, Model.NodeDataImporter {
        [Serializable]
        private class StringListForJson
        {
            public List<string> Items = new List<string>();
        }
        Dictionary<string, List<string>> bundle2AtlasBundles = null;
        struct AssetImporterSetting {
			private AssetImporter importer;
			private string assetBundleName;
			private string assetBundleVariant;

			public AssetImporterSetting(AssetImporter imp) {
				importer = imp;
				assetBundleName = importer.assetBundleName;
				assetBundleVariant = importer.assetBundleVariant;
			}

			public void WriteBack() {
				importer.SetAssetBundleNameAndVariant (assetBundleName, assetBundleVariant);
				importer.SaveAndReimport ();
			}
		}

        public enum OutputOption : int {
            BuildInCacheDirectory,
            ErrorIfNoOutputDirectoryFound,
            AutomaticallyCreateIfNoOutputDirectoryFound,
            DeleteAndRecreateOutputDirectory
        }

		private static readonly string key = "0";

        [SerializeField] private SerializableMultiTargetInt m_enabledBundleOptions;
        [SerializeField] private SerializableMultiTargetString m_outputDir;
        [SerializeField] private SerializableMultiTargetInt m_outputOption;
        [SerializeField] private SerializableMultiTargetString m_manifestName;
        [SerializeField] private bool m_overwriteImporterSetting;
        [SerializeField] private string m_assetPrefix = "Resources/";
        [SerializeField] private bool m_containFileExtension = false;
        [SerializeField] private bool m_generateBundleReport = false;
        [SerializeField] private bool m_generateBundleDetailReport = false;

        public override string ActiveStyle {
			get {
				return "node 5 on";
			}
		}

		public override string InactiveStyle {
			get {
				return "node 5";
			}
		}

		public override string Category {
			get {
				return "Build";
			}
		}

		public override Model.NodeOutputSemantics NodeInputType {
			get {
				return Model.NodeOutputSemantics.AssetBundleConfigurations;
			}
		}

		public override Model.NodeOutputSemantics NodeOutputType {
			get {
				return Model.NodeOutputSemantics.AssetBundles;
			}
		}

		public override void Initialize(Model.NodeData data) {
            m_enabledBundleOptions = new SerializableMultiTargetInt();
            m_outputDir = new SerializableMultiTargetString();
            m_outputOption = new SerializableMultiTargetInt((int)OutputOption.BuildInCacheDirectory);
            m_manifestName = new SerializableMultiTargetString();
            m_assetPrefix = string.Empty;
            m_containFileExtension = false;


            data.AddDefaultInputPoint();
			data.AddDefaultOutputPoint();
		}

		public void Import(V1.NodeData v1, Model.NodeData v2) {
			m_enabledBundleOptions = new SerializableMultiTargetInt(v1.BundleBuilderBundleOptions);
            m_outputDir = new SerializableMultiTargetString();
            m_outputOption = new SerializableMultiTargetInt((int)OutputOption.BuildInCacheDirectory);
            m_manifestName = new SerializableMultiTargetString();
            m_assetPrefix = string.Empty;
            m_containFileExtension = false;
        }
			
		public override Node Clone(Model.NodeData newData) {
			var newNode = new BundleBuilder();
			newNode.m_enabledBundleOptions = new SerializableMultiTargetInt(m_enabledBundleOptions);
            newNode.m_outputDir = new SerializableMultiTargetString(m_outputDir);
            newNode.m_outputOption = new SerializableMultiTargetInt(m_outputOption);
            newNode.m_manifestName = new SerializableMultiTargetString (m_manifestName);
            newNode.m_assetPrefix = m_assetPrefix;
            newNode.m_containFileExtension = m_containFileExtension;


            newData.AddDefaultInputPoint();
			newData.AddDefaultOutputPoint();

			return newNode;
		}

		public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged) {

			if (m_enabledBundleOptions == null) {
				return;
			}

			EditorGUILayout.HelpBox("Build Asset Bundles: Build asset bundles with given asset bundle settings.", MessageType.Info);
			editor.UpdateNodeName(node);

			bool newOverwrite = EditorGUILayout.ToggleLeft ("Keep AssetImporter settings for variants", m_overwriteImporterSetting);
			if (newOverwrite != m_overwriteImporterSetting) {
				using(new RecordUndoScope("Remove Target Bundle Options", node, true)){
					m_overwriteImporterSetting = newOverwrite;
					onValueChanged();
				}
			}

			GUILayout.Space(10f);

			//Show target configuration tab
			editor.DrawPlatformSelector(node);
			using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
				var disabledScope = editor.DrawOverrideTargetToggle(node, m_enabledBundleOptions.ContainsValueOf(editor.CurrentEditingGroup), (bool enabled) => {
					using(new RecordUndoScope("Remove Target Bundle Options", node, true)){
						if(enabled) {
                            m_enabledBundleOptions[editor.CurrentEditingGroup] = m_enabledBundleOptions.DefaultValue;
                            m_outputDir[editor.CurrentEditingGroup] = m_outputDir.DefaultValue;
                            m_outputOption[editor.CurrentEditingGroup] = m_outputOption.DefaultValue;
                            m_manifestName[editor.CurrentEditingGroup] = m_manifestName.DefaultValue;
						}  else {
                            m_enabledBundleOptions.Remove(editor.CurrentEditingGroup);
                            m_outputDir.Remove(editor.CurrentEditingGroup);
                            m_outputOption.Remove(editor.CurrentEditingGroup);
                            m_manifestName.Remove(editor.CurrentEditingGroup);
						}
						onValueChanged();
					}
				} );

				using (disabledScope) {
                    OutputOption opt = (OutputOption)m_outputOption[editor.CurrentEditingGroup];
                    var newOption = (OutputOption)EditorGUILayout.EnumPopup("Output Option", opt);
                    if(newOption != opt) {
                        using(new RecordUndoScope("Change Output Option", node, true)){
                            m_outputOption[editor.CurrentEditingGroup] = (int)newOption;
                            onValueChanged();
                        }
                    }

                    using (new EditorGUI.DisabledScope (opt == OutputOption.BuildInCacheDirectory)) {
                        var newDirPath = editor.DrawFolderSelector ("Output Directory", "Select Output Folder", 
                            m_outputDir[editor.CurrentEditingGroup],
                            Application.dataPath + "/../",
                            (string folderSelected) => {
                                var projectPath = Directory.GetParent(Application.dataPath).ToString();

                                if(projectPath == folderSelected) {
                                    folderSelected = string.Empty;
                                } else {
                                    var index = folderSelected.IndexOf(projectPath);
                                    if(index >= 0 ) {
                                        folderSelected = folderSelected.Substring(projectPath.Length + index);
                                        if(folderSelected.IndexOf('/') == 0) {
                                            folderSelected = folderSelected.Substring(1);
                                        }
                                    }
                                }
                                return folderSelected;
                            }
                        );
                        if (newDirPath != m_outputDir[editor.CurrentEditingGroup]) {
                            using(new RecordUndoScope("Change Output Directory", node, true)){
                                m_outputDir[editor.CurrentEditingGroup] = newDirPath;
                                onValueChanged();
                            }
                        }

                        var outputDir = PrepareOutputDirectory (BuildTargetUtility.GroupToTarget(editor.CurrentEditingGroup), node.Data, false, false);

                        if (opt == OutputOption.ErrorIfNoOutputDirectoryFound && 
                            editor.CurrentEditingGroup != BuildTargetGroup.Unknown &&
                            !string.IsNullOrEmpty(m_outputDir [editor.CurrentEditingGroup]) &&
                            !Directory.Exists (outputDir)) 
                        {
                            using (new EditorGUILayout.HorizontalScope()) {
                                EditorGUILayout.LabelField(outputDir + " does not exist.");
                                if(GUILayout.Button("Create directory")) {
                                    Directory.CreateDirectory(outputDir);
                                }
                            }
                            EditorGUILayout.Space();

                            string parentDir = Path.GetDirectoryName(m_outputDir[editor.CurrentEditingGroup]);
                            if(Directory.Exists(parentDir)) {
                                EditorGUILayout.LabelField("Available Directories:");
                                string[] dirs = Directory.GetDirectories(parentDir);
                                foreach(string s in dirs) {
                                    EditorGUILayout.LabelField(s);
                                }
                            }
                            EditorGUILayout.Space();
                        }

                        using (new EditorGUI.DisabledScope (!Directory.Exists (outputDir))) 
                        {
                            using (new EditorGUILayout.HorizontalScope ()) {
                                GUILayout.FlexibleSpace ();
                                if (GUILayout.Button (GUIHelper.RevealInFinderLabel)) {
                                    EditorUtility.RevealInFinder (outputDir);
                                }
                            }
                        }

                        EditorGUILayout.HelpBox ("You can use '{Platform}' variable for Output Directory path to include platform name.", MessageType.Info);
                    }

                    GUILayout.Space (8f);

                    var manifestName = m_manifestName[editor.CurrentEditingGroup];
                    var newManifestName = EditorGUILayout.TextField("Manifest Name", manifestName);
                    if(newManifestName != manifestName) {
                        using(new RecordUndoScope("Change Manifest Name", node, true)){
                            m_manifestName[editor.CurrentEditingGroup] = newManifestName;
                            onValueChanged();
                        }
                    }

                    var newAssetPrefix = EditorGUILayout.TextField("AssetPrefix", m_assetPrefix);
                    if (newAssetPrefix != m_assetPrefix)
                    {
                        m_assetPrefix = newAssetPrefix;
                        onValueChanged();
                    }

                    var new_containFileExtension = EditorGUILayout.Toggle("PathContainFileExtension", m_containFileExtension);
                    if(new_containFileExtension != m_containFileExtension) 
                    {
                        m_containFileExtension = new_containFileExtension;
                        onValueChanged();
                    }


                    var new_generateBundleReport = EditorGUILayout.Toggle("GenBundleReport", m_generateBundleReport);
                    if (new_generateBundleReport != m_generateBundleReport)
                    {
                        m_generateBundleReport = new_generateBundleReport;
                        onValueChanged();
                    }
                    if (m_generateBundleReport)
                    {
                        var new_generateBundleDetailReport = EditorGUILayout.Toggle("GenBundleDetailReport", m_generateBundleDetailReport);
                        if (new_generateBundleDetailReport != m_generateBundleDetailReport)
                        {
                            m_generateBundleDetailReport = new_generateBundleDetailReport;
                            onValueChanged();
                        }
                    }
                    GUILayout.Space (8f);

					int bundleOptions = m_enabledBundleOptions[editor.CurrentEditingGroup];

					bool isDisableWriteTypeTreeEnabled  = 0 < (bundleOptions & (int)BuildAssetBundleOptions.DisableWriteTypeTree);
					bool isIgnoreTypeTreeChangesEnabled = 0 < (bundleOptions & (int)BuildAssetBundleOptions.IgnoreTypeTreeChanges);

					// buildOptions are validated during loading. Two flags should not be true at the same time.
					UnityEngine.Assertions.Assert.IsFalse(isDisableWriteTypeTreeEnabled && isIgnoreTypeTreeChangesEnabled);

					bool isSomethingDisabled = isDisableWriteTypeTreeEnabled || isIgnoreTypeTreeChangesEnabled;

					foreach (var option in Model.Settings.BundleOptionSettings) {

						// contains keyword == enabled. if not, disabled.
						bool isEnabled = (bundleOptions & (int)option.option) != 0;

						bool isToggleDisabled = 
							(option.option == BuildAssetBundleOptions.DisableWriteTypeTree  && isIgnoreTypeTreeChangesEnabled) ||
							(option.option == BuildAssetBundleOptions.IgnoreTypeTreeChanges && isDisableWriteTypeTreeEnabled);

						using(new EditorGUI.DisabledScope(isToggleDisabled)) {
							var result = EditorGUILayout.ToggleLeft(option.description, isEnabled);
							if (result != isEnabled) {
								using(new RecordUndoScope("Change Bundle Options", node, true)){
									bundleOptions = (result) ? 
										((int)option.option | bundleOptions) : 
										(((~(int)option.option)) & bundleOptions);
									m_enabledBundleOptions[editor.CurrentEditingGroup] = bundleOptions;
									onValueChanged();
								}
							}
						}
					}
					if(isSomethingDisabled) {
						EditorGUILayout.HelpBox("'Disable Write Type Tree' and 'Ignore Type Tree Changes' can not be used together.", MessageType.Info);
					}
				}
			}
		}

		public override void Prepare (BuildTarget target, 
			Model.NodeData node, 
			IEnumerable<PerformGraph.AssetGroups> incoming, 
			IEnumerable<Model.ConnectionData> connectionsToOutput, 
			PerformGraph.Output Output) 
		{
			// BundleBuilder do nothing without incoming connections
			if(incoming == null) {
				return;
			}
			
            var bundleOutputDir = PrepareOutputDirectory (target, node, false, true);

			var bundleNames = incoming.SelectMany(v => v.assetGroups.Keys).Distinct().ToList();
			var bundleVariants = new Dictionary<string, List<string>>();

			// get all variant name for bundles
			foreach(var ag in incoming) {
				foreach(var name in ag.assetGroups.Keys) {
					if(!bundleVariants.ContainsKey(name)) {
						bundleVariants[name] = new List<string>();
					}
					var assets = ag.assetGroups[name];
					foreach(var a in assets) {
						var variantName = a.variantName;
						if(!bundleVariants[name].Contains(variantName)) {
							bundleVariants[name].Add(variantName);
						}
					}
				}
			}

			// add manifest file
            var manifestName = GetManifestName(target, node, true);
			bundleNames.Add( manifestName );
			bundleVariants[manifestName] = new List<string>() {""};

			if(connectionsToOutput != null && Output != null) {
				UnityEngine.Assertions.Assert.IsTrue(connectionsToOutput.Any());

				var outputDict = new Dictionary<string, List<AssetReference>>();
				outputDict[key] = new List<AssetReference>();

				foreach (var name in bundleNames) {
					foreach(var v in bundleVariants[name]) {
						string bundleName = (string.IsNullOrEmpty(v))? name : name + "." + v;
						AssetReference bundle = AssetReferenceDatabase.GetAssetBundleReference( FileUtility.PathCombine(bundleOutputDir, bundleName) );
						AssetReference manifest = AssetReferenceDatabase.GetAssetBundleReference( FileUtility.PathCombine(bundleOutputDir, bundleName + Model.Settings.MANIFEST_FOOTER) );
						outputDict[key].Add(bundle);
						outputDict[key].Add(manifest);
					}
				}

				var dst = (connectionsToOutput == null || !connectionsToOutput.Any())? 
					null : connectionsToOutput.First();
				Output(dst, outputDict);
			}
		}
		
		public override void Build (BuildTarget target, 
			Model.NodeData node, 
			IEnumerable<PerformGraph.AssetGroups> incoming, 
			IEnumerable<Model.ConnectionData> connectionsToOutput, 
			PerformGraph.Output Output,
			Action<Model.NodeData, string, float> progressFunc) 
		{
			if(incoming == null) {
				return;
			}
			
			var aggregatedGroups = new Dictionary<string, List<AssetReference>>();
			aggregatedGroups[key] = new List<AssetReference>();

			if(progressFunc != null) progressFunc(node, "Collecting all inputs...", 0f);

			foreach(var ag in incoming) {
				foreach(var name in ag.assetGroups.Keys) {
					if(!aggregatedGroups.ContainsKey(name)) {
						aggregatedGroups[name] = new List<AssetReference>();
					}
					aggregatedGroups[name].AddRange(ag.assetGroups[name].AsEnumerable());
				}
			}

            var bundleOutputDir = PrepareOutputDirectory (target, node, true, true);
			var bundleNames = aggregatedGroups.Keys.ToList();
			var bundleVariants = new Dictionary<string, List<string>>();

			if(progressFunc != null) progressFunc(node, "Building bundle variants map...", 0.2f);

			// get all variant name for bundles
			foreach(var name in aggregatedGroups.Keys) {
				if(!bundleVariants.ContainsKey(name)) {
					bundleVariants[name] = new List<string>();
				}
				var assets = aggregatedGroups[name];
				foreach(var a in assets) {
					var variantName = a.variantName;
					if(!bundleVariants[name].Contains(variantName)) {
						bundleVariants[name].Add(variantName);
					}
				}
			}

			int validNames = 0;
			foreach (var name in bundleNames) {
				var assets = aggregatedGroups[name];
				// we do not build bundle without any asset
				if( assets.Count > 0 ) {
					validNames += bundleVariants[name].Count;
				}
			}

			AssetBundleBuild[] bundleBuild = new AssetBundleBuild[validNames];
			List<AssetImporterSetting> importerSetting = null;

			if (!m_overwriteImporterSetting) {
				importerSetting = new List<AssetImporterSetting> ();
			}

			int bbIndex = 0;
			foreach(var name in bundleNames) {
				foreach(var v in bundleVariants[name]) {
					var assets = aggregatedGroups[name];

					if(assets.Count <= 0) {
						continue;
					}

					bundleBuild[bbIndex].assetBundleName = name;
					bundleBuild[bbIndex].assetBundleVariant = v;
					bundleBuild[bbIndex].assetNames = assets.Where(x => x.variantName == v && File.Exists(x.importFrom)).Select(x => x.importFrom).ToArray();
					//bundleBuild[bbIndex].assetNames = assets.Where(x => x.variantName == v).Select(x => x.importFrom).ToArray();
                    bundleBuild[bbIndex].addressableNames = new string[bundleBuild[bbIndex].assetNames.Length];
                    for(int i = 0; i < bundleBuild[bbIndex].addressableNames.Length; i++)
                    {
                        bundleBuild[bbIndex].addressableNames[i] = HandAssetName(bundleBuild[bbIndex].assetNames[i]);
                    }
					/**
					 * WORKAROND: This will be unnecessary in future version
					 * Unity currently have issue in configuring variant assets using AssetBundleBuild[] that
					 * internal identifier does not match properly unless you configure value in AssetImporter.
					 */
					if (!string.IsNullOrEmpty (v)) {
						foreach (var path in bundleBuild[bbIndex].assetNames) {
							AssetImporter importer = AssetImporter.GetAtPath (path);

							if (importer.assetBundleName != name || importer.assetBundleVariant != v) {
								if (!m_overwriteImporterSetting) {
									importerSetting.Add (new AssetImporterSetting(importer));
								}
								importer.SetAssetBundleNameAndVariant (name, v);
								importer.SaveAndReimport ();
							}
						}
					}
					++bbIndex;
				}
			}

			if(progressFunc != null) progressFunc(node, "Building Asset Bundles...", 0.7f);
            BuildAssetBundleOptions buildOption = (BuildAssetBundleOptions)m_enabledBundleOptions[target];
            buildOption |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            buildOption |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
#if ZEUS_ASSET_ENABLE_SBP
			CompatibilityAssetBundleManifest m = CompatibilityBuildPipeline.BuildAssetBundles(bundleOutputDir, bundleBuild, buildOption, target);
#elif ENABLE_UNITY_CUSTOM_ASSETBUNDLE_BUILD
            DistributedBuild.AssetBundleManifest m = DistributedBuild.BuildPipeline.BuildAssetBundlesWithCustomEditor(bundleOutputDir, bundleBuild, buildOption, target, DistributedBuild.DistributedBuildOptions.None);     
            var manifestFiles = Directory.GetFiles(bundleOutputDir, "output_Current_job*.manifest");
            if (manifestFiles.Length > 0)
            {
	            var platformName = target.ToString();
	            var directoryName = new FileInfo(manifestFiles[0]).Directory.FullName;
                File.Move(manifestFiles[0], Path.Combine(directoryName, platformName + ".manifest"));
                
                var filePath = manifestFiles[0].Substring(0, manifestFiles[0].Length - ".manifest".Length);
                File.Move(filePath, Path.Combine(directoryName, platformName));
            }
#else
			Debug.Log(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "Start BuildPipeline.BuildAssetBundles");
			AssetBundleManifest m = BuildPipeline.BuildAssetBundles(bundleOutputDir, bundleBuild, buildOption, target);
			Debug.Log(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "End BuildPipeline.BuildAssetBundles");
#endif

            string assetMapNameXMLPath;
            string bundle2AtlasXMLPath;
            string md5VersionXMLDir;
            string unloadableBundleJsonPath;
            if (Path.IsPathRooted(bundleOutputDir))
            {
                assetMapNameXMLPath = Path.Combine(bundleOutputDir, GetAssetMapName());
                bundle2AtlasXMLPath = Path.Combine(bundleOutputDir, GetBundleDepencyAtlasName());
                unloadableBundleJsonPath = Path.Combine(bundleOutputDir, GetUnloadableBundlesJsonName());
                md5VersionXMLDir = bundleOutputDir;
            }
            else
            {
                string dir = Application.dataPath.Replace("Assets", "") + bundleOutputDir;
                assetMapNameXMLPath = Path.Combine(dir, GetAssetMapName());
                bundle2AtlasXMLPath = Path.Combine(dir, GetBundleDepencyAtlasName());
                unloadableBundleJsonPath = Path.Combine(dir, GetUnloadableBundlesJsonName());
                md5VersionXMLDir = dir;
            }
            //清理缓存
            var files = Directory.GetFiles(bundleOutputDir,"*.xml");
            foreach (var file in files)
	            File.Delete(file);
            files = Directory.GetFiles(bundleOutputDir,"*.json");
            foreach (var file in files)
	            File.Delete(file);

            //在这里处理一下生成一下xml 输出文件对应.
            CreateXML(assetMapNameXMLPath, bundleNames);
            GenerateBundleDependencyAtlas(incoming, bundle2AtlasXMLPath);


			var output = new Dictionary<string, List<AssetReference>>();
			output[key] = new List<AssetReference>();

            var manifestName = GetManifestName (target, node, false);

            if (!string.IsNullOrEmpty (m_manifestName [target])) {
                var projectPath = Directory.GetParent (Application.dataPath).ToString ();
                var finalManifestName = GetManifestName (target, node, true);
                var from = FileUtility.PathCombine (projectPath, bundleOutputDir, manifestName);
                var to = FileUtility.PathCombine (projectPath, bundleOutputDir, finalManifestName);

                var fromPaths = new string[] { from, from + ".manifest" };
                var toPaths = new string[] { to, to + ".manifest" };

                for (var i = 0; i < fromPaths.Length; ++i) {
                    if (File.Exists (toPaths[i])) {
                        File.Delete (toPaths[i]);
                    }
                    File.Move (fromPaths[i], toPaths[i]);
                }

                manifestName = finalManifestName;
            }
            var manifest = GetManifest(bundleOutputDir, manifestName);
            //创建UnloadableBundles.json，记录加载后可卸载Unload(false)的bundle
            var unloadableBundles = UnloadableBundleUtility.GetAllUnloadableBundles(bundleBuild, manifest, bundle2AtlasBundles);
            CreateJson(unloadableBundleJsonPath, unloadableBundles);
            var generatedFiles = FileUtility.GetAllFilePathsInFolder(bundleOutputDir);
			// add manifest file
            bundleVariants.Add( manifestName.ToLower(), new List<string> { null } );
			foreach (var path in generatedFiles) {
				var fileName = path.Substring(bundleOutputDir.Length+1);
				if( IsFileIntendedItem(fileName, bundleVariants) ) {
                    if (fileName == manifestName) {
                        output[key].Add( AssetReferenceDatabase.GetAssetBundleManifestReference(path) );
                    } else {
                        output[key].Add( AssetReferenceDatabase.GetAssetBundleReference(path) );
                    }
				}
			}

			if(Output != null) {
				var dst = (connectionsToOutput == null || !connectionsToOutput.Any())? 
					null : connectionsToOutput.First();
				Output(dst, output);
			}

			if (importerSetting != null) {
				importerSetting.ForEach (i => i.WriteBack ());
			}

            AssetBundleBuildReport.AddBuildReport(new AssetBundleBuildReport(node, m, manifestName, bundleBuild, output[key], aggregatedGroups, bundleVariants));
            if (m_generateBundleReport)
            {
                CreateBuildReport(bundleOutputDir, manifestName, bundleBuild, manifest);
            }
        }

        private string GetAssetMapName()
        {
            return "assetMapName.xml";
        }
        private string GetUnloadableBundlesJsonName()
        {
            return "unloadableBundles.json";
        }

        private void CreateJson(string outPath, List<string> unloadableBundles)
        {
            var list = new StringListForJson
            {
                Items = unloadableBundles,
            };
            var json = JsonUtility.ToJson(list, true);
            File.WriteAllText(outPath, json);
        }

        private void CreateXML(string outPath, List<string> outputBundles)
        {
            //1.先转换成asset 在哪些assetbundle中会持有
            Dictionary<string, SortedSet<string>> assetMapAssetBundle = new Dictionary<string, SortedSet<string>>();
            Dictionary<string, string> assetExtMap = new Dictionary<string, string>();
            AssetBundleBuildMap map = AssetBundleBuildMap.GetBuildMap();
            foreach (var assetbundle in outputBundles)
            {
                //skip shared bundle 
                if (assetbundle.StartsWith("shared_") || assetbundle == "0")
                    continue;

                string[] assets = map.GetAssetPathsFromAssetBundle(assetbundle);
                foreach (var assetFull in assets)
                {
                    string asset = HandAssetName(assetFull);
                    SortedSet<string> mapAssetBundles;
                    if (!assetMapAssetBundle.TryGetValue(asset, out mapAssetBundles))
                    {
                        var temp = new SortedSet<string>();
                        temp.Add(assetbundle);
                        assetMapAssetBundle.Add(asset, temp);
                    }
                    else
                    {
                        mapAssetBundles.Add(assetbundle);
                    }
                    assetExtMap[asset] = Path.GetExtension(assetFull);
                }
            }

            //2.建立XML
            XmlDocument xmlDoc = new XmlDocument();
            //XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "gb2312", null);
            //xmlDoc.AppendChild(xmlDecl);
            XmlElement rootElement = xmlDoc.CreateElement("", "Assets", "");
            xmlDoc.AppendChild(rootElement);
            foreach (var pair in assetMapAssetBundle)
            {
                XmlElement assetEle = xmlDoc.CreateElement("Asset");
                assetEle.SetAttribute("path", pair.Key);
                assetEle.SetAttribute("ext", assetExtMap[pair.Key]);

                foreach (var assetbundle in pair.Value)
                {
                    XmlElement assetBundleEle = xmlDoc.CreateElement("AssetBundle");
                    assetBundleEle.InnerText = assetbundle;
                    assetEle.AppendChild(assetBundleEle);
                }

                rootElement.AppendChild(assetEle);
            }
            xmlDoc.Save(outPath);
        }

        private AssetBundleManifest GetManifest(string outPath, string manifestName)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(outPath + "/" + manifestName);
            if (bundle == null)
            {
                Debug.LogError("CreateBuildReport load manifest failed");
            }
            Object asset = bundle.LoadAsset("assetbundlemanifest");
            bundle.Unload(false);
            return (AssetBundleManifest)asset;
        }

        private void CreateBuildReport(string outPath, string manifestName, AssetBundleBuild[] bundleBuilds, AssetBundleManifest manifest)
        {
            System.Text.StringBuilder contentBuilder = new System.Text.StringBuilder();
            try
            {
                m_containFileExtension = true;
                Debug.Log("CreateBuildReport");
                Dictionary<string, AssetBundleBuild> bundleDict = new Dictionary<string, AssetBundleBuild>();
                foreach (var bundleBuild in bundleBuilds)
                {
                    bundleDict.Add(bundleBuild.assetBundleName, bundleBuild);
                }
                foreach (var bundleBuild in bundleBuilds)
                {
                    var bundleName = bundleBuild.assetBundleName;
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(Path.Combine(outPath, bundleName));
                    if (fileInfo.Exists)
                    {
                        var deps = manifest.GetAllDependencies(bundleName);
                        contentBuilder.Append(bundleName);
                        contentBuilder.Append(" dep: ");
                        contentBuilder.Append(deps.Length);
                        contentBuilder.Append(" size: ");
                        contentBuilder.Append((fileInfo.Length / 1024.0f).ToString("0.0"));
                        contentBuilder.Append("kb");
                        contentBuilder.AppendLine();
                        foreach(var assetItem in bundleBuild.assetNames)
                        {
                            contentBuilder.AppendLine("  Assets: " + assetItem);
                        }
                        
                        foreach (var dep in deps)
                        {
                            contentBuilder.Append("    " + dep);
                            var depBundleBuild = bundleDict[dep];
                            contentBuilder.Append(" : " + depBundleBuild.addressableNames.Length);

                            fileInfo = new System.IO.FileInfo(Path.Combine(outPath, dep));
                            if (fileInfo.Exists)
                            {
                                contentBuilder.Append(" size: ");
                                contentBuilder.Append((fileInfo.Length / 1024.0f).ToString("0.0"));
                                contentBuilder.Append("kb");
                                contentBuilder.AppendLine();
                                if (m_generateBundleDetailReport)
                                {
                                    foreach (var assetPath in depBundleBuild.assetNames)
                                    {
                                        contentBuilder.AppendLine("        " + HandAssetName(assetPath) + "    memorySize: " + GetMemorySizeOfAsset(assetPath) / 1024 + "kb");
                                    }
                                }
                            }
                            else
                            {
                                contentBuilder.Append(" not found! ");
                            }
                        }
                    }
                }
                var bundleReport = outPath + "/" + "bundleReport.txt";
                File.WriteAllText(bundleReport, contentBuilder.ToString());
            }
            catch(Exception ex)
            {
                Debug.LogError("CreateBuildReport error" + ex.Message);
                Debug.LogException(ex);
            }
        }

        private long GetMemorySizeOfAsset(string assetPath)
        {
            try
            {
                var assetObj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if(assetObj == null)
                {
                    return 0;
                }
                long size = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(assetObj);
                return size;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        private string HandAssetName(string name)
        {
            if (string.IsNullOrEmpty(m_assetPrefix)) 
            {
                m_assetPrefix = "Assets/";
                Debug.LogWarning("assetPrefix is empty, auto remove Assets");
            }
            /// 添加多前缀剔除 by cmm
            string[] assetPrefixArray = m_assetPrefix.Split('|');
            for (int i = 0; i < assetPrefixArray.Length; ++i)
            {
                if (name.StartsWith(assetPrefixArray[i]))
                {
                    name = name.Substring(assetPrefixArray[i].Length);
                    break;
                }
                else
                {
                    //Debug.LogError("not found assetPrefix: " + name + " assetPrefix:" + m_assetPrefix);
                }
            }
            /// 

            /// 自定义路径映射
            {
                if(name.Contains("/Config/"))
                {
                    name = "Config/" + Path.GetFileName(name);
                }
                else if (!name.StartsWith("BundleRes/"))
                {
                    name = "AssetRes/" + Path.GetFileName(name);
                }
                else
                {
                    name = name.Replace("BundleRes/", "");
                }
            }
            /// 
            if (!m_containFileExtension) 
            {
                var idx = name.LastIndexOf('.');
                if (idx != -1)
                {
                    name = name.Substring(0, idx);
                }
            }
            return name;
        }

        private string GetManifestName(BuildTarget target, Model.NodeData node, bool finalName) {
            if (finalName && !string.IsNullOrEmpty (m_manifestName [target])) {
                return m_manifestName [target];
            } else {
                return Path.GetFileName(PrepareOutputDirectory(target, node, false, false));
            }
        }

        private string PrepareOutputDirectory(BuildTarget target, Model.NodeData node, bool autoCreate, bool throwException) {

            var outputOption = (OutputOption)m_outputOption [target];

            if(outputOption == OutputOption.BuildInCacheDirectory) {
                return FileUtility.EnsureAssetBundleCacheDirExists (target, node);
            }

            var outputDir = m_outputDir [target];

            outputDir = outputDir.Replace ("{Platform}", BuildTargetUtility.TargetToAssetBundlePlatformName (target));

            if (throwException) {
                if(string.IsNullOrEmpty(outputDir)) {
                    throw new NodeException ("Output directory is empty.", 
                        "Select valid output directory from inspector.", node);
                }

                if(target != BuildTargetUtility.GroupToTarget(BuildTargetGroup.Unknown) && 
                    outputOption == OutputOption.ErrorIfNoOutputDirectoryFound) 
                {
                    if (!Directory.Exists (outputDir)) {
                        throw new NodeException ("Output directory not found.", 
                            "Create output directory or select other valid directory from inspector.", node);
                    }
                }
            }

            if (autoCreate) {
                if(outputOption == OutputOption.DeleteAndRecreateOutputDirectory) {
                    if (Directory.Exists(outputDir)) {
                        FileUtility.DeleteDirectory(outputDir, true);
                    }
                }

                if (!Directory.Exists(outputDir)) {
                    Directory.CreateDirectory(outputDir);
                }
            }

            return outputDir;
        }

        // Check if given file is generated Item
		private bool IsFileIntendedItem(string filename, Dictionary<string, List<string>> bundleVariants) {
            //这里把 生成的 xml 和 json 也输出
            if (filename.Equals(GetAssetMapName())) return true;
            if (filename.Equals(AssetBundles.GraphTool.BuildMD5.GetVersionName())) return true;
            if (filename.Equals(GetBundleDepencyAtlasName())) return true;
            if (filename.Equals(GetUnloadableBundlesJsonName())) return true;

            filename = filename.ToLower();

            int lastDotManifestIndex = filename.LastIndexOf(".manifest");
            filename = (lastDotManifestIndex > 0) ? filename.Substring(0, lastDotManifestIndex) : filename;

            // test if given file is not configured as variant
			if(bundleVariants.ContainsKey(filename)) {
				var v = bundleVariants[filename];
				if(v.Contains(null)) {
					return true;
				}
			}

			int lastDotIndex = filename.LastIndexOf('.');
			var bundleNameFromFile  = (lastDotIndex > 0) ? filename.Substring(0, lastDotIndex): filename;
			var variantNameFromFile = (lastDotIndex > 0) ? filename.Substring(lastDotIndex+1): null;

			if(!bundleVariants.ContainsKey(bundleNameFromFile)) {
				return false;
			}

            var variants = bundleVariants[bundleNameFromFile];
            return variants.Contains(variantNameFromFile);
        }

        private string GetBundleDepencyAtlasName()
        {
            return "bundle2Atlas.xml";
        }

        public void GenerateBundleDependencyAtlas(IEnumerable<PerformGraph.AssetGroups> incoming, string outPath)
        {
            var sprite2SpriteAtlas = new Dictionary<string, string>();
            bundle2AtlasBundles = new Dictionary<string, List<string>>();
            //收集atlas 信息
            foreach (var ag in incoming)
            {
                foreach (var key in ag.assetGroups.Keys)
                {
                    var assets = ag.assetGroups[key];
                    foreach (var a in assets)
                    {
                        if (TypeUtility.GetMainAssetFuzzyTypeAtPath(a.importFrom) == typeof(UnityEngine.U2D.SpriteAtlas))
                        {
                            var dependencies = AssetDatabase.GetDependencies(a.importFrom);
                            foreach (var d in dependencies)
                            {
                                if (TypeUtility.GetMainAssetFuzzyTypeAtPath(d) == typeof(UnityEngine.Texture))
                                {
                                    sprite2SpriteAtlas.Add(d, key);
                                }
                            }
                        }
                    }
                }
            }
            //收集对atlas依赖信息
            foreach (var ag in incoming)
            {
                foreach (var key in ag.assetGroups.Keys)
                {
                    var assets = ag.assetGroups[key];
                    HashSet<string> atlasSet = new HashSet<string>();
                    var dependencies = DependencyUtility.GetDependencies(assets);
                    string atlasBundle = null;
                    foreach (var d in dependencies)
                    {
                        if (sprite2SpriteAtlas.TryGetValue(d, out atlasBundle) && atlasBundle != key)
                        {
                            atlasSet.Add(atlasBundle);
                        }
                    }
                    if (atlasSet.Count > 0)
                    {
                        bundle2AtlasBundles.Add(key, new List<string>(atlasSet));
                    }
                }
            }

            //3 生成依赖配置
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement rootElement = xmlDoc.CreateElement("", "Assets", "");
            xmlDoc.AppendChild(rootElement);
            foreach (var pair in bundle2AtlasBundles)
            {
                XmlElement assetEle = xmlDoc.CreateElement("Bundle");
                assetEle.SetAttribute("path", pair.Key);

                foreach (var assetbundle in pair.Value)
                {
                    XmlElement assetBundleEle = xmlDoc.CreateElement("AssetBundle");
                    assetBundleEle.InnerText = assetbundle;
                    assetEle.AppendChild(assetBundleEle);
                }

                rootElement.AppendChild(assetEle);
            }
            xmlDoc.Save(outPath);
        }


    }
}