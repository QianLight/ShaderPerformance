// using UnityEditor;
// using UnityEngine;
// using System.Collections.Generic;
// using System.Linq;
// using Polybrush;
// using UnityEngineEditor = UnityEditor.Editor;

// namespace CFEngine.Editor
// {
//     public abstract class BrushTool : CommonToolTemplate
//     {
//         [SerializeField] protected z_ZoomOverride tempComponent;

//         protected Color innerColor, outerColor;


//         protected virtual void CreateTempComponent(z_EditableObject target, z_BrushSettings settings)
//         {
//             if (!z_Util.IsValid(target))
//                 return;

//             tempComponent = target.gameObject.AddComponent<z_ZoomOverride>();
//             tempComponent.hideFlags = HideFlags.HideAndDontSave;
//             tempComponent.SetWeights(null, 0f);
//         }

//         protected virtual void UpdateTempComponent(z_BrushTarget target, z_BrushSettings settings)
//         {
//             if (!z_Util.IsValid(target))
//                 return;

//             tempComponent.SetWeights(target.GetAllWeights(), settings.strength);
//         }

//         protected virtual void DestroyTempComponent()
//         {
//             if (tempComponent != null)
//                 GameObject.DestroyImmediate(tempComponent);
//         }

//         Called on instantiation.  Base implementation sets HideFlags.
//         public override void OnInit()
//         {
//             this.hideFlags = HideFlags.HideAndDontSave;

//             innerColor = z_Pref.GetColor(z_Pref.brushColor);
//             outerColor = z_Pref.GetGradient(z_Pref.brushGradient).Evaluate(1f);

//             innerColor.a = .9f;
//             outerColor.a = .35f;
//         }

//         Called when mode is disabled.
//         public override void OnUninit()
//         {
//             DestroyTempComponent();
//         }

//         Called by z_Editor when brush settings have been modified.
//         public virtual void OnBrushSettingsChanged(z_BrushTarget target, z_BrushSettings settings)
//         {
//             UpdateTempComponent(target, settings);
//         }

//         public override void DrawSceneGUI()
//         {
//             BrushUtility.Instance.OnSceneGUI(this);
//         }
//         Inspector GUI shown in the Editor window.
//         public virtual void DrawGUI(z_BrushSettings brushSettings)
//         {
//             if (z_GUILayout.HeaderWithDocsLink(z_GUI.TempContent(ModeSettingsHeader, "")))
//                Application.OpenURL(DocsLink);
//         }
//         public virtual void DrawGizmos(z_BrushTarget target, z_BrushSettings settings)
//         {
//             if (target != null && settings != null)
//                 foreach (z_RaycastHit hit in target.raycastHits)
//                     z_Handles.DrawBrush(hit.position, hit.normal, settings, target.localToWorldMatrix, innerColor, outerColor);

//         }
//         Called when the mouse begins hovering an editable object.
//         public virtual void OnBrushEnter(z_EditableObject target, z_BrushSettings settings)
//         {
//             if (z_Pref.GetBool(z_Pref.hideWireframe) && target.renderer != null)
//             {
//                 disable wirefame
//                 z_EditorUtility.SetSelectionRenderState(target.renderer, z_EditorUtility.GetSelectionRenderState() & z_SelectionRenderState.Outline);
//             }

//             CreateTempComponent(target, settings);
//         }

//         Called whenever the brush is moved.  Note that @target may have a null editableObject.
//         public virtual void OnBrushMove(z_BrushTarget target, z_BrushSettings settings)
//         {
//             UpdateTempComponent(target, settings);
//         }

//         Called when the mouse exits hovering an editable object.
//         public virtual void OnBrushExit(z_EditableObject target)
//         {
//             if (target.renderer != null)
//                 z_EditorUtility.SetSelectionRenderState(target.renderer, z_EditorUtility.GetSelectionRenderState());

//             DestroyTempComponent();
//         }

