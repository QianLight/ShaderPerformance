using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PainterEditor
{
    public class BrusherEditorWindow : EditorWindow 
    {
        private HitObject _hitObject;
        private BrushSettings _brushSettings;
        private GameObject _prefab;
        private GameObject _parent;
        private double _lastBrushUpdate = 0.0;
        private bool _useLayerMask;
        private LayerMask _layerMask;

        private int _ingoreCastLayerMask = 0;
        private int _ingoreCastLayer = 0;

        private bool _selectMode = false;

        private bool _initCheck = false;

        public class HitObject
        {
            public RaycastHit Hit;
            public bool    isHit;
        }
        
        [MenuItem("Tools/场景/Brusher",false,20)]
        public static void MenuInitEditorWindow()
        {
            EditorWindow.GetWindow<BrusherEditorWindow>(false).Close();
            EditorWindow.GetWindow<BrusherEditorWindow>(false).Show();
        }
        
        void OnEnable()
        {
     
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            _initCheck = true;
        }

        void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif

        }

        void OnDestroy()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif

            if (_parent)
            {
                var brushes = _parent.GetComponentsInChildren<BoxCollider>();
                foreach(var b in brushes)
                {
                    b.gameObject.layer = _prefab.layer;
                    DestroyImmediate(b);
                }
            }
        }


        void InitConfig()
        {
            if(_brushSettings == null)
            {
                _brushSettings = new BrushSettings();
                _brushSettings.SetDefaultValues();
            }

            if(_hitObject == null)
            {
                _hitObject = new HitObject();
            }

            _ingoreCastLayer = 2;
            _ingoreCastLayerMask = 1 << _ingoreCastLayer;

            if(!_parent)
            {
                _parent = GameObject.Find("BrusherLayer");
                if(!_parent)
                {
                    _parent = new GameObject("BrusherLayer");
                }
            }

            if(_initCheck)
            {
                if(_parent)
                {
                    var brushes = _parent.GetComponentsInChildren<Transform>();
                    foreach(var b in brushes)
                    {
                        if(_parent != b.gameObject)
                        {
                            if(!b.gameObject.GetComponent<BoxCollider>())
                            {
                                b.gameObject.AddComponent<BoxCollider>();
                            }
                            b.gameObject.layer = _ingoreCastLayer;
                        }
                    }
                }
                _initCheck = false;
            }
        }

        void OnGUI()
        {
            InitConfig();
            
            Event e = Event.current;
            
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius");
            _brushSettings.radius = EditorGUILayout.Slider(_brushSettings.radius,0,100);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Min size");
            _brushSettings.minSize = EditorGUILayout.FloatField(_brushSettings.minSize);
            GUILayout.Label("Max size");
            _brushSettings.maxSize = EditorGUILayout.FloatField(_brushSettings.maxSize);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Falloff");
            _brushSettings.falloff = EditorGUILayout.Slider(_brushSettings.falloff,0,1);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Parent");
            _parent = EditorGUILayout.ObjectField (_parent,typeof(GameObject),true) as GameObject;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("On/Off");
            _useLayerMask = EditorGUILayout.Toggle(_useLayerMask);
            _layerMask = EditorGUILayout.LayerField("LayerMask",_layerMask);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefab");
            _prefab = EditorGUILayout.ObjectField(_prefab,typeof(GameObject),true) as GameObject;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Select mode (On/Off) (Shortcut Key F1)");
            _selectMode = EditorGUILayout.Toggle(_selectMode);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Check Res"))
            {
                _initCheck = true;
            }
            GUILayout.EndHorizontal();

            Repaint();
        }    

        void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            
            if(e.isKey)
            {
                if(e.keyCode == KeyCode.F1)
                {
                    _selectMode = !_selectMode;
                }
            }
            
            if(_selectMode) return;

            InitConfig();
            int controlID = GUIUtility.GetControlID (FocusType.Passive);
            UnityEditor.HandleUtility.AddDefaultControl(controlID);

            if(!e.alt)
            {
                switch (e.GetTypeForControl (controlID)) 
                {
                    case EventType.MouseMove:
                    {
                        if(!e.alt)
                        {
                            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                            _hitObject.isHit = Physics.Raycast(ray,out _hitObject.Hit,1000f,_useLayerMask ? 1<<_layerMask : ~_ingoreCastLayerMask);
                        }
                    }
                    break;   
                    
                    case EventType.MouseDown:
                    {
                        
                    }
                    break;
                    case EventType.MouseUp:
                    case EventType.MouseDrag:
                    {
                        if(e.button == 0)
                        {
                            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                            _hitObject.isHit = Physics.Raycast(ray,out _hitObject.Hit,1000f,_useLayerMask ? 1<<_layerMask : ~_ingoreCastLayerMask);
                            if(!e.shift)
                            {
                                if( EditorApplication.timeSinceStartup - _lastBrushUpdate > 0.06)
                                {
                                    _lastBrushUpdate = EditorApplication.timeSinceStartup;
                                    if(_hitObject.isHit)
                                    {
                                        if(_prefab != null)
                                        {
                                            //BrushSingleObject(_hitObject.Hit,_prefab,true);
                                            BrushSameRandomObjects(_hitObject.Hit,_prefab);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                RemoveSingleObject(ref ray);
                            }

                            Event.current.Use();
                        }
                    }
                    break;
                    case EventType.ScrollWheel:
                    {
                        
                    }
                    break;
                }

                if(_hitObject.isHit)
                {
                    SceneHandles.DrawBrush(_hitObject.Hit.point,_hitObject.Hit.normal,_brushSettings,Matrix4x4.identity);
                }
            }
        }

        void BrushSameRandomObjects(RaycastHit hit,GameObject curObject)
        {
            var size = curObject.GetComponentInChildren<Renderer>().bounds.size;

            var objSurface = size.x * size.z;
            
            RaycastHit hit2;

            var surface = Mathf.PI * (1-_brushSettings.falloff)*_brushSettings.radius * _brushSettings.radius;

            int total = Mathf.CeilToInt(surface / objSurface);
            for(var i = 0;i < total;i ++)
            {
                var randomPos = Random.onUnitSphere;
                
                var dir = Vector3.Cross(randomPos,hit.normal).normalized * Random.Range(_brushSettings.falloff,1)*_brushSettings.radius;

                var ray = new Ray(hit.point + dir + hit.normal*20,-hit.normal);
                if(Physics.Raycast(ray,out hit2,1000f,_useLayerMask ? 1<<_layerMask : ~_ingoreCastLayerMask))
                {
                    if(_prefab != null)
                    {
                        BrushSingleObject(hit2,_prefab,true);
                    }
                }
            }
        }
        void BrushSingleObject(RaycastHit hit,GameObject curObject,bool needRandomDir = false)
        {
            var obj = Instantiate(curObject);
            obj.AddComponent<BoxCollider>();
            obj.layer = _ingoreCastLayer;

            if (_parent)
            {
                _parent.transform.position = Vector3.zero;
                obj.transform.parent = _parent.transform;
                obj.name = curObject.name + "_" + _parent.transform.childCount;
            }

            var axis = Vector3.Cross(Vector3.up,hit.normal);
            var rad = Mathf.Acos(Vector3.Dot(hit.normal,Vector3.up)) * 0.5f;
            var sina = Mathf.Sin(rad);
            var cosa = Mathf.Cos(rad);

            var quat = Quaternion.identity;
            quat.x = sina * axis.x;
            quat.y = sina * axis.y;
            quat.z = sina * axis.z;
            quat.w = cosa;

            if(needRandomDir)
            {
                var radRand = Random.Range(-Mathf.PI,Mathf.PI);
                var sinar = Mathf.Sin(radRand);
                var cosar = Mathf.Cos(radRand);
                var quatRand = Quaternion.identity;
                quatRand.x = sinar * hit.normal.x;
                quatRand.y = sinar * hit.normal.y;
                quatRand.z = sinar * hit.normal.z;
                quatRand.w = cosar;

                quat = quatRand * quat;
            }
            
            
            obj.transform.position = hit.point;
            obj.transform.rotation = quat * curObject.transform.rotation;
            obj.transform.localScale *= Random.Range(_brushSettings.minSize,_brushSettings.maxSize);

            Undo.RegisterCreatedObjectUndo(obj,"Apply Create Object");
        }

        void RemoveSingleObject(ref Ray ray)
        {
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit,1000,_ingoreCastLayerMask))
            {
                DestroyImmediate(hit.transform.gameObject);
            }
        }

        void Update()
        {
            
        }
    }
}


