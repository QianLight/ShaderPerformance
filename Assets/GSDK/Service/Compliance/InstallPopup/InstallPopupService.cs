using System;
using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public static class InstallPopupMethodName
    {
        public const string Show = "show";
        public const string PopupType = "popupType";
        public const string OnClickConfirm = "onClickConfirm";
        public const string OnClickCancel = "onClickCancel";
        public const string OnClickClose = "onClickClose";
    }
    
    public static class InstallPopupEventName
    {
        public const string PopupAvailableEvent = "kOnReceiveCanPopNoti";
    }
    
    internal class InstallPopupService : IInstallPopupService
    {
        public event PopupAvailableEventHandler PopupAvailableEvent;

        public InstallPopupService()
        {
            // 添加事件监听
            BindEvent();
        }

        private void BindEvent()
        {
            BridgeCallBack eventCallBack = new BridgeCallBack();
            eventCallBack.OnSuccess = OnPopupAvailableEvent;
            UNBridge.Listen(InstallPopupEventName.PopupAvailableEvent, eventCallBack);
        }

        protected void OnPopupAvailableEvent(JsonData data)
        {
            if (this.PopupAvailableEvent != null)
            {
                this.PopupAvailableEvent();
            }
        }

        public InstallPopupType AvailablePopupType()
        {
            JsonData param = new JsonData();
            object popupType = UNBridge.CallSync(InstallPopupMethodName.PopupType, param);
            if (popupType != null)
            {
                int value = (int) popupType;
                switch (value)
                {
                    case 1:
                        return InstallPopupType.Guidance;
                    case 2:
                        return InstallPopupType.System;
                    default:
                        return InstallPopupType.None;
                }
            }
            return InstallPopupType.None;
        }

        public void Show()
        {
            JsonData param = new JsonData();
            UNBridge.Call(InstallPopupMethodName.Show, param);
        }

        public void OnConfirmButtonClicked()
        {
            JsonData param = new JsonData();
            UNBridge.Call(InstallPopupMethodName.OnClickConfirm, param);
        }
        
        public void OnCancelButtonClicked()
        {
            JsonData param = new JsonData();
            UNBridge.Call(InstallPopupMethodName.OnClickCancel, param);
        }

        public void OnCloseButtonClicked()
        {
            JsonData param = new JsonData();
            UNBridge.Call(InstallPopupMethodName.OnClickClose, param);
        }
    }
}