/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
//#define SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Zeus.Core.FileSystem;
using Zeus.Core.Collections;
#if UNITY_EDITOR
using System.Xml;
#endif
using ZeusFlatBuffers;
#if SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
using UnityEngine.Build.Pipeline;
#endif

namespace Zeus.Framework.Asset
{
    /// <summary>
    /// AssetBundle通用函数
    /// </summary>
    public static class AssetBundleUtils
    {
        private class StringListForJson
        {
            public List<string> Items = new List<string>();
        }

#if UNITY_EDITOR
        private static Dictionary<string, string> _assetMapBundles;
#else
        private static Dictionary<uint, string> _assetHashMapBundles;
        private static Dictionary<string, string> _assetPathMapBundles;
#endif

        private static Dictionary<string, List<string>> _bundleMapAsset;
        private static Dictionary<string, string[]> _bundle2AtlasBundle;
        private static DictionaryLRU<string, string> _bundleName2PathCache;
        private static Dictionary<string, string[]> _bundleDepCache;
        private static HashSet<string> _unloadbleBundles;
#if SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
        private static CompatibilityAssetBundleManifest _comptmanifest = null;
#endif
        private static AssetBundleManifest _manifest = null;
        private static ZeusAssetBundleManifest _zeusManifest = null;
        private static Dictionary<string, List<string>> _bundle2Dependencies;

        private const int SCATTER_MODULUS = 200;
        public const string AssetMapNameXml = "assetMapName.xml";
        public const string Bundle2AtlasXml = "bundle2Atlas.xml";
        public const string MD5VersionXml = "MD5Version.xml";
        private const string AssetMapNameFb = "assetMapName.fb";
        private const string UnloadbleBundlesFb = "unloadbleBundles.fb";
        private const string UnloadbleBundlesJson = "unloadableBundles.json";
        private const string Bundle2AtlasFb = "bundle2Atlas.fb";

        static bool _init = false;

