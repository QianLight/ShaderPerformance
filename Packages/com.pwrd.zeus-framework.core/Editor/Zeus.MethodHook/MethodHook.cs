/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zeus.MethodHook
{
    public abstract class MethodHook
    {
        protected MethodHook()
        {

        }

        public static bool IsNet4OrAbove { get { return Environment.Version >= NetFxVersions.Net40; } }

        public static bool IsUnity3D { get; set; }

        static MethodHook s_MethodSwap;
        public static MethodHook Current
        {
            get
            {
                if (s_MethodSwap == null)
                {
                    if (IntPtr.Size == 4)
                    {
                        s_MethodSwap = new MethodHookX86();
                    }
                    else if (IntPtr.Size == 8)
                    {
                        s_MethodSwap = new MethodHookX64();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return s_MethodSwap;
            }
        }
#if UNSAFE
        public static void Swap(MethodInfo source, MethodInfo destination)
        {
            Current.SwapMethod(source, destination);
        }
#endif
        public static void Hook(MethodInfo source, MethodInfo destination)
        {
            Current.HookMethod(source, destination);
        }

        public static void UnHook(MethodInfo source)
        {
            Current.UnHookMethod(source);
        }

        public static void UnHookAll()
        {
            Current.UnHookAllMethods();
        }
#if UNSAFE
        protected static unsafe void SwapPointer(int* a, int* b)
        {
#if Debug
            byte* injInst = (byte*)*a;
            byte* tarInst = (byte*)*b;

            int temp = (((int)injInst + 5) + *(int*)(injInst + 1)) - ((int)tarInst + 5); ;

            *(int*)(injInst + 1) = (((int)tarInst + 5) + *(int*)(tarInst + 1)) - ((int)injInst + 5);

            *(int*)(tarInst + 1) = temp;
#else
            int temp = *a;
            *a = *b;
            *b = temp;
#endif
        }

        protected static unsafe void SwapPointer(long* a, long* b)
        {
#if Debug
            byte* aInst = (byte*)*a;
            byte* bInst = (byte*)*b;

            int temp = (((int)aInst + 5) + *(int*)(aInst + 1)) - ((int)bInst + 5); ;

            *(int*)(aInst + 1) = (((int)bInst + 5) + *(int*)(bInst + 1)) - ((int)aInst + 5);

            *(int*)(bInst + 1) = temp;
#else
            long temp = *a;
            *a = *b;
            *b = temp;
#endif
        }

        protected abstract void SwapMethod(MethodInfo source, MethodInfo destination);
#endif

        protected Dictionary<IntPtr, byte[]> m_HookMethodInstructionsBackupDictionary = new Dictionary<IntPtr, byte[]>();

        protected virtual void HookMethod(MethodInfo source, MethodInfo destination)
        {
            IntPtr sourcePtr = GetMethodPointer(source);
            IntPtr destPtr = GetMethodPointer(destination);

            if (!m_HookMethodInstructionsBackupDictionary.ContainsKey(sourcePtr))
            {
                //  Backup sourceInstructions to dictionary
                byte[] sourceInstructions = new byte[6];
                Marshal.Copy(sourcePtr, sourceInstructions, 0, 6);
                m_HookMethodInstructionsBackupDictionary.Add(sourcePtr, sourceInstructions);
            }
            else
            {
                //  UnHook
                UnHookMethod(sourcePtr);
            }

            if (m_HookMethodInstructionsBackupDictionary.ContainsKey(destPtr))
            {
                //  UnHook
                UnHookMethod(destPtr);
            }
            //  Hook
            var jump = JumpOpCodes(destPtr);
            Marshal.Copy(jump, 0, sourcePtr, jump.Length);
        }

        protected virtual void UnHookMethod(MethodInfo source)
        {
            IntPtr pointer = source.MethodHandle.GetFunctionPointer();
            if (m_HookMethodInstructionsBackupDictionary.ContainsKey(pointer))
            {
                UnHookMethod(pointer);
            }
        }

        protected virtual void UnHookMethod(IntPtr pointer)
        {
            byte[] sourceInstructions = m_HookMethodInstructionsBackupDictionary[pointer];
            Marshal.Copy(sourceInstructions, 0, pointer, sourceInstructions.Length);
            m_HookMethodInstructionsBackupDictionary.Remove(pointer);
        }

        protected virtual void UnHookAllMethods()
        {
            if (m_HookMethodInstructionsBackupDictionary.Count <= 0) return;

            IntPtr[] keys = m_HookMethodInstructionsBackupDictionary.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                UnHookMethod(keys[i]);
            }
        }

        protected static IntPtr GetMethodPointer(MethodInfo method)
        {
            RuntimeMethodHandle methodHandle = method.MethodHandle;
            RuntimeHelpers.PrepareMethod(methodHandle);
            return methodHandle.GetFunctionPointer();
        }

        protected static byte[] JumpOpCodes(IntPtr methodPtr)
        {
            byte[] currentPointerBytes = BitConverter.GetBytes(methodPtr.ToInt32());
            byte[] jumpOp =
            {
                0x68, currentPointerBytes[0], currentPointerBytes[1], currentPointerBytes[2], currentPointerBytes[3], 0xC3
            };
            return jumpOp;
        }

        protected byte[] BitCopy(byte[] bytes)
        {
            byte[] result = new byte[bytes.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = bytes[i];
            }
            return result;
        }

        protected string BitToString(byte[] bytes)
        {
            return string.Join(",", bytes.Select((b) => b.ToString("X00")).ToArray());
        }

#region NetFxVersions
        public static class NetFxVersions
        {
            public static readonly Version Net40 = new Version(4, 0, 30319, 42000);
            public static readonly Version Net35 = new Version(3, 5, 21022, 8);
            public static readonly Version Net35SP1 = new Version(3, 5, 30729, 1);
            public static readonly Version Net30 = new Version(3, 0, 4506, 30);
            public static readonly Version Net30SP1 = new Version(3, 0, 4506, 648);
            public static readonly Version Net30SP2 = new Version(3, 0, 4506, 2152);
            public static readonly Version Net20 = new Version(2, 0, 50727, 42);
            public static readonly Version Net20SP1 = new Version(2, 0, 50727, 1433);
            public static readonly Version Net20SP2 = new Version(2, 0, 50727, 3053);
        }
#endregion
    }
}
