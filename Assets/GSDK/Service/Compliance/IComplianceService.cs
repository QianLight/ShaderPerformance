namespace GSDK
{

	#region static service class
	public static class Compliance
	{
		public static IComplianceService Service
		{
			get
			{
				return ServiceProvider.Instance.GetService(ServiceType.Compliance) as IComplianceService;
			}
		}
	}
	#endregion
	
	
	#region IComplianceService
	public interface IComplianceService : IService
    {

		#region Methods

		IAntiAddictionService AntiAddiction { get; }

		IPrivacyService Privacy { get; }

		IProtocolService Protocol { get; }

		IInstallPopupService InstallPopup { get; }
#endregion

    }
#endregion
}