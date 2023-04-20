#if UNITY_EDITOR
#if UNITY_IPHONE

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Devops.Performance
{
public class BuildHelper
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            var targetGUID = project.GetUnityFrameworkTargetGuid();

            project.AddFrameworkToProject(targetGUID, "libz.tbd", false);

            project.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");

            File.WriteAllText(projectPath, project.WriteToString());
        }
    }
}
}
#endif
#endif