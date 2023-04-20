using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
public class AssetCollectBuildProcessor : IPreprocessBuildWithReport
{
    // Start is called before the first frame update
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        WriteAllTextIfNoExists("Assets/Resources/BuildTimestamp", report.summary.buildStartedAt.Ticks.ToString());
    }

    void WriteAllTextIfNoExists(string path, string content)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, content);
    }
}
