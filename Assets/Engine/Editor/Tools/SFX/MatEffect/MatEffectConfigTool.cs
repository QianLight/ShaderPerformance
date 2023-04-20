// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEditor;
// using UnityEngine;

// using ShaderTemplateEdior = CFEngine.Editor.CommonListEditor<CFEngine.ShaderTemplateList>;
// using ShaderTemplateContext = CFEngine.Editor.AssetListContext<CFEngine.ShaderTemplateList>;

// namespace CFEngine.Editor
// {

//     public partial class MatEffectConfigTool : BaseConfigTool<MatEffectConfig>
//     {
//         private Vector2 templateScroll = Vector2.zero;
//         private ShaderTemplateContext matTemplateContext;
//         private ShaderTemplateEdior matTemplateEditor;
//         public override void OnInit ()
//         {
//             base.OnInit ();
//             config = MatEffectConfig.instance;
//             RenderEffectSystem.Init (EngineContext.instance);

//             matTemplateContext.name = "MatTemplate";
//             // matTemplateContext.headGUI = EffectTemplateHeadGUI;
//             matTemplateContext.elementGUI = EffectTemplateConfigGUI;
//             matTemplateContext.needDelete = true;
//             matTemplateContext.needAdd = true;

//             config.matTemplate.name = "MatTemplate";

//             matTemplateEditor = new ShaderTemplateEdior (config.matTemplate, ref matTemplateContext);

//             // effectPartContext.elementGUI = EffectPartGUI;
//             // effectPartContext.needDelete = true;
//             // effectPartContext.needAdd = true;

//             // config.effectPart.name = "EffectPart";
//             // config.effectPart.path = "";
//             // effectPartEditor = new EffectPartListEdior (config.effectPart, ref effectPartContext);
//         }

//         public static int OnRenderEffectGUI (EffectPreviewContext context)
//         {
//             return context.ep.OnEffectGUI (ref context.ere.effect, context);
//         }

//         #region mattemplate
//         private void EffectTemplateConfigGUI (ref ListElementContext lec, ref ShaderTemplateContext context, ShaderTemplateList data, int i)
//         {
//             var template = data.templateList[i];
//             ToolsUtility.InitListContext (ref lec, context.defaultHeight);
//             //head line
//             string label = string.Format ("{0}.{1}", i.ToString (), string.IsNullOrEmpty (template.name) ? "empty" : template.name);

//             ToolsUtility.Label (ref lec, label, 160, true);
//             string folderPath = template.GetHash ();
//             bool templateFolder = ToolsUtility.SHButton (ref lec, config.folder, folderPath);
//             if (templateFolder)
//             {
//                 ToolsUtility.NewLineWithOffset (ref lec);
//                 ToolsUtility.TextField (ref lec, "Name", 80, ref template.name, 160, true);

//                 ToolsUtility.NewLineWithOffset (ref lec);

//                 MatEffectType effectType = (MatEffectType) template.effectType;
//                 ToolsUtility.EnumPopup (ref lec, "EffectType", 60, ref effectType, 100, true);
//                 template.effectType = (int) (MatEffectType) effectType;

//                 ToolsUtility.EnumPopup (ref lec, "", 0, ref template.shaderKey, 120);
//                 if (template.shaderKey == EShaderKeyID.Num)
//                 {
//                     ToolsUtility.TextField (ref lec, "Custom", 80, ref template.customKey, 100);
//                 }

//             }

//         }
//         // private void EffectTemplateHeadGUI (ref Rect rect, ref ShaderTemplateListContext context, EditorShaderParamTemplate data)
//         // {
//         //     rect.width = 80;
//         //     if (GUI.Button (rect, "AddTemplate"))
//         //     {
//         //     }
//         // }
//         #endregion
//         // private void MatTemplateGui (ref Rect rect)
//         // {
//         //     if (config.folder.FolderGroup ("MatTemplate", "MatTemplate", rect.width - 10))
//         //     {
//         //         if (GUILayout.Button ("AddTemplate", GUILayout.MaxWidth (160)))
//         //         {
//         //             config.matTemplate.Add (new EditorShaderParamTemplate ());
//         //         }
//         //         // EditorCommon.BeginScroll(ref templateScroll,)

//         //         EditorCommon.EndFolderGroup ();
//         //     }
//         // }
//         protected override void OnConfigGui (ref Rect rect)
//         {
//             matTemplateEditor.Draw (config.folder, ref rect);
//             // MatTemplateGui (ref rect);
//             // TestGameobjectGUI ();
//             // RenderEffectGUI (ref rect);
//             // EffectPartGUI ();
//         }

//         protected override void OnSave ()
//         {
//             // try
//             // {
//             //     string path = string.Format ("{0}Config/EffectData.bytes", LoadMgr.singleton.EngineResPath);
//             //     using (FileStream fs = new FileStream (path, FileMode.Create))
//             //     {
//             //         List<RenderEffect> effects = new List<RenderEffect> ();
//             //         int[] effectOffset = new int[EffectData.EffectNum * 2];
//             //         for (int i = 0; i < config.effectGroup.Length; ++i)
//             //         {
//             //             var group = config.effectGroup[i];
//             //             effectOffset[i * 2] = effects.Count;
//             //             if (group != null)
//             //             {
//             //                 for (int j = 0; j < group.effectList.Count; ++j)
//             //                 {
//             //                     var ere = group.effectList[j];
//             //                     effects.Add (ere.effect);

//             //                 }
//             //                 effectOffset[i * 2 + 1] = group.effectList.Count;
//             //             }
//             //         }

//             //         BinaryWriter bw = new BinaryWriter (fs);
//             //         bw.Write ((short) effects.Count);
//             //         for (int i = 0; i < effects.Count; ++i)
//             //         {
//             //             var re = effects[i];
//             //             EditorCommon.WriteVector (bw, re.data);
//             //             EditorCommon.WriteVector (bw, re.data1);
//             //             EditorCommon.WriteVector (bw, re.timeData);
//             //             bw.Write (re.priority);
//             //             bw.Write (re.partFlag);
//             //         }
//             //         for (int i = 0; i < effectOffset.Length; ++i)
//             //         {
//             //             bw.Write ((short) effectOffset[i]);
//             //         }
//             //     }
//             // }
//             // catch (Exception ex)
//             // {
//             //     DebugLog.AddErrorLog (ex.StackTrace);
//             // }

//         }
//         protected override void OnConfigUpdate ()
//         {
//             // RenderEffectSystem.PostUpdate (Time.deltaTime);
//         }
//     }
// }