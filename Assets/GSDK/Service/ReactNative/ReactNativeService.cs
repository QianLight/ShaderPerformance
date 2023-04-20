using System;
using System.Collections.Generic;
using GMSDK;
using GSDK.RNU;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GSDK
{
    public class ReactNativeService : IReactNativeService
    {
        private readonly BaseReactNativeSDK _reactNativeSDK;
        public event ReactNativeOnErrorOccuredEventHandler OnErrorOccured;
        public event ReactNativeOnMessageReceivedEventHandler OnMessageReceived;

        public ReactNativeService()
        {
            _reactNativeSDK = GMReactNativeMgr.instance.SDK;
            _reactNativeSDK.listenNativeNotification((result) =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    var message = ReactNativeInnerTools.ConvertMessage(result);
                    if (message.Type == "error_message")
                    {
                        JsonData data = JsonMapper.ToObject(message.Raw);
                        try
                        {
                            if (!data["params"].ContainsKey("scene_type"))
                            {
                                data["params"]["scene_type"] = "0";
                            }

                            if (!data["params"].ContainsKey("ingame_id"))
                            {
                                data["params"]["ingame_id"] = "";
                            }

                            OnErrorOccured.Invoke(
                                new Result(
                                    ReactNativeInnerTools.ConvertNotificationError(
                                        int.Parse((string)data["params"]["error_code"])),
                                    (string)data["params"]["error_msg"]
                                ),
                                ReactNativeInnerTools.ConvertSceneType(
                                    int.Parse((string)data["params"]["scene_type"])),
                                (string)data["params"]["ingame_id"]
                            );
                        }
                        catch (Exception e)
                        {
                            GLog.LogException(e);
                            OnMessageReceived.Invoke(message);
                        }
                    }
                    else
                    {
                        OnMessageReceived.Invoke(message);
                    }
                });
            });
        }

        public void Initialize(string roleID, string roleName, string serverID, ReactNativeInitDelegate callback)
        {
            _reactNativeSDK.updateGameConfig(roleID, roleName, serverID, result =>
            {
                callback.Invoke(ReactNativeInnerTools.ConvertResult(result));
            });
        }

        public void SyncGecko(ReactNativeInitDelegate callback)
        {
            _reactNativeSDK.syncGecko(result =>
            {
                callback.Invoke(ReactNativeInnerTools.ConvertResult(result));
            });
        }
#if UNITY_ANDROID
        public void SetDebugMode(bool isEnabled)
        {
            _reactNativeSDK.setRNDebug(isEnabled);
        }

        public void IsDebugModeEnabled(ReactNativeIsDebugModeEnabledDelegate callback)
        {
            _reactNativeSDK.getRNDebug((result) =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(ReactNativeInnerTools.ConvertResult(result), result.status);
                });
            });
        }
