using UnityEditor;

namespace UnityEngine.Rendering
{
    public abstract class VolumeRule : ScriptableObject
    {
        public string path;
        public abstract bool IsValid(SerializedProperty property);

        public abstract void GUI(SerializedObject serializedObject, SerializedProperty property);
    }
}