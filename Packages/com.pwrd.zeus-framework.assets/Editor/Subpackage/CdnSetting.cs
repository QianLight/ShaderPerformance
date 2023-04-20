/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;

namespace Zeus.Framework.Asset
{
    [System.Serializable]
    public class CdnSetting
    {
        public UploadUtil util;
        [SerializeField]
        public OSSSetting ossSetting;

        public CdnSetting()
        {
            util = UploadUtil.All;
            ossSetting = new OSSSetting();
        }
    }
}
