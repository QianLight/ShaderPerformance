using CFUtilPoolLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XEditor;

public class ClimbEditor : EditorWindow
{
    public const uint DownEnterTag = 1;
    public const uint UpEnterTag = 2;
    public const uint LadderTag = 4;
    public const uint AutoEnterTag = 8;
    public const uint AutoExitTag = 16;
    public const uint LineTag = 32;

    public static bool ClimbPointCheck = false;
    static GameObject[] goList = null;
    static HashSet<GameObject> checkSet = new HashSet<GameObject>();

    [MenuItem("XEditor/ClimbPoint/ClimbCheck")]
    static void CheckSwitch()
    {
        ClimbPointCheck = !ClimbPointCheck;

        EditorUtility.DisplayDialog("ClimbCheck", ClimbPointCheck ? "Open" : "Close", "OK");
    }

    public static void InitCheck()
    {
        if (!ClimbEditor.ClimbPointCheck) return;

        goList = GameObject.FindGameObjectsWithTag("ClimbPoint");
        checkSet.Clear();
    }

    public static void CheckPoint(double MinX, double MaxX, double MinZ, double MaxZ)
    {
        if (!ClimbEditor.ClimbPointCheck) return;
        if (goList == null) return;

        for (int i = 0; i < goList.Length; ++i)
        {
            Vector3 pos = goList[i].transform.position;
            if (MinX <= pos.x && pos.x < MaxX && MinZ <= pos.z && pos.z < MaxZ)
            {
                checkSet.Add(goList[i]);
            }
        }
    }

    public static void CheckResult()
    {
        if (!ClimbEditor.ClimbPointCheck) return;
        if (goList == null) return;

        float count = 0;

        for (int i = 0; i < goList.Length; ++i)
        {
            if(!checkSet.Contains(goList[i]))
            {
                goList[i].name = goList[i].name + "Error";
                ++count;
            }
        }

        EditorUtility.DisplayDialog("Check", count + " Error", "OK");
    }


    [MenuItem("XEditor/ClimbPoint/Build")]
    static void Build()
    {
        GameObject[] goList = GameObject.FindGameObjectsWithTag("ClimbPoint");
        Dictionary<GameObject, List<int>> dic = PreBuild(goList);
        string data = "";
        for (int i = 0; i < goList.Length; ++i)
        {
            if (i != 0) data += "\n";
            data += (int)(goList[i].transform.position.x * 100 + 0.5) + " ";
            data += (int)(goList[i].transform.position.y * 100 + 0.5) + " ";
            data += (int)(goList[i].transform.position.z * 100 + 0.5) + " ";
            data += (int)(XCommon.singleton.AngleToFloat(goList[i].transform.forward) * 100 + 0.5) + " ";
            data += uint.Parse(goList[i].name);

            if ((uint.Parse(goList[i].name) & LineTag) == LineTag)
            {
                Vector3 end = goList[i].transform.Find("End").position;
                data += " " + (int)(end.x * 100 + 0.5);
                data += " " + (int)(end.y * 100 + 0.5);
                data += " " + (int)(end.z * 100 + 0.5);
            }

            data += "\n";
            data += dic[goList[i]].Count;
            for (int j = 0; j < dic[goList[i]].Count; ++j)
            {
                data += " " + dic[goList[i]][j];
            }
        }

        string path = EditorUtility.SaveFilePanel("Select a file to save", XEditorPath.Cli, "temp.txt", "txt");

        if (!string.IsNullOrEmpty(path))
        {
            StreamWriter sw = File.CreateText(path);
            sw.WriteLine("{0}", data);
            sw.Flush();
            sw.Close();

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("ClimbPoint", "Success", "OK");
        }
    }

