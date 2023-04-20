using System.Collections.Generic;
using System.IO;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;

namespace XEditor
{

    /// <summary>
    /// 批量导出曲线
    /// </summary>
    public class BatchExportCurve
    {
        //static ColliderTable _collider = new ColliderTable();

        //public static ColliderTable Collider
        //{
        //    get
        //    {
        //        XTableReader.ReadFile(@"Table/ColliderTable", _collider);
        //        return _collider;
        //    }
        //}

        public static void OnBatchOver()
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("TIP", XEditorUtil.Config.tip, "ok");
            AssetDatabase.Refresh();
            HelperEditor.Open(Application.dataPath + "/Editor/EditorResources/Server/Animation");
        }


        
        /// <summary>
        /// 导出所有的受击动作
        /// </summary>
        //[MenuItem(@"Tools/Asset/BatchExportAnimClip", priority = 2)]
        //public static void BatchColliders()
        //{
        //    if (XEditorUtil.MakeNewScene())
        //    {
        //        try
        //        {
        //            var table = Collider.Table;
        //            for (int i = 0; i < table.Length; i++)
        //            {
        //                BatchCollider(table[i]);
        //            }
        //        }
        //        catch (System.Exception e)
        //        {
        //            EditorUtility.DisplayDialog("error", e.Message, "ok");
        //            Debug.LogError(e.Message);
        //            Debug.LogError(e.StackTrace);
        //        }
        //        finally
        //        {
        //            OnBatchOver();
        //        }
        //    }
        //}


        //private static void BatchCollider(ColliderTable.RowData row)
        //{
        //    var preRow = XAnimationLibrary.FindByColliderID((int)row.ColliderID);
        //    if (preRow == null)
        //    {
        //        EditorUtility.DisplayDialog("tip", "not find colliderid in table present: " + row.ColliderID, "ok");
        //        return;
        //    }
        //    uint presentid = preRow.PresentID;
        //    string bone = row.BindBone;
        //    if (!string.IsNullOrEmpty(bone) && row.ColliderType == 0)
        //    {
        //        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        //        XEditorUtil.ClearCreatures();
        //        XEntityPresentation.RowData data = XAnimationLibrary.AssociatedAnimations(presentid);
        //        string clipFolder = Application.dataPath + "/BundleRes/Animation/" + data.AnimLocation;
        //        GameObject prefab = XAnimationLibrary.GetDummy(presentid);
        //        GameObject go = GameObject.Instantiate(prefab);
        //        go.name = prefab.name;
        //        go.transform.position = Vector3.zero;
        //        go.transform.rotation = Quaternion.identity;
        //        go.transform.localScale = Vector3.one * data.Scale;
        //        XDestructionLibrary.AttachDress(presentid, go);
        //        DirectoryInfo dir = new DirectoryInfo(clipFolder);
        //        FileInfo[] files = dir.GetFiles("*.anim");
        //        for (int i = 0; i < files.Length; i++)
        //        {
        //            var file = files[i];
        //            if (EditorUtility.DisplayCancelableProgressBar("export with collidertable id: " + row.ColliderID, file.Name, i / (float)files.Length))
        //            {
        //                break;
        //            }
        //            string fileName = file.FullName.Replace("\\", "/");
        //            int index = fileName.IndexOf("Assets/");
        //            fileName = fileName.Substring(index);
        //            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fileName);
        //            AvatarCurveGenerator.GenerateCurve(clip, row, bone, go);
        //        }
        //    }
        //}
    }
}