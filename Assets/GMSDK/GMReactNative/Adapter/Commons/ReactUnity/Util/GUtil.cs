using System.IO;
using UnityEngine;

namespace GSDK.RNU
{
    public partial class Util
    {
        public static BundleInfo ParseUrlToBundleInfo(string url)
        {
            string urlPart = url.Substring(url.IndexOf("rupage?") + 7);

            string[] parts = urlPart.Split('&');

            BundleInfo bi = new BundleInfo();
            bi.Url = url;
            foreach (var part in parts)
            {
                string[] kv = part.Split('=');

                if (kv[0] == "rctModule")
                {
                    bi.ModuleName = kv[1];
                }

                if (kv[0] == "rctModuleParams")
                {
                    bi.Paramz = kv[1];
                }
            }

            bi.FilePathPrefix = GMReactUnityMgr.instance.preBundlePath + bi.ModuleName;
            return bi;
        }
    }
}