        ///初始化.
        public static void Init()
        {
            if (_init) return;
            DateTime start = DateTime.Now;
            _bundleName2PathCache = new DictionaryLRU<string, string>(2048);
            try
            {
                // 初始化zeusManifest
                var fbPath = _GetRelativeBundlePath("zeusBundleManifest.fb");
                byte[] content = VFileSystem.ReadAllBytes(fbPath);
                _zeusManifest = new Asset.ZeusAssetBundleManifest(content);
            }
            catch (Exception e)
            {
                Debug.Log("zeusManifest load failed, try to use unity assetsManifest...");
            }
            try
            {
                if (_zeusManifest == null)
                    _LoadManifest(); //初始化manifest
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                if (_manifest == null)
                {
                    _LoadDependenciesForManifestFile(out _bundle2Dependencies);
                }
            }
            finally
            {
                _init = true;
            }
#if UNITY_EDITOR
            _LoadAssetMapXML(_GetRelativeBundlePath(AssetMapNameXml), out _assetMapBundles, out _bundleMapAsset);
            Debug.Log("_LoadAssetMapXML" + (DateTime.Now - start).TotalSeconds);

            start = DateTime.Now;
            _LoadBundle2AtlasXML(_GetRelativeBundlePath(Bundle2AtlasXml), out _bundle2AtlasBundle);
            Debug.Log("_LoadBundle2AtlasXML" + (DateTime.Now - start).TotalSeconds);
            _bundleDepCache = new Dictionary<string, string[]>(_bundleMapAsset.Count);
#else
            int bundleCount;
            _LoadAssetMapFB(_GetRelativeBundlePath(AssetMapNameFb), out _assetHashMapBundles, out _assetPathMapBundles, out bundleCount);
            Debug.Log("_LoadAssetMapFB " + (DateTime.Now - start).TotalSeconds);

            start = DateTime.Now;
            _LoadBundle2AtlasFB(_GetRelativeBundlePath(Bundle2AtlasFb), out _bundle2AtlasBundle);
            Debug.Log("_LoadBundle2AtlasFB" + (DateTime.Now - start).TotalSeconds);
            _bundleDepCache = new Dictionary<string, string[]>(bundleCount / 8);
#endif
            _LoadUnloadbleBundles();
#if DEVELOPMENT_BUILD
            GC.Collect();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 加载打包前,asset路径 对应的AssetBundle名字
        /// </summary>
        /// <param name="path"> 映射关系的xml  </param>
        /// <param name="assetMapAssetBundle"> 打包前资源路径映射 AssetBundle名称 </param>
        /// <param name="assetBundleMapAsset"> assetBundle名称 映射 asset 路径 </param>
        public static void _LoadAssetMapXML(string path, out Dictionary<string, string> assetMapAssetBundle,
            out Dictionary<string, List<string>> assetBundleMapAsset)
        {
            Stream stream = null;
            try
            {
                stream = VFileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                assetMapAssetBundle = new Dictionary<string, string>();
                assetBundleMapAsset = new Dictionary<string, List<string>>();
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                XmlNode rootNode = doc.SelectSingleNode("Assets");
                foreach (XmlNode assetNode in rootNode.ChildNodes)
                {
                    string mapAssetBundle;
                    List<string> mapAssets;
                    XmlElement assetElement;
                    if (assetNode.Attributes != null)
                    {
                        assetElement = (XmlElement)assetNode;
                    }
                    else
                    {
                        continue;
                    }
                    string assetPath = assetElement.GetAttribute("path");
                    if (!assetPath.EndsWith(".unity")) CFEngine.SimpleTools.StringToLower(ref assetPath);

                    if (!assetMapAssetBundle.TryGetValue(assetPath, out mapAssetBundle))
                    {
                        XmlNode bundleNode = assetElement.FirstChild;
                        if (bundleNode.Attributes != null)
                        {
                            XmlElement bundleElement = (XmlElement)bundleNode;
                            assetMapAssetBundle.Add(assetPath, bundleElement.InnerText);
                            if (!assetBundleMapAsset.TryGetValue(bundleElement.InnerText, out mapAssets))
                            {
                                mapAssets = new List<string>();
                                assetBundleMapAsset.Add(bundleElement.InnerText, mapAssets);
                            }
                            mapAssets.Add(assetPath);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                assetMapAssetBundle = null;
                assetBundleMapAsset = null;
                Debug.LogError(e);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// bundle2Atlas.xml:bundle 对 atlas bundle的依赖关系
        /// </summary>
        /// <param name="Bundle"> bundle name  </param>
        /// <param name="bundle2AtlasMap"> bundle 到 atlas bundle依赖 </param>
        public static void _LoadBundle2AtlasXML(string path, out Dictionary<string, string[]> bundle2AtlasMap)
        {
            bundle2AtlasMap = new Dictionary<string, string[]>();
            var dic = new Dictionary<string, List<string>>();
            Stream stream = null;
            try
            {
                stream = VFileSystem.OpenFile(path, FileMode.Open, FileAccess.Read);
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                XmlNode rootNode = doc.SelectSingleNode("Assets");
                foreach (XmlElement assetElement in rootNode.ChildNodes)
                {
                    List<string> mapAssetBundles;

                    string bundlename = assetElement.GetAttribute("path");
                    if (!dic.TryGetValue(bundlename, out mapAssetBundles))
                    {
                        mapAssetBundles = new List<string>();
                        dic.Add(bundlename, mapAssetBundles);
                    }
                    foreach (XmlElement bundleElement in assetElement)
                    {
                        mapAssetBundles.Add(bundleElement.InnerText);
                    }
                }
                foreach (var item in dic)
                {
                    bundle2AtlasMap.Add(item.Key, item.Value.ToArray());
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public static void BuildFlatBuffer()
        {
            BuildAssetMapBundleFb();
            BuildBundle2AtlasFb();
            BuildUnloadableBundlesFb();
            BuildZeusManifestFb();
        }

        private static void BuildAssetMapBundleFb()
        {
            //Gen assetMapBundle.fb
            var builder = new FlatBufferBuilder(1);

            Dictionary<uint, List<string>> hash2Assets = new Dictionary<uint, List<string>>(_assetMapBundles.Count);
            foreach (var pair in _assetMapBundles)
            {
                uint hash = GetHashCode(pair.Key);
                List<string> assets;
                if (!hash2Assets.TryGetValue(hash, out assets))
                {
                    assets = new List<string>();
                    hash2Assets.Add(hash, assets);
                }
                assets.Add(pair.Key);
            }
            List<Offset<AssetHashMapBundleFB>> hash2Bundle = new List<Offset<AssetHashMapBundleFB>>();
            List<Offset<AssetPathMapBundleFB>> path2Bundle = new List<Offset<AssetPathMapBundleFB>>();
            foreach (var pair in hash2Assets)
            {
                if(pair.Value.Count > 1)
                {
                    foreach(var asset in pair.Value)
                    {
                        path2Bundle.Add(AssetPathMapBundleFB.CreateAssetPathMapBundleFB(builder,
                            builder.CreateString(asset), builder.CreateString(_assetMapBundles[asset])));
                    }
                }
                else
                {
                    hash2Bundle.Add(AssetHashMapBundleFB.CreateAssetHashMapBundleFB(builder,
                        pair.Key, builder.CreateString(_assetMapBundles[pair.Value[0]])));
                }
            }
            Debug.Log("hash2Bundle Count: " + hash2Bundle.Count + " name2Bundle Count: " + path2Bundle.Count);
            VectorOffset hash2BundleVector = AssetMapBundleFB.CreateH2bVector(builder, hash2Bundle.ToArray());
            VectorOffset name2BundleVector = AssetMapBundleFB.CreateP2bVector(builder, path2Bundle.ToArray());
            AssetMapBundleFB.StartAssetMapBundleFB(builder);
            AssetMapBundleFB.AddH2b(builder, hash2BundleVector);
            AssetMapBundleFB.AddP2b(builder, name2BundleVector);
            var wo = AssetMapBundleFB.EndAssetMapBundleFB(builder);
            AssetMapBundleFB.FinishAssetMapBundleFBBuffer(builder, wo);

            File.WriteAllBytes(_GetRelativeBundlePath(AssetMapNameFb), builder.SizedByteArray());
        }

        private static void BuildBundle2AtlasFb() 
        {
            //Gen bundle2Atlas.fb
            var builder = new FlatBufferBuilder(1);
            int bundleCount = _bundle2AtlasBundle.Keys.Count;

            var b2AList = new Offset<Bundle2AtlasFB>[bundleCount];
            int i = 0;
            foreach (var pair in _bundle2AtlasBundle)
            {
                StringOffset bundle = builder.CreateString(pair.Key);
                StringOffset[] atlas = new StringOffset[pair.Value.Length];
                for(int j = 0; j < pair.Value.Length; ++j)
                {
                    atlas[j] = builder.CreateString(pair.Value[j]);
                }
                VectorOffset atlasVec = Bundle2AtlasFB.CreateAtlasVector(builder, atlas);
                b2AList[i++] = Bundle2AtlasFB.CreateBundle2AtlasFB(builder, bundle, atlasVec);
            }
            VectorOffset b2AVec = Bundle2AtlasListFB.CreateB2AVector(builder, b2AList);
            Bundle2AtlasListFB.StartBundle2AtlasListFB(builder);
            Bundle2AtlasListFB.AddB2A(builder, b2AVec);
            var endOffset = Bundle2AtlasListFB.EndBundle2AtlasListFB(builder);
            Bundle2AtlasListFB.FinishBundle2AtlasListFBBuffer(builder, endOffset);

            File.WriteAllBytes(_GetRelativeBundlePath(Bundle2AtlasFb), builder.SizedByteArray());
        }

        [MenuItem("Zeus/Asset/Generate UnloadbleBundlesFb", false, 7)]
        public static void TestUnloadbleFb()
        {
            Init();
            BuildUnloadableBundlesFb();
        }

        
        [MenuItem("Zeus/Asset/Generate ZeusManifestFb", false, 7)]
        public static void BuildZeusManifestFb()
        {
            ReInit();
            Init();
            if(_zeusManifest!=null)
            {
                Debug.Log("zeusBundleManifest.fb already exists!  filepath:"+_GetRelativeBundlePath("zeusBundleManifest.fb"));
                return;
            }
             if(_manifest==null)
            {
                Debug.Log("zeusBundleManifest.fb build ERROR: _manifest cannot found!");
                return;
            }
#if UNITY_WEBGL
            var builder = ZeusAssetBundleManifest.BuildZeusAssetBundleManifest(_manifest,true);
#else
            var builder = ZeusAssetBundleManifest.BuildZeusAssetBundleManifest(_manifest,false);
#endif
            if(builder==null) return;
            File.WriteAllBytes(_GetRelativeBundlePath("zeusBundleManifest.fb"), builder.SizedByteArray());
            Debug.Log("ZeusAssetBundleManifest Build Over!  filepath:"+_GetRelativeBundlePath("zeusBundleManifest.fb"));
        }

        private static void BuildUnloadableBundlesFb()
        {
            var jsonPath = _GetRelativeBundlePath(UnloadbleBundlesJson);
            var json = File.ReadAllText(jsonPath);
            var stringListForJson = JsonUtility.FromJson<StringListForJson>(json);
            var unloadbleBundles = stringListForJson.Items;

            var builder = new FlatBufferBuilder(1);
            
            StringOffset[] soBundles = new StringOffset[unloadbleBundles.Count];
            int bundleIndex = 0;
            foreach (var bundleName in unloadbleBundles)
            {
                soBundles[bundleIndex++] = builder.CreateString(bundleName);
            }
            
            VectorOffset vecOffset = UnloadbleAssetBundle.CreateBundlesVector(builder, soBundles);
            UnloadbleAssetBundle.StartUnloadbleAssetBundle(builder);
            UnloadbleAssetBundle.AddBundles(builder, vecOffset);
            var offset = UnloadbleAssetBundle.EndUnloadbleAssetBundle(builder);
            UnloadbleAssetBundle.FinishUnloadbleAssetBundleBuffer(builder, offset);

            File.WriteAllBytes(_GetRelativeBundlePath(UnloadbleBundlesFb), builder.SizedByteArray());
            Debug.Log("unloadbleBundles count = " + unloadbleBundles.Count);
        }

#else

        public static void _LoadAssetMapFB(string path, out Dictionary<uint, string> assetHashMapAssetBundle,
            out Dictionary<string, string> assetNameMapAssetBundle,
           out int bundleCount)
        {
            byte[] content = VFileSystem.ReadAllBytes(path);
            ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(content);
            AssetMapBundleFB amb = AssetMapBundleFB.GetRootAsAssetMapBundleFB(buffer);
            assetHashMapAssetBundle = new Dictionary<uint, string>(amb.H2bLength);
            if (amb.P2bLength != 0)
            {
                assetNameMapAssetBundle = new Dictionary<string, string>(amb.P2bLength);
            }
            else
            {
                assetNameMapAssetBundle = null;
            }
            HashSet<string> bundleSet = new HashSet<string>();
            for (int i = 0; i < amb.H2bLength; ++i)
            {
                AssetHashMapBundleFB h2b = amb.H2b(i).Value;
                string tempBundleName = string.Intern(h2b.Bundle);
                assetHashMapAssetBundle[h2b.Hash] = tempBundleName;
                if (!bundleSet.Contains(tempBundleName))
                {
                    bundleSet.Add(tempBundleName);
                }
            }
            for (int i = 0; i < amb.P2bLength; ++i)
            {
                AssetPathMapBundleFB p2b = amb.P2b(i).Value;
                string tempBundleName = string.Intern(p2b.Bundle);
                assetNameMapAssetBundle[p2b.Path] = tempBundleName;
                if (!bundleSet.Contains(tempBundleName))
                {
                    bundleSet.Add(tempBundleName);
                }
            }
            bundleCount = bundleSet.Count;

            Debug.Log("hash2Bundle Count: " + assetHashMapAssetBundle.Count + " bundle Count: " + bundleCount);
        }

        public static void _LoadBundle2AtlasFB(string path, out Dictionary<string, string[]> bundle2AtlasMap)
        {
            byte[] content = VFileSystem.ReadAllBytes(path);
            ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(content);
            Bundle2AtlasListFB b2Alist = Bundle2AtlasListFB.GetRootAsBundle2AtlasListFB(buffer);
            bundle2AtlasMap = new Dictionary<string, string[]>(b2Alist.B2ALength);

            for (int i = 0; i < b2Alist.B2ALength; ++i)
            {
                Bundle2AtlasFB b2A = b2Alist.B2A(i).Value;
                string[] atlasList = new string[b2A.AtlasLength];
                for (int j = 0; j < b2A.AtlasLength; ++j)
                {
                    atlasList[j] = b2A.Atlas(j);
                }
                bundle2AtlasMap.Add(b2A.Bundle, atlasList);
            }
        }
#endif

        private static void _LoadManifest()
        {
            string mainBundlesName = "Windows";

            
            // modify by cmm
#if UNITY_IOS
            mainBundlesName = "iOS";
#elif UNITY_ANDROID
            mainBundlesName = "Android";
#else
            mainBundlesName = "Windows";
#endif

            string realPath = null;
            ulong offset = 0;
            if (!InnerPackage.TryGetFileEntry(GetAssetBundlePath(mainBundlesName), out realPath, out offset))
            {
                if (!VFileSystem.Exists(_GetRelativeBundlePath(mainBundlesName)))
                {
                    Debug.LogError($"Can't find main assetbundle.");
                }
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(realPath, 0, offset);
            if (bundle == null)
            {
                throw new Exception("Load main assetbundle failed.");
            }
            UnityEngine.Object asset = bundle.LoadAsset("assetbundlemanifest");
            UnityEngine.Object[] objects = bundle.LoadAllAssets();
            bundle.Unload(false);
#if SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
            if(asset is CompatibilityAssetBundleManifest)
            {
                _comptmanifest = (CompatibilityAssetBundleManifest)asset;
            }
            else
            {
                _manifest = (AssetBundleManifest)asset;
            }
#else
            _manifest = (AssetBundleManifest)asset;
#endif
        }

        public static void _LoadDependenciesForManifestFile(out Dictionary<string, List<string>> bundleDependenciesDictionary)
        {
            bundleDependenciesDictionary = new Dictionary<string, List<string>>();
            string manifestFilePath = _GetRelativeBundlePath("Android.manifest");
            using (StreamReader sr = new StreamReader(VFileSystem.OpenFile(manifestFilePath, FileMode.Open, FileAccess.Read)))
            {
                string line;
                string subLine = null;
                while ((line = sr.ReadLine()) != null && !line.Trim().StartsWith("Info_")) ;
                if (line != null)
                {
                    do
                    {
                        line = line.Trim();
                        if (line.StartsWith("Info_"))
                        {
                            var nameline = sr.ReadLine();
                            var bundleName = nameline.Split(':')[1].Trim();
                            bundleDependenciesDictionary[bundleName] = new List<string>();
                            sr.ReadLine();
                            while ((subLine = sr.ReadLine()) != null && subLine.Trim().StartsWith("Dependency_"))
                            {
                                string dep = subLine.Split(':')[1].Trim();
                                bundleDependenciesDictionary[bundleName].Add(dep);
                            }
                        }
                        line = subLine;
                    } while (subLine != null);
                }
            }
        }

        public static Dictionary<string, List<string>> LoadDependenciesForManifestFile(string filePath)
        {
            var bundleDependenciesDictionary = new Dictionary<string, List<string>>();
            using (StreamReader sr = new StreamReader(VFileSystem.OpenFile(filePath, FileMode.Open, FileAccess.Read)))
            {
                string line;
                string subLine = null;
                while ((line = sr.ReadLine()) != null && !line.Trim().StartsWith("Info_")) ;
                if (line != null)
                {
                    do
                    {
                        line = line.Trim();
                        if (line.StartsWith("Info_"))
                        {
                            var nameline = sr.ReadLine();
                            var bundleName = nameline.Split(':')[1].Trim();
                            bundleDependenciesDictionary[bundleName] = new List<string>();
                            sr.ReadLine();
                            while ((subLine = sr.ReadLine()) != null && subLine.Trim().StartsWith("Dependency_"))
                            {
                                string dep = subLine.Split(':')[1].Trim();
                                bundleDependenciesDictionary[bundleName].Add(dep);
                            }
                        }
                        line = subLine;
                    } while (subLine != null);
                }
            }

            return bundleDependenciesDictionary;
        }

        private static void _LoadUnloadbleBundles()
        {
            _unloadbleBundles = new HashSet<string>();
            try
            {
                var fbPath = _GetRelativeBundlePath(UnloadbleBundlesFb);
                if (VFileSystem.ExistsFile(fbPath))
                {
                    byte[] content = VFileSystem.ReadAllBytes(fbPath);

                    ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(content);
                    UnloadbleAssetBundle uab = UnloadbleAssetBundle.GetRootAsUnloadbleAssetBundle(buffer);
                    for (int i = 0; i < uab.BundlesLength; i++)
                    {
                        var bundle = uab.Bundles(i);
                        bundle = string.Intern(bundle);
                        _unloadbleBundles.Add(bundle);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.ToString());
                Debug.LogWarning("Unloadble bundle load failed");
            }
            Debug.Log("_LoadUnloadbleBundles count : " + _unloadbleBundles.Count);
        }

        /// <summary>
        /// 获取指定assetBundle 的 直接依赖的assetBundle的名字
        /// </summary>
        /// <param name="abName"> assetBundle名称 </param>
        /// <returns></returns>
        public static string[] GetDirectDependencies(string abName)
        {
            if (!_init)
            {
                Debug.LogError("Please init");
                return null;
            }
            string[] deps = null;
            if (!_bundleDepCache.TryGetValue(abName, out deps))
            {
#if SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
                if(_zeusManifest != null)
                {
                    deps = _zeusManifest.GetDirectDependencies(abName);
                }
                else if(_manifest != null)
                {
                    deps = _manifest.GetDirectDependencies(abName);
                }
                else
                {
                    deps = _comptmanifest.GetDirectDependencies(abName);
                }
#else
                if (_zeusManifest != null)
                {
                    deps = _zeusManifest.GetDirectDependencies(abName);
                }
                else if (_manifest != null)
                {
                    deps = _manifest.GetDirectDependencies(abName);
                }
                else
                {
                    deps = _bundle2Dependencies[abName].ToArray();
                }
#endif
                string[] atlasArray = null;
                if (_bundle2AtlasBundle.TryGetValue(abName, out atlasArray))
                {
                    string[] allDeps = new string[deps.Length + atlasArray.Length];
                    Array.Copy(deps, allDeps, deps.Length);
                    Array.Copy(atlasArray, 0, allDeps, deps.Length, atlasArray.Length);
                    deps = allDeps;
                }
                _bundleDepCache.Add(abName, deps);
            }
            return deps;
        }
#if UNITY_WEBGL
        public static Hash128 GetAssetBundleHash(string abName)
        {
            if (!_init)
            {
                Debug.LogError("Please init");
                return new Hash128();
            }
            if (_zeusManifest != null)
            {
                return _zeusManifest.GetAssetBundleHash(abName);
            }
            else if (_manifest != null)
            {
                return _manifest.GetAssetBundleHash(abName);
            }
            else
            {
                Debug.LogError("AssetsUtils Error: _manifest not found!");
                return new Hash128();
            }
        }
#endif

        private static void GetDependencis(string abName, ref HashSet<string> dependenciesSet)
        {
            var deps = GetDirectDependencies(abName);
            foreach (var dep in deps)
            {
                if (dependenciesSet.Contains(dep))
                {
                    continue;
                }
                dependenciesSet.Add(dep);
                GetDependencis(dep, ref dependenciesSet);
            }
        }

        /// <summary>
        /// 根据 给定打bundle前的路径,获取assetBundle名称,asset名称
        /// </summary>
        /// <param name="resPath"> 资源打bundle前的路径 </param>
        /// <param name="assetBundleName"> assetbundle路径 </param>
        /// <param name="assetName"> 资源路径 </param>
        /// <returns></returns>
        public static bool TryGetAssetBundleName(string resPath, out string assetBundleName, out string assetName)
        {

            if (resPath != null)
            {
                resPath = resPath.Replace("\\", "/");
            }

            string assetBundlePath;

#if UNITY_EDITOR
            if (_assetMapBundles == null || string.IsNullOrEmpty(resPath) || !_assetMapBundles.TryGetValue(resPath, out assetBundlePath))
            {
                assetBundleName = null;
                assetName = null;
                return false;
            }
#else
            if (string.IsNullOrEmpty(resPath) || _assetHashMapBundles == null)
            {
                assetBundleName = null;
                assetName = null;
                return false;
            }
            uint hashCode = GetHashCode(resPath);
            if (!_assetHashMapBundles.TryGetValue(hashCode, out assetBundlePath))
            {
                if (_assetPathMapBundles == null)
                {
                    assetBundleName = null;
                    assetName = null;
                    return false;
                }
                else if (!_assetPathMapBundles.TryGetValue(resPath, out assetBundlePath))
                {
                    assetBundleName = null;
                    assetName = null;
                    return false;
                }
            }
#endif
            // 在这里，由于资源只能指定打入一个bundle中去，所以这里的只对应一个assetbundle ( 被依赖带入进bundle,会在多个bundle中出现,但是这些资源不能指定加载的,所以不考虑 )
            assetBundleName = assetBundlePath;
#if SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
            if(_comptmanifest != null)
            {
                assetName = resPath;
            }
            else
            {
                int idx = resPath.LastIndexOf('/');
                assetName = resPath.Substring(idx + 1);
            }
#else
            //int idx = resPath.LastIndexOf('/');
            //assetName = resPath.Substring(idx + 1);
            assetName = resPath;
#endif
            return true;
        }

        public static bool ExistResPath(string resPath)
        {
            if (resPath != null)
            {
                resPath = resPath.Replace("\\", "/");
            }

            string assetBundlePath;
#if UNITY_EDITOR
            if (_assetMapBundles == null || string.IsNullOrEmpty(resPath) || !_assetMapBundles.TryGetValue(resPath, out assetBundlePath))
            {
                return false;
            }
#else
            if (string.IsNullOrEmpty(resPath) || _assetHashMapBundles == null)
            {
                return false;
            }
            uint hashCode = GetHashCode(resPath);
            if (!_assetHashMapBundles.ContainsKey(hashCode))
            {
                if (_assetPathMapBundles == null)
                {
                    return false;
                }
                else if (!_assetPathMapBundles.ContainsKey(resPath))
                {
                    return false;
                }
            }
#endif
            return true;
        }


        /// <summary>
        /// 获取 AssetBundle 读取的path.
        /// </summary>
        /// <param name="abName"> assetBundle名字 </param>
        /// <returns></returns>
        public static string GetAssetBundlePath(string abName)
        {
            string bundlePath = null;
            if (!_bundleName2PathCache.TryGetValue(abName, out bundlePath))
            {
                var virtualPath = _GetRelativeBundlePath(abName);
                bundlePath = VFileSystem.GetRealPath(virtualPath);
                _bundleName2PathCache.Add(abName, bundlePath);
            }
            return bundlePath;
        }

        /// <summary>
        /// 获取 AssetBundle 读取的path 并判断bundle是否存在（可能在二包里还没下载到）
        /// </summary>
        /// <param name="abName"> assetBundle名字 </param>
        /// <returns></returns>
        public static bool TryGetAssetBundlePath(string abName, out string abPath)
        {
            bool isBundleExist = false;
            abPath = GetAssetBundlePath(abName);
            if (InnerPackage.IsInnerPath(abPath) || File.Exists(abPath))
            {
                isBundleExist = true;
            }
            return isBundleExist;
        }

        /// <summary>
        /// 清理缓存重新初始化
        /// </summary>
        internal static void ReInit()
        {
            if (_init)
            {
#if UNITY_EDITOR
                _assetMapBundles.Clear();
                _bundleMapAsset.Clear();
#else
                _assetHashMapBundles.Clear();
                _assetPathMapBundles?.Clear();
#endif
                _bundle2AtlasBundle.Clear();
                _bundleName2PathCache.Clear();
                _bundleDepCache.Clear();
                _bundle2Dependencies?.Clear();
#if SUPPORT_SBP_COMPATIBILITY_AB_MANIFEST
                _comptmanifest = null;
#endif
                _unloadbleBundles.Clear();
                _manifest = null;
                _zeusManifest = null;
                _init = false;
            }
            Init();
        }

        internal static bool IsUnloadbleBundle(string bundleName)
        {
            if (_unloadbleBundles == null)
            {
                _LoadUnloadbleBundles();
            }

            return _unloadbleBundles.Contains(bundleName);
        }

        public static string GetAssetBundleOuterPackagePath(string abName)
        {
            string relativePath = _GetRelativeBundlePath(abName);
            return OuterPackage.GetRealPath(relativePath);
        }

        // modify by cmm
        public static string _GetBundleRootPath()
        {
#if UNITY_ANDROID
            return "Bundles/Android/";
#elif UNITY_IOS
            return "Bundles/iOS/";
#else
            return "Bundles/Windows/";
#endif
        }

        private static string _GetRelativeBundlePath(string abName)
        {
            string bundlesPath = _GetBundleRootPath();
#if UNITY_EDITOR
            return bundlesPath + abName;
#else
            bool useBundleScatter = AssetManager.AssetSetting.bundleLoaderSetting.useBundleScatter;
            if (!string.IsNullOrEmpty(abName) && useBundleScatter)
            {
                string sub = GetBundleScatterFolder(abName);
                return string.Format("{0}{1}/{2}", bundlesPath, sub, abName);
            }
            else
            {
                return bundlesPath + abName;
            }
#endif
        }

        public static string GetBundleScatterFolder(string abName)
        {
            int hashCode = (int)GetHashCode(abName);
            hashCode = hashCode >= 0 ? hashCode : -hashCode;
            return (hashCode % SCATTER_MODULUS).ToString("D3");
        }

        public static uint GetHashCode(string content)
        {
            uint seed = 131; // 31 131 1313 13131 131313 etc..
            uint hash = 0;

            for (int i = 0; i < content.Length; i++)
            {
                hash = hash * seed + content[i];
            }
            return (hash & 0x7FFFFFFF);
        }

        public static string[] GetAllDependencies(string abName)
        {
            if (!_init)
            {
                Debug.LogError("Please init");
                return null;
            }
            HashSet<string> dependenciesSet = new HashSet<string>();
            dependenciesSet.Add(abName);
            GetDependencis(abName, ref dependenciesSet);
            dependenciesSet.Remove(abName);
            string[] deps = new string[dependenciesSet.Count];
            dependenciesSet.CopyTo(deps);
            return deps;
        }

        public static string RemoveSceneFileExtension(string sceneName)
        {
            if (sceneName.EndsWith(".unity"))
            {
                sceneName = sceneName.Substring(0, sceneName.LastIndexOf(".unity"));
            }
            return sceneName;
        }


        public static List<string> GetAllAssetBundles()
        {
            if (!_init)
            {
                Debug.LogError("Please init");
                return null;
            }
            else
            {
                if (_zeusManifest != null)
                {
                    return new List<string>(_zeusManifest.GetAllAssetBundles());
                }
                else if (_manifest != null)
                {
                    return new List<string>(_manifest.GetAllAssetBundles());
                }
                else
                {
                    List<string> assetBundleList = new List<string>();
                    foreach (var tmp in _bundle2Dependencies.Keys)
                    {
                        assetBundleList.Add(tmp);
                    }
                    return assetBundleList;
                }
            }
        }

#if UNITY_EDITOR
        public static Dictionary<string, string> GetAssetMapBundles()
        {
            return _assetMapBundles;
        }

        public static Dictionary<string, List<string>> GetBundleMapAssets()
        {
            return _bundleMapAsset;
        }
#endif

        public static bool ContainAsset(string assetPath)
        {
            return _bundleMapAsset.ContainsKey(assetPath);
        }
    }
}
