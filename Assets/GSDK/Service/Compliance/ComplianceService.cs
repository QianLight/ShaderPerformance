namespace GSDK
{
    public class ComplianceService : IComplianceService
    {
        #region Variables


        private readonly IAntiAddictionService _antiAddictionService = new AntiAddictionService();

        private readonly IPrivacyService _privacyService = new PrivacyService();

        private readonly IProtocolService _protocolService = new ProtocolService();

        private readonly IInstallPopupService _installPopupService = new InstallPopupService();

        #endregion


#region Properties

        public IAntiAddictionService AntiAddiction
        {
            get { return _antiAddictionService; }
        }

        public IPrivacyService Privacy
        {
            get { return _privacyService; }
        }

        public IProtocolService Protocol
        {
            get { return _protocolService; }
        }
        public IInstallPopupService InstallPopup
        {
            get { return _installPopupService; }
        }
#endregion
    }
}