using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Profiler = UnityEngine.Profiling.Profiler;

namespace com.pwrd.hlod.editor
{
    public class MemoryUtils
    {
        StringBuilder sb = new StringBuilder();

        public void Log()
        {
#if ENABLE_PROFILER

            sb.AppendLine("Profiler Statistic:");
            sb.AppendLine("GetMonoHeapSizeLong:" + SizeOf(UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong()));
            sb.AppendLine("GetTempAllocatorSize:" + SizeOf(UnityEngine.Profiling.Profiler.GetTempAllocatorSize()));
            sb.AppendLine("GetMonoUsedSizeLong:" + SizeOf(UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong()));
            sb.AppendLine("GetTotalAllocatedMemoryLong:" + SizeOf(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()));
            sb.AppendLine("GetTotalReservedMemoryLong:" + SizeOf(UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong()));
            sb.AppendLine("GetAllocatedMemoryForGraphicsDriver:" + SizeOf(UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver()));
            sb.AppendLine("GetTotalUnusedReservedMemoryLong:" + SizeOf(UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong()));

            HLODDebug.Log(sb);
#endif
        }

        private string SizeOf(long size)
        {
            var units = new string[] {"B", "Kb", "Mb", "Gb"};
            int index = 0;
            double value = size;
            while (value >= 512)
            {
                index++;
                value /= 1024;
            }

            return value.ToString("0.00") + units[index];
        }
    }
}