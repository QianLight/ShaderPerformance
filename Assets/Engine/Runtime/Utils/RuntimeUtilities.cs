using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
#endif


namespace CFEngine
{
    using UnityEngine.SceneManagement;
    using UnityObject = UnityEngine.Object;

    public static class RuntimeUtilities
    {
        #region Res        

        static Texture2D m_TransparentTexture;
        public static Texture2D transparentTex
        {
            get
            {
                if (m_TransparentTexture == null)
                {
                    m_TransparentTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false)
                    {
                    name = "_ClearTex",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Repeat,
                    anisoLevel = 0,
                    };
                    m_TransparentTexture.SetPixel (0, 0, Color.clear);
                    m_TransparentTexture.Apply ();
                }

                return m_TransparentTexture;
            }
        }

        static Mesh s_FullscreenTriangle;
        public static Mesh fullscreenTriangle
        {
            get
            {
                if (s_FullscreenTriangle != null)
                    return s_FullscreenTriangle;

                s_FullscreenTriangle = new Mesh { name = "Fullscreen Triangle" };

                // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
                // this directly in the vertex shader using vertex ids :(
                s_FullscreenTriangle.SetVertices (new List<Vector3>
                {
                    new Vector3 (-1f, -1f, 0f),
                    new Vector3 (-1f, 3f, 0f),
                    new Vector3 (3f, -1f, 0f)
                });
                s_FullscreenTriangle.SetIndices (new [] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
                s_FullscreenTriangle.UploadMeshData (true);

                return s_FullscreenTriangle;
            }
        }
        static Mesh s_IdentityQuad;
        public static Mesh IdentityQuad
        {
            get
            {
                if (s_IdentityQuad != null)
                    return s_IdentityQuad;

                s_IdentityQuad = new Mesh { name = "Identity Quad" };

                s_IdentityQuad.SetVertices (new List<Vector3>
                {
                    new Vector3 (-0.5f, -0.5f, 3),
                    new Vector3 (0.5f, -0.5f, 2),
                    new Vector3 (0.5f, 0.5f, 1),
                    new Vector3 (-0.5f, 0.5f, 0),
                });
                s_IdentityQuad.SetUVs (0, new List<Vector2>
                {
                    new Vector2 (0, 0),
                    new Vector2 (1, 0),
                    new Vector2 (1, 1),
                    new Vector2 (0, 1),
                });
                s_IdentityQuad.SetIndices (new [] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0, false);
                s_IdentityQuad.UploadMeshData (true);

                return s_IdentityQuad;
            }
        }

        static Mesh s_BillboardQuad;
        public static Mesh BillboardQuad
        {
            get
            {
                if (s_BillboardQuad != null)
                    return s_BillboardQuad;

                s_BillboardQuad = new Mesh { name = "BillboardQuad" };
                s_BillboardQuad.SetVertices(new List<Vector3>
                {
                    new Vector3 (-0.9f, -1.3f,0),
                    new Vector3 (0.9f, -1.3f, 0),
                    new Vector3 (0.9f, 0.8f, 0),
                    new Vector3 (-0.9f, 0.8f, 0),
                });
                s_BillboardQuad.SetUVs(0, new List<Vector2>
                {
                    new Vector2 (0.1f, 0),
                    new Vector2 (0.9f, 0),
                    new Vector2 (0.9f, 1.0f),
                    new Vector2 (0.1f, 1.0f),
                });
                s_BillboardQuad.SetIndices(new[] { 3, 2, 1, 1, 0, 3 }, MeshTopology.Triangles, 0, false);
                s_BillboardQuad.UploadMeshData(false);
             
                return s_BillboardQuad;
            }
        }




        static Mesh s_viewPortQuad;
        static List<Vector3> s_viewPortVec;
        public static Mesh GetScreenMesh (Rect viewPort)
        {
            if (s_viewPortQuad == null)
            {
                s_viewPortQuad = new Mesh { name = "viewPort Triangle" };
                s_viewPortQuad.MarkDynamic ();
                s_viewPortQuad.SetIndices (new [] { 0, 1, 3, 1, 2, 3 }, MeshTopology.Triangles, 0, false);
                s_viewPortQuad.SetUVs (0, new List<Vector2>
                {
                    new Vector2 (0, 0),
                    new Vector2 (0, 1),
                    new Vector2 (1, 1),
                    new Vector2 (1, 0)
                });
                s_viewPortVec = new List<Vector3> (4);
                s_viewPortVec.Add (Vector3.zero);
                s_viewPortVec.Add (Vector3.zero);
                s_viewPortVec.Add (Vector3.zero);
                s_viewPortVec.Add (Vector3.zero);
            }
            s_viewPortVec[0] = new Vector3 (viewPort.xMin, viewPort.yMin, 0f);
            s_viewPortVec[1] = new Vector3 (viewPort.xMin, viewPort.yMax, 0f);
            s_viewPortVec[2] = new Vector3 (viewPort.xMax, viewPort.yMax, 0f);
            s_viewPortVec[3] = new Vector3 (viewPort.xMax, viewPort.yMin, 0f);
            s_viewPortQuad.SetVertices (s_viewPortVec);

            return s_viewPortQuad;
        }

        static Material s_CopyMaterial;
        public static Material GetCopyMaterial ()
        {
            return s_CopyMaterial;
        }
        public static void SetCopyMaterial (Shader shader)
        {
            if (s_CopyMaterial == null)
            {
                s_CopyMaterial = new Material (shader)
                {
                name = "PostProcess - Copy",
                hideFlags = HideFlags.HideAndDontSave
                };
            }
        }
        static Material s_DebugMat;
        static AssetHandler debugAH;
        public static Material GetDebugMat ()
        {
            if(s_DebugMat==null)
            {
                LoadMgr.GetAssetHandler(ref debugAH, "Config/Debug_ShadowMap_2DArray", ResObject.ResExt_Mat);
                EngineUtility.LoadAsset(debugAH, ResObject.ResExt_Mat);
                s_DebugMat = debugAH.obj as Material;
            }

            return s_DebugMat;
        }
        #endregion

        #region Rendering
        public static void CommitCmd (ref ScriptableRenderContext context, CommandBuffer cmd, bool clear = true)
        {
            context.ExecuteCommandBuffer (cmd);
            if (clear)
                cmd.Clear ();
        }

        public static void ClearRenderTarget (CommandBuffer cmd, ClearFlag clearFlag, ref Color clearColor)
        {
            if (clearFlag != ClearFlag.None)
                cmd.ClearRenderTarget ((clearFlag & ClearFlag.Depth) != 0, (clearFlag & ClearFlag.Color) != 0, clearColor);
        }

        public static void SetRenderTarget (
            CommandBuffer cmd,
            ref RenderTargetBinding rtBind,
            ClearFlag clearFlag, Color clearColor)
        {
            cmd.SetRenderTarget (rtBind);
            ClearRenderTarget (cmd, clearFlag, ref clearColor);
        }

        public static void BlitFullscreenTriangle (
            this CommandBuffer cmd,
            ref RenderTargetIdentifier source,
            ref RenderTargetIdentifier destination,
            PropertySheet propertySheet, int pass)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        }

