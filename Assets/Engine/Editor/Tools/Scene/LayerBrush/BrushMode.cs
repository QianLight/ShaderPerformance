using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{

    public class BrushMode
    {
        protected GameObject m_SelObject;
        protected Material m_CurMaterial;
        public bool m_SupportsLayered = true;
        public static bool isEditing = false;
        private void CheckForLayeredSupport (GameObject go)
        {
            m_SupportsLayered = false;
            if (go != null)
            {
                m_CurMaterial = Util.GetMaterials (go);
                var layerShaders = AssetsConfig.instance.layerShader;
                if (layerShaders != null && m_CurMaterial != null)
                {
                    var shader = m_CurMaterial.shader;
                    for (int i = 0; i < layerShaders.Length; ++i)
                    {
                        if (shader == layerShaders[i])
                        {
                            m_SupportsLayered = true;
                            go.layer = LayerEditLayer;
                            Refresh ();
                            break;
                        }
                    }
                }
            }
        }

        public virtual void OnSelectObjChange()
        {
            
        }
        
        public void OnSelectionChanged ()
        {
            if(m_SelObject!= Selection.activeObject as GameObject)
            {
                OnSelectObjChange();
                m_SelObject = Selection.activeObject as GameObject;
                CheckForLayeredSupport(m_SelObject);
            }
     
      //      CheckForLayeredSupport (m_SelObjects
        }
        public void OnSelectionEnd ()
        {
            m_SelObject = null;
            CheckForLayeredSupport (m_SelObject);
        }
        public static int LayerEditLayer = LayerMask.NameToLayer ("LayerEdit");
        public static int LayerEditMask = 1 << LayerEditLayer;
        protected RaycastHit MainRay ()
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            RaycastHit hit;
            Physics.Raycast (mouseRay, out hit, float.MaxValue, LayerEditMask);
            return hit;
        }

        public virtual void PreDraw ()
        {
            if (!m_SupportsLayered)
            {
                isEditing = false;
                GUIUtility.GetControlID(LayerBrushTool.s_LayeredBrushEditorHash,  FocusType.Keyboard);
                EditorGUILayout.HelpBox ("Not Layer Material", MessageType.Warning);

                var layerShaders = AssetsConfig.instance.layerShader;
                if (layerShaders != null)
                {
                    for (int i = 0; i < layerShaders.Length; ++i)
                    {
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.ObjectField ("", layerShaders[i], typeof (Shader), false, GUILayout.MaxWidth (300));
                        if (GUILayout.Button ("Set", GUILayout.MaxWidth (80)))
                        {
                            if (m_CurMaterial != null)
                            {
                                m_CurMaterial.shader = layerShaders[i];
                                m_CurMaterial.SetVector ("_Param2", new Vector4 (1, 1, 0, 0)); //hard code here
                                m_CurMaterial.SetVector ("_Param3", new Vector4 (1, 1, 0, 0));
                                m_CurMaterial.SetVector ("_Param4", new Vector4 (1, 1, 0, 0));

                                m_CurMaterial.SetColor ("_Color0", Color.white);
                                m_CurMaterial.SetColor ("_Color1", Color.white);
                                m_CurMaterial.SetColor ("_Color2", Color.white);
                                CheckForLayeredSupport (m_SelObject);
                            }
                        }
                        EditorGUILayout.EndHorizontal ();
                    }

                }
            }
            else
            {
                // var mode = m_CurMaterial.GetFloat ("_WeightsMode");
                // if ((int) mode != m_nMode)
                // {
                //     EditorGUILayout.BeginHorizontal ();
                //     //float target = 0.0f;
                //     if (mode == 1.0f)
                //     {
                //         EditorGUILayout.HelpBox ("当前材质是 Mask!", MessageType.Warning);
                //     }
                //     else
                //     {
                //         EditorGUILayout.HelpBox ("当前材质是 Vectex!", MessageType.Warning);
                //         //target = 1.0f;
                //     }
                //     EditorGUILayout.EndHorizontal ();
                // }
            }
        }

        public virtual void Refresh () { }

        public virtual void DrawBrushSettings () { }
        public virtual void OnEnable () { }
        public virtual void OnDisable () { }
        public virtual void OnPrePaint (RaycastHit hit) { }
        public virtual void OnEndPaint () { }
        public virtual void OnPaint (RaycastHit hit) { }

        public virtual void DrawGizmos (Event e) { }

        public virtual void Update () { }
    }
}