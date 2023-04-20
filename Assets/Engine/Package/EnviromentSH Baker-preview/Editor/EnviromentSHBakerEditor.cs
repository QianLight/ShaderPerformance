// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Experimental.Rendering;
// using UnityEngine.Rendering;

// namespace CFEngine.Editor
// {
//     public class EnviromentSHBakerEditor : EditorWindow
//     {
//         const int WIDTH = 128;
//         string[] options = { "SkyBox", "Trilight", "Flat" };
//         const string PATH_PREFIX = "Engine/Package/EnviromentSH Baker-preview/";
//         const string PATH_PREFIX_FULL = "Assets/Engine/Package/EnviromentSH Baker-preview/";

//         private string fileName;
//         private int mode;
//         private Texture cubeMap;
//         private float ambientIntensity = 1f;
//         private Color ambientColor;
//         private Color skyColor;
//         private Color equatorColor;
//         private Color groundColor;
//         private Cubemap cube;
//         private Mesh mPreviewMesh;

//         private Texture start_cubeMap;
//         private float start_ambientIntensity;
//         private Color start_ambientColor;
//         private Color start_skyColor;
//         private Color start_equatorColor;
//         private Color start_groundColor;

//         private PreviewRenderUtility previewUtility;
//         private Vector2 previewDir = new Vector2 (-10, -10);
//         private Material previewMat;
//         private MaterialPropertyBlock materialBlock;
//         private string previewTip;
//         private float[] previewCoeffs;
//         private Color[] faces;
//         private EnviromentSHBakerHelper helper = null;
//         private bool previewPrimitiveCube = false;

//         private GUIContent mModeContent = new GUIContent ("Source", "Specifies the material that is used to simulate the sky or other distant background in the Scene.");
//         public GUIContent mIntensityContent = new GUIContent ("Intensity Multiplier", "Controls the brightness of the skybox lighting in the Scene.");
//         private GUIContent mfileNameContent = new GUIContent ("FileName", "path for save");
//         private GUIContent mSkyColorContent = new GUIContent ("SkyColor", "SkyColor");
//         private GUIContent mEquatorColorContent = new GUIContent ("EquatorColor", "EquatorColor");
//         private GUIContent mGroundColorContent = new GUIContent ("GroundColor", "GroundColor");
//         private GUIContent mAmbientColorContent = new GUIContent ("AmbientColor", "AmbientColor");

//         [MenuItem (@"Test/EnviromentSH Baker")]
//         static void Init ()
//         {
//             EnviromentSHBakerEditor win = EditorWindow.GetWindow (typeof (EnviromentSHBakerEditor)) as EnviromentSHBakerEditor;
//             win.titleContent = new GUIContent ("EnviromentSH Baker");
//             win.minSize = new Vector2 (250, 400);
//             win.Show ();
//         }

//         public void OnEnable ()
//         {
//             helper = EnviromentSHBakerHelper.singleton;
//             helper.Init(AssetsConfig.instance.shBaker);
//             previewMat = (Material)AssetDatabase.LoadAssetAtPath<Material>(PATH_PREFIX_FULL + "Shader/DrawSH.mat");
//             fileName = PATH_PREFIX + "SH Cache/file temp";
//             materialBlock = new MaterialPropertyBlock();
//             previewCoeffs = new float[28];
//             faces = new Color[6];
//         }

//         public void OnGUI ()
//         {
//             fileName = EditorGUILayout.TextField (mfileNameContent, fileName);
//             EditorGUILayout.BeginHorizontal ();
//             EditorGUILayout.LabelField (string.Empty, GUILayout.Width (150));
//             if (GUILayout.Button ("Load"))
//             {
//                 ApplyFileToPreview (fileName);
//             }
//             if (GUILayout.Button ("Apply file in scene"))
//             {
//                 float[] coeffs = helper.ReadFile (fileName);
//                 for (int i = 0; i < 28; i++)
//                     coeffs[i] = coeffs[i];
//                 helper.ApplySHToObjects (coeffs);
//                 Debug.Log ("success");
//             }
//             EditorGUILayout.EndHorizontal ();
//             EditorGUILayout.LabelField (string.Empty);

//             mode = EditorGUILayout.Popup (mModeContent, mode, options);
//             //0,1,2=="SkyBox", "Trilight", "Flat" 
//             if (mode == 0)
//             {
//                 cubeMap = EditorGUILayout.ObjectField ("Sky Box", cubeMap, typeof (UnityEngine.Texture), true) as Texture;
//                 if (cubeMap != null)
//                     ambientIntensity = EditorGUILayout.Slider (mIntensityContent, ambientIntensity, 0, 40f);
//             }
//             else if (mode == 1)
//             {
//                 skyColor = EditorGUILayout.ColorField (mSkyColorContent, skyColor, true, false, true);
//                 equatorColor = EditorGUILayout.ColorField (mEquatorColorContent, equatorColor, true, false, true);
//                 groundColor = EditorGUILayout.ColorField (mGroundColorContent, groundColor, true, false, true);
//             }
//             else if (mode == 2)
//             {
//                 ambientColor = EditorGUILayout.ColorField (mAmbientColorContent, ambientColor, true, false, true);
//             }

