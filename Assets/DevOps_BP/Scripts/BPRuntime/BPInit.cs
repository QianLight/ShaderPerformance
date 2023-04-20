using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blueprint;

namespace Blueprint
{
    [DisallowMultipleComponent]
    public class BPInit : MonoBehaviour
    {
        private void Awake()
        {
            Init();
        }

        public static void Init()
        {
            BPClassManager.Init();
            DelayControl.Init();
            Blueprint.Asset.BpAssetManager.Init();
            PoolManager.Init();
        }
    }
}