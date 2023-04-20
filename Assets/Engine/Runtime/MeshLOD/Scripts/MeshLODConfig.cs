using System.Collections.Generic;
using UnityEngine;

namespace MeshLOD
{
    [CreateAssetMenu(fileName = "MeshLODCOnfig", menuName = "MeshLODCOnfig", order = 0)]
    public class MeshLODConfig : ScriptableObject
    {
        public List<int> lodDistances;
    }
}