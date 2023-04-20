using System.Collections.Generic;
using JetBrains.Annotations;

namespace Standard_Assets.Performance
{
    public class AndroidGPUType
    {
        public static List<string[]> AdrenoGPU = new List<string[]>()
        {
            new[] {"660", "650", "642", "640"},
            new[] {"630"},
            new[] {"540", "530", "620"}
        };
        public static List<string[]> MaliGPU = new List<string[]>()
        {
            new[] {"G78MP24", "G78MP22", "G78MP10", "G76MP16"},
            new[] {"G77MC9", "G57MP6"},
            new[] {"G77MC7", "G72MP12", "G76MP10", "G76MP5", "G76MP4", "G57MC5", "G52MP6"}
        };
        
    }
    
}