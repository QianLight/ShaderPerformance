/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

namespace Zeus.Framework.Asset
{
    public static partial class AwaiterExtensions
    {
        public static AsyncAwaitOperationAwaiter<T> GetAwaiter<T>(this T operation) where T : AsyncAwaitOperation
        {
            return AsyncAwaitOperationAwaiter<T>.GetAwaiter(operation);
        }
    }
}