//         Called when the mouse begins a drag across a valid target.
//         public virtual void OnBrushBeginApply(z_BrushTarget target, z_BrushSettings settings) { }

//         Called every time the brush should apply itself to a valid target.  Default is on mouse move.
//         public abstract void OnBrushApply(z_BrushTarget target, z_BrushSettings settings);

//         Called when a brush application has finished.  Use this to clean up temporary resources or apply
//         deferred actions to a mesh (rebuild UV2, tangents, whatever).
//         public virtual void OnBrushFinishApply(z_BrushTarget target, z_BrushSettings settings)
//         {
//             DestroyTempComponent();
//         }

//         Draw scene gizmos.  Base implementation draws the brush preview.


//         public abstract void RegisterUndo(z_BrushTarget brushTarget);

//         public virtual void UndoRedoPerformed(List<GameObject> modified)
//         {
//             DestroyTempComponent();
//         }
//     }

//     public class BrushUtility
//     {
//         private static BrushUtility g_BrushUtility;


//         public BrushUtility()
//         {
//         }

//         private GUIContent gc_SaveBrushSettings = null;
//         private GUIContent[] mirrorSpaceGuiContent = null;
//         private Vector2 scroll = Vector2.zero;
//         private int currentBrushIndex = 0;
//         private string[] availableBrushes_str = null;
//         private List<z_BrushSettings> availableBrushes = null;
//         private z_BrushSettings brushSettingsAsset;
//         private z_BrushSettingsEditor _brushEditor = null;
//         public z_BrushMirror brushMirror = z_BrushMirror.None;
//         public z_MirrorCoordinateSpace mirrorSpace = z_MirrorCoordinateSpace.World;

//         public z_BrushSettings brushSettings;


//         const double EDITOR_TARGET_FRAMERATE_LOW = .016667;
//         const double EDITOR_TARGET_FRAMERATE_HIGH = .03;

//         private double lastBrushUpdate = 0.0;

//         private GameObject[] ignoreDrag = new GameObject[8];
//         All objects that have been hovered by the mouse
//         private Dictionary<GameObject, z_BrushTarget> hovering = new Dictionary<GameObject, z_BrushTarget>();
//         The current brush status
//         public z_BrushTarget brushTarget = null;
//         private GameObject lastHoveredGameObject = null;
//         private bool applyingBrush = false;

//         When dragging brush only test the first object selected for rays
//         public bool lockBrushToFirst = false;
//         If true the mouse will always try to raycast against selection first, even if a mesh is in the way.
//         public bool ignoreUnselected = true;
//         private static List<Ray> rays = new List<Ray>();

//         private List<GameObject> undoQueue = new List<GameObject>();

//         private BrushTool currentBrushTool;
//         private z_BrushSettingsEditor brushEditor
//         {
//             get
//             {
//                 if (_brushEditor == null && brushSettings != null)
//                 {
//                     _brushEditor = (z_BrushSettingsEditor)UnityEngineEditor.CreateEditor(brushSettings);
//                 }
//                 else if (_brushEditor.target != brushSettings)
//                 {
//                     GameObject.DestroyImmediate(_brushEditor);

//                     if (brushSettings != null)
//                         _brushEditor = (z_BrushSettingsEditor)UnityEngineEditor.CreateEditor(brushSettings);
//                 }

//                 return _brushEditor;
//             }
//         }

//         public static BrushUtility Instance
//         {
//             get
//             {
//                 if (g_BrushUtility == null)
//                     g_BrushUtility = new BrushUtility();
//                 return g_BrushUtility;
//             }
//         }

//         public void OnEnable()
//         {
//             if (gc_SaveBrushSettings == null)
//                 gc_SaveBrushSettings = new GUIContent(z_IconUtility.GetIcon("Icon/save.png"),
//                     "Save the brush settings as a preset");


//             if (mirrorSpaceGuiContent == null)
//             {
//                 mirrorSpaceGuiContent = new GUIContent[]
//                 {
//                     new GUIContent(z_IconUtility.GetIcon("Icon/World"), "Mirror rays in world space"),
//                     new GUIContent(z_IconUtility.GetIcon("Icon/Camera"), "Mirror rays in camera space")
//                 };
//             }

