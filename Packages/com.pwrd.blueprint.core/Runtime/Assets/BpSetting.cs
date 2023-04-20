using UnityEngine;

namespace Blueprint.Asset
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BpSetting", order = 1)]
    public class BpSetting : ScriptableObject
    {
        public string loaderClassName;
    }
}

