#if UNITY_EDITOR
namespace CFEngine
{
    public static class HaltonSeq
    {
        public static float Get(int index, int radix)
        {
            float result = 0f;
            float fraction = 1f / (float)radix;

            while (index > 0)
            {
                result += (float)(index % radix) * fraction;

                index /= radix;
                fraction /= (float)radix;
            }

            return result;
        }
    }
}
#endif