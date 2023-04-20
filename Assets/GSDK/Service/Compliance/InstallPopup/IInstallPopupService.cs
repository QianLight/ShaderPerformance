using System;
using System.Collections.Generic;

namespace GSDK
{
    /// <summary>
    ///  ATT弹窗
    /// 用法；InstallPopup.Service.XXX();
    /// </summary>
    public static class InstallPopup
    {
        public static IInstallPopupService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.InstallPopup) as IInstallPopupService; }
        }
    }

    /// <summary>
    /// 当GSDK完成弹窗相关初始化和配置拉取后，发送该事件
    /// 注意：监听此事件时机需要早于GSDK初始化完成回调之前，建议在调用GSDK初始化接口之前监听
    /// </summary>
    public delegate void PopupAvailableEventHandler();

    public interface IInstallPopupService : IService
    {
        /// <summary>
        /// 当GSDK完成弹窗相关初始化和配置拉取后，如果允许弹窗，则会触发此事件
        /// 注意：监听此事件时机需要早于GSDK初始化完成回调之前，建议在调用GSDK初始化接口之前监听
        /// </summary>
        event PopupAvailableEventHandler PopupAvailableEvent;

        /// <summary>
        /// 弹出GSDK内置弹窗
        /// 使用场景：仅设置“自定义弹窗时机”时使用
        /// 详细介绍：组合弹出GSDK内置的默认样式/部分样式自定义的引导弹窗和系统弹窗
        ///         其中是弹出“引导弹窗+系统弹窗”还是“系统弹窗”由云端配置决定
        /// </summary>
        void Show();
        
        /// <summary>
        /// 获取允许弹窗的类型
        /// 使用场景；设置“使用开发者完全自定义的引导弹窗样式”时使用
        ///         在接收到PopupAvailableEvent事件后使用，确认当前允许弹出什么类型弹窗
        /// 详细介绍：当云控配置为“引导弹窗+系统弹窗”类型，此时可以正常弹出开发者完全自定义的引导弹窗
        ///         当云控配置为“系统弹窗”或“不允许弹窗”类型，此时不允许弹出开发者完全自定义的引导弹窗
        /// </summary>
        /// <returns>返回当前云端平台配置的允许弹窗类型</returns>
        InstallPopupType AvailablePopupType();

        /// <summary>
        /// 点击确定（完全自定义弹窗时使用）
        /// 当开发者使用完全自定义弹窗样式时，在点击弹窗确定按钮后“必须”调用此方法
        /// </summary>
        void OnConfirmButtonClicked();

        /// <summary>
        /// 点击关闭（完全自定义弹窗时使用）
        /// 当开发者使用完全自定义弹窗样式时，在点击弹窗关闭按钮后“必须”调用此方法
        /// </summary>
        void OnCloseButtonClicked();

        /// <summary>
        /// 点击取消（完全自定义弹窗时使用）
        /// 当开发者使用完全自定义弹窗样式时，在点击弹窗取消按钮后“必须”调用此方法
        /// </summary>
        void OnCancelButtonClicked();
    }

    /// <summary>
    /// 允许弹窗类型枚举
    /// </summary>
    public enum InstallPopupType
    {
        /// <summary>
        /// 不允许引导弹窗和系统弹窗
        /// </summary>
        None,

        /// <summary>
        /// 允许引导弹窗+系统弹窗
        /// </summary>
        Guidance,

        /// <summary>
        /// 只允许系统弹窗
        /// </summary>
        System
    }
}