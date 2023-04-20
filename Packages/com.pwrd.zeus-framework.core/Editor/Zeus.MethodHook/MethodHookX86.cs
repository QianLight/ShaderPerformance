/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Zeus.MethodHook
{
    internal class MethodHookX86 : MethodHook
    {
        internal MethodHookX86() : base()
        {
        }
#if UNSAFE
        protected override void SwapMethod(MethodInfo source, MethodInfo destination)
        {
            RuntimeHelpers.PrepareMethod(source.MethodHandle);
            RuntimeHelpers.PrepareMethod(destination.MethodHandle);
            unsafe
            {
                int offset = IsUnity3D ? 0 : 2;
                int* src = (int*)source.MethodHandle.Value.ToPointer() + offset;
                int* dest = (int*)destination.MethodHandle.Value.ToPointer() + offset;
                SwapPointer(src, dest);
            }
        }
#endif
    }
}
