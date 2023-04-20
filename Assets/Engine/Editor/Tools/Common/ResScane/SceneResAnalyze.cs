using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class SceneResScanPolicy : ScanPolicy
    {
        public override string ScanType
        {
            get { return "Scene"; }
        }
        public override string ResExt
        {
            get { return "*.scenebytes"; }
        }

        public override void Prepare()
        {

        }
        public override ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            var scene = result.Add(null, name, path);
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
                    br.ReadInt32();//version
                    br.ReadInt32();//head length
                    br.ReadInt32(); //chunk count
                    br.ReadInt32(); //splat count
                    int count = br.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        string nameWithExt = br.ReadString();
                        string physicDir = br.ReadString(); //physicDir
                        br.ReadInt32(); //dir type
                        result.Add(scene, nameWithExt, physicDir + nameWithExt);
                    }
                    //mat path
                    count = br.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        br.ReadString();//key
                        int c = br.ReadInt32();
                        for (int j = 0; j < c; ++j)
                        {
                            br.ReadUInt32();//key
                            br.ReadString();//value
                        }
                    }
                    count = br.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        br.ReadString();//ds name
                        int c = br.ReadInt32();
                        for (int j = 0; j < c; ++j)
                        {
                            br.ReadUInt32();//hash
                            br.ReadString();
                            //string sfxPath = string.Format("{0}Runtime/SFX/{1}.bytes",
                            //    LoadMgr.singleton.BundlePath, sfxname);
                            //result.Add(scene, sfxname + ".bytes", sfxPath);
                        }
                    }
                    if (fs.Position < fs.Length)
                    {
                        count = br.ReadInt32();
                        for (int i = 0; i < count; ++i)
                        {
                            var sfxname = br.ReadString();
                            string sfxPath = string.Format("{0}Runtime/SFX/{1}.bytes",
                                LoadMgr.singleton.BundlePath, sfxname);
                            result.Add(scene, sfxname + ".bytes", sfxPath);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(name + "_scene_" + e.StackTrace);
            }
            return scene;
        }
    }
}