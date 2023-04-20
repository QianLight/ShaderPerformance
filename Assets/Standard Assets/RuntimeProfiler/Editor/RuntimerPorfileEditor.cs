using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(RuntimeProfilerCtrl))]
public class RuntimeProfilerCtrlInspector : Editor {
 
    RuntimeProfilerCtrl ctrl;
    public override void OnInspectorGUI(){
        ctrl=target as RuntimeProfilerCtrl;
        bool IsProfiler=EditorGUILayout.Toggle("IsProfiler",ctrl.IsProfile);
        if(ctrl.IsProfile!=IsProfiler){
            ctrl.IsProfile=IsProfiler;
        }
        base.DrawDefaultInspector();
    }
}
