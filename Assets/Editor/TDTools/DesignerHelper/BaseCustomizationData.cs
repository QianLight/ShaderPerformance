using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TDTools
{
    public class CustomizationDataConfig
    {
        [SerializeField]
        public List<CustomizationTableData> Table = new List<CustomizationTableData>();
        [SerializeField]
        public List<string> TablePath = new List<string>();
        [SerializeField]
        public List<string> TablePathDesc = new List<string>();
        [SerializeField]
        public List<CustomizationSceneData> Scene = new List<CustomizationSceneData>();
        [SerializeField]
        public List<int> IDHistory = new List<int>();
        [SerializeField]
        public int MaxHistory = 10;
    }

    public class CustomizationSceneData
    {
        [SerializeField]
        public string SceneName;
        [SerializeField]
        public string ScenePath;
        [SerializeField]
        public int index;
    }

    public class CustomizationTableData
    {
        [SerializeField]
        public string TableName;
        [SerializeField]
        public string TableDir;
        [SerializeField]
        public string TableFreezeRange;
        [SerializeField]
        public int index;
    }
}
