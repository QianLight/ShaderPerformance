using FMODUnity;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XEditor
{

    [CustomEditor(typeof(FmodPlayableAsset))]
    public class PlayableFmodEditor : Editor
    {
        private FmodPlayableAsset asset;
        private Rect prevRect;
        Texture openIcon, circle, circle2;
        private FacialExpressionCurve facialCurve = null;
        private void OnEnable()
        {
            openIcon = EditorGUIUtility.Load("FMOD/transportOpen.png") as Texture;
            circle = EditorGUIUtility.Load("FMOD/preview.png") as Texture;
            circle2 = EditorGUIUtility.Load("FMOD/previewemitter.png") as Texture;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            asset = target as FmodPlayableAsset;
            Undo.RecordObject(asset, "obj change");

            asset.clip = EditorGUILayout.TextField("Clip", asset.clip);            

            if (!string.IsNullOrEmpty(asset.curvePath))
            {
                if(GameObject.Find(asset.curvePath) != null)
                    facialCurve = GameObject.Find(asset.curvePath).GetComponent<FacialExpressionCurve>();
                if (facialCurve == null && GameObject.Find(asset.curvePath1) != null)
                    facialCurve = GameObject.Find(asset.curvePath1).GetComponent<FacialExpressionCurve>();
            }

            facialCurve = EditorGUILayout.ObjectField("Facial", facialCurve, typeof(FacialExpressionCurve), true) as FacialExpressionCurve;

            asset.m_isRecord = EditorGUILayout.Toggle("Record", asset.m_isRecord);
            EditorGUILayout.Space();

            if (TimelineGlobalConfig.Instance.m_keyValues == null)
            {
                TimelineGlobalConfig.Instance.ReadConfig();
            }
            if (asset.windowSize == 0)
            {
                asset.windowSize = int.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncWindowSize"]);
            }
            if (asset.amplitudeThreshold == 0)
            {
                asset.amplitudeThreshold = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncThreshold"]);
            }
            if (asset.facialSpeed == 0)
            {
                asset.facialSpeed = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncMoveTowardsSpeed"]);
            }

            asset.windowSize = EditorGUILayout.IntField("口型窗口大小", asset.windowSize);
            asset.amplitudeThreshold = EditorGUILayout.FloatField("口型阈值", asset.amplitudeThreshold);
            asset.facialSpeed = EditorGUILayout.FloatField("口型速度", asset.facialSpeed);

            if (facialCurve == null)
            {
                asset.curvePath = String.Empty;
                asset.curvePath1 = String.Empty;
                asset.facialCurve = null;
            }
            else
            {
                asset.facialCurve = facialCurve;
                asset.curvePath = GetGameObjectPath(facialCurve.transform, false);
                asset.curvePath1 = GetGameObjectPath(facialCurve.transform, true);
            }

            if (!string.IsNullOrEmpty(asset.clip))
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorEventRef eventRef = EventManager.EventFromPath(asset.clip);

                if (eventRef == null)
                {
                    EditorGUILayout.LabelField("Clip Path Wrong");
                    return;
                }

                EditorGUILayout.LabelField("GUID", eventRef.Guid.ToString("b"));

                StringBuilder builder = new StringBuilder();
                eventRef.Banks.ForEach((x) => { builder.Append(Path.GetFileNameWithoutExtension(x.Path)); builder.Append(", "); });
                EditorGUILayout.LabelField("Banks", builder.ToString(0, builder.Length - 2));

                var desc = eventRef.Is3D ? "3D" : "2D";
                EditorGUILayout.LabelField("Panning", desc);

                EditorGUILayout.LabelField("Stream", eventRef.IsStream.ToString());

                EditorGUILayout.LabelField("Oneshot", eventRef.IsOneShot.ToString());

                var lengt = eventRef.Length;
                TimeSpan t = TimeSpan.FromMilliseconds(lengt);
                var time = string.Format("{0:D2}:{1:D2}:{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);
                EditorGUILayout.LabelField("Length", lengt > 0 ? time : "N/A");

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                var transportButtonStyle = new GUIStyle();
                transportButtonStyle.padding.left = 4;
                transportButtonStyle.padding.top = 10;
                if (GUILayout.Button(new GUIContent(openIcon, "Show Event in FMOD Studio"), transportButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    string cmd = string.Format("studio.window.navigateTo(studio.project.lookup(\"{0}\"))", eventRef.Guid.ToString("b"));
                    FMODUnity.EditorUtils.SendScriptCommand(cmd);
                }
                EditorGUILayout.EndHorizontal();

                var rect = GUILayoutUtility.GetLastRect();
                if (rect.x > 1)
                {
                    prevRect = rect;
                    prevRect.x = -20;
                    prevRect.y += 20;
                    prevRect.height = 160;
                }
                for (int i = 0; i < 32; i++) EditorGUILayout.Space();
                Preview(eventRef);
            }
        }

        float previewDistance = 0;
        float previewOrientation = 0;
        Vector2 eventPosition;

        private void Preview(EditorEventRef e)
        {
            GUILayout.BeginArea(prevRect);

            var originalColour = GUI.color;
            if (!e.Is3D)
            {
                GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
            }

            Rect rect = new Rect(150, 10, 128, 128);
            GUI.DrawTexture(rect, circle);

            Vector2 centre = rect.center;
            Rect rect2 = new Rect(rect.center.x + eventPosition.x - 6, rect.center.y + eventPosition.y - 6, 12, 12);
            GUI.DrawTexture(rect2, circle2);

            GUI.color = originalColour;

            if (e.Is3D && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && rect.Contains(Event.current.mousePosition))
            {
                var newPosition = Event.current.mousePosition;
                Vector2 delta = (newPosition - centre);
                float distance = delta.magnitude;
                if (distance < 60)
                {
                    eventPosition = newPosition - rect.center;
                    previewDistance = distance / 60.0f * e.MaxDistance;
                    delta.Normalize();
                    float angle = Mathf.Atan2(delta.y, delta.x);
                    previewOrientation = angle + Mathf.PI * 0.5f;
                }
                Event.current.Use();
            }

            FMODUnity.EditorUtils.PreviewUpdatePosition(previewDistance, previewOrientation);

            GUILayout.EndArea();
        }

        public static string GetGameObjectPath(Transform transform, bool getGamePath)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                string str = transform.name;
                if (transform.parent == null && getGamePath)
                {
                    str += "(Clone)";
                }
                path = str + "/" + path;
            }
            return path;
        }
    }
}