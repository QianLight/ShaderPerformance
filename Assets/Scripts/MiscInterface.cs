using CFEngine;
public class MiscInterface : IMiscInterface
{
    public void UnZip (string name)
    {
#if UNITY_ANDROID
        Android.UnZip (name);
#endif
    }

    public bool IsUnZipFinish (string name)
    {
#if UNITY_ANDROID
        return Android.IsZiped (name);
#else
        return true;
#endif
    }
}