//             if (GUILayout.Button ("Bake"))
//             {
//                 bool success = false;
//                 if (mode == 0)
//                 {
//                     if (cubeMap == null)
//                     {
//                         Debug.LogError ("SkyBox can not be Null");
//                         return;
//                     }
//                     else if (!TextureIsCube (cubeMap))
//                     {
//                         Debug.LogError ("SkyBox is not CubeMap");
//                         return;
//                     }
//                 }
//                 else if (string.IsNullOrEmpty (fileName))
//                 {
//                     Debug.LogError ("fileName can not be null or empty");
//                     return;
//                 }

//                 if (mode == 0)
//                 {
//                     helper.BakeSkyBoxSphericalHarmonics (cubeMap, ambientIntensity, fileName);
//                     success = true;
//                 }
//                 else if (mode == 1)
//                 {
//                     PreparePreviewCubeMap ();
//                     helper.BakeSkyBoxSphericalHarmonics (cube, 1f, fileName);
//                     success = true;
//                 }
//                 else if (mode == 2)
//                 {
//                     SphericalHarmonicsL2 sh = new SphericalHarmonicsL2 ();
//                     sh.AddAmbientLight (ambientColor);
//                     helper.SaveShaderField (sh, fileName);
//                     success = true;
//                 }
//                 if (success)
//                 {
//                     ApplyFileToPreview (fileName);
//                     AssetDatabase.Refresh ();
//                     Debug.Log ("success");

//                 }
//             }

//             EditorGUILayout.BeginHorizontal ();
//             if (GUILayout.Button (previewPrimitiveCube? "Cube": "Sphere", GUILayout.Width (55)))
//             {
//                 previewPrimitiveCube = !previewPrimitiveCube;
//                 PreparePreviewMesh ();
//             }
//             GUILayout.Label (previewTip);
//             EditorGUILayout.EndHorizontal ();
//             DrawPreview (new Rect (0, 200, 300, 300));

//             // 实时预览
//             if (GUI.changed)
//             {
//                 bool changed = false;
//                 if (cubeMap != start_cubeMap)
//                 {
//                     start_cubeMap = cubeMap;
//                     changed = true;
//                 }
//                 if (ambientIntensity != start_ambientIntensity)
//                 {
//                     start_ambientIntensity = ambientIntensity;
//                     changed = true;
//                 }
//                 if (ambientColor != start_ambientColor)
//                 {
//                     start_ambientColor = ambientColor;
//                     changed = true;
//                 }
//                 if (skyColor != start_skyColor)
//                 {
//                     start_skyColor = skyColor;
//                     changed = true;
//                 }
//                 if (equatorColor != start_equatorColor)
//                 {
//                     start_equatorColor = equatorColor;
//                     changed = true;
//                 }
//                 if (groundColor != start_groundColor)
//                 {
//                     start_groundColor = groundColor;
//                     changed = true;
//                 }
//                 if (changed)
//                 {
//                     OnSomethingChanged ();
//                 }
//             }
//         }

//         private bool TextureIsCube (Texture tex)
//         {
//             RenderTexture rt = tex as RenderTexture;
//             if (rt != null && rt.dimension == TextureDimension.Cube)
//                 return true;
//             Cubemap cube = tex as Cubemap;
//             if (cube != null)
//                 return true;
//             return false;
//         }

//         public static Vector2 Drag2D (Vector2 scrollPosition, Rect position)
//         {
//             int id = GUIUtility.GetControlID ("Slider".GetHashCode (), FocusType.Passive);
//             Event evt = Event.current;
//             switch (evt.GetTypeForControl (id))
//             {
//                 case EventType.MouseDown:
//                     if (position.Contains (evt.mousePosition) && position.width > 50f)
//                     {
//                         GUIUtility.hotControl = id;
//                         evt.Use ();
//                         EditorGUIUtility.SetWantsMouseJumping (1);
//                     }
//                     break;
//                 case EventType.MouseUp:
//                     if (GUIUtility.hotControl == id)
//                         GUIUtility.hotControl = 0;
//                     EditorGUIUtility.SetWantsMouseJumping (0);
//                     break;
//                 case EventType.MouseDrag:
//                     if (GUIUtility.hotControl == id)
//                     {
//                         scrollPosition -= evt.delta * (evt.shift ? 3 : 1) / Mathf.Min (position.width, position.height) * 140.0f;
//                         scrollPosition.y = Mathf.Clamp (scrollPosition.y, -90, 90);
//                         evt.Use ();
//                         GUI.changed = true;
//                     }
//                     break;
//             }
//             return scrollPosition;
//         }