//             if (brushSettings == null)
//                 SetBrushSettings(z_EditorUtility.GetDefaultAsset<z_BrushSettings>("Brush Settings/Default.asset"));


//             force update the preview
//             lastHoveredGameObject = null;

//             lockBrushToFirst = z_Pref.GetBool(z_Pref.lockBrushToFirst);
//             ignoreUnselected = z_Pref.GetBool(z_Pref.ignoreUnselected);

//             Undo.undoRedoPerformed -= UndoRedoPerformed;
//             Undo.undoRedoPerformed += UndoRedoPerformed;
//         }
//         public void OnDisable()
//         {
//             Undo.undoRedoPerformed -= UndoRedoPerformed;
//             Finish(currentBrushTool);
//         }

//         public void OnDestroy()
//         {

//             if (brushSettings != null)
//                 GameObject.DestroyImmediate(brushSettings);

//             if (_brushEditor != null)
//                 GameObject.DestroyImmediate(_brushEditor);
//         }

//         public void OnGUI()
//         {
//             if (!z_Pref.GetBool(z_Pref.lockBrushSettings))
//             {
//                scroll = EditorGUILayout.BeginScrollView(scroll);
//             }

//             /**
//              * Brush preset selector
//              */
//             GUILayout.BeginHorizontal();
//             EditorGUI.BeginChangeCheck();

//             currentBrushIndex = EditorGUILayout.Popup(currentBrushIndex, availableBrushes_str, "Popup");

//             if (EditorGUI.EndChangeCheck())
//             {
//                 if (currentBrushIndex >= availableBrushes.Count)
//                     SetBrushSettings(z_BrushSettingsEditor.AddNew());
//                 else
//                     SetBrushSettings(availableBrushes[currentBrushIndex]);
//             }

//             if (GUILayout.Button(gc_SaveBrushSettings))
//             {
//                 if (brushSettings != null && brushSettingsAsset != null)
//                 {
//                     integer 0, 1 or 2 corresponding to ok, cancel and alt buttons
//                     int res = EditorUtility.DisplayDialogComplex("Save Brush Settings", "Overwrite brush preset or save as a new preset? ", "Save", "Save As", "Cancel");

//                     if (res == 0)
//                     {
//                         brushSettings.CopyTo(brushSettingsAsset);
//                         EditorGUIUtility.PingObject(brushSettingsAsset);
//                     }
//                     else if (res == 1)
//                     {
//                         z_BrushSettings dup = z_BrushSettingsEditor.AddNew();
//                         string name = dup.name;
//                         brushSettings.CopyTo(dup);
//                         dup.name = name;    // want to retain the unique name generated by AddNew()
//                         SetBrushSettings(dup);
//                         EditorGUIUtility.PingObject(brushSettingsAsset);
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogWarning("Something went wrong saving brush settings.");
//                 }
//             }
//             GUILayout.EndHorizontal();

//             brushEditor.OnInspectorGUI();

//             GUILayout.BeginHorizontal();
//             brushMirror = (z_BrushMirror)z_GUILayout.BitMaskField((uint)brushMirror, System.Enum.GetNames(typeof(z_BrushMirror)), "Set Brush Mirroring");
//             mirrorSpace = (z_MirrorCoordinateSpace)GUILayout.Toolbar((int)mirrorSpace, mirrorSpaceGuiContent, "Command");
//             GUILayout.EndHorizontal();

//             EditorGUILayout.EndScrollView();
//         }

//         public void Finish(BrushTool brushTool)
//         {
//             if (lastHoveredGameObject != null)
//             {
//                 OnBrushExit(brushTool, lastHoveredGameObject);
//                 FinalizeAndResetHovering();
//                 lastHoveredGameObject = null;
//             }
//         }
//         public void SetBrushTool(BrushTool brushTool)
//         {
//             currentBrushTool = brushTool;
//         }
//         private void SetBrushSettings(z_BrushSettings settings)
//         {
//             if (settings == null)
//                 return;