    static Dictionary<GameObject, List<int>> PreBuild(GameObject[] goList)
    {
        Dictionary<GameObject, List<int>> dic = new Dictionary<GameObject, List<int>>();

        bool isLine = false;

        Vector3 s1p0;
        Vector3 s1p1;
        Vector3 s2p0;
        Vector3 s2p1;

        for (int i = 0; i < goList.Length; ++i)
        {
            List<int> list = new List<int>();

            isLine = (uint.Parse(goList[i].name) & LineTag) == LineTag;
            s1p0 = goList[i].transform.position;
            s1p1 = isLine? goList[i].transform.Find("End").position: s1p0;

            for (int j = 0; j < goList.Length; ++j)
            {
                if (i == j) continue;

                if (!isLine && ((uint.Parse(goList[j].name) & LineTag) != LineTag))
                {
                    if (Vector3.Distance(goList[i].transform.position, goList[j].transform.position) <= XGloabelConfLibrary.ClimbMaxRange)
                    {
                        list.Add(j);
                    }
                }
                else
                {
                    s2p0 = goList[j].transform.position;
                    s2p1 = (uint.Parse(goList[j].name) & LineTag) == LineTag ? goList[j].transform.Find("End").position : s2p0;

                    if (XCommon.singleton.SqrDist3DSegmentToSegment(s1p0, s1p1, s2p0, s2p1) <= XGloabelConfLibrary.ClimbMaxRange * XGloabelConfLibrary.ClimbMaxRange)
                    {
                        list.Add(j);
                    }
                }
            }
            dic.Add(goList[i], list);
        }
        return dic;
    }

    static GameObject climbline;
    static Material _material;
    static List<GameObject> lineList = new List<GameObject>();
    [MenuItem("XEditor/ClimbPoint/ShowInScene")]
    static void ShowInScene()
    {
        ClearLine();
        climbline = new GameObject("ClimbLine");
        _material = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Editor/MapEditor/Res/GridMat.mat", typeof(Material)) as Material;
        GameObject[] goList = GameObject.FindGameObjectsWithTag("ClimbPoint");
        Dictionary<GameObject, List<int>> dic = PreBuild(goList);
        for (int i = 0; i < goList.Length; ++i)
        {
            if((uint.Parse(goList[i].name) & LineTag) == LineTag)
            {
                DrawLineMesh(goList[i].transform.position, goList[i].transform.Find("End").position, Color.yellow);
            }

            for (int j = 0; j < dic[goList[i]].Count; ++j)
            {
                int target = dic[goList[i]][j];
                if (i > target) continue;
                Vector3 start = (uint.Parse(goList[i].name) & LineTag) == LineTag ? 0.5f * (goList[i].transform.position + goList[i].transform.Find("End").position) : goList[i].transform.position;
                Vector3 end = (uint.Parse(goList[target].name) & LineTag) == LineTag ? 0.5f * (goList[target].transform.position + goList[target].transform.Find("End").position) : goList[target].transform.position;
                DrawLineMesh(start, end, Color.blue);
            }
        }
    }

    static void DrawLineMesh(Vector3 start, Vector3 end, Color color)
    {
        GameObject go = new GameObject(lineList.Count.ToString());
        go.transform.parent = climbline.transform;

        go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = new Material[] { _material };
        lineList.Add(go);
        MeshFilter mf = go.GetComponent<MeshFilter>();

        Mesh mMesh = new Mesh();
        mMesh.hideFlags = HideFlags.DontSave;
        mMesh.name = go.name;

        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color32> cols = new List<Color32>();
        List<int> indecies = new List<int>();

        Vector3[] factor = {
            new Vector3(0.1f, 0, 0.1f) ,
            new Vector3(-0.1f, 0, 0.1f),
            new Vector3(0.1f, 0.1f,0),
            new Vector3(-0.1f,0.1f,0)
        };

        int count = factor.Length * 4;
        for (int index = 0; index < count; index += 4)
        {

            verts.Add(start - factor[index / 4]);
            verts.Add(start + factor[index / 4]);
            verts.Add(end + factor[index / 4]);
            verts.Add(end - factor[index / 4]);


            indecies.Add(index + 0);
            indecies.Add(index + 1);
            indecies.Add(index + 2);

            indecies.Add(index + 2);
            indecies.Add(index + 3);
            indecies.Add(index + 0);

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(1f, 0f));

            for (int i = 0; i < 4; ++i)
                cols.Add(color);
        }


        mMesh.vertices = verts.ToArray();
        mMesh.triangles = indecies.ToArray();
        mMesh.uv = uvs.ToArray();
        mMesh.colors32 = cols.ToArray();

        mf.mesh = mMesh;
    }

    [MenuItem("XEditor/ClimbPoint/Clear")]
    static void ClearLine()
    {
        for (int i = 0; i < lineList.Count; ++i)
        {
            DestroyImmediate(lineList[i]);
        }
        lineList.Clear();

        DestroyImmediate(GameObject.Find("ClimbLine"));
    }

}