//         private void DrawPreview (Rect drawRect)
//         {
//             if (previewUtility == null)
//             {
//                 previewUtility = new PreviewRenderUtility ();
//                 previewUtility.camera.farClipPlane = 500;
//                 previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
//                 previewUtility.camera.transform.position = new Vector3 (0, 0, -8);
//                 PreparePreviewMesh ();
//             }
//             previewDir = Drag2D (previewDir, drawRect);
//             previewUtility.BeginPreview (drawRect, GUIStyle.none);
//             DoRenderPreview (drawRect);
//             previewUtility.EndAndDrawPreview (drawRect);

//         }

//         private void DoRenderPreview (Rect drawRect)
//         {
//             previewUtility.camera.transform.position = -Vector3.forward * 7;
//             previewUtility.camera.transform.rotation = Quaternion.identity;
//             //InternalEditorUtility.SetCustomLighting(previewUtility.lights, new Color(0.6f, 0.6f, 0.6f, 1f));

//             Quaternion rot = Quaternion.identity;
//             rot = Quaternion.Euler (previewDir.y, 0, 0) * Quaternion.Euler (0, previewDir.x, 0);
//             previewUtility.camera.transform.position = Quaternion.Inverse (rot) * previewUtility.camera.transform.position;
//             previewUtility.camera.transform.LookAt (Vector3.zero);
//             rot = Quaternion.identity;

//             previewUtility.DrawMesh (mPreviewMesh, Vector3.zero, rot, previewMat, 0, materialBlock);

//             previewUtility.camera.Render ();
//         }

//         private void OnSomethingChanged ()
//         {
//             if (mode == 0)
//             {
//                 if (cubeMap == null)
//                 {
//                     return;
//                 }
//                 else if (!TextureIsCube (cubeMap))
//                 {
//                     return;
//                 }
//             }
//             if (mode == 0)
//             {
//                 helper.BakeSkyBoxSphericalHarmonics (cubeMap, ambientIntensity, fileName, false);
//                 EnviromentSHBakerHelper.PrepareCoefs (helper.AmbientProbe, ref previewCoeffs);
//                 helper.ApplySHToMaterialBlock (previewCoeffs, materialBlock);
//                 previewTip = "Skybox SH RealTime Preview";
//             }
//             else if (mode == 1)
//             {
//                 PreparePreviewCubeMap ();
//                 helper.BakeSkyBoxSphericalHarmonics (cube, 1f, fileName, false);
//                 EnviromentSHBakerHelper.PrepareCoefs (helper.AmbientProbe, ref previewCoeffs);
//                 helper.ApplySHToMaterialBlock (previewCoeffs, materialBlock);
//                 previewTip = "Gradient SH RealTime Preview";
//             }
//             else if (mode == 2)
//             {
//                 SphericalHarmonicsL2 sh = new SphericalHarmonicsL2 ();
//                 sh.AddAmbientLight (ambientColor);
//                 EnviromentSHBakerHelper.PrepareCoefs (sh, ref previewCoeffs);
//                 helper.ApplySHToMaterialBlock (previewCoeffs, materialBlock);
//                 previewTip = "Ambient SH RealTime Preview";
//             }
//         }

//         private void PreparePreviewCubeMap ()
//         {
//             faces[0] = equatorColor;
//             faces[1] = equatorColor;
//             faces[2] = skyColor;
//             faces[3] = groundColor;
//             faces[4] = equatorColor;
//             faces[5] = equatorColor;
//             if (cube == null)
//                 cube = new Cubemap (WIDTH, DefaultFormat.HDR, TextureCreationFlags.MipChain);
//             for (int f = 0; f < 6; f++)
//             {
//                 for (int i = 0; i < WIDTH; i++)
//                 {
//                     for (int j = 0; j < WIDTH; j++)
//                     {
//                         cube.SetPixel ((CubemapFace) f, i, j, faces[f]);
//                     }
//                 }
//             }
//             cube.Apply ();
//         }

//         private void PreparePreviewMesh ()
//         {
//             var go = GameObject.CreatePrimitive (previewPrimitiveCube ? PrimitiveType.Cube : PrimitiveType.Sphere);
//             var meshFilter = go.GetComponent<MeshFilter> ();
//             mPreviewMesh = meshFilter.sharedMesh;
//             DestroyImmediate (go);
//         }

//         private void ApplyFileToPreview (string fileName)
//         {
//             float[] coeffs = helper.ReadFile (fileName);
//             helper.ApplySHToMaterialBlock (coeffs, materialBlock);
//             previewTip = fileName;
//         }

//         public void OnDisable ()
//         {
//             if (previewUtility != null)
//             {
//                 previewUtility.Cleanup ();
//                 previewUtility = null;
//             }
//         }
//     }
// }