//             if (brushSettings != null && brushSettings != settings)
//                 GameObject.DestroyImmediate(brushSettings);

//             brushSettingsAsset = settings;
//             brushSettings = settings.DeepCopy();
//             brushSettings.hideFlags = HideFlags.HideAndDontSave;
//             RefreshAvailableBrushes();

//             SceneTool.DoRepaint();
//         }

//         private void RefreshAvailableBrushes()
//         {
//             availableBrushes = Resources.FindObjectsOfTypeAll<z_BrushSettings>().Where(x => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(x))).ToList();

//             if (availableBrushes.Count < 1)
//                 availableBrushes.Add(z_EditorUtility.GetDefaultAsset<z_BrushSettings>("Brush Settings/Default.asset"));

//             currentBrushIndex = System.Math.Max(availableBrushes.FindIndex(x => x.name.Equals(brushSettings.name)), 0);

//             availableBrushes_str = availableBrushes.Select(x => x.name).ToArray();

//             ArrayUtility.Add<string>(ref availableBrushes_str, string.Empty);
//             ArrayUtility.Add<string>(ref availableBrushes_str, "Add Brush...");
//         }

//         public void OnSceneGUI(BrushTool brushTool)
//         {
//             Event e = Event.current;

//             if (z_SceneUtility.SceneViewInUse(e))
//             {
//                 return;
//             }

//             int controlID = GUIUtility.GetControlID(FocusType.Passive);

//             if (z_Util.IsValid(brushTarget))
//                 HandleUtility.AddDefaultControl(controlID);

//             switch (e.GetTypeForControl(controlID))
//             {
//                 case EventType.MouseMove:
//                     Handles:
//                     		OnBrushEnter
//                     		OnBrushExit
//                     		OnBrushMove
//                     if (EditorApplication.timeSinceStartup - lastBrushUpdate > GetTargetFramerate(brushTarget))
//                     {
//                         lastBrushUpdate = EditorApplication.timeSinceStartup;
//                         UpdateBrush(brushTool, e.mousePosition);
//                     }
//                     break;

//                 case EventType.MouseDown:
//                 case EventType.MouseDrag:
//                     Handles:
//                     		OnBrushBeginApply
//                     		OnBrushApply
//                     		OnBrushFinishApply
//                     if (EditorApplication.timeSinceStartup - lastBrushUpdate > GetTargetFramerate(brushTarget))
//                     {
//                         lastBrushUpdate = EditorApplication.timeSinceStartup;
//                         UpdateBrush(brushTool, e.mousePosition, true);
//                         ApplyBrush(brushTool);
//                     }
//                     break;

//                 case EventType.MouseUp:
//                     if (applyingBrush)
//                     {
//                         OnFinishApplyingBrush(brushTool);
//                     }
//                     break;

//                 case EventType.ScrollWheel:
//                     ScrollBrushSettings(brushTool, e);
//                     break;
//             }
//             if (z_Util.IsValid(brushTarget))
//                 brushTool.DrawGizmos(brushTarget, brushSettings);

//         }
//         double GetTargetFramerate(z_BrushTarget target)
//         {
//             if (z_Util.IsValid(target) && target.vertexCount > 24000)
//                 return EDITOR_TARGET_FRAMERATE_LOW;

//             return EDITOR_TARGET_FRAMERATE_HIGH;
//         }

//         /**
//          *	Get a z_EditableObject matching the GameObject go or create a new one.
//          */
//         z_BrushTarget GetBrushTarget(GameObject go)
//         {
//             z_BrushTarget target = null;

//             if (go.name.StartsWith(AssetsConfig.GlobalAssetsConfig.SceneChunkStr))
//             {
//                 MeshFilter mf = go.GetComponent<MeshFilter>();
//                 if (mf != null)
//                 {
//                     if (!hovering.TryGetValue(go, out target))
//                     {
//                         z_EditableObject obj = z_EditableObject.Create(go, true);
//                         if (obj != null)
//                         {
//                             target = new z_BrushTarget(obj);
//                             hovering.Add(go, target);
//                         }

