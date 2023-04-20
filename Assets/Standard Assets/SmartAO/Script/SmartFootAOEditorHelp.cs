#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
public class SmartFootAOEditorHelp
{
    public static Bounds8Point GetFootMaxBounds(Transform root, Bounds8Point bounds, Vector3 mid, float footHeight, bool left)
    {
        Bounds8Point localBounds = Bounds8World2Local(root, bounds);

        float deltaX = Mathf.Abs(localBounds.LocalExtents.x);
        float deltaY = Mathf.Abs(localBounds.LocalExtents.y);
        float deltaZ = Mathf.Abs(localBounds.LocalExtents.z);
        if (left)
        {
            localBounds.P0 = new Vector3(localBounds.LocalCenter.x - deltaX, footHeight, localBounds.LocalCenter.z - deltaZ);
            localBounds.P1 = new Vector3(mid.x, footHeight, localBounds.LocalCenter.z - deltaZ);
            localBounds.P2 = new Vector3(mid.x, footHeight, localBounds.LocalCenter.z + deltaZ);
            localBounds.P3 = new Vector3(localBounds.LocalCenter.x - deltaX, footHeight, localBounds.LocalCenter.z + deltaZ);

            localBounds.P4 = localBounds.LocalCenter + new Vector3(-deltaX, -deltaY, -deltaZ);
            localBounds.P5 = new Vector3(mid.x, localBounds.LocalCenter.y - deltaY, localBounds.LocalCenter.z - deltaZ);
            localBounds.P6 = new Vector3(mid.x, localBounds.LocalCenter.y - deltaY, localBounds.LocalCenter.z + deltaZ);
            localBounds.P7 = localBounds.LocalCenter + new Vector3(-deltaX, -deltaY, deltaZ);
        }
        else
        {
            localBounds.P0 = new Vector3(mid.x, footHeight, localBounds.LocalCenter.z - deltaZ);
            localBounds.P1 = new Vector3(localBounds.LocalCenter.x + deltaX, footHeight, localBounds.LocalCenter.z - deltaZ);
            localBounds.P2 = new Vector3(localBounds.LocalCenter.x + deltaX, footHeight, localBounds.LocalCenter.z + deltaZ);
            localBounds.P3 = new Vector3(mid.x, footHeight, localBounds.LocalCenter.z + deltaZ);

            localBounds.P4 = new Vector3(mid.x, localBounds.LocalCenter.y - deltaY, localBounds.LocalCenter.z - deltaZ);
            localBounds.P5 = localBounds.LocalCenter + new Vector3(deltaX, -deltaY, -deltaZ);
            localBounds.P6 = localBounds.LocalCenter + new Vector3(deltaX, -deltaY, deltaZ);
            localBounds.P7 = new Vector3(mid.x, localBounds.LocalCenter.y - deltaY, localBounds.LocalCenter.z + deltaZ);
        }
        return localBounds;
    }
    public static Bounds8Point InitFromSkin(SkinnedMeshRenderer sk)
    {
        Transform transform = sk.transform;
        Bounds localBounds = sk.bounds;
        Bounds8Point point = new Bounds8Point();
        Vector3 wpos = transform.position;
        Vector3 scale = transform.lossyScale;
        Quaternion rotation = transform.rotation;
        point.LocalCenter = localBounds.center;
        point.LocalExtents = localBounds.extents;
        //Vector3 center = new Vector3(localBounds.center.x * scale.x, localBounds.center.y * scale.y, localBounds.center.z * scale.z);
        //Vector3 extents = new Vector3(localBounds.extents.x * scale.x, localBounds.extents.y * scale.y, localBounds.extents.z * scale.z);
        //Debug.LogError(wpos + point.Center);
        float deltaX = Mathf.Abs(point.LocalExtents.x);
        float deltaY = Mathf.Abs(point.LocalExtents.y);
        float deltaZ = Mathf.Abs(point.LocalExtents.z);

        point.P0 = (point.LocalCenter + new Vector3(-deltaX, deltaY, -deltaZ));
        point.P1 = (point.LocalCenter + new Vector3(deltaX, deltaY, -deltaZ));
        point.P2 = (point.LocalCenter + new Vector3(deltaX, deltaY, deltaZ));
        point.P3 = (point.LocalCenter + new Vector3(-deltaX, deltaY, deltaZ));

        point.P4 = (point.LocalCenter + new Vector3(-deltaX, -deltaY, -deltaZ));
        point.P5 = (point.LocalCenter + new Vector3(deltaX, -deltaY, -deltaZ));
        point.P6 = (point.LocalCenter + new Vector3(deltaX, -deltaY, deltaZ));
        point.P7 = (point.LocalCenter + new Vector3(-deltaX, -deltaY, deltaZ));

        return point;
    }
    //public static Bounds8Point InitFromBounds(Bounds bounds)
    //{
    //    Bounds8Point point = new Bounds8Point();
    //    point.Center = bounds.center;
    //    point.Extents = bounds.extents;
    //    Debug.LogError(point.Center);
    //    float deltaX = Mathf.Abs(point.Extents.x);
    //    float deltaY = Mathf.Abs(point.Extents.y);
    //    float deltaZ = Mathf.Abs(point.Extents.z);

    //    point.P0 = point.Center + new Vector3(-deltaX, deltaY, -deltaZ);
    //    point.P1 = point.Center + new Vector3(deltaX, deltaY, -deltaZ);
    //    point.P2 = point.Center + new Vector3(deltaX, deltaY, deltaZ);
    //    point.P3 = point.Center + new Vector3(-deltaX, deltaY, deltaZ);

