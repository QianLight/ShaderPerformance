using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using CFEngine;

internal class XServerCurveMgr : MonoBehaviour
{
    [MenuItem(@"Assets/Server Curve/Generate Selected")]
    static void ServerCurve()
    {
        UnityEngine.Object[] os = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);

        foreach (UnityEngine.Object o in os)
            XServerCurveGenerator.GenerateCurve(o as GameObject);
    }

    [MenuItem(@"Assets/Server Curve/Generate All")]
    static void ServerCurveAll()
    {
        //UnityEngine.Object[] os = AssetDatabase.LoadAllAssetsAtPath(AssetsConfig.GlobalAssetsConfig.ResourcePath + "Curve");
        List<UnityEngine.Object> os = new List<UnityEngine.Object>();
        XResourceHelper.LoadAllAssets(AssetsConfig.instance.ResourcePath + "/Curve", os);

        foreach (UnityEngine.Object o in os)
            XServerCurveGenerator.GenerateCurve(o as GameObject);

		//ServerCurveTable ();

    }

	[MenuItem(@"Assets/Server Curve/Generate Client Table")]
	static public void ServerCurveTable()
	{
        //UnityEngine.Object[] os = AssetDatabase.LoadAllAssetsAtPath(AssetsConfig.GlobalAssetsConfig.ResourcePath + "Curve");
        List<UnityEngine.Object> os = new List<UnityEngine.Object>();
        XResourceHelper.LoadAllAssets(AssetsConfig.instance.ResourcePath + "/Curve", os);
		
		XServerCurveGenerator.GenerateCurveTable (os);
	}
}
