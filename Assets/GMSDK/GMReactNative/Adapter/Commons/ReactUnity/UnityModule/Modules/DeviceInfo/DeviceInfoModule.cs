using System.Collections;
using UnityEngine;

namespace GSDK.RNU
{

    public class DeviceInfoModule : SimpleUnityModule
    {
        public static string NAME = "DeviceInfo";

        public override string GetName()
        {
            return NAME;
        }

        public override Hashtable GetConstants()
        {

            Rect parentRect = RNUMainCore.GetGameGoParentRect();
            Util.Log("parentRect zero {0}", parentRect == Rect.zero);
            float height = (parentRect == Rect.zero ? Screen.height: parentRect.height);
            float width = (parentRect == Rect.zero ? Screen.width: parentRect.width);

            Hashtable constants = new Hashtable();
            constants.Add("Dimensions", new Hashtable
                    {
                        {
                            "windowPhysicalPixels", new Hashtable
                                    {
                                        {"height", height},
                                        {"fontScale", 1},
                                        {"scale", 1},
                                        {"width", width},
                                        {"densityDpi", 420}
                                    }
                        },
                        {
                            "screenPhysicalPixels", new Hashtable
                                    {
                                        {"height", height},
                                        {"fontScale", 1},
                                        {"scale", 1},
                                        {"width", width},
                                        {"densityDpi", 420}
                                    }
                        },
                    }
            );
            return constants;
        }
    }
}
