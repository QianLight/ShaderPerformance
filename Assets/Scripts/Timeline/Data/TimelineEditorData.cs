
#if UNITY_EDITOR
using UnityEngine;

public class TimelineEditorData : ScriptableObject
{    
    public string defaultSceneName;
    public int headLength;
    public Vector3 cameraStartPos;
    public Vector3 cameraStartRot;
}
#endif