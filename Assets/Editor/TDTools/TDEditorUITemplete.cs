using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools
{
    static class TemplateContainerExtension
    {
        public static void BindAndRegister(this TemplateContainer tc, FileSelectorData fsd, EventCallback<MouseUpEvent> callback)
        {
            var so = new SerializedObject(fsd);
            tc.Bind(so);
            tc.Q<Button>("FileBtn").RegisterCallback(callback);
        }
    }

    public class FileSelectorData : ScriptableObject
    {
        public string FileTitle = "";
        public string FilePath = "";
    }
}

