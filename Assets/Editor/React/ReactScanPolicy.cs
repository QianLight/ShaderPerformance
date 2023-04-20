using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class ReactScanPolicy : ScanPolicy
    {
        public override string ScanType
        {
            get { return "React"; }
        }

        public override string ResExt
        {
            get { return "*.bytes"; }
        }
        public override ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            var react = result.Add(null, name, path);
            try
            {
                var reactData = XEditor.XDataIO<CFUtilPoolLib.XReactData>.singleton.DeserializeData(path);
                if (reactData != null)
                {
                    //> anim
                    if (!string.IsNullOrEmpty(reactData.ClipName))
                    {
                        result.Add(react, reactData.ClipName,
                                string.Format("{0}/{1}", AssetsConfig.instance.ResourcePath, reactData.ClipName + ".anim"));
                    }
                    if (!string.IsNullOrEmpty(reactData.ClipName2))
                    {
                        result.Add(react, reactData.ClipName2,
                                string.Format("{0}/{1}", AssetsConfig.instance.ResourcePath, reactData.ClipName2 + ".anim"));
                    }

                    //> avatar mask
                    if (!string.IsNullOrEmpty(reactData.AvatarMask))
                    {
                        result.Add(react, reactData.AvatarMask,
                                string.Format("{0}/{1}", AssetsConfig.instance.ResourcePath, reactData.AvatarMask + ".mask"));
                    }

                    //> fx
                    if (reactData.Fx != null && reactData.Fx.Count > 0)
                    {
                        for (int i = 0; i < reactData.Fx.Count; ++i)
                        {
                            if (!string.IsNullOrEmpty(reactData.Fx[i].Fx))
                            {
                                result.Add(react, reactData.Fx[i].Fx,
                                        string.Format("{0}/{1}", AssetsConfig.instance.ResourcePath, reactData.Fx[i].Fx + ".prefab"));
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("scan err : " + name);
                }
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(name + "_react_" + e.StackTrace);
            }
            return react;
        }
    }
}