//                     }
//                     else if (!z_Util.IsValid(target))
//                     {
//                         hovering[go] = new z_BrushTarget(z_EditableObject.Create(go, true));
//                     }
//                 }

//             }


//             return target;
//         }

//         /**
//          * Update the current brush object and weights with the current mouse position.
//          */
//         void UpdateBrush(BrushTool brushTool, Vector2 mousePosition, bool isDrag = false)
//         {
//             Must check HandleUtility.PickGameObject only during MouseMoveEvents or errors will rain.
//             GameObject go = null;
//             brushTarget = null;

//             if (isDrag && lockBrushToFirst && lastHoveredGameObject != null)
//             {
//                 go = lastHoveredGameObject;
//                 brushTarget = GetBrushTarget(go);
//             }
//             else if (ignoreUnselected || isDrag)
//             {
//                 GameObject cur = null;
//                 int i = 0;
//                 int max = 0;    // safeguard against unforeseen while loop errors crashing unity

//                 do
//                 {
//                     int tmp;
//                     overloaded PickGameObject ignores array of GameObjects, this is used
//                     when there are non-selected gameObjects between the mouse and selected
//                     gameObjects.
//                     cur = HandleUtility.PickGameObject(mousePosition, ignoreDrag, out tmp);

//                     if (cur != null)
//                     {
//                         if (!z_EditorUtility.InSelection(cur.transform))
//                         {
//                             if (!ignoreDrag.Contains(cur))
//                             {
//                                 if (i >= ignoreDrag.Length - 1)
//                                     z_Util.Resize(ref ignoreDrag, ignoreDrag.Length * 2);

//                                 ignoreDrag[i++] = cur;
//                             }
//                         }
//                         else
//                         {
//                             brushTarget = GetBrushTarget(cur);

//                             if (brushTarget != null)
//                             {
//                                 go = cur;
//                             }
//                             else
//                             {
//                                 if (i >= ignoreDrag.Length - 1)
//                                     z_Util.Resize(ref ignoreDrag, ignoreDrag.Length * 2);

//                                 ignoreDrag[i++] = cur;
//                             }
//                         }
//                     }
//                 } while (go == null && cur != null && max++ < 128);
//             }
//             else
//             {
//                 go = HandleUtility.PickGameObject(mousePosition, false);

//                 if (go != null && z_EditorUtility.InSelection(go))
//                     brushTarget = GetBrushTarget(go);
//                 else
//                     go = null;
//             }

//             bool mouseHoverTargetChanged = false;

//             Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePosition);

//             if the mouse hover picked up a valid editable, raycast against that.  otherwise
//             raycast all meshes in selection
//             if (go == null)
//             {
//                 foreach (var kvp in hovering)
//                 {
//                     z_BrushTarget t = kvp.Value;

//                     if (z_Util.IsValid(t) && DoMeshRaycast(mouseRay, t))
//                     {
//                         brushTarget = t;
//                         go = t.gameObject;
//                         break;
//                     }
//                 }

//             }
//             else
//             {
//                 if (!DoMeshRaycast(mouseRay, brushTarget))
//                 {
//                     if (!isDrag || !lockBrushToFirst)
//                     {
//                         go = null;
//                         brushTarget = null;
//                     }

//                     return;
//                 }
//             }

//             if hovering off another gameobject, call OnBrushExit on that last one and mark the
//             target as having been changed
//             if (go != lastHoveredGameObject)
//             {
//                 OnBrushExit(brushTool, lastHoveredGameObject);
//                 mouseHoverTargetChanged = true;
//                 lastHoveredGameObject = go;
//             }

//             if (brushTarget == null)
//                 return;

//             if (mouseHoverTargetChanged)
//             {
//                 OnBrushEnter(brushTool, brushTarget, brushSettings);

