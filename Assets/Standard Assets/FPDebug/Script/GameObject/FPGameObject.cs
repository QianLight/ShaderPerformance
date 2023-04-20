using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class FPGameObject
{
    public static int InstanceID;
    public static Scene CurrentScene;
    public static Dictionary<int, GameObject> CurrentList = new Dictionary<int, GameObject>();
    public static SCList GetObjectList()
    {
        CurrentList.Clear();
        SCList sList = new SCList();
        sList.List = new List<SC>();

        int sCount = SceneManager.sceneCount;
        for (int i = 0; i < sCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            sList.List.Add(GetScene(scene));
        }
        sList.List.Add(GetScene(CurrentScene));
        CurrentList.Remove(InstanceID);
        return sList;
    }
    static SC GetScene(Scene scene)
    {
        SC sc = new SC();
        sc.N = scene.name;
        sc.OList = new List<OI>();
        sc.PList = new List<PP>();
        GameObject[] os = scene.GetRootGameObjects();
        for (int j = 0; j < os.Length; j++)
        {
            GetOIList(sc.OList, sc.PList, os[j].transform, null);
        }
        return sc;
    }
    public static void GetOIList(List<OI> oList, List<PP> pList, Transform t, Transform p)
    {
        OI oi = new OI();
        oi.ID = t.gameObject.GetInstanceID();
        oi.PID = (p == null ? 0 : p.gameObject.GetInstanceID());
        oi.N = t.name;
        oi.E = t.gameObject.activeSelf ? 1 : 0;
        oi.Po = t.localPosition;
        oi.Ro = t.localRotation.eulerAngles;
        oi.Sc = t.localScale;
        MeshFilter filter = t.GetComponent<MeshFilter>();
        if (filter != null)
        {
            Mesh mesh = filter.sharedMesh;
            if (mesh != null)
            {
                oi.Si = mesh.bounds.size;
                oi.R = 1;
            }
        }

        UnityEngine.Rendering.Volume ppm = t.GetComponent<UnityEngine.Rendering.Volume>();
        if (ppm != null)
        {
            UnityEngine.Rendering.VolumeProfile vpf = ppm.profileRef;
            if (vpf != null)
            {
                PP pp = new PP();
                pp.ID = oi.ID;

                pp.VL = new List<VL>();
                foreach (var item in vpf.components)
                {
                    VL vl = new VL();
                    vl.Name = item.name;
                    vl.Enable = item.active;
                    pp.VL.Add(vl);
                }
                pList.Add(pp);
            }
        }

        oList.Add(oi);
        CurrentList[oi.ID] = t.gameObject;
        foreach (Transform i in t)
        {
            GetOIList(oList, pList, i, t);
        }
    }
}
