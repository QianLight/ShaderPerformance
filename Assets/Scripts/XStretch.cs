using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class XStretch : MonoBehaviour

{
//    public Vector2 offset_L = new Vector2(44,0);
//    public Vector2 offset_R = new Vector2(-44,0);
//    private RectTransform rt;
//    public bool mIsIphoneDevice = false;
//    /*
//    *iPhoneX: “iPhone10,3”, “iPhone10,6”
//    *iPhoneXR: “iPhone11,8”
//    *iPhoneXS: “iPhone11,2”
//    *iPhoneXS Max: “iPhone11,6”
//    *iPhone11 :iPhone12,1
//    *iPhone11 Pro:iPhone12,3
//    *iPhone11 Pro Max:iPhone12,5
//    */
//   // public string[] filterDevices = {"iPhone10,3","iPhone10,6","iPhone11,8","iPhone11,2","iPhone11,6","iPhone12,1","iPhone12,3","iPhone12,5"};
//    void Start()
//    {
//        rt = GetComponent<RectTransform>();
//        rt.anchorMin = Vector2.zero;
//        rt.anchorMax = Vector2.one;
//        rt.anchoredPosition = Vector2.zero;

//        mIsIphoneDevice = false;
//        string modelStr = SystemInfo.deviceModel;
//#if UNITY_IOS
//            for(int i = 0,length = filterDevices.Length;i < length ;i++){
//                if(mIsIphoneDevice) break;
//                mIsIphoneDevice = modelStr.Equals(filterDevices[i]);
//            }      
//#endif
//            Rebuild();
//    }
    
     
//    public void Rebuild(bool isIphoneDevice)
//        {
//            if (mIsIphoneDevice == isIphoneDevice) return;
//            mIsIphoneDevice = isIphoneDevice;
//            Rebuild();
//        }
//    private void Rebuild()
//    {
//            if (!Application.isPlaying) return;
//            if(mIsIphoneDevice)
//            {   
//                rt.offsetMin = offset_L;
//                rt.offsetMax = offset_R;
//            }
//            else
//            {
//                rt.offsetMin = Vector2.zero;
//                rt.offsetMax = Vector2.zero;
//            }
//    }
}