    //    point.P4 = point.Center + new Vector3(-deltaX, -deltaY, -deltaZ);
    //    point.P5 = point.Center + new Vector3(deltaX, -deltaY, -deltaZ);
    //    point.P6 = point.Center + new Vector3(deltaX, -deltaY, deltaZ);
    //    point.P7 = point.Center + new Vector3(-deltaX, -deltaY, deltaZ);

    //    return point;
    //}
    public static Bounds8Point Bounds8Local2World(Transform transform, Bounds8Point bounds)
    {
        Bounds8Point point = bounds;
        point.P0 = transform.TransformPoint(bounds.P0);
        point.P1 = transform.TransformPoint(bounds.P1);
        point.P2 = transform.TransformPoint(bounds.P2);
        point.P3 = transform.TransformPoint(bounds.P3);

        point.P4 = transform.TransformPoint(bounds.P4);
        point.P5 = transform.TransformPoint(bounds.P5);
        point.P6 = transform.TransformPoint(bounds.P6);
        point.P7 = transform.TransformPoint(bounds.P7);
        return point;
    }
    public static Bounds8Point Bounds8World2Local(Transform transform, Bounds8Point bounds)
    {
        Bounds8Point point = bounds;
        point.P0 = transform.InverseTransformPoint(bounds.P0);
        point.P1 = transform.InverseTransformPoint(bounds.P1);
        point.P2 = transform.InverseTransformPoint(bounds.P2);
        point.P3 = transform.InverseTransformPoint(bounds.P3);

        point.P4 = transform.InverseTransformPoint(bounds.P4);
        point.P5 = transform.InverseTransformPoint(bounds.P5);
        point.P6 = transform.InverseTransformPoint(bounds.P6);
        point.P7 = transform.InverseTransformPoint(bounds.P7);
        return point;
    }
    public static Transform FindObj(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }
        Transform result = null;
        foreach (Transform t in root)
        {
            result = FindObj(t, name);
            if (result != null)
            {
                break;
            }
        }
        return result;
    }

    void GetAABB(SkinnedMeshRenderer sk)
    {
        //SerializedObject obj = new SerializedObject(sk);
        //Debug.LogError(obj.FindProperty("collider").type);
        //调用方法的一些标志位，这里的含义是Public并且是实例方法，这也是默认的值
        BindingFlags flag = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic;

        PropertyInfo[] infos = sk.GetType().GetProperties();
        foreach (PropertyInfo info in infos)
        {
            if (info.Name.Contains("m_AABB"))
                Debug.LogError(info);
        }
    }

    public static Bounds8Point GetSliceBounds8(SkinnedMeshRenderer sk, Bounds8Point bounds, float ppm, List<Vector3> debugList = null)
    {
        //做模糊的时候是5个像素
        float extAdd = ppm * 5;
        Bounds8Point result = new Bounds8Point();

        List<Vector3> list = new List<Vector3>();
        Mesh mesh = sk.sharedMesh;
        Transform transform = sk.transform;
        if (mesh != null)
        {
            Vector3[] vs = mesh.vertices;
            for (int i = 0; i < vs.Length; i++)
            {
                Vector3 point = transform.TransformPoint(vs[i]);
                if (point.x >= bounds.P4.x && point.y >= bounds.P4.y && point.z >= bounds.P4.z
                    && point.x <= bounds.P2.x && point.y <= bounds.P2.y && point.z <= bounds.P2.z)
                {
                    list.Add(point);
                }
            }
        }

        Vector3[] nearCorners = new Vector3[4];
        Vector3[] farCorners = new Vector3[4];

        if(debugList != null)
        {
            debugList.AddRange(list);
        }

        Vector3 tmpV3;
        float minX = 0;
        float maxX = 0;

        float minY = 0;
        float maxY = 0;

        float minZ = 0;
        float maxZ = 0;
        for (int i = 0; i < list.Count; i++)
        {
            tmpV3 = list[i];
            if (i == 0)
            {
                minX = maxX = tmpV3.x;
                minY = maxY = tmpV3.y;
                minZ = maxZ = tmpV3.z;
                continue;
            }
            if (minX > tmpV3.x)
                minX = tmpV3.x;
            if (maxX < tmpV3.x)
                maxX = tmpV3.x;

            if (minY > tmpV3.y)
                minY = tmpV3.y;
            if (maxY < tmpV3.y)
                maxY = tmpV3.y;

            if (minZ > tmpV3.z)
                minZ = tmpV3.z;
            if (maxZ < tmpV3.z)
                maxZ = tmpV3.z;
        }
        maxX += extAdd;
        maxY += extAdd;
        maxZ += extAdd;

        minX -= extAdd;
        minY -= extAdd;
        minZ -= extAdd;
        nearCorners[0] = new Vector3(maxX, maxY, maxZ);
        nearCorners[1] = new Vector3(maxX, maxY, minZ);
        nearCorners[2] = new Vector3(minX, maxY, minZ);
        nearCorners[3] = new Vector3(minX, maxY, maxZ);

        farCorners[0] = new Vector3(maxX, minY, maxZ);
        farCorners[1] = new Vector3(maxX, minY, minZ);
        farCorners[2] = new Vector3(minX, minY, minZ);
        farCorners[3] = new Vector3(minX, minY, maxZ);


        result.P0 = nearCorners[0];
        result.P1 = nearCorners[1];
        result.P2 = nearCorners[2];
        result.P3 = nearCorners[3];
        result.P4 = farCorners[0];
        result.P5 = farCorners[1];
        result.P6 = farCorners[2];
        result.P7 = farCorners[3];
        return result;
    }

}
#endif
