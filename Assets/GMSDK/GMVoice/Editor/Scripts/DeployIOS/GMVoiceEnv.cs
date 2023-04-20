using System.IO;


public class GMVoiceEnv
{
	public readonly string PATH_VOICE_EDITOR;
	public readonly string PATH_VOICE_LIBRARYS_IOS;
	public readonly string PATH_VOICE_LIBRARYS_ANDROID;

	static GMVoiceEnv instance;

	public static GMVoiceEnv Instance
    {
        get
        {
            if (instance == null) {
				instance = new GMVoiceEnv();
            }
            return instance;
        }
    }

	public GMVoiceEnv() {
		PATH_VOICE_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMVoice");
		PATH_VOICE_LIBRARYS_IOS = Path.Combine(PATH_VOICE_EDITOR, @"Librarys/iOS");
		PATH_VOICE_LIBRARYS_ANDROID = Path.Combine(PATH_VOICE_EDITOR, @"Librarys/Android");
    }
}
