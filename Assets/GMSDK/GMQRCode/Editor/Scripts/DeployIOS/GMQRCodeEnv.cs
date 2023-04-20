using System.IO;

public class GMQRCodeEnv {
	public readonly string PATH_QRCode_EDITOR;
	public readonly string PATH_QRCode_LIBRARYS_IOS;
	public readonly string PATH_QRCode_LIBRARYS_ANDROID;

	static GMQRCodeEnv instance;

	public static GMQRCodeEnv Instance
	{
		get
		{
			if (instance == null) {
				instance = new GMQRCodeEnv();
			}
			return instance;
		}
	}

	public GMQRCodeEnv()
	{
		PATH_QRCode_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMQRCode");
		PATH_QRCode_LIBRARYS_IOS = Path.Combine(PATH_QRCode_EDITOR, @"Librarys/iOS");
		PATH_QRCode_LIBRARYS_ANDROID = Path.Combine(PATH_QRCode_EDITOR, @"Librarys/Android");
	}
}
