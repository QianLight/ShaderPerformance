using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Material", "包含无用纹理采样的材质", "t:material", "")]
    public class MaterialPropertiesCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                return true;

            bool bPass = CheckMatProperties(material);
            if(!bPass)
            {
                output = "包含无用的纹理采样";
            }
            return bPass;
        }

        bool CheckMatProperties(Material material)
        {
            Material mat = material;

            if (mat)
            {
                SerializedObject psSource = new SerializedObject(mat);
                SerializedProperty emissionProperty = psSource.FindProperty("m_SavedProperties");
                SerializedProperty texEnvs = emissionProperty.FindPropertyRelative("m_TexEnvs");
                //SerializedProperty floats = emissionProperty.FindPropertyRelative("m_Floats");
                //SerializedProperty colos = emissionProperty.FindPropertyRelative("m_Colors");

                bool check = CheckMaterialSerializedProperty(texEnvs, mat);
                //CleanMaterialSerializedProperty(floats, mat);
                //CleanMaterialSerializedProperty(colos, mat);

                if (check)
                    return false;

                psSource.ApplyModifiedProperties();
            }
            return true;
        }

        /// <summary>
        /// true: has useless propeties
        /// </summary>
        /// <param name="property"></param>
        /// <param name="mat"></param>
        private bool CheckMaterialSerializedProperty(SerializedProperty property, Material mat)
        {
            for (int j = property.arraySize - 1; j >= 0; j--)
            {
                string propertyName = property.GetArrayElementAtIndex(j).displayName;
                //string propertyName = property.GetArrayElementAtIndex(j).FindPropertyRelative("first").FindPropertyRelative("name").stringValue;
                Debug.Log("Find property in serialized object : " + propertyName);
                if (!mat.HasProperty(propertyName))
                {
                    if (propertyName.Equals("_MainTex"))
                    {
                        //_MainTex是内建属性，是置空不删除，否则UITexture等控件在获取mat.maintexture的时候会报错
                        if (property.GetArrayElementAtIndex(j).FindPropertyRelative("second").FindPropertyRelative("m_Texture").objectReferenceValue != null)
                        {
                            property.GetArrayElementAtIndex(j).FindPropertyRelative("second").FindPropertyRelative("m_Texture").objectReferenceValue = null;
                            Debug.Log("Set _MainTex is null");
                        }
                    }
                    else
                    {
                        return true;
                        //property.DeleteArrayElementAtIndex(j);
                        //Debug.Log("Delete property in serialized object : " + propertyName);
                    }
                }
            }
            return false;
        }
    }
}


