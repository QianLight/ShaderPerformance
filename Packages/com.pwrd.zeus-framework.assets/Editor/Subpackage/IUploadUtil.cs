/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;

public interface IUploadUtil
{
    void OnGUI();
    void UploadBundle(string folder);
    void UploadBundle();
}