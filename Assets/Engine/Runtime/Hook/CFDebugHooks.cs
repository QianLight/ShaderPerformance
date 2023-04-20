#if UNITY_EDITOR
using System.Diagnostics;
using System.Reflection;
using CFEngine;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public abstract class CFDebugHook
{
    public abstract string Name { get; }
    private bool installed;

    private SavedBool savedEnabled;

    private SavedBool SavedSavedEnabled
    {
        get
        {
            if (savedEnabled == null)
            {
                savedEnabled = new SavedBool($"{nameof(CFDebugHook)}.{GetType().FullName}.enabled");
            }

            return savedEnabled;
        }
    }

    public bool Enabled
    {
        get { return SavedSavedEnabled.Value; }
        set
        {
            if (value != SavedSavedEnabled.Value)
            {
                if (value)
                {
                    Install();
                }
                else
                {
                    Uninstall();
                }

                savedEnabled.Value = value;
            }
        }
    }

    public void Init()
    {
        InitInternal();
        if (Enabled)
        {
            Install();
        }
    }

    public void Install()
    {
        if (!installed)
        {
            CFHookManager.Install(Source, Replace, Proxy);
            installed = true;       
        }
    }

    public void Uninstall()
    {
        if (installed)
        {
            CFHookManager.Uninstall(Source);
            installed = false;       
        }
    }

    protected abstract MethodInfo Source { get; }
    protected abstract MethodInfo Replace { get; }
    protected abstract MethodInfo Proxy { get; }

    protected virtual void InitInternal()
    {
    }

    public virtual void OnGUI()
    {
        
    }

    protected void Log(string content, UnityEngine.Object context = null)
    {
        string stackTrace = new StackTrace().ToString();
        if (context)
        {
            Debug.Log($"[DebugHook] : {content}\n{stackTrace}", context);
        }
        else
        {
            Debug.Log($"[DebugHook] : {content}\n{stackTrace}");
        }
    }
}

public class CFDebugHookRendererEnabled : CFDebugHook
{
    public override string Name => $"{typeof(Renderer).FullName}.{nameof(Renderer.enabled)}";
    protected override MethodInfo Source => typeof(Renderer).GetProperty(nameof(Renderer.enabled)).GetSetMethod();
    protected override MethodInfo Replace => GetType().GetMethod(nameof(SetRendererEnabledReplace));
    protected override MethodInfo Proxy => GetType().GetMethod(nameof(SetRendererEnabledProxy));

    public void SetRendererEnabledReplace(bool enabled)
    {
        Renderer renderer = (object)this as Renderer;
        if (renderer)
        {
            Transform transform = renderer.transform;
            string path = EditorCommon.GetSceneObjectPath(transform);
            Log($"Renderer.set_enabled: value = {enabled}, path = {path}", transform);
            SetRendererEnabledProxy(enabled);       
        }
    }

    public void SetRendererEnabledProxy(bool enabled)
    {
    }
}

public class CFDebugHookTransformPosition : CFDebugHook
{
    public override string Name => "transform.set_position";
    protected override MethodInfo Source => typeof(Transform).GetProperty(nameof(Transform.position)).GetSetMethod();
    protected override MethodInfo Replace => GetType().GetMethod(nameof(SetTransformPositionReplace));
    protected override MethodInfo Proxy => GetType().GetMethod(nameof(SetTransformPositionProxy));

    private static SavedBool logCameraOnly =
        new SavedBool($"{nameof(CFDebugHookTransformPosition)}.{nameof(logCameraOnly)}");

    public void SetTransformPositionReplace(Vector3 position)
    {
        Transform transform = (object) this as Transform;
        if (!transform)
            return;

        string path = EditorCommon.GetSceneObjectPath(transform);
        if (logCameraOnly.Value)
        {
            Camera camera = transform.GetComponent<Camera>();
            if (camera)
            {
                Log($"transform.set_position: path = {path}, value = {position}, camera = {camera.name}");
            }
        }
        else
        {
            Log($"transform.set_position: path = {path}, value = {position}");
        }
        
        SetTransformPositionProxy(position);
    }

    public void SetTransformPositionProxy(Vector3 position)
    {
        
    }

    public override void OnGUI()
    {
        logCameraOnly.Value = EditorGUILayout.Toggle("Log Camera Only", logCameraOnly.Value);
    }
}
#endif