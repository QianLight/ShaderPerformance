/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
namespace Zeus.Framework.Asset
{
    public enum ChunkListStatus
    {
        Ready,//二包资源就绪
        MissingFile,//远端没找到二包资源
        NetError//客户端网络错误
    }
}
