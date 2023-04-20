/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Zeus.Framework.KeepAlive
{
    public class UnityMobilePlatformKeepAlive
    {
        private static bool _allowTurnOff = true;

#if !UNITY_EDITOR && UNITY_ANDROID
        static AndroidJavaClass _KeepAliveManagerClass;
        /// <summary>
        /// 某些版本的Unity在子线程创建或者获取Java的类对象会报找不到类型的错误，
        /// 因此采用先在主线程获取类对象，再在子线程调用其函数的方式
        /// </summary>
        public static void GenAndroidJavaClass()
        {
            if (_KeepAliveManagerClass == null)
            {
                _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
            }
        }
        public static void DisposeAndroidJavaClass()
        {
            if (_KeepAliveManagerClass != null)
            {
                _KeepAliveManagerClass.Dispose();
                _KeepAliveManagerClass = null;
            }
        }
#elif !UNITY_EDITOR && UNITY_IOS
        [DllImport("__Internal")]
        public static extern void NativeTurnOn();
        [DllImport("__Internal")]
        public static extern void NativeTurnOff();
        [DllImport("__Internal")]
        public static extern bool NativeIsOn();
        [DllImport("__Internal")]
        public static extern void NativeSetMusicVolume(float volum);
        [DllImport("__Internal")]
        public static extern void NativeSetCustomMusicClips(string musicClips);
#endif

        public static void TurnOn(string notificationStr)
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
            if (_KeepAliveManagerClass == null)
            {
                _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
            }
            _KeepAliveManagerClass.CallStatic<AndroidJavaObject>("getInstance").Call("TurnOn",notificationStr);
#elif UNITY_IOS
            NativeTurnOn();
#endif
#endif
        }

        public static void TurnOff()
        {
            if(!_allowTurnOff)
            {
                return;
            }
#if !UNITY_EDITOR
#if UNITY_ANDROID
            if (_KeepAliveManagerClass == null)
            {
                _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
            }
            _KeepAliveManagerClass.CallStatic<AndroidJavaObject>("getInstance").Call("TurnOff");
#elif UNITY_IOS
            NativeTurnOff();
#endif
#endif
        }

        public static bool IsTurnOn()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
            if (_KeepAliveManagerClass == null)
            {
                _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
            }
            return _KeepAliveManagerClass.CallStatic<AndroidJavaObject>("getInstance").Call<bool>("IsOn");
#elif UNITY_IOS
            return NativeIsOn();
#endif
#endif
            return false;
        }

        public static bool IsWorking()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
            if (_KeepAliveManagerClass == null)
            {
                _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
            }
            return _KeepAliveManagerClass.CallStatic<AndroidJavaObject>("getInstance").Call<bool>("IsOpen");
#elif UNITY_IOS
            //TODO
#endif
#endif
            return true;
        }

        /// <summary>
        /// 是否允许关闭保活功能
        /// </summary>
        /// <param name="allow">true：允许关闭  false：不允许关闭</param>
        public static void SetAllowTurnOff(bool allow)
        {
            _allowTurnOff = allow;
        }

        /// <summary>
        /// 修改APP保活模块后台音乐音量，0.0f～1.0f，仅iOS平台生效
        /// </summary>
        /// <param name="volum"></param>
        public static void SetMusicVolume(float volum)
        {
            float oldVolum = volum;
            volum = Mathf.Clamp01(volum);
            if (oldVolum != volum)
            {
                UnityEngine.Debug.LogError("[KeepAlive] Music Volume Must Between 0 and 1, Cur Value:" + oldVolum);
            }
#if !UNITY_EDITOR
#if UNITY_ANDROID
#elif UNITY_IOS
            NativeSetMusicVolume(volum);
#endif
#endif
        }

        /// <summary>
        /// 自定义APP保活模块后台播放的音乐，支持多条音乐，使用“;”分隔，仅iOS平台生效
        /// </summary>
        /// <param name="musicClips"></param>
        public static void SetCustomMusicClips(string musicClips)
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
#elif UNITY_IOS
            NativeSetCustomMusicClips(musicClips);
#endif
#endif
        }

        public static int GetKeepAliveNotificationId()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
            if (_KeepAliveManagerClass == null)
            {
                _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
            }
            return _KeepAliveManagerClass.CallStatic<int>("GetKeepAliveNotificationId");
#elif UNITY_IOS
#endif
#endif
            return 0;
        }

        /// <summary>
        /// 设置推送的图标及其颜色。
        /// 原因：在国外某些手机上，app的推送的图标会出现纯白色的异常，原因可见以下网址，故增加smallicon和smallicon的rgb设置接口。 https://blog.csdn.net/SImple_a/article/details/103594842?utm_medium=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic&depth_1-utm_source=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic
        /// </summary>
        /// <param name="name">图标名</param>
        /// <param name="type">文件夹</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetNotificationSmallIcon(string name, string type, int r, int g, int b)
        {
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);
#if !UNITY_EDITOR
#if UNITY_ANDROID
        if (_KeepAliveManagerClass == null)
        {
            _KeepAliveManagerClass = new AndroidJavaClass("com.zeus.androidkeepalive.KeepAliveManager");
        }
        _KeepAliveManagerClass.CallStatic<AndroidJavaObject>("getInstance").Call("SetNotificationSmallIcon", name, type, r, g, b);
#elif UNITY_IOS
#endif
#endif
        }
    }
}