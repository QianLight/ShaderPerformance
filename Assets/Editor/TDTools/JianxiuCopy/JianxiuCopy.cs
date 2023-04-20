using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using CFEngine;
using System.IO;
using Cinemachine;
using System;

namespace TDTools {
    public class JianxiuCopy : EditorWindow
    {
        [MenuItem("Tools/TDTools/监修相关工具/移除晃动 (Assets-Jianxiu-BundleRes中的)")]
        public static void RemoveAllBlur() { 
            RemoveBlurAt("", "");
        }


        public static void RemoveBlurAt(string path, string upperpath) {
            //Debug.Log(path);
            DirectoryInfo d = new DirectoryInfo(Application.dataPath + "/Jianxiu/BundleRes/SpecialAction/RadialBlurV2/Data" + upperpath + "/" + path);
            var ds = d.GetDirectories();
            foreach (var dd in ds)
                RemoveBlurAt(dd.Name, upperpath + "/" + path);
            var df = d.GetFiles();
            foreach (var ff in df) {
                //Debug.Log("Assets/BundleRes/SpecialAction/RadialBlurV2/Data" + upperpath + path + "/" + ff.Name);
                var blurs = AssetDatabase.LoadAssetAtPath<RadialBlurDataV2>("Assets/Jianxiu/BundleRes/SpecialAction/RadialBlurV2/Data" + upperpath + path + "/" + ff.Name);
                if (blurs != null)
                {
                    blurs.rangeScale = 0;
                    EditorUtility.SetDirty(blurs);
                    //Debug.Log(blurs.name);
                }
            }
        }

        [MenuItem("Tools/TDTools/监修相关工具/移除模糊 (Assets-Jianxiu-BundleRes中的)")]
        public static void RemoveAllShake() {
            RemoveShakeAt("","");
        }

        public static void RemoveShakeAt(string path, string upperpath) {
            DirectoryInfo d = new DirectoryInfo(Application.dataPath + "/Jianxiu/BundleRes/Prefabs/Cinemachine/Shack" + upperpath + "/" + path);
            var ds = d.GetDirectories();
            foreach (var dd in ds)
                RemoveBlurAt(dd.Name, upperpath + "/" + path);
            var df = d.GetFiles();
            foreach (var ff in df)
            {
                //Debug.Log("Assets/BundleRes/SpecialAction/RadialBlurV2/Data" + upperpath + path + "/" + ff.Name);
                var shake = AssetDatabase.LoadAssetAtPath<NoiseSettings>("Assets/Jianxiu/BundleRes/Prefabs/Cinemachine/Shack" + upperpath + path + "/" + ff.Name);
                if (shake != null)
                {
                    for (int i = 0; i < shake.PositionNoise.Length; i++)
                    {
                        shake.PositionNoise[i].X.Amplitude = 0;
                        shake.PositionNoise[i].Y.Amplitude = 0;
                        shake.PositionNoise[i].Z.Amplitude = 0;
                    }
                    for (int i = 0; i < shake.OrientationNoise.Length; i++)
                    {
                        shake.OrientationNoise[i].X.Amplitude = 0;
                        shake.OrientationNoise[i].Y.Amplitude = 0;
                        shake.OrientationNoise[i].Z.Amplitude = 0;
                    }
                    EditorUtility.SetDirty(shake);
                    //Debug.Log(shake.name);
                }
            }
        }


        [MenuItem("Tools/TDTools/监修相关工具/从Assets-BundleRes拷贝到Assets-Jianxiu-BundleRes中")]
        public static void CopyToJianxiu() {
            Copy(Application.dataPath + "/BundleRes/Prefabs/Cinemachine/Shack",
                Application.dataPath + "/Jianxiu/BundleRes/Prefabs/Cinemachine/Shack");

            Copy(Application.dataPath + "/BundleRes/SpecialAction/RadialBlurV2/Data",
                Application.dataPath + "/Jianxiu/BundleRes/SpecialAction/RadialBlurV2/Data");
        }


        [MenuItem("Tools/TDTools/监修相关工具/从Assets-Jianxiu-BundleRes拷贝的Assets-BundleRes中")]
        public static void CopyFromJianxiu() {
            CopyR(Application.dataPath + "/BundleRes/Prefabs/Cinemachine/Shack",
                Application.dataPath + "/Jianxiu/BundleRes/Prefabs/Cinemachine/Shack");

            CopyR(Application.dataPath + "/BundleRes/SpecialAction/RadialBlurV2/Data",
                Application.dataPath + "/Jianxiu/BundleRes/SpecialAction/RadialBlurV2/Data");
        }

        public static void CopyR(string to, string from) {
            Copy(from, to);
        }

        public static void Copy(string from, string to) {
            DirectoryInfo d = new DirectoryInfo(from);


            var df = d.GetFiles();
            try
            {

                foreach (var ff in df)
                {
                    //Debug.Log("From: " +  from + "/" + ff.Name);
                    //Debug.Log("To: " + to + "/" + ff.Name);
                    if (!Directory.Exists(to))
                        Directory.CreateDirectory(to);
                    File.Copy(from + "/" + ff.Name, to + "/" + ff.Name, true);
                }
            } catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            var ds = d.GetDirectories();
            foreach (var dd in ds)
                Copy(from + "/" + dd.Name, to + "/" + dd.Name);
        }

    }
}