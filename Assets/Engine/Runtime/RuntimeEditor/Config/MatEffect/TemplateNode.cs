// #if UNITY_EDITOR
// using System;
// using UnityEngine;

// namespace CFEngine
// {
//     [Node (module = "MatEffect")]
//     [Serializable]
//     public class EffectTemplate : AbstractNode
//     {
//         [Input][Output] public int groupID;
//         [Editable] public string title;
//         [Editable] public string key;
//         [Editable] public Vector4 defaultValue;
//         [Editable] public MatEffectType effectType;
//         [Editable] public LerpType lerpType = LerpType.None;
//         public ShaderParamTemplate template = new ShaderParamTemplate ();
//         public override object OnRequestValue (Port port)
//         {
//             return null;
//         }

//     }
// }
// #endif