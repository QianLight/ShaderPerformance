// #if UNITY_EDITOR
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace CFEngine
// {

//     [Serializable]
//     public class EditorShaderParamTemplate : ShaderParamTemplate, IFolderHash
//     {
//         [NonSerialized]
//         public float height = 0;
//         public string name;
//         public EShaderKeyID shaderKey = EShaderKeyID._Color;
//         public string customKey = "";

//         public Vector2 xRange = new Vector2();
//         public Vector2 yRange = new Vector2();
//         public Vector2 zRange = new Vector2();
//         public Vector2 wRange = new Vector2();

//         public string hash = "";
//         public string GetHash ()
//         {
//             if (string.IsNullOrEmpty (hash))
//             {
//                 hash = FolderConfig.Hash ();
//             }
//             return hash;
//         }
//     }

//     [Serializable]
//     public class ShaderTemplateList : BaseAssetConfig
//     {
//         public List<EditorShaderParamTemplate> templateList = new List<EditorShaderParamTemplate> ();

//         public override IList GetList () { return templateList; }

//         public override Type GetListType () { return typeof (List<EditorShaderParamTemplate>); }

//         public override void OnAdd () { templateList.Add (new EditorShaderParamTemplate ()); }

//         public override float GetHeight (int index) { return templateList[index].height; }

//         public override void SetHeight (int index, float height) { templateList[index].height = height; }

//     }
//     // public class MatEffectConfig : AssetBaseConifg<MatEffectConfig>
//     // {
//     //     public ShaderTemplateList matTemplate = new ShaderTemplateList ();
//     // }
// }
// #endif