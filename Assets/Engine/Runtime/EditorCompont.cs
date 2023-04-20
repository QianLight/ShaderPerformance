#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CFEngine
{
    public interface IEditorComponetInitor<out ClassTToInit> 
    {
        ClassTToInit Init();
        ClassTToInit GetComponent();
    }
    
    public interface IEditorComponetInitorSelectObject<out ClassTToInit> 
    {
        ClassTToInit Init(GameObject selectedGameObject);
        ClassTToInit GetComponent();
    }
    
    public interface IEditorComponet
    {
        void DrawGUI();
        void Destroy();
        string Name();
        public void GetEditorWindow(EditorWindow editorWindow);
    }
    public abstract class EditorMonoComponetSelectObject<ThisClassT> : MonoBehaviour,IEditorComponet 
        where ThisClassT: EditorMonoComponetSelectObject<ThisClassT> 
    {
        public class InitorSelectObject<T> :IEditorComponetInitorSelectObject<T> where T: EditorMonoComponetSelectObject<T>
        {
            private T _component;
            public T GetComponent()
            {
                return _component;
            }
            public T Init(GameObject selectedGameObject = null)
            {
                GameObject go = new GameObject();
                _component = go.AddComponent<T>();
                go.name = _component.Name();
                _component.Init(selectedGameObject);
                return _component;
            }
        }
        static public InitorSelectObject<ThisClassT> initor = new InitorSelectObject<ThisClassT>();

        public abstract void Init(GameObject selectedGameObject = null);
        public abstract void DrawGUI();
        public abstract void Destroy();
        public abstract string Name();

        protected EditorWindow _editorWindow;
        public void GetEditorWindow(EditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }
    }

    public abstract class EditorComponetSelectObject<ThisClassT> : IEditorComponet 
        where ThisClassT: EditorComponetSelectObject<ThisClassT>, new()
    {
        public class InitorSelectObject<T> :IEditorComponetInitorSelectObject<T> where T: EditorComponetSelectObject<T>, new()
        {
            private T _component;
            public T GetComponent()
            {
                return _component;
            }
            public T Init(GameObject selectedGameObject = null)
            {
                _component = new T();
                _component.Init(selectedGameObject);
                return _component;
            }
            
        }
        public static InitorSelectObject<ThisClassT> initor = new InitorSelectObject<ThisClassT>();
        public abstract void Init(GameObject selectedGameObject = null);
        public abstract void DrawGUI();
        public abstract void Destroy();
        public abstract string Name();
        
        protected EditorWindow _editorWindow;
        public void GetEditorWindow(EditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }
    }
    
    public abstract class EditorComponet<ThisClassT> : IEditorComponet 
        where ThisClassT: EditorComponet<ThisClassT>, new()
    {
        public class Initor<T> :IEditorComponetInitor<T> where T: EditorComponet<T>, new()
        {
            private T _component;
            public T GetComponent()
            {
                return _component;
            }
            public T Init()
            {
                _component = new T();
                _component.Init();
                return _component;
            }
            
        }
        public static Initor<ThisClassT> initor = new Initor<ThisClassT>();
        public abstract void Init();
        public abstract void DrawGUI();
        public abstract void Destroy();
        public abstract string Name();

        protected EditorWindow _editorWindow;
        public void GetEditorWindow(EditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
        }
    }

    public class PropertyEditor
    {
        private static Type _propertyEditor;
        private static MethodInfo _openPropertyEditor;
        public static void GetPropertyEditor()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type[] types;
            
            foreach (var assembly in assemblies)
            {
                types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass &&type.ToString().Equals("UnityEditor.PropertyEditor"))
                    {
                        _propertyEditor = type;
                    }
                    
                }
            }
            _openPropertyEditor = _propertyEditor.GetMethod("OpenPropertyEditor",BindingFlags.Static|BindingFlags.Public| BindingFlags.NonPublic);
        }

        public static void OpenPropertyEditor(UnityEngine.Object obj,bool showWindow = true)
        {
            object[] param = new object[2];
            param[0] = obj;
            param[1] = showWindow;
            if (_propertyEditor == null)
            {
                GetPropertyEditor();
            }
            _openPropertyEditor.Invoke(null,param);
        }
    }

    public static class EditorComponentGUI 
    {
        public static void DrawEditorField(string label, ref int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label,GUILayout.Width(150));
            value = EditorGUILayout.IntField(value);
            GUILayout.EndHorizontal();
        }
        public static void DrawEditorField(string label, ref string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label,GUILayout.Width(150));
            value = EditorGUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }
        public static void DrawEditorField(string label, ref Color value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label,GUILayout.Width(150));
            value = EditorGUILayout.ColorField(value);
            GUILayout.EndHorizontal();
        }

        public static class EditorField<T>
        {
            public delegate T DrawField(T value, params GUILayoutOption[] options);
        }

        public static void DrawEditorField<T>(string label, ref T value, EditorField<T>.DrawField drawField)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label,GUILayout.Width(150));
            value = drawField(value);
            GUILayout.EndHorizontal();
        }

        public delegate void GUIMethod();
        public static void DrawHorizontal(GUIMethod method)
        {
            GUILayout.BeginHorizontal();
            method();
            GUILayout.EndHorizontal();
        }
        public static void DrawVertical(GUIMethod method)
        {
            GUILayout.BeginVertical();
            method();
            GUILayout.EndVertical();
        }
        public static void DrawButton(string label, GUIMethod method, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(label, options))
            {
                method();
            }
        }
        
        /// <summary>
        /// Only be used in UNITY EDITOR! 仅在编辑器下使用！
        /// </summary>
        /// <param name="obj"></param>
        public static void EditorDestroy(this Object obj)
        {
            if (obj == null)
                return;
            
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
#endif