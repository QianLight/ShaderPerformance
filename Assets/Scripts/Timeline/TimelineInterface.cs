using CFUtilPoolLib;


public interface ICsFmod
{
    void SetParam(AudioChannel channel, string key, float param);
}

public interface IDisplayTrack
{
    void GUIDisplayName();
}

#if UNITY_EDITOR

public class EditorFmod : ICsFmod
{
    private FMOD.Studio.EventInstance _inst;

    public EditorFmod(FMOD.Studio.EventInstance inst)
    {
        _inst = inst;
    }

    public void SetParam(AudioChannel channel, string key, float param)
    {
        if (_inst.isValid())
        {
            _inst.setParameterByName(key, param);
        }
    }

}

#endif