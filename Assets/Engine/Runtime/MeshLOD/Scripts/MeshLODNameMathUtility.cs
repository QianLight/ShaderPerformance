using System.Collections.Generic;

namespace MeshLOD
{
    public class MeshLODNameMathUtility
    {
        private static MeshLODNameMathUtility _instance;

        public static MeshLODNameMathUtility Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MeshLODNameMathUtility();
                }

                return _instance;
            }
        }

        private readonly Dictionary<string, List<string>> _specialNameDic;

        public MeshLODNameMathUtility()
        {
            _specialNameDic = new Dictionary<string, List<string>>();
            
            _specialNameDic.Add("OP_C1JDBD_03_wujian 1", new List<string>()
            {
                "OP_C1JDBD_03_wujian 1",
                "OP_C1JDBD_03_wujian_LOD1",
                "OP_C1JDBD_03_wujian_LOD2"
            });
            
            _specialNameDic.Add("OP_C1JDBD_03_wujian", new List<string>()
            {
                "OP_C1JDBD_03_wujian",
                "OP_C1JDBD_03_wujian_LOD1 1",
                "OP_C1JDBD_03_wujian_LOD2 1"
            });
        }

        public bool CheckIsSpecialObj(string srcName)
        {
            if (_specialNameDic == null)
            {
                return false;
            }

            return _specialNameDic.ContainsKey(srcName); 
        }
        
        public string GetLODName(string srcName, int lodLevel)
        {
            if (!_specialNameDic.ContainsKey(srcName))
            {
                return string.Empty;
            }

            List<string> lodNameList = _specialNameDic[srcName];
            if (lodNameList == null || lodNameList.Count <= lodLevel)
            {
                return string.Empty;
            }
            
            return lodNameList[lodLevel];
        }
    }
}