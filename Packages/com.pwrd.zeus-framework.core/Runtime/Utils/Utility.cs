/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Zeus.Core
{
    public class Utility
    {
        public static Transform GetTransformInChildren(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }
            if (parent.name == childName)
            {
                return parent;
            }
            if (parent.childCount == 0)
            {
                return null;
            }
            Transform result = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                result = GetTransformInChildren(parent.GetChild(i), childName);
                if (result != null)
                {
                    return result;
                }
            }
            return result;
        }
        public static SkinnedMeshRenderer AddMaterialsToEnd(SkinnedMeshRenderer smr, Material[] materials)
        {
            List<Material> list = new List<Material>();
            list.AddRange(smr.materials);
            list.AddRange(materials);
            smr.materials = list.ToArray();
            return smr;
        }
        public static SkinnedMeshRenderer RemoveMaterialsInEnd(SkinnedMeshRenderer smr, int removeLength)
        {
            int matLength = smr.materials.Length;

            if (matLength >= removeLength)
            {
                List<Material> list = new List<Material>();
                list.AddRange(smr.materials);
                for (int i = 0; i < removeLength; i++)
                {
                    list.RemoveAt(list.Count - 1);
                }
                smr.materials = list.ToArray();
            }
            else
            {
                Debug.LogErrorFormat("removeLength:{0} out of the number of smr materials:{1}", removeLength, matLength);
            }
            return smr;
        }
        public static Vector3[] GetWorldCorners(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return null;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return corners;
        }

        /// <summary>
        /// 删除物体上的一个材质
        /// 已知引用的材质
        /// </summary>
        public static void RemoveMaterial(Renderer renderer, Material material)
        {
            List<Material> materialsList = new List<Material>();
            materialsList.AddRange(renderer.sharedMaterials);

            for (int i = materialsList.Count - 1; i >= 0; --i)
            {
                if (materialsList[i] == material)
                {
                    materialsList.RemoveAt(i);
                    renderer.sharedMaterials = materialsList.ToArray();
                    return;
                }
            }
        }

        public static void RemoveMaterials(Renderer renderer, Material[] materials)
        {
            for (int i = 0; i < materials.Length; ++i)
                RemoveMaterial(renderer, materials[i]);
        }

        public readonly static int UI_LAYER = 1 << 5;
        public readonly static int UI_SCENE = 1 << 16;
        public readonly static int EVERYTHING_LAYER = -1;
        public static int GetAllLayerExcludeUI()
        {
            return EVERYTHING_LAYER & (~(UI_LAYER | UI_SCENE));
        }

        public static bool IsGameObjectNull(GameObject go)
        {
            if (go == null)
            {
                return true;
            }
            return false;
        }
        public static bool IsTransformNull(Transform tr)
        {
            if (tr == null)
            {
                return true;
            }
            return false;
        }
        public static bool IsObjectNull(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return true;
            }
            return false;
        }

        public static int EnumToNum(System.ValueType value)
        {
            return System.Convert.ToInt32(value);
        }
        public static string Object2String(System.Object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return obj.ToString();
        }

        /// <summary>
        /// 给材质设置一个唯一名字
        /// </summary>
        public static string SetMaterialUniqueName(Material material)
        {
            string id = Time.realtimeSinceStartup.ToString();

            material.name = string.Concat(material.name, id);

            return id;
        }

        public static Material GetMaterialByName(Renderer renderer, string name)
        {
            foreach (var one in renderer.materials)
            {
                if (one.name.Contains(name))
                    return one;
            }

            return null;
        }

        public static bool Regex_IsMatch(string input, string pattern)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
        }

        public static String FileReadAllText(string fileName)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                return "";
            }
        }

        public static void FileWriteAllText(string fileName, string content)
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), content);
        }

        public static void ColorHx16ToRGBA(string colorHx16toRGB, out int r, out int g, out int b, out int a)
        {
            r = Convert.ToInt32("0x" + colorHx16toRGB.Substring(0, 2), 16);

            g = Convert.ToInt32("0x" + colorHx16toRGB.Substring(2, 2), 16);

            b = Convert.ToInt32("0x" + colorHx16toRGB.Substring(4, 2), 16);
            a = 255;
            if (colorHx16toRGB.Length > 6)
            {
                a = Convert.ToInt32("0x" + colorHx16toRGB.Substring(6, 2), 16);
            }
        }

        public static string RGBAToColorHx16(int r, int g, int b, int a)
        {
            if (r < 0 || g < 0 || b < 0 || a < 0 || r > 255 || g > 255 || b > 255 || a > 255)
            {
                Debug.LogErrorFormat("Only accept numbers between 0 - 255, r:{0},g:{1},b:{2},{3}", r, g, b, a);
                return "";
            }

            string R = Convert.ToString(r, 16);
            if (R == "0")
                R = "00";
            string G = Convert.ToString(g, 16);
            if (G == "0")
                G = "00";
            string B = Convert.ToString(b, 16);
            if (B == "0")
                B = "00";
            string A = Convert.ToString(a, 16);
            if (A == "0")
                A = "00";
            return R + G + B + A;
        }
    }
}