#endif

        public void OpenReferralPage(ReactNativeOpenPageDelegate callback)
        {
            _reactNativeSDK.startBindPage(result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(ReactNativeInnerTools.ConvertOpenReferralPageResult(result),
                        ReactNativeInnerTools.ConvertWindow(result));
                });
            });
        }
        public void FetchPages(ReactNativeScene scene, ReactNativeFetchPagesDelegate callback)
        {
            if (scene.LoadFromCache)
            {
                _reactNativeSDK.getSceneData(scene.Type, result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertResult(result),
                            ReactNativeInnerTools.ConvertPages(result.list));
                    });
                });
            }
            else
            {
                _reactNativeSDK.openFaceVerify(scene.Type, result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertResult(result),
                            ReactNativeInnerTools.ConvertPages(result.list));
                    });
                });
            }
        }

        public void FetchBadges(ReactNativeSceneType type, ReactNativeFetchBadgesDelegate callback)
        {
            // 刷新红点数据。
            _reactNativeSDK.openFaceVerify("", result => { });

            _reactNativeSDK.queryActivityNotifyDataByType(ReactNativeInnerTools.ConvertSceneType(type), result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(ReactNativeInnerTools.ConvertResult(result),
                        ReactNativeInnerTools.ConvertBadgeData(result.data));
                });
            });
        }

        public void FetchConfigValue(string key, ReactNativeFetchConfigValueDelegate callback)
        {
            _reactNativeSDK.QueryConfigValueByKey(key,
                (result) =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertResult(result), result.values);
                    });
                });
        }

        public List<ReactNativeWindow> GetRNWindows()
        {
            List<RNPage> list = _reactNativeSDK.getRNPages();
            List<ReactNativeWindow> windows = new List<ReactNativeWindow>();
            foreach (RNPage item in list)
            {
                windows.Add(ReactNativeInnerTools.ConvertWindow(item));
            }
            return windows;

        }

        public void CloseAllRNWindows()
        {
            _reactNativeSDK.closeAllPages();
        }

        public void showTestPage(ReactNativeOnShowTestPageEventHandler callback)
        {
            _reactNativeSDK.showTestPage(result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(ReactNativeInnerTools.ConvertResult(result));
                });
            });
        }
        
        /*
         * 游戏设置父节点
         */
        public void SetGameGoParent(string parentGoName)
        {
            _reactNativeSDK.SetGameGoParent(parentGoName);
        }

        public void SetGameGoParent(GameObject parentGo)
        {
            _reactNativeSDK.SetGameGoParent(parentGo);
        }
        
        
        public void SetGameAdvancedInjection(IRuGameAdvancedInjection gim)
        {
            _reactNativeSDK.SetGameAdvancedInjection(gim);
        }
        
        public void SetRNUTouchIgnore(GameObject gameObject, bool isIgnore)
        {
            _reactNativeSDK.SetRNUTouchIgnore(gameObject, isIgnore);
        }
        
        /// <summary>
        /// 隐藏/打开 九尾活动面板
        /// </summary>
        public void SetGumihoPanelActive(bool isActive)
        {
            _reactNativeSDK.SetGumihoPanelActive(isActive);
        }

        /*
         * 游戏的字体路径
         */
        public void SetGameFont(string fontName, Font font)        
        {
            _reactNativeSDK.SetGameFont(fontName,font);
        }
        
        /*
         * 提供游戏的用户信息，如角色信息，区服信息等，提供更通用的设置接口
         */
        public void SetGameData(Dictionary<string, object> gameData)       
        {
            _reactNativeSDK.SetGameData(gameData);
        }
        
        
        /*
        * open debugPage
        */
        public void OpenDebugPage(string ip, string port, GameObject parentGo)
        {
            _reactNativeSDK.OpenDebugPage(ip, port, parentGo);
        }
        public void OpenDebugPage(string debugURL, GameObject parentGo)
        {
            _reactNativeSDK.OpenDebugPage(debugURL, parentGo);
        } 

#if UNITY_IOS
        public void setOrientation(ReactNativeOrientationType type, ReactNativeSetOrientationDelegate callback)
        {
            _reactNativeSDK.SetOrientation((int)type, result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(ReactNativeInnerTools.ConvertResult(result));
                });
            });
        }
