#if GM_PROFILER_TEST_ON
using UnityEngine.Profiling;

#endif

namespace GSDK
{
    public class GSDKProfilerTools
    {
        public static void BeginSample(string tag)
        {
#if GM_PROFILER_TEST_ON
            Profiler.BeginSample(tag);
#endif
        }

        public static void EndSample()
        {
#if GM_PROFILER_TEST_ON
            Profiler.EndSample();
#endif
        }
    }
}