//                 brush is in use, adding a new object to the undo
//                 if (applyingBrush && !undoQueue.Contains(go))
//                 {
//                     int curGroup = Undo.GetCurrentGroup();
//                     brushTarget.editableObject.isDirty = true;
//                     OnBrushBeginApply(brushTool, brushTarget, brushSettings);
//                     Undo.CollapseUndoOperations(curGroup);
//                 }
//             }

//             OnBrushMove(brushTool);

//             SceneView.RepaintAll();
//             SceneTool.DoRepaint();
//         }

//         /**
//          * Calculate the weights for this ray.
//          */
//         bool DoMeshRaycast(Ray mouseRay, z_BrushTarget target)
//         {
//             if (!z_Util.IsValid(target))
//                 return false;

//             target.ClearRaycasts();

//             z_EditableObject editable = target.editableObject;

//             rays.Clear();
//             rays.Add(mouseRay);

//             if (brushMirror != z_BrushMirror.None)
//             {
//                 for (int i = 0; i < 3; i++)
//                 {
//                     if (((uint)brushMirror & (1u << i)) < 1)
//                         continue;

//                     int len = rays.Count;

//                     for (int n = 0; n < len; n++)
//                     {
//                         Vector3 flipVec = ((z_BrushMirror)(1u << i)).ToVector3();

//                         if (mirrorSpace == z_MirrorCoordinateSpace.World)
//                         {
//                             Vector3 cen = editable.gameObject.GetComponent<Renderer>().bounds.center;
//                             rays.Add(new Ray(Vector3.Scale(rays[n].origin - cen, flipVec) + cen,
//                                                 Vector3.Scale(rays[n].direction, flipVec)));
//                         }
//                         else
//                         {
//                             Transform t = SceneView.lastActiveSceneView.camera.transform;
//                             Vector3 o = t.InverseTransformPoint(rays[n].origin);
//                             Vector3 d = t.InverseTransformDirection(rays[n].direction);
//                             rays.Add(new Ray(t.TransformPoint(Vector3.Scale(o, flipVec)),
//                                                 t.TransformDirection(Vector3.Scale(d, flipVec))));
//                         }
//                     }
//                 }
//             }

//             bool hitMesh = false;

//             int[] triangles = editable.editMesh.GetTriangles();

//             foreach (Ray ray in rays)
//             {
//                 z_RaycastHit hit;

//                 if (z_SceneUtility.WorldRaycast(ray, editable.transform, editable.editMesh.vertices, triangles, out hit))
//                 {
//                     target.raycastHits.Add(hit);
//                     hitMesh = true;
//                 }
//             }

//             z_SceneUtility.CalculateWeightedVertices(target, brushSettings);

//             return hitMesh;
//         }

//         void ApplyBrush(BrushTool brushTool)
//         {
//             if (!z_Util.IsValid(brushTarget))
//                 return;

//             if (!applyingBrush)
//             {
//                 undoQueue.Clear();
//                 applyingBrush = true;
//                 OnBrushBeginApply(brushTool, brushTarget, brushSettings);
//             }

//             brushTool.OnBrushApply(brushTarget, brushSettings);

//             SceneView.RepaintAll();
//         }

//         void OnBrushBeginApply(BrushTool brushTool, z_BrushTarget brushTarget, z_BrushSettings settings)
//         {
//             z_SceneUtility.PushGIWorkflowMode();
//             brushTool.RegisterUndo(brushTarget);
//             undoQueue.Add(brushTarget.gameObject);
//             brushTool.OnBrushBeginApply(brushTarget, brushSettings);
//         }

//         void ScrollBrushSettings(BrushTool brushTool, Event e)
//         {
//             float nrm = 1f;

//             switch (e.modifiers)
//             {
//                 case EventModifiers.Control:
//                     nrm = Mathf.Sin(Mathf.Max(.001f, brushSettings.normalizedRadius)) * .03f * (brushSettings.brushRadiusMax - brushSettings.brushRadiusMin);
//                     brushSettings.radius = brushSettings.radius - (e.delta.y * nrm);
//                     break;