#endif
        internal void FetchBadgeByID(ulong id, ReactNativeFetchBadgeDelegate callback)
        {
            // 刷新红点数据。
            _reactNativeSDK.openFaceVerify("", result => { });

            _reactNativeSDK.queryActivityNotifyDataById(id.ToString(), result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(ReactNativeInnerTools.ConvertResult(result), ReactNativeInnerTools.ConvertBadge(result.data));
                });
            });
        }

        internal void OpenPage(ReactNativePage page, Dictionary<string, object> parameters,
            ReactNativeOpenPageDelegate callback)
        {
            _reactNativeSDK.openPage(page.URL, page.InGameID, parameters,
                result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertOpenPageResult(result),
                            ReactNativeInnerTools.ConvertWindow(result));
                    });
                });
        }

        internal void OpenPage(ReactNativePage page, Dictionary<string, object> parameters,
            ReactNativeOpenPageDelegate callback, ReactNativePageCloseDelegate closeDelegate)
        {
            _reactNativeSDK.openPage(page.URL, page.InGameID, parameters,
                result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertOpenPageResult(result),
                            ReactNativeInnerTools.ConvertWindow(result));
                    });
                }, closeResult => {
                    InnerTools.SafeInvoke(() =>
                    {
                        closeDelegate.Invoke(ReactNativeInnerTools.ConvertResult(closeResult), ReactNativeInnerTools.ConvertWindow(closeResult));
                    });
                });
        }

        internal void CloseWindow(string id, ReactNativeControlWindowDelegate callback)
        {
            _reactNativeSDK.closePage(id,
                result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertControlWindowResult(result));
                    });
                });
        }

        internal void ShowWindow(string id, ReactNativeControlWindowDelegate callback)
        {
            _reactNativeSDK.showPage(id,
                result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertControlWindowResult(result));
                    });
                });
        }

        internal void HideWindow(string id, ReactNativeControlWindowDelegate callback)
        {
            _reactNativeSDK.hidePage(id,
                result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(ReactNativeInnerTools.ConvertControlWindowResult(result));
                    });
                });
        }

        internal void SendMessage(string id, string @event, string message, ReactNativeSendMessageDelegate callback)
        {
            _reactNativeSDK.sendMessageToPage(id, @event, message,
                result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(new Result(
                            ReactNativeInnerTools.ConvertReactNativeSendMessageError(result.code), result.message));
                    });
                });
        }
    }

    internal class ReactNativeWindowImplementation : ReactNativeWindow
    {
        private string _id;
        private ReactNativeWindowType _type;
        private bool _showing;
        private string _url;
        private string _inGameID;
        private ReactNativeSceneType _sceneType;

        public override string ID
        {
            get { return _id; }
        }

        public override ReactNativeWindowType Type
        {
            get { return _type; }
        }

        public override bool Showing
        {
            get { return _showing; }
        }

        public override string Url
        {
            get { return _url; }
        }

        public override string InGameID
        {
            get { return _inGameID; }
        }

        public override ReactNativeSceneType SceneType
        {
            get { return _sceneType; }
        }

        internal ReactNativeWindowImplementation(string windowId, ReactNativeWindowType type)
        {
            _id = windowId;
            _type = type;
        }

        internal ReactNativeWindowImplementation(string windowId, bool showing, string url, string inGameID, ReactNativeSceneType sceneType)
        {
            _id = windowId;
            _type = ReactNativeWindowType.ReactNative;
            _showing = showing;
            _url = url;
            _inGameID = inGameID;
            _sceneType = sceneType;
        }

        public override void Close(ReactNativeControlWindowDelegate callback)
        {
            if (Type != ReactNativeWindowType.ReactNative)
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(new Result(ErrorCode.ReactNativeInvalidOperation, "not rn page"));
                });
            }
            else
            {
                (ReactNative.Service as ReactNativeService).CloseWindow(_id, callback);
            }
        }

        public override void Show(ReactNativeControlWindowDelegate callback)
        {
            if (Type != ReactNativeWindowType.ReactNative)
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(new Result(ErrorCode.ReactNativeInvalidOperation, "not rn page"));
                });
            }
            else
            {
                (ReactNative.Service as ReactNativeService).ShowWindow(_id, callback);
            }
        }

        public override void Hide(ReactNativeControlWindowDelegate callback)
        {
            if (Type != ReactNativeWindowType.ReactNative)
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(new Result(ErrorCode.ReactNativeInvalidOperation, "not rn page"));
                });
            }
            else
            {
                (ReactNative.Service as ReactNativeService).HideWindow(_id, callback);
            }
        }

        public override void Send(string @event, string message, ReactNativeSendMessageDelegate callback)
        {
            if (Type != ReactNativeWindowType.ReactNative)
            {
                InnerTools.SafeInvoke(() =>
                {
                    callback.Invoke(new Result(ErrorCode.ReactNativeInvalidOperation, "not rn page"));
                });
            }
            else
            {
                if (string.IsNullOrEmpty(@event))
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        callback.Invoke(new Result(ErrorCode.ReactNativeEventIsEmpty, "not rn page"));
                    });
                }
                else
                {
                    (ReactNative.Service as ReactNativeService).SendMessage(_id, @event, message, callback);
                }
            }
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType().IsSubclassOf(typeof(ReactNativeWindow)))
            {
                ReactNativeWindow rhs = (ReactNativeWindow)obj;
                if (rhs.ID == ID)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal class ReactNativePageImplementation : ReactNativePage
    {
        private ulong _id;
        private string _url;
        private string _inGameID;

        public override ulong ID
        {
            get { return _id; }
        }

        public override string URL
        {
            get { return _url; }
        }

        public override string InGameID
        {
            get { return _inGameID; }
        }

        internal ReactNativePageImplementation(ulong id, string url, string inGameID)
        {
            _id = id;
            _url = url;
            _inGameID = inGameID;
        }

        public override void Open(Dictionary<string, object> parameters, ReactNativeOpenPageDelegate callback)
        {
            (ReactNative.Service as ReactNativeService).OpenPage(this, parameters, callback);
        }

        public override void Open(Dictionary<string, object> parameters, ReactNativeOpenPageDelegate callback, ReactNativePageCloseDelegate closeDelegate)
        {
            (ReactNative.Service as ReactNativeService).OpenPage(this, parameters, callback, closeDelegate);
        }

        public override void FetchBadge(ReactNativeFetchBadgeDelegate callback)
        {
            (ReactNative.Service as ReactNativeService).FetchBadgeByID(_id, callback);
        }
    }
}