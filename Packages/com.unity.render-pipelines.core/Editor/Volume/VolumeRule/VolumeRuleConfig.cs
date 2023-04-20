using System.Collections.Generic;

namespace UnityEngine.Rendering
{
    [CreateAssetMenu(menuName = "URP/规范资源")]
    public class VolumeRuleConfig : ScriptableObject
    {
        public List<VolumeRule> rules = new List<VolumeRule>();
    }
}