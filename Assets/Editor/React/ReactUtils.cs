#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    static class ReactCommon
    {
        public static readonly float FRAME_RATIO = (1.0f / 30.0f);
        /* stles */
        public static GUIStyle BoldLabelstyle = null;// new GUIStyle(EditorStyles.boldLabel);
        public static GUIStyle BoldLabelStyle_Black = null;//new GUIStyle(EditorStyles.boldLabel);
        public static GUIStyle NormalLabelStyle = null;//new GUIStyle();
        public static GUIStyle GreyLabelStyle = null;//new GUIStyle();
        public static GUIStyle NormalLabelStyleBlack = null;// new GUIStyle();

        /* Color  */
        public static Color InitColor;
        public static Color BoxColor;
        public static Color BGColor = Color.white;
        public static Color TextColor = Color.white;

        public static void Init()
        {
            if (BoldLabelstyle == null) BoldLabelstyle = new GUIStyle(/*EditorStyles.boldLabel*/);
            if (BoldLabelStyle_Black == null) BoldLabelStyle_Black = new GUIStyle(/*EditorStyles.boldLabel*/);
            if (NormalLabelStyle == null) NormalLabelStyle = new GUIStyle();
            if (GreyLabelStyle == null) GreyLabelStyle = new GUIStyle();
            if (NormalLabelStyleBlack == null) NormalLabelStyleBlack = new GUIStyle();

            InitColor = GUI.color;

            BoxColor = ReactUtils.GetColor("#D0D0D0");
            BGColor = ReactUtils.GetColor("#4D4D4D");

            BoldLabelstyle.fontSize = 13;
            //BoldLabelstyle.normal.textColor = TextColor;

            BoldLabelStyle_Black.fontSize = 13;
            BoldLabelStyle_Black.normal.textColor = Color.black;

            //NormalLabelStyle.normal.textColor = TextColor;
            //NormalLabelStyleBlack.normal.textColor = Color.black;

            GreyLabelStyle.normal.textColor = Color.gray;
            GreyLabelStyle.fontSize = 14;


        }
    }

    static class ReactUtils
    {
        /* GUIContent */
        public static GUIContent NoneContent = null;// new GUIContent();

        public static void DrawBox(Rect rect, Color color, GUIContent content = null, string tooltip = null)
        {
            GUI.color = color;
            if (tooltip != null)
            {
                if (NoneContent == null) NoneContent = new GUIContent();

                NoneContent.tooltip = tooltip;
            }
            GUI.Box(rect, content == null ? NoneContent : content);
            if (tooltip != null)
            {
                NoneContent.tooltip = null;
            }

            ResetColor();
        }

        public static void ResetColor()
        {
            GUI.color = ReactCommon.InitColor;
        }


        public static Color GetColor(string html)
        {
            Color col;
            ColorUtility.TryParseHtmlString(html, out col);
            return col;
        }

        public static Mesh CreateConeMesh(float radius, float height, int subdivisions = 12)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0f);
            for (int i = 0, n = subdivisions - 1; i < subdivisions; i++)
            {
                float ratio = (float)i / n;
                float r = ratio * (Mathf.PI * 2f);
                float x = Mathf.Cos(r) * radius;
                float z = Mathf.Sin(r) * radius;
                vertices[i + 1] = new Vector3(x, 0f, z);
                uv[i + 1] = new Vector2(ratio, 0f);
            }
            vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);

            // construct bottom

            for (int i = 0, n = subdivisions - 1; i < n; i++)
            {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }

            // construct sides

            int bottomOffset = subdivisions * 3;
            for (int i = 0, n = subdivisions - 1; i < n; i++)
            {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

    }


}
#endif