        public static void CopyRT (
            this CommandBuffer cmd,
            ref RenderTargetIdentifier source,
            ref RenderTargetIdentifier destination, int pass = 0)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, GetCopyMaterial (), 0, pass);
        }
        public static void CopyRT(
            this CommandBuffer cmd,
            ref RenderTargetIdentifier source,
            RenderTexture destination, int pass = 0)
        {
            cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
            cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, GetCopyMaterial(), 0, pass);
        }

        public static void CopyRT(
            this CommandBuffer cmd,
            RenderTexture source,
            ref RenderTargetIdentifier destination, int pass = 0)
        {
            cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
            cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, GetCopyMaterial(), 0, pass);
        }
        public static void BlitFullscreenTriangle(
            this CommandBuffer cmd,
            ref RenderTargetIdentifier source,
            ref RenderTargetBinding rtBinding,
            PropertySheet propertySheet, int pass)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (rtBinding);
            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        }
        public static void BlitFullscreenTriangle (
            this CommandBuffer cmd,
            RenderTexture source,
            ref RenderTargetIdentifier destination,
            Material mat, int pass)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, mat, 0, pass);
        }

        public static void BlitFullscreenTriangle (
            this CommandBuffer cmd,
            ref RenderTargetIdentifier source,
            RenderTexture destination,
            Material mat, int pass)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, mat, 0, pass);
        }
        public static void BlitFullscreenTriangle (
            this CommandBuffer cmd,
            RenderTexture source,
            ref RenderTargetIdentifier destination,
            PropertySheet propertySheet, int pass)
        {
            if (source != null)
                cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        }

        public static void BlitFullscreenTriangle (
            this CommandBuffer cmd,
            ref RenderTargetIdentifier source,
            RenderTexture destination,
            PropertySheet propertySheet, int pass)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
            cmd.SetRenderTarget (destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

            cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        }

        public static void DebugCmd (
            this CommandBuffer cmd)
        {
            DebugLog.AddEngineLog2("cmd size:{0}",cmd.sizeInBytes.ToString());
        }
        //=====================================================================

        // public static void BlitFullscreenTriangle (
        //     this CommandBuffer cmd,
        //     Texture2D source,
        //     RenderTexture destination,
        //     PropertySheet propertySheet, int pass, bool clear = false)
        // {
        //     if (source != null)
        //         cmd.SetGlobalTexture (ShaderIDs.MainTex, source);
        //     cmd.SetRenderTarget (destination);

        //     if (clear)
        //         cmd.ClearRenderTarget (true, true, Color.clear);

        //     cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        // }

        public static void EnableKeyword (string keyword, bool isEnable)
        {
            if (isEnable)
            {
                Shader.EnableKeyword (keyword);
            }
            else
            {
                Shader.DisableKeyword (keyword);
            }
        }

        #endregion

        #region Unity specifics & misc methods
        public static float Luminance (in Color color) => color.r * 0.2126729f + color.g * 0.7151522f + color.b * 0.072175f;
        public static RenderTextureFormat defaultHDRRenderTextureFormat
        {
            get
            {
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_TVOS || UNITY_SWITCH || UNITY_EDITOR
                RenderTextureFormat format = RenderTextureFormat.RGB111110Float;
#if UNITY_EDITOR
                var target = EditorUserBuildSettings.activeBuildTarget;
                if (target != BuildTarget.Android && target != BuildTarget.iOS && target != BuildTarget.tvOS && target != BuildTarget.Switch)
                    return RenderTextureFormat.ARGBHalf;
#endif // UNITY_EDITOR
                if (format.IsSupported ())
                    return format;
#endif // UNITY_ANDROID || UNITY_IPHONE || UNITY_TVOS || UNITY_SWITCH || UNITY_EDITOR
                return RenderTextureFormat.ARGB32;
            }
        }
        public static bool isLinearColorSpace
        {
            get { return QualitySettings.activeColorSpace == ColorSpace.Linear; }
        }

        public static bool isFloatingPointFormat (RenderTextureFormat format)
        {
            return format == RenderTextureFormat.DefaultHDR || format == RenderTextureFormat.ARGBHalf || format == RenderTextureFormat.ARGBFloat ||
                format == RenderTextureFormat.RGFloat || format == RenderTextureFormat.RGHalf ||
                format == RenderTextureFormat.RFloat || format == RenderTextureFormat.RHalf ||
                format == RenderTextureFormat.RGB111110Float;
        }

        public static void CreateIfNull<T> (ref T obj)
        where T : class, new ()
        {
            if (obj == null)
                obj = new T ();
        }
        public static void SetLightInfo (LightingParam li, int dirKey, int colorKey, float w, bool setLightColor = true)
        {
            Shader.SetGlobalVector (dirKey, li.value.lightDir);
            if (setLightColor)
            {
                Vector4 lightColorIntensity;
                {
                    // lightColorIntensity = new Vector4 (
                    //     Mathf.Pow (li.value.lightColor.r * li.value.lightDir.w, 2.2f),
                    //     Mathf.Pow (li.value.lightColor.g * li.value.lightDir.w, 2.2f),
                    //     Mathf.Pow (li.value.lightColor.b * li.value.lightDir.w, 2.2f), w);
                    lightColorIntensity = new Vector4 (
                        li.value.lightColor.r * li.value.lightDir.w,
                        li.value.lightColor.g * li.value.lightDir.w,
                        li.value.lightColor.b * li.value.lightDir.w, w);
                }
                Shader.SetGlobalVector (colorKey, lightColorIntensity);
            }
        }

        [Conditional ("UNITY_EDITOR")]
        public static void BeginProfile (CommandBuffer cmd, string name)
        {
#if UNITY_EDITOR
            cmd.BeginSample (name);
#endif
        }

        [Conditional ("UNITY_EDITOR")]
        public static void EndProfile (CommandBuffer cmd, string name)
        {
#if UNITY_EDITOR
            cmd.EndSample (name);
#endif
        }

        #endregion

        #region Maths

        public static float Exp2 (float x)
        {
            return Mathf.Exp (x * 0.69314718055994530941723212145818f);
        }
        // public static float MatrixDelta(Matrix4x4 mat0, Matrix4x4 mat1)
        // {
        //     float delta = mat0.m00 - mat1.m00;
        //     delta += mat0.m33 - mat1.m33;
        //     delta += mat0.m23 - mat1.m23;
        //     delta += mat0.m13 - mat1.m13;
        //     delta += mat0.m03 - mat1.m03;
        //     delta += mat0.m32 - mat1.m32;
        //     delta += mat0.m22 - mat1.m22;
        //     delta += mat0.m02 - mat1.m02;
        //     delta += mat0.m12 - mat1.m12;
        //     delta += mat0.m21 - mat1.m21;
        //     delta += mat0.m11 - mat1.m11;
        //     delta += mat0.m01 - mat1.m01;
        //     delta += mat0.m30 - mat1.m30;
        //     delta += mat0.m20 - mat1.m20;
        //     delta += mat0.m10 - mat1.m10;
        //     delta += mat0.m31 - mat1.m31;
        //     return delta;
        // }
        //        // Adapted heavily from PlayDead's TAA code
        //        // https://github.com/playdeadgames/temporal/blob/master/Assets/Scripts/Extensions.cs
        //        public static Matrix4x4 GetJitteredPerspectiveProjectionMatrix(Camera camera, Vector2 offset)
        //        {
        //            float vertical = Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView);
        //            float horizontal = vertical * camera.aspect;
        //            float near = camera.nearClipPlane;
        //            float far = camera.farClipPlane;

        //            offset.x *= horizontal / (0.5f * camera.pixelWidth);
        //            offset.y *= vertical / (0.5f * camera.pixelHeight);

        //            float left = (offset.x - horizontal) * near;
        //            float right = (offset.x + horizontal) * near;
        //            float top = (offset.y + vertical) * near;
        //            float bottom = (offset.y - vertical) * near;

        //            var matrix = new Matrix4x4();

        //            matrix[0, 0] = (2f * near) / (right - left);
        //            matrix[0, 1] = 0f;
        //            matrix[0, 2] = (right + left) / (right - left);
        //            matrix[0, 3] = 0f;

        //            matrix[1, 0] = 0f;
        //            matrix[1, 1] = (2f * near) / (top - bottom);
        //            matrix[1, 2] = (top + bottom) / (top - bottom);
        //            matrix[1, 3] = 0f;

        //            matrix[2, 0] = 0f;
        //            matrix[2, 1] = 0f;
        //            matrix[2, 2] = -(far + near) / (far - near);
        //            matrix[2, 3] = -(2f * far * near) / (far - near);

        //            matrix[3, 0] = 0f;
        //            matrix[3, 1] = 0f;
        //            matrix[3, 2] = -1f;
        //            matrix[3, 3] = 0f;

        //            return matrix;
        //        }

        //        public static Matrix4x4 GetJitteredOrthographicProjectionMatrix(Camera camera, Vector2 offset)
        //        {
        //            float vertical = camera.orthographicSize;
        //            float horizontal = vertical * camera.aspect;

        //            offset.x *= horizontal / (0.5f * camera.pixelWidth);
        //            offset.y *= vertical / (0.5f * camera.pixelHeight);

        //            float left = offset.x - horizontal;
        //            float right = offset.x + horizontal;
        //            float top = offset.y + vertical;
        //            float bottom = offset.y - vertical;

        //            return Matrix4x4.Ortho(left, right, bottom, top, camera.nearClipPlane, camera.farClipPlane);
        //        }

        //        public static Matrix4x4 GenerateJitteredProjectionMatrixFromOriginal(RenderContext context, Matrix4x4 origProj, Vector2 jitter)
        //        {
        //#if UNITY_2017_2_OR_NEWER
        //            var planes = origProj.decomposeProjection;

        //            float vertFov = Math.Abs(planes.top) + Math.Abs(planes.bottom);
        //            float horizFov = Math.Abs(planes.left) + Math.Abs(planes.right);

        //            var planeJitter = new Vector2(jitter.x * horizFov / context.screenWidth,
        //                                          jitter.y * vertFov / context.screenHeight);

        //            planes.left += planeJitter.x;
        //            planes.right += planeJitter.x;
        //            planes.top += planeJitter.y;
        //            planes.bottom += planeJitter.y;

        //            var jitteredMatrix = Matrix4x4.Frustum(planes);

        //            return jitteredMatrix;
        //#else
        //            var rTan = (1.0f + origProj[0, 2]) / origProj[0, 0];
        //            var lTan = (-1.0f + origProj[0, 2]) / origProj[0, 0];

        //            var tTan = (1.0f + origProj[1, 2]) / origProj[1, 1];
        //            var bTan = (-1.0f + origProj[1, 2]) / origProj[1, 1];

        //            float tanVertFov = Math.Abs(tTan) + Math.Abs(bTan);
        //            float tanHorizFov = Math.Abs(lTan) + Math.Abs(rTan);

        //            jitter.x *= tanHorizFov / context.screenWidth;
        //            jitter.y *= tanVertFov / context.screenHeight;

        //            float left = jitter.x + lTan;
        //            float right = jitter.x + rTan;
        //            float top = jitter.y + tTan;
        //            float bottom = jitter.y + bTan;

        //            var jitteredMatrix = new Matrix4x4();

        //            jitteredMatrix[0, 0] = 2f / (right - left);
        //            jitteredMatrix[0, 1] = 0f;
        //            jitteredMatrix[0, 2] = (right + left) / (right - left);
        //            jitteredMatrix[0, 3] = 0f;

        //            jitteredMatrix[1, 0] = 0f;
        //            jitteredMatrix[1, 1] = 2f / (top - bottom);
        //            jitteredMatrix[1, 2] = (top + bottom) / (top - bottom);
        //            jitteredMatrix[1, 3] = 0f;

        //            jitteredMatrix[2, 0] = 0f;
        //            jitteredMatrix[2, 1] = 0f;
        //            jitteredMatrix[2, 2] = origProj[2, 2];
        //            jitteredMatrix[2, 3] = origProj[2, 3];

        //            jitteredMatrix[3, 0] = 0f;
        //            jitteredMatrix[3, 1] = 0f;
        //            jitteredMatrix[3, 2] = -1f;
        //            jitteredMatrix[3, 3] = 0f;

        //            return jitteredMatrix;
        //#endif
        //        }

        #endregion

