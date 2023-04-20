//#if UNITY_EDITOR

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    public enum DecalType
    {
        Circle,
        Rect,
        Fan,
        Tex
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class ProjectorControl : MonoBehaviour
    {
        public DecalConfig config = new DecalConfig ()
        {
            length = 10,
            width = 1,
            height = 1,
        };
        public FloatAnim param0x;
        public FloatAnim param0y;
        public FloatAnim param0z;
        public FloatAnim param0w;
        public ColorAnim color0;
        public ColorAnim color1;
        public FloatAnim width;
        public FloatAnim height;
        public FloatAnim angle;
        public string texPath;
        public bool isPng = true;

        public float startTime = 0;
        public float endTime = 1;
        public EditorFlagMask flag = new EditorFlagMask ();
        private Transform trans;
        //private ProcessLoadCb texCb;

        public static uint DecalAnimMask_Param0x = 0x00000001;
        public static uint DecalAnimMask_Param0y = 0x00000002;
        public static uint DecalAnimMask_Param0z = 0x00000004;
        public static uint DecalAnimMask_Param0w = 0x00000008;

        public static uint DecalAnimMask_Color0 = 0x00000010;
        public static uint DecalAnimMask_Color1 = 0x00000020;
        public static uint DecalAnimMask_Width = 0x00000040;
        public static uint DecalAnimMask_Height = 0x00000080;
        public static uint DecalAnimMask_Angle = 0x00000100;
#if UNITY_EDITOR
        public void InitParam ()
        {
            if (config.decalType == (int) DecalType.Circle)
            {
                FloatAnim.InitCreate (ref param0x, 0, 5, 0);
                FloatAnim.InitCreate (ref param0y, 0, 5, 0);
                FloatAnim.InitCreate (ref param0z, 0, 2, 0);
                FloatAnim.InitCreate (ref param0w, 0, 5, 0);
                ColorAnim.InitCreate (ref color0, Color.white);
                ColorAnim.InitCreate (ref color1, Color.clear);
                FloatAnim.InitCreate (ref width, 0, 32, 1);
            }
            else if (config.decalType == (int) DecalType.Tex)
            {
                FloatAnim.InitCreate (ref width, 0, 32, 1);
                FloatAnim.InitCreate (ref height, 0, 32, 1);
                FloatAnim.InitCreate (ref angle, 0, 360, 0);
                ColorAnim.InitCreate (ref color0, Color.white);
            }
        }
        private void OnEnable ()
        {
            InitParam ();
            OnStart ();
        }
#endif

        // public ISFXOwner Owner { get; set; }
        private Transform GetTrans ()
        {
            if (trans == null)
            {
                trans = this.transform;
            }
            return trans;
        }
        private void UpdateSize (bool rect, bool rot)
        {
            float cos = 1;
            float sin = 0;
            if (rot)
            {
                float angle = config.angleY * Mathf.Deg2Rad;
                cos = Mathf.Cos (angle);
                sin = Mathf.Sin (angle);
            }
            float width = config.width;
            float height = width;
            float widthInv = config.width > 0 ? 1 / config.width : 1;
            float heightInv = widthInv;
            if (rect)
            {
                height = config.height;
                heightInv = config.height > 0 ? 1 / config.height : 1;
            }

            config.dd.rotSizeInv.x = cos;
            config.dd.rotSizeInv.y = sin;
            config.dd.rotSizeInv.z = widthInv;
            config.dd.rotSizeInv.w = heightInv;
            config.rect.x = config.dd.decalPos.x - width * 0.5f;
            config.rect.z = config.dd.decalPos.x + width * 0.5f;
            config.rect.y = config.dd.decalPos.z - height * 0.5f;
            config.rect.w = config.dd.decalPos.z + height * 0.5f;
        }

        private bool UpdateCircle (float time, float timePercent)
        {
            bool sizeChanged = false;
            config.width = width.Evaluate (time, ref sizeChanged);
            config.height = config.width;

            config.dd.param0.x = param0x.Evaluate (time);
            config.dd.param0.y = param0y.Evaluate (time);
            config.dd.param0.z = param0z.Evaluate (time);
            config.dd.param0.w = param0w.Evaluate (time);
            config.dd.color0 = color0.Evaluate (timePercent);
            config.dd.color1 = color1.Evaluate (timePercent);
            return sizeChanged;
        }
        // private bool TexLoadCb (ref ResHandle resHandle, ref Vector4Int param, System.Object loadHolder)
        // {
        //     if (resHandle.obj != null)
        //     {
        //         config.res.Set (ref resHandle);
        //         return true;
        //     }
        //     return false;
        // }

        public void InitTex ()
        {
            config.dd.param0.x = -2;
            if (!string.IsNullOrEmpty (texPath))
            {
                // if (texCb == null)
                //     texCb = TexLoadCb;
                // string ext = isPng? ".png": ".tga";
                // LoadMgr.GetResHandler (ref config.res, texPath, ext);
                // LoadMgr.singleton.LoadAsset<Texture> (ref config.res, ext, ref LoadMgr.loadParam, texCb);

            }
        }

        private bool UpdateTex (float time, float timePercent)
        {
            bool sizeChanged = false;
            config.width = width.Evaluate (time, ref sizeChanged);
            config.height = height.Evaluate (time, ref sizeChanged);
            config.angleY = angle.Evaluate (time, ref sizeChanged);

            config.dd.color0 = color0.Evaluate (timePercent);

            return sizeChanged;
        }
        public bool IsEnable ()
        {
            return this.enabled && this.gameObject.activeInHierarchy;
        }

        public void OnStart ()
        {
            InnerUpdate (0, 0, true);
            if (config.decalType == (int) DecalType.Circle)
            {
                param0x.animHelper.InitCreate (DecalAnimMask_Param0x, flag);
                param0y.animHelper.InitCreate (DecalAnimMask_Param0y, flag);
                param0z.animHelper.InitCreate (DecalAnimMask_Param0z, flag);
                param0w.animHelper.InitCreate (DecalAnimMask_Param0w, flag);

                color0.animHelper.InitCreate (DecalAnimMask_Color0, flag);
                color1.animHelper.InitCreate (DecalAnimMask_Color1, flag);
                width.animHelper.InitCreate (DecalAnimMask_Width, flag);
            }
            else if (config.decalType == (int) DecalType.Tex)
            {
                width.animHelper.InitCreate (DecalAnimMask_Width, flag);
                height.animHelper.InitCreate (DecalAnimMask_Height, flag);
                angle.animHelper.InitCreate (DecalAnimMask_Angle, flag);
                color0.animHelper.InitCreate (DecalAnimMask_Color0, flag);
            }
        }
        public void OnStop ()
        {

        }
        public bool InnerUpdate (float time, float timePercent, bool forceUpdate)
        {
            bool update = false;
            bool play = false;
            bool rect = true;
            bool rot = true;
            if (config.decalType == (int) DecalType.Circle)
            {
                rect = false;
                rot = false;
                play = true;
                update = forceUpdate;
                update |= UpdateCircle (time, timePercent);

            }
            else if (config.decalType == (int) DecalType.Tex)
            {
                if (forceUpdate)
                {
                    InitTex ();
                }
                //if (config.res.obj is Texture2D)
                //{
                //    rect = true;
                //    rot = true;
                //    update = UpdateTex (time, timePercent);
                //    update = true;
                //    play = true;
                //}
            }
            if (update)
            {
                UpdateSize (rect, rot);
            }
            return play;
        }

        public void OnUpdate (float time, EngineContext context)
        {
            if (trans != null && time >= startTime && time < endTime)
            {
                if (InnerUpdate (time, (time - startTime) / (endTime - startTime), false))
                {
                    config.dd.decalPos = trans.position;
                    DecalSystem.Play (context, ref config);
                }
            }

        }

#if UNITY_EDITOR

        void OnDrawGizmos ()
        {
            Color c = Gizmos.color;
            Gizmos.color = Color.white;
            float halfWidth = config.width * 0.5f;
            float halfHeight = config.height * 0.5f;
            Quaternion rot = Quaternion.AngleAxis (config.angleY, Vector3.up);
            Vector3 pos = transform.position;
            Handles.ArrowHandleCap (101, pos, rot, halfHeight + 0.2f, EventType.Repaint);

            Vector3 forward = rot * Vector3.forward;
            Vector3 right = Vector3.Cross (Vector3.up, forward).normalized;

            Vector3 v0 = pos - halfWidth * right - halfHeight * forward;
            Vector3 v1 = pos + halfWidth * right - halfHeight * forward;
            Vector3 v2 = pos + halfWidth * right + halfHeight * forward;
            Vector3 v3 = pos - halfWidth * right + halfHeight * forward;
            Vector3 far = 10 * Vector3.down;
            Vector3 v4 = v0 + far;
            Vector3 v5 = v1 + far;
            Vector3 v6 = v2 + far;
            Vector3 v7 = v3 + far;
            Gizmos.DrawLine (v0, v1);
            Gizmos.DrawLine (v1, v2);
            Gizmos.DrawLine (v2, v3);
            Gizmos.DrawLine (v3, v0);

            Gizmos.DrawLine (v4, v5);
            Gizmos.DrawLine (v5, v6);
            Gizmos.DrawLine (v6, v7);
            Gizmos.DrawLine (v7, v4);

            Gizmos.DrawLine (v0, v4);
            Gizmos.DrawLine (v1, v5);
            Gizmos.DrawLine (v2, v6);
            Gizmos.DrawLine (v3, v7);

            Gizmos.color = c;

        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor (typeof (ProjectorControl))]
    public class ProjectorControlEditor : UnityEngineEditor
    {
        SerializedProperty startTime;
        SerializedProperty endTime;
        SerializedProperty decalType;
        SerializedProperty param0x;
        SerializedProperty param0y;
        SerializedProperty param0z;
        SerializedProperty param0w;
        SerializedProperty color0;
        SerializedProperty color1;
        SerializedProperty width;
        SerializedProperty height;
        SerializedProperty angle;

        SerializedProperty texPath;
        SerializedProperty isPng;

        private void OnEnable ()
        {
            startTime = serializedObject.FindProperty ("startTime");
            endTime = serializedObject.FindProperty ("endTime");
            decalType = serializedObject.FindProperty ("config.decalType");
            param0x = serializedObject.FindProperty ("param0x");
            param0y = serializedObject.FindProperty ("param0y");
            param0z = serializedObject.FindProperty ("param0z");
            param0w = serializedObject.FindProperty ("param0w");
            color0 = serializedObject.FindProperty ("color0");
            color1 = serializedObject.FindProperty ("color1");
            width = serializedObject.FindProperty ("width");
            height = serializedObject.FindProperty ("height");
            angle = serializedObject.FindProperty ("angle");

            texPath = serializedObject.FindProperty ("texPath");
            isPng = serializedObject.FindProperty ("isPng");
            ProjectorControl pc = target as ProjectorControl;
            pc.param0x.animHelper.Reset ();
            pc.param0y.animHelper.Reset ();
            pc.param0z.animHelper.Reset ();
            pc.param0w.animHelper.Reset ();

            pc.color0.animHelper.Reset ();
            pc.color1.animHelper.Reset ();
            pc.width.animHelper.Reset ();
            pc.height.animHelper.Reset ();

            pc.angle.animHelper.Reset ();
        }

        public override void OnInspectorGUI ()
        {
            ProjectorControl pc = target as ProjectorControl;
            serializedObject.Update ();
            // EditorGUILayout.PropertyField (length);
            DecalType dt = (DecalType) decalType.intValue;
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.PropertyField (startTime);
            if (EditorGUI.EndChangeCheck ())
            {
                if (startTime.floatValue < 0)
                {
                    startTime.floatValue = 0;
                }
                if (startTime.floatValue >= endTime.floatValue)
                {
                    endTime.floatValue = startTime.floatValue + 0.1f;
                }
            }
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.PropertyField (endTime);
            if (EditorGUI.EndChangeCheck ())
            {
                if (endTime.floatValue < 0.1f)
                {
                    endTime.floatValue = 0.1f;
                }
                if (startTime.floatValue >= endTime.floatValue)
                {
                    startTime.floatValue = endTime.floatValue - 0.1f;
                }
            }
            EditorGUI.BeginChangeCheck ();
            DecalType newdt = (DecalType) EditorGUILayout.EnumPopup ("DecalType", dt);
            if (EditorGUI.EndChangeCheck ())
            {
                decalType.intValue = (int) newdt;
                pc.config.decalType = (int) newdt;
                pc.InitParam ();
                pc.OnStart ();
            }
            if (newdt == DecalType.Circle)
            {
                pc.width.DrawGUI (width, "Size");
                pc.param0x.DrawGUI (param0x, "ColorTransition");
                pc.param0y.DrawGUI (param0y, "ColorScale");
                pc.param0z.DrawGUI (param0z, "ColorOutlineWidth");
                pc.param0w.DrawGUI (param0w, "ColorOutlineScale");
                pc.color0.DrawGUI (color0, "Color");
                pc.color1.DrawGUI (color1, "OutlineColor");

            }
            else if (newdt == DecalType.Tex)
            {
                //pc.width.DrawGUI (width, "Width");
                //pc.height.DrawGUI (height, "Height");
                //pc.angle.DrawGUI (width, "Angle");
                //pc.color0.DrawGUI (color0, "Color");
                //Texture tex = pc.config.res.obj as Texture;
                //Texture newTex = EditorGUILayout.ObjectField ("Tex", tex, typeof (Texture), false) as Texture;
                //if (tex != newTex)
                //{
                //    string path = null;
                //    if (newTex != null)
                //    {
                //        path = AssetDatabase.GetAssetPath (newTex);
                //        if (path.StartsWith (AssetsConfig.instance.ResourcePath))
                //        {
                //            int index = AssetsConfig.instance.ResourcePath.Length + 1; // /
                //            path = path.Substring (index);
                //            isPng.boolValue = path.ToLower ().EndsWith (".png");
                //            index = path.LastIndexOf (".");
                //            if (index >= 0)
                //            {
                //                path = path.Substring (0, index);
                //            }
                //            pc.texPath = path;
                //        }
                //        else
                //        {
                //            newTex = null;
                //            path = null;
                //        }
                //    }
                //    else
                //    {
                //        path = null;
                //    }
                //    texPath.stringValue = path;

                //    EngineContext context = EngineContext.instance;
                //    if (context != null)
                //    {
                //        DecalSystem.Stop (context, ref pc.config);
                //    }
                //    pc.config.res.obj = newTex;
                //    pc.InitTex ();
                //}
            }
            serializedObject.ApplyModifiedProperties ();
        }
    }
#endif
}
//#endif