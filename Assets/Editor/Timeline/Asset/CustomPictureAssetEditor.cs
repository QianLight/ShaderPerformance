using FMODUnity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    [CustomEditor(typeof(CustomPictureAsset))]
    public class CustomPictureAssetEditor : Editor
    {
        private CustomPictureAsset asset;

        public override void OnInspectorGUI()
        {
            asset = target as CustomPictureAsset;
            Undo.RecordObject(asset, "obj change");

            asset.m_pictureType = (EPictureType)EditorGUILayout.EnumPopup("PictureType", asset.m_pictureType);
            asset.m_useBigBg = EditorGUILayout.Toggle("UseBigBg", asset.m_useBigBg);
            asset.m_useShake = EditorGUILayout.Toggle("UseShake", asset.m_useShake);

            if (GUILayout.Button("add"))
            {
                if (asset.m_customPictureInfos == null) asset.m_customPictureInfos = new List<CustomPictureInfo>();
                CustomPictureInfo item = new CustomPictureInfo();
                if (asset.m_customPictureInfos.Count >= 2) return;
                asset.m_customPictureInfos.Add(item);
            }

            if (asset.m_customPictureInfos != null)
            {
                for (int i = 0; i < asset.m_customPictureInfos.Count; ++i)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("pic" + (i + 1), GUILayout.Width(100));
                    if (GUILayout.Button("remove", GUILayout.Width(100)))
                    {
                        asset.m_customPictureInfos.RemoveAt(i);
                        break;
                    }
                    GUILayout.EndHorizontal();
                    DrawOnePictureInfo(asset.m_customPictureInfos[i]);
                    GUILayout.Space(10);
                }
            }
        }

        private void DrawOnePictureInfo(CustomPictureInfo pictureInfo)
        {
            pictureInfo.m_pictureEffect = (EPictureEffect)EditorGUILayout.EnumPopup("PictureEffect", pictureInfo.m_pictureEffect);
            pictureInfo.m_startPosition = EditorGUILayout.Vector2Field("StartPosition", pictureInfo.m_startPosition);
            pictureInfo.m_endPosition = EditorGUILayout.Vector2Field("EndPosition", pictureInfo.m_endPosition);
            pictureInfo.m_positionTweenType = (CustomPictureAsset.TweenType)EditorGUILayout.EnumPopup("CurveType", pictureInfo.m_positionTweenType);
            pictureInfo.m_positionDuration = EditorGUILayout.FloatField("PositionDuration", pictureInfo.m_positionDuration);

            pictureInfo.m_startScale = EditorGUILayout.FloatField("StartScale", pictureInfo.m_startScale);
            pictureInfo.m_endScale = EditorGUILayout.FloatField("EndScale", pictureInfo.m_endScale);
            pictureInfo.m_scaleTweenType = (CustomPictureAsset.TweenType)EditorGUILayout.EnumPopup("CurveType", pictureInfo.m_scaleTweenType);
            pictureInfo.m_scaleDuration = EditorGUILayout.FloatField("ScaleDuration", pictureInfo.m_scaleDuration);


            GUILayout.BeginHorizontal();
            pictureInfo.m_path = EditorGUILayout.TextField("Path", pictureInfo.m_path);
            if (GUILayout.Button("...", GUILayout.Width(50)))
            {
                string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
                string dir = "BundleRes";
                int index = path.IndexOf(dir);
                if (index >= 0)
                {
                    path = path.Substring(index + dir.Length);
                }
                path = path.TrimStart("/".ToCharArray());
                path = path.TrimEnd(".png".ToCharArray());
                pictureInfo.m_path = path;
            }
            GUILayout.EndHorizontal();
            pictureInfo.m_startAlpha = EditorGUILayout.FloatField("StartAlpha", pictureInfo.m_startAlpha);
            pictureInfo.m_middleAlpha = EditorGUILayout.FloatField("MiddleAlpha", pictureInfo.m_middleAlpha);
            pictureInfo.m_endAlpha = EditorGUILayout.FloatField("EndAlpha", pictureInfo.m_endAlpha);
            pictureInfo.m_startToMiddleAlphaDuration = EditorGUILayout.FloatField("StaToMidAlphaDuration", pictureInfo.m_startToMiddleAlphaDuration);
            pictureInfo.m_middleToEndAlphaDuration = EditorGUILayout.FloatField("MidToEndAlphaDuration", pictureInfo.m_middleToEndAlphaDuration);
        }
    }
}