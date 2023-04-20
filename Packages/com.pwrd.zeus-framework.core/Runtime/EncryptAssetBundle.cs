using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Encrypt
{
    public static class EncryptAssetBundle
    {
        public static bool BundleEncrypt = false;
        public static bool Mask_Asset_Bundle = false;
        public static int EncrtptOffset
        {
            //8个空byte
            get
            {
                return 8;
            }
        }

        public static void AssetBundleEncrypt(int buildTarget)
        {
            Debug.Log("AssetBundleEncrypt");
            if (!BundleEncrypt)
            {
                return;
            }
            Debug.Log("StartEncryptBundle");
            string filepath = string.Empty;
            switch (buildTarget)
            {
                case 9:
                    filepath = Application.dataPath.Replace("Assets", "") + "Bundles/iOS";
                    break;
                case 13:
                    filepath = Application.dataPath.Replace("Assets", "") + "Bundles/Android";
                    break;
                default:
                    break;
            }

            Directory.GetFiles(filepath).Select(p => new FileInfo(p)).AsParallel().ForAll(p =>
            {

                if (p.FullName.EndsWith(".ab") || p.FullName.StartsWith("shared_"))
                {
                    byte[] oldData = File.ReadAllBytes(p.FullName);
                    int newOldLen = EncrtptOffset + oldData.Length;
                    var newData = new byte[newOldLen];
                    for (int tb = 0; tb < oldData.Length; tb++)
                    {
                        newData[EncrtptOffset + tb] = oldData[tb];
                    }
                    FileStream fs = File.OpenWrite(p.FullName);//打开写入进去
                    fs.Write(newData, 0, newOldLen);
                    fs.Close();
                }

            });

            Debug.Log("EndEncryptBundle");
        }

        public static void SetMaskAssetBundle(bool enable)
        {
            if (Mask_Asset_Bundle)
            {
                //Application.maskAssetBundle = enable;
                Debug.Log("SetMaskAssetBundle:" + enable);

                Type t = typeof(Application);
                PropertyInfo maskAssetBundle = t.GetProperty("maskAssetBundle");
                Debug.Log("Application.maskAssetBundle:" + maskAssetBundle.GetValue(maskAssetBundle));
                maskAssetBundle.SetValue(maskAssetBundle, enable);
                Debug.Log("Application.maskAssetBundle:" + maskAssetBundle.GetValue(maskAssetBundle));
            }

        }

        public static AssetBundle LoadABOffset(string path, uint crc = 0u, ulong offset = 0uL)
        {
#if ENCRYPT_ASSET_BUNDLE
            ulong encryptOffset = (ulong)EncrtptOffset;
            return AssetBundle.LoadFromFile(path, crc, encryptOffset + offset);
#else
            return AssetBundle.LoadFromFile(path, crc, offset);
#endif
        }

        public static AssetBundleCreateRequest LoadABOffsetAsync(string path, uint crc = 0u, ulong offset = 0uL)
        {
#if ENCRYPT_ASSET_BUNDLE
            ulong encryptOffset = (ulong)EncrtptOffset;
            return AssetBundle.LoadFromFileAsync(path, crc, encryptOffset + offset);
#else
            return AssetBundle.LoadFromFileAsync(path, crc, offset);
#endif
        }
    }
}