/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;

namespace Zeus.Core
{
    public class WaitForTrigger : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return !_IsTrigger;
            }
        }

        private bool _IsTrigger;

        public void Trigger()
        {
            _IsTrigger = true;
        }
    }
}
