using UnityEngine;
using BluePrint;
using UnityEditor;

namespace EditorNode
{
    public class MobUnitNode : TimeTriggerNode<EcsData.XMobUnitData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header7";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.TemplateID = EditorGUITool.IntField("TemplateID", HosterData.TemplateID);
            HosterData.LifewithinSkill = EditorGUITool.Toggle("LifewithinSkill", HosterData.LifewithinSkill);

            EditorGUITool.Vector3Field("Offset", ref HosterData.OffsetX, ref HosterData.OffsetY, ref HosterData.OffsetZ);
            HosterData.MobAtTarget = EditorGUITool.Toggle("MobAtTarget", HosterData.MobAtTarget);
            CheckAndDo(HosterData.MobAtTarget,
                () => HosterData.MultiTargetIndex = Mathf.Max(0, EditorGUITool.IntField("MultiTargetIndex", HosterData.MultiTargetIndex + 1) - 1),
                () => HosterData.MultiTargetIndex = 0);

            HosterData.Angle = EditorGUITool.FloatField(  "Angle", HosterData.Angle);
            HosterData.Random = EditorGUITool.Toggle("Random", HosterData.Random);
            if(HosterData.Random)
            {
                HosterData.RandomRange = EditorGUITool.FloatField(  "RandomRange", HosterData.RandomRange);
            }
        }
    }
}