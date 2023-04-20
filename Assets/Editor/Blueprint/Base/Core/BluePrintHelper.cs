using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CFUtilPoolLib;

namespace BluePrint
{
    class BluePrintHelper
    {
        public static string strNodeTag1 = "BluePrint/Tag1";
        public static string strNodeBackground = "BluePrint/Box12";
        public static string strNoteBackground = "BluePrint/Box13";
        public static string strNodeSelectBackground = "BluePrint/BoxHighlighter2";
        public static string strNodeHoverBackground = "BluePrint/BoxHighlighter3";
        public static string strNodeExcutedBackground = "BluePrint/BoxHighlighterBlue";
        public static string strDefaultNodeHeader = "BluePrint/Header2";
        public static string strPinMainConnected = "BluePrint/PinArrowRight";
        public static string strPinMainNoConnect = "BluePrint/PinArrowRight1";
        public static string strPinMainActived = "BluePrint/PinArrowRightActive";
        public static string strPinDataConnected = "BluePrint/PinData";
        public static string strPinDataNoConnect = "BluePrint/PinData";
        public static string strNodeTabOn = "BluePrint/TabOn";
        public static string strNodeTabOff = "BluePrint/TabOff";

        public static Color PinDataLineColor = new Color32(189, 39, 39, 255);
        public static Color PinMainNoActiveColor = Color.white;
        public static Color PinErrorLineColor = Color.red;
        public static Color PinMainActiveColor = Color.blue;

        public static GameObject FindGameObject(Transform root, string name)
        {
            Transform ret = null;

            if (root == null)
            {
                foreach (GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    Transform t = rootObj.transform;
                    ret = XCommon.singleton.FindChildRecursively(t, name);
                    if (ret != null) return ret.gameObject;
                }
            }
            else
            {
                ret = XCommon.singleton.FindChildRecursively(root, name);
                if (ret != null) return ret.gameObject;
            }
  
            return null;
        }

    }
}
