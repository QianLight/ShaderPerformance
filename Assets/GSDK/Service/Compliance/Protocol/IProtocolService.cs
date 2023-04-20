using System;
using GSDK;

namespace GSDK
{
    /// <summary>
    /// 面板关闭的回调
    /// </summary>
#if UNITY_STANDALONE_WIN && !GMEnderOn
    public delegate void PanelClosedDelegate(bool agree);
#else
    public delegate void PanelClosedDelegate();
#endif
    public delegate void ProtocolAddressDelegate(ProtocolAddressResult protocolAddressResult);
	
    #region IProtocolService
    public interface IProtocolService
    {	    
        #region 接口

        /// <summary>
        /// 检测隐私协议是否更新（无UI）
        /// </summary>
        /// <returns>true为协议已更新 / false为协议未更新</returns>
        bool IsProtocolUpdated();
	    
        /// <summary>
        /// 某些游戏，登录场景内有独立的【协议】按钮，点击后需要调起协议条款界面 （有UI）
        /// </summary>
        /// <param name="panelClosedCallback">面板被用户关闭的回调</param>
        void ShowLicense(PanelClosedDelegate panelClosedCallback = null);

        /// <summary>
        /// 通过该接口可以获取到隐私协议和用户协议url
        /// </summary>
        /// <param name="panelClosedCallback">面板被用户关闭的回调</param>
        void SdkProtocolAddress(Action<ProtocolAddressResult> callback);
	    
        #endregion

    }
    #endregion
}