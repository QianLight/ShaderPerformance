using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;
using CFEngine;
using XLevel;

public class LevelTools : XSingleton<LevelTools>, ILevelInterface
{
    private LevelGridDrawer m_drawer = new LevelGridDrawer();

    public bool Deprecated
    {
        get;
        set;
    }

    public void DrawMapGrid(List<SceneChunk> chunks)
    {
        m_drawer.DrawMapGrid(chunks);
    }

    public void DrawMapGrid(string sceneName)
    {
        m_drawer.DrawMapGrid(sceneName);
    }

    public void ClearMapGrid()
    {
        m_drawer.Clear();
    }

    public void DrawWayPoints(List<Vector3> points)
    {
        m_drawer.DrawWayPoints(points);
    }

    public void ClearWayPoints()
    {
        m_drawer.ClearWayPoints();
    }
}