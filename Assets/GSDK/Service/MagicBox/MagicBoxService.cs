namespace GSDK
{
    public class MagicBoxService : IMagicBoxService
    {
        public void Show()
        {
            GMMagicBoxMgr.instance.SDK.ShowMagicBox();
        }

        public void Hide()
        {
            GMMagicBoxMgr.instance.SDK.HideMagicBox();
        }
    }
}