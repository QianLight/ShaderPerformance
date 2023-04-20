#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFEngine
{
    public class AntiResScanConfig:AssetBaseConifg<AntiResScanConfig>
    {
        public List<AntiResScanJob> jobList = new List<AntiResScanJob>();
    }
    [Serializable]
    public class AntiResScanJob
    {
        public string resourceFolder;
        public string targetFolder;
    }

}
#endif
