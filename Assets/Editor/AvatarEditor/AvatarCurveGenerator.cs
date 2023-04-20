using System.IO;
using System.Text;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    public class AvatarCurveGenerator
    {

        /// <summary>
        /// 匹量处理接口
        /// </summary>
        public static void GenerateSkillCurve(AnimationClip clip, uint colliderid)
        {
            //System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            //var table = BatchExportCurve.Collider;
            //foreach (var row in table.Table)
            //{
            //    if (row.ColliderID == colliderid)
            //    {
            //        var preRow = XAnimationLibrary.FindByColliderID((int)colliderid);
            //        if(preRow==null)
            //        {
            //            EditorUtility.DisplayDialog("tip", "not find config in table present with  colliderid: " + colliderid, "ok");
            //            continue;
            //        }
            //        uint presentid = preRow.PresentID;
            //        string bone = row.BindBone;
            //        if (!string.IsNullOrEmpty(bone) && row.ColliderType == 1)
            //        {
            //            XEditorUtil.ClearCreatures();
            //            GameObject prefab = XAnimationLibrary.GetDummy(presentid);
            //            GameObject go = GameObject.Instantiate(prefab);
            //            go.name = prefab.name;
            //            go.transform.position = Vector3.zero;
            //            go.transform.rotation = Quaternion.identity;
            //            go.transform.localScale = Vector3.one * preRow.Scale;
            //            XDestructionLibrary.AttachDress(presentid, go);
            //            AvatarCurveGenerator.GenerateCurve(clip, row, bone, go);
            //        }
            //    }
            //}
        }


        private static Transform LoopSearch(Transform tran, string name)
        {
            Transform t = tran.Find(name);
            if (t == null)
            {
                int cnt = tran.childCount;
                for (int i = 0; i < cnt; i++)
                {
                    Transform ty = tran.GetChild(i);
                    var rt = LoopSearch(ty, name);
                    if (rt != null) return rt;
                }
                return null;
            }
            else
            {
                return t;
            }
        }

        private static void Write(StreamWriter sw, Transform tran, Vector3 offset)
        {
            Vector3 pos = tran.position;
            Quaternion rot = tran.rotation;
            sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                (pos.x + offset.x).ToString("f2"),
                (pos.y + offset.y).ToString("f2"),
                (pos.z + offset.z).ToString("f2"),
                rot.x.ToString("f2"),
                rot.y.ToString("f2"),
                rot.z.ToString("f2"),
                rot.w.ToString("f2"));
        }

        //public static void GenerateCurve(AnimationClip clip, ColliderTable.RowData row, string bone, GameObject go, int fps = 30)
        //{
        //    Transform tran = null;
        //    tran = bone == go.name ? go.transform : LoopSearch(go.transform, bone);

        //    if (tran == null)
        //    {
        //        Debug.LogError("not found the bone named: " + bone);
        //    }
        //    else
        //    {
        //        float len = clip.length;
        //        float rate = fps;
        //        Debug.Log(clip.name + " rate: " + rate + " len:" + len + " cnt: " + (int)(len * fps) + " fps:" + rate);
        //        int cnt = (int)(len * rate);
        //        string path = XEditorPath.GetEditorBasedPath("Server/Animation/");
        //        string filename = row.ColliderID + "_" + clip.name + ".txt";
        //        using (FileStream writer = new FileStream(path + filename, FileMode.Create,FileAccess.ReadWrite))
        //        {
        //            StreamWriter sw = new StreamWriter(writer, Encoding.Default);
        //            for (int i = 0; i < cnt; i++)
        //            {
        //                clip.SampleAnimation(go, i / rate);
        //                Vector3 pos = new Vector3(row.BoneX, row.BoneY, row.BoneZ);
        //                Vector3 offset = tran.rotation * pos;
        //                Write(sw, tran, offset);
        //            }
        //            sw.Close();
        //        }
        //    }
        //}


        public static void ExportAnim()
        {
            Object obj = Selection.activeObject;
            if (obj != null && (obj is AnimationClip))
            {
                Debug.Log(obj.name);
                AnimationClip clip = obj as AnimationClip;
                var bindings = AnimationUtility.GetCurveBindings(clip);
                for (int i = 0; i < bindings.Length; i++)
                {
                    AnimationClipCurveData data = new AnimationClipCurveData(bindings[i]);
                    Debug.Log(data.propertyName + "  " + data.type + " " + data.path);
                }
            }
        }
    }
}