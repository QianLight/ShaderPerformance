#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace CFEngine
{
    public class ReDirectRes
    {
        public string physicDir = "";
        public int logicPathType = 0;
        public static int LogicPath_Common = 0;
        public static int LogicPath_SceneRes = 1;
    }
    public class ResStatistic
    {
        public string type;
        public List<ResRedirectInfo> res = new List<ResRedirectInfo> ();
    }
    public class ResAssets
    {
        public List<string> resNames = new List<string> ();
        public Dictionary<string, ReDirectRes> editorResReDirect = new Dictionary<string, ReDirectRes> ();

        public void AddResReDirct (UnityEngine.Object obj, string assetName)
        {
            if (obj != null)
            {
                string path = AssetDatabase.GetAssetPath (obj);

                string dir = Path.GetDirectoryName (path);
                dir = dir.Replace ("\\", "/");
                string name = assetName + Path.GetExtension (path).ToLower ();
                if (!editorResReDirect.ContainsKey (name))
                {
                    editorResReDirect[name] = new ReDirectRes ()
                    {
                        physicDir = dir + "/"
                    };
                }
            }
        }

        public void AddResReDirct (string dir,
            string assetNameWithExt,
            int logicPathType,
            bool outputError = true)
        {
            if (File.Exists (dir + assetNameWithExt))
            {
                ReDirectRes reDirect;
                if (editorResReDirect.TryGetValue (assetNameWithExt, out reDirect))
                {
                    if (reDirect.physicDir != dir)
                    {
                        DebugLog.AddErrorLog2 ("res:{0} already exist old value {1},add valule {2}",
                            assetNameWithExt, reDirect.physicDir, dir);
                    }

                }
                else
                {
                    editorResReDirect[assetNameWithExt] = new ReDirectRes ()
                    {
                        physicDir = dir,
                        logicPathType = logicPathType
                    };
                }
            }
            else
            {

                if (outputError)
                    DebugLog.AddEngineLog2 ("res not exist:{0}", dir + assetNameWithExt);
                else
                    DebugLog.AddEngineLog2 ("res not exist:{0}", dir + assetNameWithExt);
            }
        }

        public int AddResName (string str)
        {
            if (!string.IsNullOrEmpty (str) &&
                !resNames.Contains (str))
            {
                resNames.Add (str);
                return resNames.Count - 1;
            }

            return -1;
        }

        public void SaveHeadString (BinaryWriter bw)
        {
            bw.Write (resNames.Count);
            for (int i = 0; i < resNames.Count; ++i)
            {
                bw.Write (resNames[i]);
            }
        }

        public int SaveStringIndex (BinaryWriter bw, string str)
        {
            short index = (short) resNames.IndexOf (str);
            if (index < 0 && !string.IsNullOrEmpty (str))
            {
                DebugLog.AddErrorLog2 ("not find str:{0}", str);
            }
            bw.Write (index);
            return index;
        }
    }
}
#endif