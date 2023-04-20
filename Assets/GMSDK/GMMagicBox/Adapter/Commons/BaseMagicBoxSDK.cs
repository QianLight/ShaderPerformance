using System;
using System.Collections;
using System.Collections.Generic;
using UNBridgeLib.LitJson;
using GMSDK;
using UnityEngine;
using UNBridgeLib;

namespace GMSDK
{

    public class BaseMagicBoxSDK
    {

        public BaseMagicBoxSDK()
        {
#if UNITY_ANDROID
            UNBridge.Call(MagicBoxMethodName.Init, null);
#endif
        }


        /// <summary>
        /// 开启MagicBox
        /// </summary>
        public void ShowMagicBox()
        {
            UNBridge.Call(MagicBoxMethodName.Show, new JsonData());
        }

        /// <summary>
        /// 关闭MagicBox
        /// </summary>
        public void HideMagicBox()
        {
            UNBridge.Call(MagicBoxMethodName.Hide, new JsonData());
        }
    }
}
