/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;

namespace Zeus.Core
{
    public class WaitForYesOrNo : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return !_IsAnswer;
            }
        }

        public bool IsYes
        {
            get
            {
                return _Yes;
            }
        }

        private bool _IsAnswer;
        private bool _Yes;

        public void YesOrNo(bool yes)
        {
            _IsAnswer = true;
            _Yes = yes;
        }
    }
}
