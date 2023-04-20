using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
#if USE_SIMPLYGON
using ICSharpCode.SharpZipLib.Zip;
#endif
using UnityEditor;
using UnityEngine.SceneManagement;
using Simplygon;
using UnityEditor.FBX;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Athena.MeshSimplify
{
    public static class SimplygonTool
    {

        #region Simplygon Download Or Update

        private static string scriptPath = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
        private static string tempSimplygonDirectory = Application.dataPath + "/../Temp/Simplygon";
        private static string zipName = "SimplygonSDK.zip";
        private static List<string> needFiles = new List<string> {"Simplygon.dll", "SimplygonCWrapper.dll", "Simplygon.Unity.EditorPlugin.dll", "SimplygonLicenseApplication.exe"};
        private const string licensePath = "/HKEY_CURRENT_USER/Software/Microsoft/Windows/CurrentVersion/Explorer/FeatureUsage/AppSwitched";
        
        [InitializeOnLoadMethod]
        public static void AutoCheckUpdateSimplygonVersion()
        {
            if (PlayerPrefs.HasKey("EnableAutoCheckUpdateSimplygonVersion") && PlayerPrefs.GetInt("EnableAutoCheckUpdateSimplygonVersion") == 1)
            {
                if (Application.isPlaying) return;
                if (CheckUpdateSimplygonVersion())
                {
                    DisplayUpdateSimplygonDialog();
                }
            }
        }

        public static bool CheckUpdateSimplygonVersion()
        {
            var zipPath = GetSimplygonZipPath();
            var versionTxtPath = scriptPath + "/Plugins/version.txt";
            var name = Path.GetFileNameWithoutExtension(zipPath);
            var curVersion = File.ReadAllText(versionTxtPath);
            return !name.Equals(curVersion);
        }

        public static void DisplayUpdateSimplygonDialog()
        {
            var download = EditorUtility.DisplayDialog("Simplygon Update", "当前Simplygon已过时，请下载最新版本！", "下载", "关闭");
            if (download)
            {
                DownloadSimplygon();
            }
        }
        
        /// <summary>
        /// 下载Simplygon
        /// </summary>
        [MenuItem("*Athena*/Simplygon/下载最新版Simplygon", priority = 0)]
        public static void DownloadSimplygon()
        {
            try
            {
                DeleteTempDirectory();
                ShowProcess(0);
                var zipLine = GetSimplygonZipPath();
                if (!string.IsNullOrEmpty(zipLine))
                {
                    var version = Path.GetFileNameWithoutExtension(zipLine);
                    UnityWebRequest unityWebRequest = UnityWebRequest.Get(zipLine);
                    var downloadHandler = new SimplygonDownloadHandler();
                    downloadHandler.processAction = (progress) =>
                    {
                        ShowProcess(progress);
                    };
                    downloadHandler.completeAction = (data) =>
                    {
                        EditorUtility.ClearProgressBar();
                        if (Directory.Exists(tempSimplygonDirectory) == false) Directory.CreateDirectory(tempSimplygonDirectory);
                        File.WriteAllBytes(tempSimplygonDirectory + "/" + zipName, data);
                        var versionTxtPath = scriptPath + "/Plugins/version.txt";
                        File.WriteAllText(versionTxtPath, version);
                        UnzipSimplygon();
                        ReplaceProjectDllByLoadDll();
                        DeleteTempDirectory();
                        RegistSimplygon();
                        unityWebRequest.Dispose();
                    };
                    unityWebRequest.downloadHandler = downloadHandler;
                    unityWebRequest.SendWebRequest();
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                }
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                throw e;
            }
        }

        public static string GetSimplygonZipPath()
        {
            UnityWebRequest unityWebRequest = UnityWebRequest.Get("https://www.simplygon.com/Downloads");
            unityWebRequest.SendWebRequest();
            while (!unityWebRequest.isDone)
            {
                
            }
            
            var lines = unityWebRequest.downloadHandler.text.Split('\n').ToList();
            var downloadUrlTokeyLine = lines.Find(s => s.Contains("name=\"__RequestVerificationToken\""));
            var downloadUrlTokeyValue = GetTokeyValue(downloadUrlTokeyLine, "value");
            if (string.IsNullOrEmpty(downloadUrlTokeyValue) == false)
            {
                downloadUrlTokeyValue = downloadUrlTokeyValue.Replace("\"", "");
                var downloadUrl = "https://www.simplygon.com/Downloads/GetLatestBuilds/?__RequestVerificationToken=" +
                                  downloadUrlTokeyValue;

                unityWebRequest = UnityWebRequest.Get(downloadUrl);
                unityWebRequest.SendWebRequest();
                while (!unityWebRequest.isDone)
                {

                }

                lines = unityWebRequest.downloadHandler.text.Split('\n').ToList();
                var zipLine = lines.Find(s => s.Contains("SimplygonSDK.zip"));
                zipLine = zipLine.Remove(0, zipLine.IndexOf("\"") + 1);
                zipLine = zipLine.Remove(zipLine.LastIndexOf("\""));

                if (string.IsNullOrEmpty(zipLine))
                {
                    Debug.LogError("simplygon zip url load error");
                    return null;
                }
                return zipLine;
            }
            unityWebRequest.Dispose();
            return null;
        }

        /// <summary>
        /// 注册Simplygon
        /// </summary>
        [MenuItem("*Athena*/Simplygon/注册Simplygon", priority = 1)]
        public static void RegistSimplygon()
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = scriptPath + "/Plugins/SimplygonLicenseApplication.exe";
            // info.Arguments = "-InstallLicense <LicenseKey> [-DontSendTelemetry]";
            System.Diagnostics.Process.Start(info);
        }

        /// <summary>
        /// 取消注册Simplygon
        /// </summary>
        [MenuItem("*Athena*/Simplygon/取消注册Simplygon", priority = 2)]
        public static void UnregistSimplygon()
        {
            //C:\ProgramData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            //C:\Users\Administrator\AppData\Local
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var appDataLicensePath = Path.Combine(appDataPath, "Microsoft/SimplygonSDK/Simplygon_license.dat");
            var localAppDataLicensePath = Path.Combine(localAppDataPath, "Microsoft/SimplygonSDK/Simplygon_license.dat");
            if (File.Exists(localAppDataLicensePath)) File.Delete(localAppDataLicensePath);
            if (File.Exists(appDataLicensePath))
            {
#if UNITY_IOS
                appDataLicensePath = appDataLicensePath.Replace("\\", "/");
#else
                appDataLicensePath = appDataLicensePath.Replace("/", "\\");
#endif
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                info.FileName = scriptPath + "/Plugins/deletelicense.bat";
                info.Verb = "runas";
                System.Diagnostics.Process.Start(info);
            }
        }
        
        /// <summary>
        /// 解压Zip
        /// </summary>
        private static void UnzipSimplygon()
        {
#if USE_SIMPLYGON
            ZipFile zipFile0 = new ZipFile(tempSimplygonDirectory + "/" + zipName);
            for (int i = 0; i < zipFile0.Count; i++)
            {
                var name = zipFile0[i].Name.Replace("\\", "/");;
                if (name.Contains("/")) name = name.Remove(0, name.LastIndexOf("/") + 1);
                foreach (var needFile in needFiles)
                {
                    if (name.Equals(needFile))
                    {
                        Debug.Log(needFile);
                        var dllStream = zipFile0.GetInputStream(zipFile0[i]);
                        using(var stream = new FileStream(tempSimplygonDirectory + "/" + needFile, FileMode.Create, FileAccess.ReadWrite))
                        {
                            dllStream.CopyTo(stream);
                        }
                        dllStream.Close();
                    }
                }
            }
            zipFile0.Close();
#else
            Debug.LogError("要使用simplygon，请添加宏 USE_SIMPLYGON");
#endif
        }
        
        /// <summary>
        /// 替换工程dll
        /// </summary>
        private static void ReplaceProjectDllByLoadDll()
        {
            string dllFolder = scriptPath.Replace("\\", "/") + "/Plugins";
            for (int i = 0; i < needFiles.Count; i++)
            {
                var dllPath = dllFolder + "/" + needFiles[i];
                if (File.Exists(dllPath)) File.Delete(dllPath);
                File.Copy(tempSimplygonDirectory + "/" + needFiles[i], dllPath);
                AssetDatabase.ImportAsset(dllPath);
                PluginImporter pluginImporter = PluginImporter.GetAtPath(dllPath) as PluginImporter;
                pluginImporter?.SetCompatibleWithEditor(true);
                pluginImporter?.SaveAndReimport();
            }
        }

        private static void ShowProcess(float process)
        {
             EditorUtility.DisplayProgressBar("Update Simplygon", "Update Progress: " + process.ToString("p2"), process);
        }
        
        private static string GetTokeyValue(string tokey, string key)
        {
            var items = tokey.Split(' ');
            foreach (var item in items)
            {
                if (item.Contains("="))
                {
                    var keyValue = item.Split('=');
                    if (keyValue.Length == 2)
                    {
                        if (keyValue[0].Equals(key))
                        {
                            return keyValue[1];
                        }
                    }
                }
            }
            return null;
        }
        
        private class SimplygonDownloadHandler : DownloadHandlerScript
        {
            public Action<float> processAction;
            public Action<byte[]> completeAction;

            private ulong contentLength;
            private List<byte> AllDatas;

            public SimplygonDownloadHandler()
            {
                AllDatas = new List<byte>();
            }
            
            protected override void ReceiveContentLengthHeader(ulong contentLength)
            {
                this.contentLength = contentLength;
                base.ReceiveContentLengthHeader(contentLength);
            }
            
            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                AllDatas.AddRange(data);
                processAction?.Invoke((float)AllDatas.Count / contentLength);
                return base.ReceiveData(data, dataLength);
            }

            protected override void CompleteContent()
            {
                completeAction?.Invoke(AllDatas.ToArray());
                base.CompleteContent();
            }
        }
        #endregion
        
        private const string tempPath = "Assets/Simplygon/Generated/Temp";

        public static string SimplygonTempPath
        {
            get { return tempPath; }
        }
        
        private static Material defaultMaterial;
        public static Material DefaultMaterial
        {
            get
            {
                if (defaultMaterial == null)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    defaultMaterial = cube.GetComponent<MeshRenderer>().sharedMaterial;
                    GameObject.DestroyImmediate(cube);
                }
                return defaultMaterial;
            }
        }
        
        /// <summary>
        /// 模型减面（并转换为FBX）
        /// </summary>
        public static List<string> ReduceToFbx(SimplygonData simplygonData, ReduceMethod method)
        {
            List<string> fbxPaths = new List<string>();
            var list = Reduce(simplygonData, method);
            string objName = simplygonData.target.name;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var indexName = GetLODName(objName + simplygonData.nameExt, i);
                    var fbxPath = simplygonData.outputFolder + "/" + indexName + ".fbx";
                    if (list[i])
                    {
                        ModelExporter.ExportObject(fbxPath, list[i]);
                        //FBXExporter.ExportGameObjToFBX(list[i], fbxPath);
                        fbxPaths.Add(fbxPath);
                    }
                }
            }
            DeleteTempAsset();
            AssetDatabase.DeleteAsset(GetOutFbxFolderPath(simplygonData));
            AssetDatabase.SaveAssets();
            Dispose();
            return fbxPaths;
        }
        
        public static List<GameObject> Reduce(SimplygonData simplygonData, ReduceMethod method)
        {
            if (simplygonData.target == null || simplygonData.lodSimplygonItemDatas == null || simplygonData.lodSimplygonItemDatas.Count == 0) 
                return null;
            var pos = simplygonData.target.transform.position;
            var rot = simplygonData.target.transform.rotation;
            var scale = simplygonData.target.transform.localScale;

            var inputFile = GetModelPath(simplygonData.target);
            
            List<GameObject> list = new List<GameObject>();
            if (method == ReduceMethod.Simplygon)
            {
                using (ISimplygon sg = SimplygonLoader.InitSimplygon(out EErrorCodes simplygonErrorCode, out string simplygonErrorMessage))
                {
                    if (simplygonErrorCode != EErrorCodes.NoError)
                    {
                        DisplayError(simplygonErrorCode, simplygonErrorMessage);
                        return null;
                    }

                    var sceneName = SceneManager.GetActiveScene().name.ToLower();
                    for (int i = 0; i < simplygonData.lodSimplygonItemDatas.Count; i++)
                    {
                        int lodIndex = i;
                        if (simplygonData.isTerrain) lodIndex = lodIndex + 1; 
                        var ext = "_LOD" + i;
                        GameObject obj = null;
                        string outputFile = GetOutFbxFilePath(simplygonData, ext);
                        if (simplygonData.lodSimplygonItemDatas[i].reductionUnit == ReductionUnit.Default)
                        {
                            obj = RunReduce(sg, simplygonData.target, inputFile, outputFile, simplygonData.lodSimplygonItemDatas[i], method);
                        }
                        else
                        {
                            obj = RunReduceMeshes(sg, simplygonData.target, outputFile, simplygonData.lodSimplygonItemDatas[i], method);
                        }
                        obj.transform.position = pos;
                        obj.transform.rotation = rot;
                        obj.transform.localScale = scale;
                    
                        list.Add(obj);
                    }
                }
            }
            else
            {
                for (int i = 0; i < simplygonData.lodSimplygonItemDatas.Count; i++)
                {
                    int lodIndex = i;
                    if (simplygonData.isTerrain) lodIndex = lodIndex + 1; 
                    var ext = "_LOD" + i;
                    GameObject obj = null;
                    string outputFile = GetOutFbxFilePath(simplygonData, ext);
                    if (simplygonData.lodSimplygonItemDatas[i].reductionUnit == ReductionUnit.Default)
                    {
                        obj = RunReduce(null, simplygonData.target, inputFile, outputFile, simplygonData.lodSimplygonItemDatas[i], method);
                    }
                    else
                    {
                        obj = RunReduceMeshes(null, simplygonData.target, outputFile, simplygonData.lodSimplygonItemDatas[i], method);
                    }
                    obj.transform.position = pos;
                    obj.transform.rotation = rot;
                    obj.transform.localScale = scale;
                    
                    list.Add(obj);
                }
            }
            return list;
        }
        
        public static void DeleteTempAsset()
        {
            AssetDatabase.DeleteAsset(SimplygonTempPath);
            AssetDatabase.DeleteAsset(GetTempFolderPath(null));
        }

        public static void DisplayError(EErrorCodes errorCodes, string errorMessage)
        {
            Debug.LogError($"Simplygon Init Error, {errorCodes} : {errorMessage}");
            switch (errorCodes)
            {
                case EErrorCodes.NoLicense: //注册
                case EErrorCodes.MaxNodesReachForThisLicense: 
                    bool isRegist = EditorUtility.DisplayDialog("Simplygon Init Error", "当前没有秘钥或已达到最大使用数量，请切换账号重新注册！", "注册", "关闭");
                    if (isRegist)
                    {
                        RegistSimplygon();
                    }
                    break;
                case EErrorCodes.YourLicenseRequiresLatestSimplygon: //下载
                case EErrorCodes.LicenseNotForThisVersion:
                case EErrorCodes.DLLOrDependenciesNotFound:
                    EditorUtility.DisplayDialog("Simplygon Init Error", "当前dll版本为旧版本，请重启Unity并下载dll文件和注册！", "关闭");
                    break;
                case EErrorCodes.DLLFailedToLoad: //安装（确实配置）
                    bool isInstall = EditorUtility.DisplayDialog("Simplygon Init Error", "当前电脑.Net配置不全，请手动安装Simplygon.exe以补全配置（安装后可卸载）！", "安装", "关闭");
                    if (isInstall)
                    {
                        Application.OpenURL("https://www.simplygon.com/downloads");
                    }
                    break;
                case EErrorCodes.NoNetworkCardFound:
                    EditorUtility.DisplayDialog("Simplygon Init Error", "当前电脑未联网！", "关闭");
                    break;
            }
        }

        public static void Dispose()
        {
            SimplygonLoader.DisposeSimplygon();
            EditorUtility.ClearProgressBar();
        }

        internal static GameObject RunReduceMeshes(ISimplygon sg, GameObject target, string outputFile, ReductionSetting simplygonItemData, ReduceMethod method)
        {
            string tempMeshAssetFolder = GetTempMeshAssetFolderPath(target);
            if (Directory.Exists(tempMeshAssetFolder) == false) Directory.CreateDirectory(tempMeshAssetFolder);
            
            Dictionary<Mesh, GameObject> meshObjDic = new Dictionary<Mesh, GameObject>();
            foreach (var meshFilter in target.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null && meshObjDic.ContainsKey(meshFilter.sharedMesh) == false)
                {
                    // meshes.Add(meshFilter.sharedMesh);
                    var meshObj = new GameObject(meshFilter.sharedMesh.name);
                    meshObj.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
                    meshObj.AddComponent<MeshRenderer>().sharedMaterial = meshFilter?.GetComponent<MeshRenderer>()?.sharedMaterial ?? DefaultMaterial;
                    PrefabUtility.SaveAsPrefabAssetAndConnect(meshObj, tempMeshAssetFolder + "/" + meshObj.name + ".prefab", InteractionMode.AutomatedAction);
                    meshObjDic.Add(meshFilter.sharedMesh, meshObj);
                }
            }

            var obj = GameObject.Instantiate(target);
            obj.name = target.name;
            var mfs = obj.GetComponentsInChildren<MeshFilter>().ToList();
            foreach (var meshObjItem in meshObjDic)
            {
                var path = GetModelPath(meshObjItem.Value);
                var meshObj = RunReduce(sg, meshObjItem.Value, path, tempMeshAssetFolder + "/" + meshObjItem.Value.name + ".fbx", simplygonItemData, method);
                var matchMfs = mfs.FindAll(s => s.sharedMesh.Equals(meshObjItem.Key));
                foreach (var matchMf in matchMfs)
                {
                    matchMf.sharedMesh = meshObj.GetComponent<MeshFilter>().sharedMesh;
                }
                GameObject.DestroyImmediate(meshObjItem.Value);
            }
            ModelExporter.ExportObject(outputFile, obj);
            //FBXExporter.ExportGameObjToFBX(obj, outputFile);
            AssetDatabase.ImportAsset(outputFile);
            GameObject.DestroyImmediate(obj);
            return AssetDatabase.LoadAssetAtPath<GameObject>(outputFile);
        }

        internal static GameObject RunReduce(ISimplygon sg, GameObject target,  string inputFile, string outputFile, ReductionSetting simplygonItemData, ReduceMethod method)
        {
            if (method == ReduceMethod.Simplygon)
            {

                using (spSceneImporter sgSceneImporter = sg.CreateSceneImporter())
                {
                    sgSceneImporter.SetImportFilePath(inputFile);
                    if (sgSceneImporter.Run() != EErrorCodes.NoError)
                        throw new System.Exception($"Failed to load {inputFile}.");
                    using (spScene sgScene = sgSceneImporter.GetScene())
                    using (spReductionPipeline sgReductionPipeline = sg.CreateReductionPipeline())
                    {
                        using (spReductionSettings sgReductionSettings = sgReductionPipeline.GetReductionSettings())
                        {
                            sgReductionSettings.SetReductionTargets(EStopCondition.All, true,
                                simplygonItemData.enableTriangleCount, simplygonItemData.enableMaxDeviation,
                                simplygonItemData.enableScreenSize);
                            sgReductionSettings.SetReductionTargetTriangleRatio(simplygonItemData.triangleRatio);
                            sgReductionSettings.SetDataCreationPreferences(
                                (EDataCreationPreferences)((int)simplygonItemData.useDataPreferences));
                            if (simplygonItemData.enableScreenSize)
                                sgReductionSettings.SetReductionTargetOnScreenSize(simplygonItemData.screenSize);
                            if (simplygonItemData.enableMaxEdgeLength)
                                sgReductionSettings.SetMaxEdgeLength(simplygonItemData.maxEdgeLength);
                            if (simplygonItemData.enableMaxDeviation)
                                sgReductionSettings.SetReductionTargetMaxDeviation(simplygonItemData.maxDeviation);
                            if (simplygonItemData.enableTriangleCount)
                                sgReductionSettings.SetReductionTargetTriangleCount(simplygonItemData.triangleCount);

                            sgReductionSettings.SetReductionHeuristics(
                                (EReductionHeuristics)((int)simplygonItemData.reductionHeuristics));
                            sgReductionSettings.SetGeometryImportance(simplygonItemData.geometryImportance);
                            sgReductionSettings.SetMaterialImportance(simplygonItemData.materialImportance);
                            sgReductionSettings.SetTextureImportance(simplygonItemData.textureImportance);
                            sgReductionSettings.SetShadingImportance(simplygonItemData.shadingImportance);
                            sgReductionSettings.SetGroupImportance(simplygonItemData.groupImportance);
                            sgReductionSettings.SetVertexColorImportance(simplygonItemData.vertexColorImportance);
                            sgReductionSettings.SetEdgeSetImportance(simplygonItemData.edgeSetImportance);
                            sgReductionSettings.SetSkinningImportance(simplygonItemData.skinningImportance);
                            // sgReductionSettings.SetCurvatureImportance(simplygonItemData.curvatureImportance);   //官方已移除

                            sgReductionSettings.SetCreateGeomorphGeometry(false);
                            sgReductionSettings.SetAllowDegenerateTexCoords(false);
                            sgReductionSettings.SetKeepSymmetry(false);
                            sgReductionSettings.SetUseAutomaticSymmetryDetection(false);
                            sgReductionSettings.SetUseSymmetryQuadRetriangulator(true);
                            sgReductionSettings.SetSymmetryAxis(ESymmetryAxis.X);
                            sgReductionSettings.SetSymmetryOffset(0.0f);
                            sgReductionSettings.SetSymmetryDetectionTolerance(0.0001f);
                            sgReductionSettings.SetInwardMoveMultiplier(1.0f);
                            sgReductionSettings.SetInwardMoveMultiplier(1.0f);
                            sgReductionSettings.SetUseHighQualityNormalCalculation(true);
                            sgReductionSettings.SetMergeGeometries(false);
                            sgReductionSettings.SetKeepUnprocessedSceneMeshes(false);
                            sgReductionSettings.SetLockGeometricBorder(simplygonItemData.lockGeometricBorder);
                        }

                        using (spNormalCalculationSettings sgNormalCalculationSettings =
                               sgReductionPipeline.GetNormalCalculationSettings())
                        {
                            sgNormalCalculationSettings.SetReplaceNormals(false);
                            sgNormalCalculationSettings.SetReplaceTangents(false);
                            sgNormalCalculationSettings.SetHardEdgeAngle(75.0f);
                            sgNormalCalculationSettings.SetRepairInvalidNormals(true);
                            // sgNormalCalculationSettings.SetReorthogonalizeTangentSpace(true);
                            // sgNormalCalculationSettings.SetScaleByArea(true);
                            // sgNormalCalculationSettings.SetScaleByAngle(true);
                            // sgNormalCalculationSettings.SetSnapNormalsToFlatSurfaces(false);
                        }

                        // Start the reduction pipeline. 
                        sgReductionPipeline.RunScene(sgScene, EPipelineRunMode.RunInThisProcess);
                        using (spScene sgProcessedScene = sgReductionPipeline.GetProcessedScene())
                        {
                            using (spSceneExporter sgSceneExporter = sg.CreateSceneExporter())
                            {
                                sgSceneExporter.SetScene(sgProcessedScene);
                                sgSceneExporter.SetExportFilePath(outputFile);

                                if (sgSceneExporter.Run() != EErrorCodes.NoError)
                                    throw new System.Exception($"Failed to save {outputFile}.");
                                AssetDatabase.ImportAsset(outputFile);
                                AssetDatabase.Refresh();
                                return AssetDatabase.LoadAssetAtPath<GameObject>(outputFile);
                            }
                        }
                    }
                }
            }
            else if (method == ReduceMethod.UE4)
            {
                try
                {
                    Process process = new Process();
                    
                    process.StartInfo.FileName = scriptPath + "\\Plugins\\Hlod.exe";
                    process.StartInfo.UseShellExecute = false;
                    
                    process.StartInfo.CreateNoWindow = true;
                    
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    // string ipath = Application.dataPath.Replace("Assets", "") + inputFile;
                    string opath = Application.dataPath.Replace("Assets", "") + outputFile;
                    
                    StringBuilder sb = new StringBuilder();

                    sb.Append("\"");
                    sb.Append("method|0;");
                    sb.Append("inPath|" + inputFile + ";");
                    sb.Append("outFbxPath|" + opath + ";");
                    sb.Append("percent|" + simplygonItemData.triangleRatio + ";");
                    sb.Append("lockBound|" + (simplygonItemData.lockGeometricBorder ? 1 : 0) + ";");
                    sb.Append("weldingThreshold|" + simplygonItemData.weldingThreshold + ";");
                    
                    sb.Append("lockEdgeLength|" + (simplygonItemData.enableMaxEdgeLength ? 1 : 0) + ";");
                    sb.Append("maxEdgeLength|" + simplygonItemData.maxEdgeLength + ";");
                    sb.Append("lockMaxDeviation|" + (simplygonItemData.enableMaxDeviation ? 1 : 0) + ";");
                    sb.Append("maxDeviation|" + simplygonItemData.maxDeviation + ";");

                    sb.Append("debug|1;");
                    sb.Append("\"");

                    process.StartInfo.Arguments = sb.ToString();

                    process.EnableRaisingEvents = true;

                    process.StartInfo.RedirectStandardOutput = true;
                        
                    process.OutputDataReceived += ChangeOutput;
                            
                    process.Start();
                    EditorUtility.DisplayProgressBar("正在减面", "减面文件：" + outputFile.Substring(outputFile.LastIndexOf('/') + 1), 0.1f);

                    process.BeginOutputReadLine();

                    process.WaitForExit();
                    
                    EditorUtility.ClearProgressBar();
                    
                    int ExitCode = process.ExitCode;
                    
                    Debug.Log("执行结束:" + ExitCode);
                    
                    process.Close();

                     AssetDatabase.ImportAsset(outputFile);
                     AssetDatabase.Refresh();
                    return AssetDatabase.LoadAssetAtPath<GameObject>(outputFile);//实际是mesh的Obj
                }
                catch (Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.LogError(e.Message);
                }
            }
            return null;
        }
        
        //输出重定向函数
        private static void ChangeOutput(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data)) //字符串不为空时
                UnityEngine.Debug.Log(outLine.Data);//将进程的输出信息转移
        }

        /// <summary>
        /// 删除临时文件夹
        /// </summary>
        public static void DeleteTempDirectory()
        {
            if (Directory.Exists(tempSimplygonDirectory))
                Directory.Delete(tempSimplygonDirectory, true);
        }

        /// <summary>
        /// get lod name
        /// </summary>
        private static string GetLODName(string name, int lodIndex)
        {
            return name + (lodIndex == 0 ? "" : "_" + lodIndex);
        }
        
        private static string GetModelPath(GameObject obj)
        {
            var assetPath = GetTempFolderPath(obj) + ".fbx";
            ModelExporter.ExportObject(assetPath, obj);
            //FBXExporter.ExportGameObjToFBX(obj, assetPath);
            return assetPath;
        }

        /// <summary>
        /// get asset output fbx path
        /// </summary>
        private static string GetOutFbxFilePath(SimplygonData simplygonData, string ext = "")
        {
            var sceneName = (IsFbx(simplygonData.target) && !simplygonData.isTerrain) ? "" : SceneManager.GetActiveScene().name.ToLower();
            string outputName = simplygonData.target.name;
            string outputFolder = simplygonData.outputFolder + "/" + sceneName + "/" + outputName;
            if (Directory.Exists(outputFolder) == false)
                Directory.CreateDirectory(outputFolder);
            return outputFolder + "/" + outputName + ext + ".fbx";
        }
        
        /// <summary>
        /// get asset output fbx folder path
        /// </summary>
        private static string GetOutFbxFolderPath(SimplygonData simplygonData)
        {
            var sceneName = (IsFbx(simplygonData.target) && !simplygonData.isTerrain) ? "" : SceneManager.GetActiveScene().name.ToLower();
            string outputName = simplygonData.target.name;
            string outputFolder = simplygonData.outputFolder + "/" + sceneName + "/" + outputName;
            if (Directory.Exists(outputFolder) == false)
                Directory.CreateDirectory(outputFolder);
            return outputFolder;
        }
        
        /// <summary>
        /// get asset mesh tempFolder
        /// </summary>
        private static string GetTempMeshAssetFolderPath(GameObject target)
        {
            var path = SimplygonTempPath + "/" + target.name;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// get out tempFolder
        /// </summary>
        private static string GetTempFolderPath(GameObject target)
        {
            var tempFolder = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), "temp/Simplygon");
            var path = target ? Path.Combine(tempFolder, target.name) : tempFolder;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path.Replace("\\", "/");
        }
        
        private static bool IsFbx(GameObject obj)
        {
            try
            {
                ModelImporter modelImporter = ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(obj)) as ModelImporter;
                if (modelImporter)
                {
                    return true;
                }
            }
            catch{ }
            return false;
        }
    }
}