//                 case EventModifiers.Shift:
//                     nrm = Mathf.Sin(Mathf.Max(.001f, brushSettings.falloff)) * .03f;
//                     brushSettings.falloff = brushSettings.falloff - e.delta.y * nrm;
//                     break;

//                 case EventModifiers.Control | EventModifiers.Shift:
//                     nrm = Mathf.Sin(Mathf.Max(.001f, brushSettings.strength)) * .03f;
//                     brushSettings.strength = brushSettings.strength - e.delta.y * nrm;
//                     break;

//                 default:
//                     return;
//             }

//             EditorUtility.SetDirty(brushSettings);

//             if (brushTool != null)
//             {
//                 UpdateBrush(brushTool, Event.current.mousePosition);
//                 brushTool.OnBrushSettingsChanged(brushTarget, brushSettings);
//             }

//             e.Use();
//             SceneTool.DoRepaint();
//             SceneView.RepaintAll();
//         }

//         void OnBrushEnter(BrushTool brushTool, z_BrushTarget target, z_BrushSettings settings)
//         {
//             brushTool.OnBrushEnter(target.editableObject, settings);
//         }

//         void OnBrushMove(BrushTool brushTool)
//         {
//             if (z_Util.IsValid(brushTarget))
//                 brushTool.OnBrushMove(brushTarget, brushSettings);
//         }

//         void OnBrushExit(BrushTool brushTool, GameObject go)
//         {
//             z_BrushTarget target;

//             if (go == null || !hovering.TryGetValue(go, out target) || !z_Util.IsValid(target))
//                 return;
//             if (brushTool != null)
//                 brushTool.OnBrushExit(target.editableObject);

//             if (!applyingBrush)
//                 target.editableObject.Revert();
//         }

//         void OnFinishApplyingBrush(BrushTool brushTool)
//         {
//             z_SceneUtility.PopGIWorkflowMode();

//             applyingBrush = false;
//             brushTool.OnBrushFinishApply(brushTarget, brushSettings);
//             FinalizeAndResetHovering();

//             for (int i = 0; i < ignoreDrag.Length; i++)
//                 ignoreDrag[i] = null;
//         }

//         void FinalizeAndResetHovering()
//         {
//             foreach (var kvp in hovering)
//             {
//                 z_BrushTarget target = kvp.Value;

//                 if (!z_Util.IsValid(target))
//                     continue;

//                 if mesh hasn't been modified, revert it back
//                 to the original mesh so that unnecessary assets
//                 aren't allocated.  if it has been modified, let
//                 the editableObject apply those changes to the
//                 pb_Object if necessary.
//                 if (!target.editableObject.isDirty)
//                     target.editableObject.Revert();
//                 else
//                     target.editableObject.Apply(true, true);
//             }

//             hovering.Clear();
//             brushTarget = null;
//             lastHoveredGameObject = null;

//             SceneTool.DoRepaint();
//         }

//         void UndoRedoPerformed()
//         {
//            if (z_Pref.GetBool(z_Pref.rebuildCollisions))
//            {
//                foreach (GameObject go in undoQueue.Where(x => x != null))
//                {
//                    MeshCollider mc = go.GetComponent<MeshCollider>();
//                    MeshFilter mf = go.GetComponent<MeshFilter>();

//                    if (mc == null || mf == null || mf.sharedMesh == null)
//                        continue;

//                    mc.sharedMesh = null;
//                    mc.sharedMesh = mf.sharedMesh;
//                }
//            }

//            hovering.Clear();
//            brushTarget = null;
//            lastHoveredGameObject = null;
//            //if (m_brushTool != null)
//            //    m_brushTool.UndoRedoPerformed(undoQueue);
//            undoQueue.Clear();

//            SceneView.RepaintAll();
//            SceneTool.DoRepaint();
//         }
//     }
// }