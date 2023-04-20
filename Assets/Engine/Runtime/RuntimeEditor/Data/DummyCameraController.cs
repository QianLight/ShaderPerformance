#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [Serializable]
    public struct EditorDymmyCamera
    {
        public float fov;
        public Transform ct;
        public Color color;
        public Transform light;
        public Vector3 dir;
        public Color lightColor;
        public float outlineAdd;
        public Transform cameraSfx;
        public bool folder;

        private GetCameraTransform cb;

        public GetCameraTransform GetCb ()
        {
            if (cb == null)
                cb = GetCameraParam;
            return cb;
        }

        public void GetCameraParam (ref Vector3 pos, ref Quaternion rot)
        {
            if (ct != null)
            {
                pos = ct.position;
                rot = ct.rotation;
            }
        }
    }

    [DisallowMultipleComponent, ExecuteInEditMode]
    public sealed class DummyCameraController : MonoBehaviour
    {
        // public DummyCamera[] cameras = new DummyCamera[3];
        public EditorDymmyCamera[] editorCameras = new EditorDymmyCamera[3]
        {
            new EditorDymmyCamera () { fov = 55, color = Color.gray },
            new EditorDymmyCamera () { fov = 55, color = Color.gray },
            new EditorDymmyCamera () { fov = 55, color = Color.gray },
        };

        [Range(2, 3)]
        public int playerCount = 3;

        [NonSerialized]
        public bool test;
        public Vector2[] splitPoint3 = new Vector2[2]
        {
            new Vector2 (-270, 5),
            new Vector2 (270, -5),
        };

        public Vector2[] splitPoint2 = new Vector2[3]
        {
            new Vector2 (-812, 0),
            new Vector2 (0, 0),
            new Vector2 (812, 0),
        };

        private Vector2[] points = new Vector2[4]
        {
            Vector2.zero,
            new Vector2 (1.0f / 3, 1.0f / 3),
            new Vector2 (2.0f / 3, 2.0f / 3),
            Vector2.one
        };

        public Color backGroundColor = Color.black;

        public RectTransform uiCanvas;
        public Camera uiCamera;

        private DrawRenderCb drawCb;
        private Dictionary<Renderer, Material> matPool = new Dictionary<Renderer, Material> ();
        private Dictionary<Renderer, Material> outlinePool = new Dictionary<Renderer, Material> ();
        private bool init = false;

        public void Clear (Dictionary<Renderer, Material> pool)
        {
            var it = pool.GetEnumerator ();
            while (it.MoveNext ())
            {
                var v = it.Current.Value;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy (v);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate (v);
                }
            }
            pool.Clear ();
        }
        public void Clear ()
        {
            Clear (matPool);
            Clear (outlinePool);
        }

        private void DrawRender (EngineContext context, Transform t, ref DummyCamera dc, bool isSfx, bool processSfx)
        {
            if (t != null)
            {
                if (t.TryGetComponent<SFXWrapper> (out var sfx))
                {
                    isSfx |= true;
                    if (!processSfx)
                    {
                        return;
                    }
                }
                
                Renderer r = t.GetComponent<Renderer> ();
                if (r != null && r.enabled && t.gameObject.activeInHierarchy)
                {
                    var mat = r.sharedMaterial;
                    if (mat != null)
                    {
                        var db = SharedObjectPool<DrawBatch>.Get ();
                        db.id = dc.id;
                        db.render = r;
                        r.SetPropertyBlock(null);
                        if (!matPool.TryGetValue (r, out var matInstance))
                        {
                            matInstance = new Material (mat);
                            matPool[r] = matInstance;
                        }
                        else
                        {
                            matInstance.CopyPropertiesFromMaterial (mat);
                        }
                        db.mat = matInstance;
                        matInstance.EnableKeyword ("_LOCAL_WORLD_OFFSET");
                        Transform rt = t;
                        if (r is SkinnedMeshRenderer)
                        {
                            var smr = r as SkinnedMeshRenderer;
                            rt = smr.rootBone;
                        }
                        Matrix4x4 local2World = isSfx?Matrix4x4.TRS (
                                    dc.forward * rt.localPosition.magnitude,
                                    rt.localRotation*dc.rot, rt.localScale):
                            Matrix4x4.TRS (rt.position - dc.pos, rt.rotation, rt.localScale);
                        matInstance.SetMatrix (ShaderManager.custom_ObjectToWorld, local2World);
                        matInstance.SetInt (ShaderManager._Stencil, dc.id + 1);
                        SplitScreenSystem.SetLightInfo (context, ref dc, matInstance);
                        var mode = EditorCommon.GetBlendMode (matInstance);
                        if (mode == BlendMode.Transparent || isSfx)
                        {
                            context.transparentDrawCall.Push (db);
                        }
                        else
                        {
                            context.opaqueDrawCall.Push (db);
                            int passCount = matInstance.passCount;
                            for (int i = 0; i < passCount; ++i)
                            {
                                var passName = matInstance.GetPassName (i);
                                if (passName == "OUTLINE")
                                {
                                    if (!outlinePool.TryGetValue (r, out var outline))
                                    {
                                        int offset = WorldSystem.GetEffectMatOffset (EEffectMaterial.Outline);
                                        var outlineSrc = WorldSystem.GetEffectMat (offset, 1);
                                        outline = new Material (outlineSrc);
                                        outlinePool[r] = outline;
                                    }
                                    outline.SetTexture (ShaderManager._MainTex, matInstance.GetTexture (ShaderManager._MainTex));
                                    var colorOutline = matInstance.GetColor (ShaderManager._ColorOutline);
                                    colorOutline.a += dc.outlineAdd;
                                    outline.SetColor (ShaderManager._ColorOutline, colorOutline);
                                    outline.SetColor (ShaderManager._Color0, matInstance.GetColor (ShaderManager._Color0));
                                    outline.SetMatrix (ShaderManager.custom_ObjectToWorld, local2World);
                                    Matrix4x4 _MatrixITMV = (local2World.inverse * dc.invView);
                                    _MatrixITMV = _MatrixITMV.transpose;
                                    outline.SetMatrix (ShaderManager.custom_MatrixITMV, _MatrixITMV);

                                    outline.SetInt (ShaderManager._Stencil, dc.id + 1);
                                    var outlinedb = SharedObjectPool<DrawBatch>.Get ();
                                    outlinedb.id = dc.id;
                                    outlinedb.render = r;
                                    outlinedb.mat = outline;
                                    context.opaqueDrawCall.Push (outlinedb);
                                    break;
                                }
                            }

                        }
                    }

                }
                for (int i = 0; i < t.childCount; ++i)
                {
                    DrawRender (context, t.GetChild (i), ref dc, sfx, processSfx);
                }
            }
        }

        void DrawRender (EngineContext context,
            ref DummyCamera dc)
        {
            ref var ec = ref editorCameras[dc.id];
            DrawRender (context, ec.ct, ref dc, false, false);
            if (ec.cameraSfx != null)
            {
                DrawRender (context, ec.cameraSfx, ref dc, true, true);
            }
        }

        private void SetStencilRange (EngineContext context, float left0, float left1, float right0, float right1, int index)
        {
            ref var c = ref context.dummyCameras[index];
            ref var ec = ref editorCameras[index];
            c.fov = ec.fov;
            c.c = ec.color;
            c.outlineAdd = ec.outlineAdd;

            if (ec.light != null)
            {
                c.lightDir = ec.light.rotation * -Vector3.forward;
            }
            else
            {
                c.lightDir = -Vector3.forward;
            }
            ec.dir = c.lightDir;
            c.roleLightColor = ec.lightColor;
            c.stencilRange = new Vector4 (left0, left1, right0, right1);
            // c.range.x = left0 > left1?left1 : left0;
            // c.range.y = right0 > right1?right0 : right1;
            if (drawCb == null)
                drawCb = DrawRender;
            SplitScreenSystem.SetSlotObject (context, null, index, ec.GetCb (), ec.fov, drawCb);
        }

        private void SyncPos (EngineContext context, int i)
        {
            ref var c = ref context.dummyCameras[i];
            ref var ec = ref editorCameras[i];
            if (ec.ct != null)
            {
                c.pos = ec.ct.position;
                c.rot = ec.ct.rotation;
            }
            else
            {
                c.pos = this.transform.position;
                c.rot = this.transform.rotation;
            }
        }

        public void Update ()
        {
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                SplitScreenSystem.SetPlayerCount(playerCount);
                Vector3 scale = Vector3.one;
                context.uiCamera = uiCamera;
                if (uiCamera != null && uiCanvas != null)
                {
                    scale = uiCanvas.localScale;
                }
                SplitScreenSystem.EnableSpitScreen (context, test,ref scale);
                if (test)
                {
                    var camera = context.CameraRef;
                    var halfWidth = camera.pixelWidth * 0.5f;
                    var halfHeight = camera.pixelHeight * 0.5f;
                    if (playerCount == 3)
                    {
                        points[0] = Vector2.zero;
                        points[3] = Vector2.one;
                        var sp0 = splitPoint3[0];
                        sp0.y = Mathf.Tan(sp0.y * Mathf.Deg2Rad);
                        SplitScreenSystem.CalcStencilRange(context, halfWidth, halfHeight, ref sp0, ref points[1], ref scale);
                        var sp1 = splitPoint3[1];
                        sp1.y = Mathf.Tan(sp1.y * Mathf.Deg2Rad);
                        SplitScreenSystem.CalcStencilRange(context, halfWidth, halfHeight, ref sp1, ref points[2], ref scale);
                    }
                    else
                    {
                        var sp0 = splitPoint2[0];
                        sp0.y = Mathf.Tan(sp0.y * Mathf.Deg2Rad);
                        SplitScreenSystem.CalcStencilRange(context, halfWidth, halfHeight, ref sp0, ref points[0], ref scale);

                        var sp1 = splitPoint2[1];
                        sp1.y = Mathf.Tan(sp1.y * Mathf.Deg2Rad);
                        SplitScreenSystem.CalcStencilRange(context, halfWidth, halfHeight, ref sp1, ref points[1], ref scale);

                        var sp2 = splitPoint2[2];
                        sp2.y = Mathf.Tan(sp2.y * Mathf.Deg2Rad);
                        SplitScreenSystem.CalcStencilRange(context, halfWidth, halfHeight, ref sp2, ref points[2], ref scale);

                        context.dummyCameraConfig.backgroundColor = backGroundColor;
                    }

                    for (int i = 0; i < playerCount; ++i)
                    {
                        ref var point0 = ref points[i];
                        ref var point1 = ref points[i + 1];
                        SetStencilRange(context, point0.x, point0.y, point1.x, point1.y, i);
                        SyncPos(context, i);
                    }
                }
                else
                {
                    for (int i = 0; i < playerCount; ++i)
                    {
                        SyncPos (context, i);
                    }
                }
                SplitScreenSystem.OnUpdate (context);
                init = true;
            }
        }

        private void OnDrawGizmos ()
        {
            EngineContext context = EngineContext.instance;
            if (context != null && init)
            {
                SplitScreenSystem.OnDrawGizmo (context);
            }

        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (DummyCameraController))]
    public class DummyCameraControllerEditor : UnityEngineEditor
    {
        SerializedProperty playerCount;
        SerializedProperty backGroundColor;
        SerializedProperty uiCanvas;
        SerializedProperty uiCamera;
        private int switchIndex = -1;
        GUIContent lightColor;

        private void OnEnable ()
        {
            playerCount = serializedObject.FindProperty("playerCount");
            backGroundColor = serializedObject.FindProperty("backGroundColor");
            uiCanvas = serializedObject.FindProperty("uiCanvas");
            uiCamera = serializedObject.FindProperty("uiCamera");
            lightColor = new GUIContent ("LightColor");
           
        }
        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            DummyCameraController dcc = target as DummyCameraController;
            EditorGUI.BeginChangeCheck ();
            dcc.test = EditorGUILayout.Toggle ("Test", dcc.test);
            if (EditorGUI.EndChangeCheck ())
            {
                dcc.Clear ();
            }
            EditorGUILayout.PropertyField(playerCount);
            EditorGUILayout.PropertyField(uiCanvas);
            EditorGUILayout.PropertyField(uiCamera);
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                if (playerCount.intValue == 3)
                {
                    if (GUILayout.Button("Reset", GUILayout.MaxWidth(80)))
                    {
                        dcc.splitPoint3 = new Vector2[2]
                        {
                new Vector2 (-270, 0),
                new Vector2 (270, 0),
                            };
                    }
                    var camera = context.CameraRef;
                    var pixelWidth = camera.pixelWidth;
                    float width = pixelWidth / 2;

                    for (int i = 0; i < dcc.splitPoint3.Length; ++i)
                    {
                        ref var sp = ref dcc.splitPoint3[i];
                        sp.x = EditorGUILayout.Slider(string.Format("Offset_{0}", i.ToString()), sp.x, -width, width);
                        sp.y = EditorGUILayout.Slider(string.Format("Angle_{0}", i.ToString()), sp.y, -180, 180);
                    }
                }
                else
                {
                    if (GUILayout.Button("Reset", GUILayout.MaxWidth(80)))
                    {
                        dcc.splitPoint2 = new Vector2[3]
                        {
                        new Vector2(-812, 0),
                        new Vector2(0, 0),
                        new Vector2(812, 0),
                            };
                    }
                    var camera = context.CameraRef;
                    var pixelWidth = camera.pixelWidth;
                    float width = pixelWidth / 2;

                    for (int i = 0; i < dcc.splitPoint2.Length; ++i)
                    {
                        ref var sp = ref dcc.splitPoint2[i];
                        sp.x = EditorGUILayout.Slider(string.Format("Offset_{0}", i.ToString()), sp.x, -width, width);
                        sp.y = EditorGUILayout.Slider(string.Format("Angle_{0}", i.ToString()), sp.y, -180, 180);
                    }

                    EditorGUILayout.PropertyField(backGroundColor);
                }

            }
            int switchTarget = -1;
            for (int i = 0; i < dcc.editorCameras.Length; ++i)
            {
                ref var ec = ref dcc.editorCameras[i];
                EditorGUILayout.BeginHorizontal ();
                ec.folder = EditorGUILayout.Foldout (ec.folder, string.Format ("Camera.{0}", i, ToString ()));
                if (switchIndex == -1)
                {
                    if (GUILayout.Button ("Select", GUILayout.MaxWidth (80)))
                    {
                        switchIndex = i;
                    }
                }
                else
                {
                    if (switchIndex != i)
                    {
                        if (GUILayout.Button ("Switch", GUILayout.MaxWidth (80)))
                        {
                            switchTarget = i;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal ();

                if (ec.folder)
                {
                    ec.ct = EditorGUILayout.ObjectField ("", ec.ct, typeof (Transform), true) as Transform;
                    ec.fov = EditorGUILayout.Slider ("Fov", ec.fov, 0, 180);
                    ec.color = EditorGUILayout.ColorField ("Background", ec.color);
                    ec.light = EditorGUILayout.ObjectField ("Light", ec.light, typeof (Transform), true) as Transform;
                    ec.lightColor = EditorGUILayout.ColorField (lightColor, ec.lightColor, false, false, true);
                    ec.outlineAdd = EditorGUILayout.Slider ("OutlineAdd", ec.outlineAdd, 0, 1);
                    ec.cameraSfx = EditorGUILayout.ObjectField ("CameraSfx", ec.cameraSfx, typeof (Transform), true) as Transform;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Vector3Field ("", ec.dir);
                    EditorGUI.indentLevel--;
                }
            }
            if (switchTarget >= 0)
            {
                var dc0 = dcc.editorCameras[switchIndex];
                dcc.editorCameras[switchIndex] = dcc.editorCameras[switchTarget];
                dcc.editorCameras[switchTarget] = dc0;
                switchIndex = -1;
            }
            serializedObject.ApplyModifiedProperties ();
        }
    }
}
#endif