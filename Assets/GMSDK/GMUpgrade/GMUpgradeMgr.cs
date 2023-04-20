using GMSDK;

public class GMUpgradeMgr : ServiceSingleton<GMUpgradeMgr>
{
    private BaseUpgradeSDK m_sdk;
    public BaseUpgradeSDK SDK { get { return this.m_sdk; } }

    private GMUpgradeMgr()
    {
        if (m_sdk == null)
        {
            m_sdk = new BaseUpgradeSDK();
        }
    }
}