#if UNITY_EDITOR
        // Quick extension method to get the first attribute of type T on a given Type
        public static T GetAttribute<T> (this Type type) where T : Attribute
        {
            Assert.IsTrue (type.IsDefined (typeof (T), false), "Attribute not found");
            return (T) type.GetCustomAttributes (typeof (T), false) [0];
        }

        // Returns all attributes set on a specific member
        // Note: doesn't include inherited attributes, only explicit ones
        public static Attribute[] GetMemberAttributes<TType, TValue> (Expression<Func<TType, TValue>> expr)
        {
            Expression body = expr;

            if (body is LambdaExpression)
                body = ((LambdaExpression) body).Body;

            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var fi = (FieldInfo) ((MemberExpression) body).Member;
                    return fi.GetCustomAttributes (false).Cast<Attribute> ().ToArray ();
                default:
                    throw new InvalidOperationException ();
            }
        }

        // Returns a string path from an expression - mostly used to retrieve serialized properties
        // without hardcoding the field path. Safer, and allows for proper refactoring.
        public static string GetFieldPath<TType, TValue> (Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    me = expr.Body as MemberExpression;
                    break;
                default:
                    throw new InvalidOperationException ();
            }

            var members = new List<string> ();
            while (me != null)
            {
                members.Add (me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = new StringBuilder ();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append (members[i]);
                if (i > 0) sb.Append ('.');
            }

            return sb.ToString ();
        }

        public static object GetParentObject (string path, object obj)
        {
            var fields = path.Split ('.');

            if (fields.Length == 1)
                return obj;

            var info = obj.GetType ().GetField (fields[0], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            obj = info.GetValue (obj);

            return GetParentObject (string.Join (".", fields, 1, fields.Length - 1), obj);
        }

        public static Light CreateLight (string name, ref LightInfo li, int mask, bool create, Transform parent = null)
        {
            GameObject go = null;
            if (parent != null)
            {
                var t = parent.Find (name);
                if (t != null)
                {
                    go = t.gameObject;
                }
            }
            else
            {
                go = GameObject.Find (name);
            }

            if (go == null)
            {
                if (!create)
                    return null;
                go = new GameObject (name);
            }
            Light l = go.GetComponent<Light> ();
            if (l == null)
            {
                l = go.AddComponent<Light> ();
                l.transform.rotation = Quaternion.LookRotation (-li.lightDir);
                l.type = LightType.Directional;
                l.color = li.lightColor;
                l.intensity = li.lightDir.w;
                GameObject dummyLightGo = GameObject.Find ("DummyLights");
                if (dummyLightGo == null)
                {
                    dummyLightGo = new GameObject ("DummyLights");
                }
                l.cullingMask = mask;
                go.transform.parent = dummyLightGo.transform;
            }

            return l;
        }

        public static void PrepareLight (ref LightInfo lightInfo, string name, bool create, int mask = 0)
        {
            var lightWrapper = lightInfo.lightWrapper;
            if (lightWrapper.light == null)
            {
                lightWrapper.light = CreateLight (name, ref lightInfo, mask, create);
            }
            else
            {
                lightWrapper.light.gameObject.name = name;
            }
            if (lightWrapper.light != null)
            {
                //lightWrapper.lightRot = EditorCommon.GetTransformRotatGUI (lightWrapper.light.transform);
                if (lightWrapper.rotGui == null)
                {
                    lightWrapper.rotGui = new RotGUI ();
                    lightWrapper.rotGui.OnInit (lightWrapper.light.transform);
                }
                else if (lightWrapper.rotGui.tran != lightWrapper.light.transform)
                {
                    lightWrapper.rotGui.OnInit (lightWrapper.light.transform);
                }
            }

        }
        public static void DrawLightHandle (Transform lightTrans, ref Vector3 pos, float intensity, Color c, string text)
        {
            EditorGUI.BeginChangeCheck ();
            Transform lt = lightTrans.transform;
            Quaternion rot = Handles.RotationHandle (lt.rotation, pos);
            if (EditorGUI.EndChangeCheck ())
            {
                Undo.RecordObject (lt, text);
                lt.rotation = rot;
            }

            Handles.color = c;
            Handles.ArrowHandleCap (100, pos, rot, intensity, EventType.Repaint);
            Handles.Label (pos, text);
        }

        public static void DrawLightHandle (Transform t, ref Vector3 centerPos, float right, float up, LightingParam param, string text)
        {
            var wrapper = param.value.lightWrapper;
            if (wrapper.light != null)
            {
                Vector3 pos = centerPos + t.right * right + t.up * up;
                float intensity = wrapper.light.intensity;
                if (intensity < 0.1f)
                    intensity = 0.1f;
                DrawLightHandle (wrapper.light.transform, ref pos, intensity, wrapper.light.color, text);
            }
        }

        public static void DrawLightHandle (Transform t, ref Vector3 centerPos, ref Vector3 dir, ref Color color, float right, float up, LightingParam param, string text)
        {
            var wrapper = param.value.lightWrapper;
            if (wrapper != null && wrapper.light != null)
            {
                Vector3 pos = centerPos + t.right * right + t.up * up;
                DrawLightHandle (wrapper.light.transform, ref pos, wrapper.light.intensity, wrapper.light.color, text);
                Handles.color = color;
                Quaternion rot = Quaternion.LookRotation (-dir);
                Handles.ArrowHandleCap (100, pos, rot, color.maxColorComponent, EventType.Repaint);
            }
        }

        public static void DrawLightHandle (Transform t, ref Vector3 centerPos, float right, float up, ref Color color, Transform lightTrans, string text)
        {
            if (lightTrans != null)
            {
                Vector3 pos = centerPos + t.right * right + t.up * up;
                DrawLightHandle (lightTrans, ref pos, 1, color, text);
            }
        }

        public static void DrawScreenArrow (ref Quaternion dir, float right, float up, float l, string text)
        {
            if (SceneView.lastActiveSceneView != null &&
                SceneView.lastActiveSceneView.camera != null)
            {
                Transform t = SceneView.lastActiveSceneView.camera.transform;
                Vector3 pos = t.position + t.forward * 10 + t.right * right + t.up * up;
                Handles.ArrowHandleCap (101, pos, dir, l, EventType.Repaint);
                Handles.Label (pos, text);
            }
        }

        public static void DrawModelSpaceLightHandle (Transform t, ref Vector3 centerPos, ref Vector4 rot, float xOffset, float yOffset, string text)
        {
            if (t != null)
            {
                Vector3 pos = centerPos + t.right * xOffset + t.up * yOffset;
                EditorGUI.BeginChangeCheck ();
                Quaternion q = new Quaternion (rot.x, rot.y, rot.z, rot.w);
                Quaternion r = Handles.RotationHandle (q, pos);
                if (EditorGUI.EndChangeCheck ())
                {
                    Undo.RecordObject (t, text);
                    rot.x = r.x;
                    rot.y = r.y;
                    rot.z = r.z;
                    rot.w = r.w;
                }

                Handles.color = Color.yellow;
                Handles.ArrowHandleCap (100, pos, r, 0.5f, EventType.Repaint);
                Handles.Label (pos, text);
            }
        }
        public static void SyncLight (ref LightInfo li)
        {
            if (li.lightWrapper != null)
            {
                Light l = li.lightWrapper.light;
                if (l != null)
                {
                    li.lightDir = l.transform.rotation * -Vector3.forward;
                    li.lightColor = l.color;
                    li.lightDir.w = (l.enabled && l.gameObject.activeInHierarchy) ? l.intensity : 0;
                }
            }
        }
        

        static Dictionary<string, ParamOverride> paramCache = new Dictionary<string, ParamOverride> ();
        public static ParamOverride GetParam (SerializedProperty sp)
        {
            EnvSetting setting = sp.serializedObject.targetObject as EnvSetting;
            if (setting != null)
            {
                ParamOverride po;
                string key = string.Format ("{0}_{1}", setting.GetEnvType (), sp.propertyPath);
                if (!paramCache.TryGetValue (key, out po))
                {
                    FieldInfo fi = sp.serializedObject.targetObject.GetType ().GetField (sp.propertyPath);
                    po = fi.GetValue (sp.serializedObject.targetObject) as ParamOverride;
                    paramCache[key] = po;
                }
                return po;
            }
            return null;
        }
        public static RuntimeParamOverride GetRuntimeParam (SerializedProperty sp, out EnvParam envParam)
        {
            envParam = null;
            EnvSetting setting = sp.serializedObject.targetObject as EnvSetting;
            if (setting != null)
            {
                string settingName = setting.GetType ().Name;
                string fieldPath = sp.propertyPath;
                uint paramHash = EngineUtility.XHashLowerRelpaceDot (0, settingName);
                uint hash = EngineUtility.XHashLowerRelpaceDot (paramHash, fieldPath);

                if (EnvSetting.paramIndex.TryGetValue (hash, out RuntimeParamOverride runtimeParam))
                {
                    envParam = runtimeParam.activeEnvParam;
                    return runtimeParam;
                }
            }
            return null;
        }
        public static void DrawInstpectorRT (RenderTexture rt, Material srcMat, ref Material mat, float size = 256)
        {
            Texture tex = Texture2D.blackTexture;
            if (rt != null)
                tex = rt;
            float w = size;
            float h = size;
            if (size == -1)
            {
                size = tex.height;
                w = tex.width;
                h = tex.height;
            }
            GUILayout.Space (size + 20);
            Rect r = GUILayoutUtility.GetLastRect ();
            r.y += 10;
            r.width = w;
            r.height = h;
            if (mat == null)
            {
                mat = new Material (srcMat);
            }
            EditorGUI.DrawPreviewTexture (r, tex, mat);
        }
        public static void DrawInstpectorTex (Texture mainTex, Shader srcShader, ref Material mat, float size = 256)
        {
            Texture tex = Texture2D.blackTexture;
            if (mainTex != null)
                tex = mainTex;
            float w = size;
            float h = size;
            if (size == -1)
            {
                size = tex.height;
                w = tex.width;
                h = tex.height;
            }
            GUILayout.Space (size + 20);
            Rect r = GUILayoutUtility.GetLastRect ();
            r.y += 10;
            r.width = w;
            r.height = h;
            if (mat == null)
            {
                mat = new Material (srcShader);
            }
            if (mat.HasProperty (ShaderIDs.MainTex))
            {
                mat.SetTexture (ShaderIDs.MainTex, tex);
            }
            EditorGUI.DrawPreviewTexture (r, tex, mat);
        }

        public static bool BindLightmap (MaterialPropertyBlock mpb, LigthmapComponent lc)
        {
            if (mpb != null)
            {
                Texture2D colorMap;
                if (lc.ligthmapRes.colorCombineShadowMask != null && LightmapCombineManager.Instance.CheckIsUseCombineLightmap())
                {
                    colorMap = lc.ligthmapRes.colorCombineShadowMask;
                }
                else
                {
                    colorMap = lc.ligthmapRes.color;
                }
                
                Texture2D shadowMask = lc.ligthmapRes.shadowMask;
                Texture2D dirMap = lc.ligthmapRes.dir;
                float shadowMaskLerp = 1;
                if (shadowMask == null)
                {
                    shadowMask = Texture2D.whiteTexture;
                }
                if (dirMap == null)
                {
                    dirMap = Texture2D.whiteTexture;
                }
                if (colorMap != null)
                {
                    mpb.SetFloat ("shadowMaskLerp", shadowMaskLerp);
                    mpb.SetTexture (ShaderManager._CustomLightmap, colorMap);
                    mpb.SetTexture(ShaderManager._CustomShadowMask, shadowMask);
                    // mpb.SetTexture (ShaderManager._ShaderKeyShadowMask, shadowMask);
                    //mpb.SetTexture (ShaderManager._ShaderKeyLightmapDir, dirMap);
                    mpb.SetTexture ("unity_LightmapInd", dirMap);
                    mpb.SetVector (ShaderManager._LightMapUVST, lc.lightmapUVST);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static void OnLightmapInspectorGUI (LigthmapComponent lc, SerializedProperty lightmapScale)
        {
            // EditorGUILayout.LabelField ("Lightmap", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField (lightmapScale);
            EditorGUILayout.IntField ("Index", lc.lightMapIndex);
            EditorGUILayout.Vector4Field ("UVST", lc.lightmapUVST);
            lc.ligthmapRes.color= EditorGUILayout.ObjectField ("Color", lc.ligthmapRes.color, typeof (Texture2D), false) as Texture2D;
            lc.ligthmapRes.shadowMask = EditorGUILayout.ObjectField("ShadowMask", lc.ligthmapRes.shadowMask, typeof(Texture2D), false) as Texture2D;
            lc.ligthmapRes.colorCombineShadowMask = EditorGUILayout.ObjectField("ColorCombineShadowMask", lc.ligthmapRes.colorCombineShadowMask, typeof(Texture2D), false) as Texture2D;
        }

        // public static void BindDynamicLights (Transform lightsParent,
        //     ref List<ISFXHandler> lights)
        // {
        //     if (AssetsConfig.instance.dyanmicLights == null)
        //         return;
        //     GameObject lightGo = null;
        //     if (lightsParent != null)
        //     {
        //         Transform lightTrans = lightsParent.Find ("DynamicLights");
        //         if (lightTrans == null)
        //         {
        //             lightGo = GameObject.Instantiate (AssetsConfig.instance.dyanmicLights);
        //             lightGo.name = "DynamicLights";
        //             lightTrans = lightGo.transform;
        //             lightTrans.parent = lightsParent;
        //         }
        //         else
        //         {
        //             lightGo = lightTrans.gameObject;
        //         }

        //     }
        //     else
        //     {
        //         lightGo = GameObject.Find ("GlobalDynamicLights");
        //         if (lightGo == null)
        //         {
        //             lightGo = GameObject.Instantiate (AssetsConfig.instance.dyanmicLights);
        //             lightGo.name = "GlobalDynamicLights";
        //         }
        //         Transform lightTrans = lightGo.transform;
        //         lightTrans.parent = lightsParent;
        //     }
        //     if (lights == null)
        //     {
        //         lights = new List<ISFXHandler> ();
        //     }
        //     for (int i = 0; i < lights.Count; ++i)
        //     {
        //         var sfx = lights[i];
        //         sfx.OnStop ();
        //     }
        //     lights.Clear ();
        //     var monos = EngineUtility.GetScripts<MonoBehaviour> (lightGo);
        //     for (int i = 0; i < monos.Count; ++i)
        //     {
        //         var mono = monos[i];
        //         if (mono is ISFXHandler)
        //         {
        //             var sfx = mono as ISFXHandler;
        //             sfx.Owner = null;
        //             sfx.OnStart ();
        //             lights.Add (sfx);
        //         }
        //     }
        // }

        // public static void RefreshLights (bool addLight, int pointLightCount,
        //     List<ISFXHandler> lights, ref Vector4[] lightData)
        // {
        //     int lightCount = 0;
        //     int lightLength = lightData.Length / 2;
        //     if (lights == null)
        //         return;
        //     for (int i = 0; i < lightData.Length; ++i)
        //     {
        //         lightData[i] = Vector4.zero;
        //     }
        //     for (int i = 0; i < lights.Count && i < lightLength; ++i)
        //     {
        //         var light = lights[i];
        //         if (light is DynamicCSLightRender && lightCount < pointLightCount)
        //         {
        //             var pointLight = light as DynamicCSLightRender;

        //             Vector4 posWithBias = pointLight.transform.position;
        //             posWithBias.w = pointLight.rangeBias;

        //             float oneOverLightRangeSqr = 1.0f / Mathf.Max (0.0001f, pointLight.range * pointLight.range);
        //             Vector4 lightColorIntensity = new Vector4 (
        //                 Mathf.Pow (pointLight.color.r * pointLight.intensity, 2.2f),
        //                 Mathf.Pow (pointLight.color.g * pointLight.intensity, 2.2f),
        //                 Mathf.Pow (pointLight.color.b * pointLight.intensity, 2.2f), oneOverLightRangeSqr);
        //             int lightIndex = lightCount + 1;
        //             ref var lightPos = ref lightData[lightIndex * 2];
        //             ref var lightColor = ref lightData[lightIndex * 2 + 1];
        //             lightPos = posWithBias;
        //             lightColor = lightColorIntensity;
        //             lightCount++;
        //         }
        //         else if (light is DynamicDirectionLight && addLight)
        //         {
        //             var dirLight = light as DynamicDirectionLight;

        //             Vector4 dir = -dirLight.transform.forward;
        //             dir.w = dirLight.IsEnable () ? 1 : 0;

        //             Vector4 lightColorIntensity = new Vector4 (
        //                 Mathf.Pow (dirLight.lightColor.r * dirLight.intensity, 2.2f),
        //                 Mathf.Pow (dirLight.lightColor.g * dirLight.intensity, 2.2f),
        //                 Mathf.Pow (dirLight.lightColor.b * dirLight.intensity, 2.2f), 0);
        //             ref var lightPos = ref lightData[0];
        //             ref var lightColor = ref lightData[1];
        //             lightPos = dir;
        //             lightColor = lightColorIntensity;
        //         }
        //     }
        // }

        public static void DrawPlane (Vector3 pos, Vector3 normal, Vector3 up, float width = 10, float height = 5)
        {
            Vector3 right = Vector3.Cross (up, normal);
            Vector3 p0 = -width * right - height * up + pos;
            Vector3 p1 = width * right - height * up + pos;
            Vector3 p2 = width * right + height * up + pos;
            Vector3 p3 = -width * right + height * up + pos;

            Gizmos.DrawLine (p0, p1);
            Gizmos.DrawLine (p1, p2);
            Gizmos.DrawLine (p2, p3);
            Gizmos.DrawLine (p3, p0);

        }

        static Dictionary<string, GUIContent> s_GUIContentCache = new Dictionary<string, GUIContent> ();

        public static GUIContent GetContent (string textAndTooltip)
        {
            if (string.IsNullOrEmpty (textAndTooltip))
                return GUIContent.none;

            GUIContent content;

            if (!s_GUIContentCache.TryGetValue (textAndTooltip, out content))
            {
                var s = textAndTooltip.Split ('|');
                content = new GUIContent (s[0]);

                if (s.Length > 1 && !string.IsNullOrEmpty (s[1]))
                    content.tooltip = s[1];

                s_GUIContentCache.Add (textAndTooltip, content);
            }

            return content;
        }
        public static float DistSqr (float x0, float y0, float x1, float y1)
        {
            float deltaX = x0 - x1;
            float deltaY = y0 - y1;
            return deltaX * deltaX + deltaY * deltaY;
        }

        public static bool TestCircleRect (float x, float z, float radius, ref Vector2 min, ref Vector2 max)
        {
            float left = min.x - radius;
            float right = max.x + radius;
            float top = max.y + radius;
            float bottom = min.y - radius;
            if (x < left || x > right || z > top || z < bottom)
            {
                return false;
            }
            float r2 = radius * radius;
            if (x <= min.x && z <= min.y && DistSqr (x, z, min.x, min.y) >= r2 ||
                x >= max.x && z <= min.y && DistSqr (x, z, max.x, min.y) >= r2 ||
                x >= max.x && z >= max.y && DistSqr (x, z, max.x, max.y) >= r2 ||
                x <= min.x && z >= max.y && DistSqr (x, z, min.x, max.y) >= r2)
            {
                return false;
            }
            return true;
        }
#endif

    }
}