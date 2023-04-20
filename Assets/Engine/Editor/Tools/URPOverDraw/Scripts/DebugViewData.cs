//
// URP Debug Views for Unity
// (c) 2019 PH Graphics
//

using UnityEngine;

namespace URPDebugViews
{
    //[CreateAssetMenu(fileName = "Data", menuName = "Tools/Debug View Data", order = 1)]
    public class DebugViewData : ScriptableObject
    {
        public Material MatTransparent;
        public Material MatQueue;
        [HideInInspector]
        public bool RenderObjectsNormallyBefore = false;
    }
}