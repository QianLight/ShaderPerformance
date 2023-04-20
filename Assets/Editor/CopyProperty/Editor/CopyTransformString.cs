using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;

public class CopyTransformString
{
    [MenuItem("GameObject/Copy Transform Property", priority = 39)]
    static void CopyTransform()
    {
        if (Selection.activeObject != null)
        {
            GameObject obj = Selection.activeObject as GameObject;
            string str = string.Empty;
            str += obj.transform.localPosition.x + "=" + obj.transform.localPosition.y + "=" + obj.transform.localPosition.z;
            str += "|" + obj.transform.localEulerAngles.x + "=" + obj.transform.localEulerAngles.y + "=" + obj.transform.localEulerAngles.z;
            str += "|" + obj.transform.localScale.x + "=" + obj.transform.localScale.y + "=" + obj.transform.localScale.z;
            GUIUtility.systemCopyBuffer = str;
            Debug.Log("Parent point is : " + obj.name + " " + obj.transform.position);
        }
    }

    [MenuItem("GameObject/Copy Transform Canvas Path", priority = 40)]
    static void CopyTransformPath()
    {
        if (Selection.activeObject != null)
        {
            GameObject go = Selection.activeObject as GameObject;
            string str = string.Empty;
            Transform tr = go.transform;
            string name = tr.name;
            string firstChar = name[0].ToString().ToLower();
            name = firstChar + name.Substring(1);
            string varName = "m_" + name;
            while (!tr.parent.name.Contains("Canvas"))
            {
                str = tr.name + "/" + str;
                tr = tr.parent;
            }
            str = str.TrimEnd('/');
            string format = "Get(ref {0}, \"{1}\");";
            str = string.Format(format, varName, str);
            GUIUtility.systemCopyBuffer = str;
            Debug.LogError(str);
        }
    }

    [MenuItem("GameObject/Copy Transform Full Path", priority = 41)]
    static void CopyTransformFullPath()
    {
        if (Selection.activeObject != null)
        {
            Transform target = Selection.activeTransform;
            Transform parent = target;
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Insert(0, '/');
                }
                stringBuilder.Insert(0, parent.name);
                parent = parent.parent;
                if (!parent)
                {
                    break;
                }
            }
            string path = stringBuilder.ToString();
            GUIUtility.systemCopyBuffer = path;
            Debug.Log($"Copy success, {target} path is {path}", target);
        }
    }
}
