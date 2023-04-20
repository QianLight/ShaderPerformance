using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace CFEngine.Editor
{
    // [CustomEditor (typeof (TextureImporter))]
    // [CanEditMultipleObjects]
    // internal class CustomTextureImporterInspector : AssetImporterEditor
    // {
    //     private static CustomAssetInspector inspectorInstance;
    //     public override bool showImportedObject { get { return false; } }

    //     private TexCompressConfig config;
    //     private string info;
    //     private TextureImporter texImport;
    //     protected override bool useAssetDrawPreview { get { return false; } }
    //     protected override void Awake ()
    //     {
    //         if (inspectorInstance == null)
    //         {
    //             inspectorInstance = new CustomAssetInspector ("TextureImporterInspector");
    //         }
    //         inspectorInstance.Awake ();
    //     }
    //     public override void OnEnable ()
    //     {
    //         if (inspectorInstance != null && targets != null && target != null)
    //         {
    //             inspectorInstance.Init (targets);
    //             inspectorInstance.OnEnable ();
    //             if (targets.Length == 1)
    //             {
    //                 string path = AssetDatabase.GetAssetPath (target);
    //                 config = TextureAssets.GetTexureType (path);
    //                 if (config != null)
    //                 {
    //                     texImport = target as TextureImporter;
    //                     bool hasAlpha = texImport != null ? texImport.DoesSourceTextureHaveAlpha () : false;
    //                     info = string.Format ("Type:{0} AlphaChannel:{1}", config.ToString (), hasAlpha);
    //                 }
    //             }
    //         }
    //     }
    //     public override void OnInspectorGUI ()
    //     {
    //         if (targets != null)
    //         {
    //             if (inspectorInstance != null)
    //                 inspectorInstance.OnInspectorGUI ();
    //             if (info != null)
    //             {
    //                 EditorGUILayout.LabelField (info);
    //                 if (texImport != null)
    //                 {
    //                     EditorGUILayout.BeginHorizontal ();

    //                     uint flag = 0;
    //                     if (!string.IsNullOrEmpty (texImport.userData))
    //                     {
    //                         uint.TryParse (texImport.userData, out flag);
    //                     }
    //                     bool ignoreImport = EditorCommon.HasFlag (flag, TexFlag.IgnoreImport);
    //                     bool isIgnoreImport = EditorGUILayout.Toggle ("IgnoreImport", ignoreImport);
    //                     if (isIgnoreImport != ignoreImport)
    //                     {
    //                         EditorCommon.SetFlag (ref flag, TexFlag.IgnoreImport, isIgnoreImport);
    //                         texImport.userData = flag.ToString ();
    //                     }
    //                     EditorGUILayout.EndHorizontal ();

    //                 }
    //             }
    //         }

    //     }

    //     public override bool HasModified ()
    //     {
    //         if (inspectorInstance != null)
    //             return inspectorInstance.HasModified ();
    //         return false;
    //     }
    //     protected override void ResetValues ()
    //     {
    //         if (inspectorInstance != null)
    //             inspectorInstance.ResetValues ();
    //     }
    //     protected override void Apply ()
    //     {
    //         if (inspectorInstance != null)
    //             inspectorInstance.Apply ();
    //     }
        
    //     public override void ReloadPreviewInstances()
    //     {
    //         // preview.ReloadPreviewInstances();
    //     }
    // }
}