using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class JoystickEditor : MonoBehaviour
{

    private static Dictionary<int, int> m_functionDictInPersistentFile = new Dictionary<int, int>();
    private static string m_sdkFile;

    [MenuItem("Help/LuaUTF8")]
    private static void LuaUTF8()
    {
        string path = Application.streamingAssetsPath + "/lua";
        DirectoryInfo dir = new DirectoryInfo(path);
        ConvertToUTF8(dir);
    }

    /// <summary>
    /// 将lua文件保存为utf8格式
    /// </summary>
    /// <param name="dir"></param>
    public static void ConvertToUTF8(DirectoryInfo dir)
    {

        FileInfo[] allFile = dir.GetFiles();
        foreach (FileInfo item in allFile)
        {
            if (item.FullName.EndsWith(".lua.txt"))
            {
                //if (item.FullName.Equals(@"H:\op\res\OPProject\Assets\StreamingAssets\lua\framework\common\event.lua.txt"))
                {
                    Debug.Log(item.FullName);
                    string content = File.ReadAllText(item.FullName);
                    var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                    using (var sink = new StreamWriter(item.FullName, false, utf8WithoutBom))
                    {
                        sink.Write(content);
                    }
                    //File.WriteAllText(item.FullName, content, Encoding.UTF8);
                }
            }
        }

        DirectoryInfo[] allDir = dir.GetDirectories();
        foreach (DirectoryInfo d in allDir)
        {
            ConvertToUTF8(d);
        }
    }

    [MenuItem("Help/UseSDK")]
    private static void UseSDK()
    {
        ReadFile();
        m_functionDictInPersistentFile[2] = 1;
        WriteFile();
    }

    [MenuItem("Help/DontUseSDK")]
    private static void DontUseSDK()
    {
        ReadFile();
        m_functionDictInPersistentFile[2] = 0;
        WriteFile();
    }

    private static void ReadFile()
    {
        try
        {
            m_sdkFile = Path.Combine(Application.persistentDataPath, "sdk/sdk.txt");
            if (!File.Exists(m_sdkFile))
            {
                File.Create(m_sdkFile);
            }
            string[] strs = File.ReadAllLines(m_sdkFile);
            if (strs != null)
            {
                for (int i = 0; i < strs.Length; ++i)
                {
                    string[] colums = strs[i].Split('=');
                    int key = int.Parse(colums[0]);
                    int value = int.Parse(colums[1]);
                    m_functionDictInPersistentFile[key] = value;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("read sdk failed! " + ex.ToString());
        }
    }

    private static void WriteFile()
    {
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, "sdk");
            m_sdkFile = Path.Combine(Application.persistentDataPath, "sdk/sdk.txt");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(m_sdkFile))
            {
                FileStream stream = File.Create(m_sdkFile);
                stream.Close();
            }
            string[] strs = new string[m_functionDictInPersistentFile.Count];
            int i = 0;
            foreach (var item in m_functionDictInPersistentFile)
            {
                strs[i] = item.Key + "=" + item.Value;
                i++;
            }
            File.WriteAllLines(m_sdkFile, strs);
        }
        catch (Exception ex)
        {
            Debug.LogError("write sdk failed! " + ex.ToString());
        }
    }


    [MenuItem("Help/StartPoint")]
    private static void CreateStartPoint()
    {
        GameObject startPoint = null;
        startPoint = GameObject.Find("startPoint");
        if(startPoint == null)
        {
            startPoint = new GameObject("startPoint");
            startPoint.transform.position = Vector3.zero;
            startPoint.transform.localScale = Vector3.one;
            startPoint.transform.localEulerAngles = Vector3.zero;
        }
        StartGame startGameComp = startPoint.GetComponent<StartGame>();
        if (startGameComp == null) startGameComp = startPoint.AddComponent<StartGame>();

        if(Camera.main != null)
        {
            CameraController cameraControllerComp = Camera.main.GetComponent<CameraController>();
            if (cameraControllerComp == null) cameraControllerComp = Camera.main.gameObject.AddComponent<CameraController>();
        }
        else
        {
            Debug.LogError("no Main Camera!");
        }

        GameObject Chunk_0_0 = GameObject.Find("Chunk_0_0");
        if(Chunk_0_0 != null)
        {
            Transform firstChild = Chunk_0_0.transform.GetChild(0);
            if(firstChild != null)
            {
                startPoint.transform.position = firstChild.position + new Vector3(0, 4, 0);
            }
        }
    }


    [MenuItem("Help/AnimatorCount")]
    private static void AnimatorCount()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[]; 
        int count = 0;
        int enableCount = 0;
        int disableCount = 0;
        foreach (GameObject child in objs)    
        {
            Animator animator = child.GetComponent<Animator>();
            if(animator != null)
            {
                count++;
                Debug.Log(child.gameObject.name);
                if (animator.enabled) enableCount++;
                else disableCount++;
            }
        }
        Debug.Log("AnimatorCount = " + count + "  enableCount=" + enableCount + "  disableCount=" + disableCount);
    }

    [MenuItem("Help/TimelineAnimatorCount")]
    private static void TimelineAnimatorCount()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        int count = 0;
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                string path = TransformPath(child);
                if(!path.Contains("EditorScene") && (path.Contains("Orignal_") || path.Contains("timeline_Role_Root")))
                {
                    count++;
                    Debug.LogError(path);
                }
            }
        }
        Debug.Log("TimelineAnimatorCount = " + count);
    }

    [MenuItem("Help/AnimatorEnableCount")]
    private static void AnimatorEnableCount()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        int count = 0;
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null && animator.enabled)
            {
                count++;
                Debug.Log(child.gameObject.name);
            }
        }
        Debug.Log("AnimatorCount = " + count + "  enableCount=" + count);
    }

    [MenuItem("Help/AnimatorControllerNotNullCount")]
    private static void AnimatorControllerNotNullCount()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        int count = 0;
        int enableCount = 0;
        int disableCount = 0;
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                count++;
                if (animator.enabled) enableCount++;
                else disableCount++;
                Debug.Log(child.gameObject.name);
            }
        }
        Debug.Log("AnimatorCount = " + count + "  enableCount=" + enableCount + "  disableCount=" + disableCount);
    }

    [MenuItem("Help/AnimatorEnable")]
    private static void AnimatorEnable()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
            }
        }
        Debug.Log("animator enable!");
    }

    [MenuItem("Help/AnimatorDisable")]
    private static void AnimatorDisable()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }
        }

        Debug.Log("animator disable!");
    }

    [MenuItem("Help/AnimatorDisableControllerNotNull")]
    private static void AnimatorDisableControllerNotNull()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.enabled = false;
            }
        }
        Debug.Log("animator disable!");
    }

    [MenuItem("Help/AnimatorDisableControllerNull")]
    private static void AnimatorDisableWhenAuntimeAnimatorControllerIsNotNull()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                animator.enabled = false;
            }
        }
        Debug.Log("animator disable!");
    }


    [MenuItem("Help/AlwaysAnimate")]
    private static void AnimatorAlwaysAnimate()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }
        Debug.Log("AnimatorCullingMode.AlwaysAnimate!");
    }

    [MenuItem("Help/CullUpdateTransforms")]
    private static void AnimatorCullUpdateTransforms()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }
        }
        Debug.Log("AnimatorCullingMode.CullUpdateTransforms!");
    }

    [MenuItem("Help/CullCompletely")]
    private static void AnimatorCullCompletely()
    {
        GameObject[] objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject child in objs)
        {
            Animator animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
            }
        }
        Debug.Log("AnimatorCullingMode.CullCompletely!");
    }

    static string TransformPath(GameObject go)
    {
        if (go != null)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while(parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }
        return string.Empty;
    }
}
