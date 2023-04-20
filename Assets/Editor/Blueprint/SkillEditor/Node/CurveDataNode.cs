using EcsData;
using UnityEngine;
using BluePrint;
using UnityEditor;

namespace EditorNode
{
    public class CurveNode : EventTriggerNode<XCurveData>
    {
        private TextAsset forwardCurve;
        private TextAsset sideCurve;
        private TextAsset upCurve;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            HeaderImage = "BluePrint/Header3";

            if (GetRoot.NeedInitRes)
            {
                forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveForward, typeof(TextAsset)) as TextAsset;
                sideCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveSide, typeof(TextAsset)) as TextAsset;
                upCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveUp, typeof(TextAsset)) as TextAsset;
            }
        }

        public override void DrawDataInspector()
        {
            forwardCurve = EditorGUITool.ObjectField("Forward", forwardCurve, typeof(TextAsset), false) as TextAsset;
            if (forwardCurve != null)
            {
                HosterData.CurveForward = AssetDatabase.GetAssetPath(forwardCurve).Replace(CurvePath, "");
                ParamTemplate("ForwardRatio", HosterData,
                () => { HosterData.ForwardRatio = EditorGUITool.FloatField("ForwardRatio", HosterData.ForwardRatio); });
            }
            else HosterData.CurveForward = "";

            sideCurve = EditorGUITool.ObjectField("Side", sideCurve, typeof(TextAsset), false) as TextAsset;
            if (sideCurve != null)
            {
                HosterData.CurveSide = AssetDatabase.GetAssetPath(sideCurve).Replace(CurvePath, "");
                ParamTemplate("SideRatio", HosterData,
                () => { HosterData.SideRatio = EditorGUITool.FloatField("SideRatio", HosterData.SideRatio); });
            }
            else HosterData.CurveSide = "";

            HosterData.UsingUp = EditorGUITool.Toggle("UsingUp", HosterData.UsingUp);
            if (HosterData.UsingUp)
            {
                upCurve = EditorGUITool.ObjectField("Up", upCurve, typeof(TextAsset), false) as TextAsset;
                if (upCurve != null)
                {
                    HosterData.CurveUp = AssetDatabase.GetAssetPath(upCurve).Replace(CurvePath, "");
                    ParamTemplate("UpRatio", HosterData,
                    () => { HosterData.UpRatio = EditorGUITool.FloatField("UpRatio", HosterData.UpRatio); });
                }
                else HosterData.CurveUp = "";
            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            forwardCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveForward, typeof(TextAsset)) as TextAsset;
            sideCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveSide, typeof(TextAsset)) as TextAsset;
            upCurve = AssetDatabase.LoadAssetAtPath(CurvePath + HosterData.CurveUp, typeof(TextAsset)) as TextAsset;

            if (forwardCurve == null)
            {
                hasError = true;
                LogError("Node_" + HosterData.Index + "£¬forwardCurve È±Ê§£¡£¡£¡");
                return false;
            }

            return true;
        }
    }
}
