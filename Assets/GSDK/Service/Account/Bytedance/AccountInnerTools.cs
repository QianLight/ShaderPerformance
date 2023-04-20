using System.Collections.Generic;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class AccountInnerTools
    {
        public const string Tag = "Account";

        public static Result ConvertAccountError(int error, string message)
        {
            return new Result(error, message);
        }

        public static Result ConvertAccountServerError(int error, string message)
        {
            return new Result(error, message);
        }

        public static AccountType Convert(GMSDK.GMUserType old)
        {
            switch (old)
            {
                case GMSDK.GMUserType.GMUserTypeNone: return AccountType.None;
                case GMSDK.GMUserType.GMUserTypeGuest: return AccountType.Guest;
                case GMSDK.GMUserType.GMUserTypePhone: return AccountType.Phone;
                case GMSDK.GMUserType.GMUserTypeTT: return AccountType.TouTiao;
                case GMSDK.GMUserType.GMUserTypeAwe: return AccountType.Awe;
                case GMSDK.GMUserType.GMUserTypeGoogle: return AccountType.Google;
                case GMSDK.GMUserType.GMUserTypeFacebook: return AccountType.Facebook;
                case GMSDK.GMUserType.GMUserTypeTwitter: return AccountType.Twitter;
                case GMSDK.GMUserType.GMUserTypeLine: return AccountType.Line;
                case GMSDK.GMUserType.GMUserTypeKakaoTalk: return AccountType.KakaoTalk;
                case GMSDK.GMUserType.GMUserTypeVK: return AccountType.VK;
                case GMSDK.GMUserType.GMUsertypeApple: return AccountType.Apple;
                case GMSDK.GMUserType.GMUsertypePassword: return AccountType.Password;
                case GMSDK.GMUserType.GMUserTypeHuoShan: return AccountType.HuoShan;
                case GMSDK.GMUserType.GMUsertypeXiGua: return AccountType.XiGua;
                case GMSDK.GMUserType.GMUsertypeTapTap: return AccountType.TapTap;
                case GMSDK.GMUserType.GMUsertypeEmail: return AccountType.Email;
                case GMSDK.GMUserType.GMUserTypeTikTok: return AccountType.TikTok;
                case GMSDK.GMUserType.GMUserTypeGooglePlay: return AccountType.GooglePlay;
                default:
                    return AccountType.None;
            }

            // return (AccountType)old;
        }

        public static GMSDK.GMUserType Convert(AccountType type)
        {
            switch (type)
            {
                case AccountType.None: return GMSDK.GMUserType.GMUserTypeNone;
                case AccountType.Guest: return GMSDK.GMUserType.GMUserTypeGuest;
                case AccountType.Phone: return GMSDK.GMUserType.GMUserTypePhone;
                case AccountType.TouTiao: return GMSDK.GMUserType.GMUserTypeTT;
                case AccountType.Awe: return GMSDK.GMUserType.GMUserTypeAwe;
                case AccountType.Google: return GMSDK.GMUserType.GMUserTypeGoogle;
                case AccountType.Facebook: return GMSDK.GMUserType.GMUserTypeFacebook;
                case AccountType.Twitter: return GMSDK.GMUserType.GMUserTypeTwitter;
                case AccountType.Line: return GMSDK.GMUserType.GMUserTypeLine;
                case AccountType.KakaoTalk: return GMSDK.GMUserType.GMUserTypeKakaoTalk;
                case AccountType.VK: return GMSDK.GMUserType.GMUserTypeVK;
                case AccountType.Apple: return GMSDK.GMUserType.GMUsertypeApple;
                case AccountType.Password: return GMSDK.GMUserType.GMUsertypePassword;
                case AccountType.HuoShan: return GMSDK.GMUserType.GMUserTypeHuoShan;
                case AccountType.XiGua: return GMSDK.GMUserType.GMUsertypeXiGua;
                case AccountType.TapTap: return GMSDK.GMUserType.GMUsertypeTapTap;
                case AccountType.Email: return GMSDK.GMUserType.GMUsertypeEmail;
                case AccountType.TikTok: return GMSDK.GMUserType.GMUserTypeTikTok;
                case AccountType.GooglePlay: return GMSDK.GMUserType.GMUserTypeGooglePlay;
                default:
                    return GMSDK.GMUserType.GMUserTypeNone;
            }
        }


        public static AccountInfo Convert(GMSDK.LoginWithPoorNetworkResultRet ret)
        {
            AccountInfo accountInfo = new AccountInfo
            {
                AccountType = Convert(ret.userInfo.userType),
                OpenID = ret.sdkOpenId,
                Token = ret.token,
                UserID = ret.userInfo.userId,
                IsGuest = ret.userInfo.isGuest,
                BindedUserInfoCollection = new List<UserDetailInfo>()
            };

            if (ret.userInfo.connectInfos != null)
            {
                foreach (var info in ret.userInfo.connectInfos)
                {
                    UserDetailInfo detail = Convert(info);
                    accountInfo.BindedUserInfoCollection.Add(detail);
                }
            }

            return accountInfo;
        }

        public static AccountInfo Convert(GMSDK.LoginResultFullRet ret)
        {
            AccountInfo accountInfo = new AccountInfo();
            if (ret == null)
            {
                return accountInfo;
            }

            accountInfo.OpenID = ret.sdkOpenId;
            accountInfo.Token = ret.token;
            accountInfo.BindedUserInfoCollection = new List<UserDetailInfo>();

            if (ret.userInfo == null)
            {
                accountInfo.AccountType = AccountType.None;
                accountInfo.IsGuest = false;
                accountInfo.UserID = "";
            }
            else
            {
                accountInfo.AccountType = Convert(ret.userInfo.userType);
                accountInfo.IsGuest = ret.userInfo.isGuest;
                accountInfo.UserID = ret.userInfo.userId;
                if (ret.userInfo.connectInfos != null)
                {
                    foreach (var info in ret.userInfo.connectInfos)
                    {
                        UserDetailInfo detail = Convert(info);
                        accountInfo.BindedUserInfoCollection.Add(detail);
                    }
                }
            }

            return accountInfo;
        }

        // 注意！由于GMSDK.LoginResultRet里没有提供OpenID
        // 所以返回的LoginedInfo的OpenID是空的
        public static AccountInfo Convert(GMSDK.LoginResultRet ret)
        {
            AccountInfo accountInfo = new AccountInfo();
            if (ret == null)
            {
                return accountInfo;
            }

            accountInfo.OpenID = "";
            accountInfo.Token = ret.token;
            accountInfo.BindedUserInfoCollection = new List<UserDetailInfo>();

            if (ret.userInfo == null)
            {
                accountInfo.AccountType = AccountType.None;
                accountInfo.IsGuest = false;
                accountInfo.UserID = "";
                accountInfo.CancelLogoff = false;
            }
            else
            {
                accountInfo.AccountType = Convert(ret.userInfo.userType);
                accountInfo.IsGuest = ret.userInfo.isGuest;
                accountInfo.UserID = ret.userInfo.userId;
                accountInfo.LoginTime = ret.userInfo.loginTime;
                accountInfo.CancelLogoff = ret.userInfo.cancelLogoff;
                if (ret.userInfo.connectInfos != null)
                {
                    foreach (var info in ret.userInfo.connectInfos)
                    {
                        UserDetailInfo detail = Convert(info);
                        accountInfo.BindedUserInfoCollection.Add(detail);
                    }
                }
            }

            return accountInfo;
        }

        // 同上，OpenID是空的
        public static AccountInfo Convert(GMSDK.BindLoginResultRet ret)
        {
            AccountInfo accountInfo = new AccountInfo();
            if (ret == null)
            {
                return accountInfo;
            }

            accountInfo.OpenID = "";
            accountInfo.Token = ret.token;
            accountInfo.BindedUserInfoCollection = new List<UserDetailInfo>();

            if (ret.userInfo == null)
            {
                accountInfo.AccountType = AccountType.None;
                accountInfo.IsGuest = false;
                accountInfo.UserID = "";
            }
            else
            {
                accountInfo.AccountType = Convert(ret.userInfo.userType);
                accountInfo.IsGuest = ret.userInfo.isGuest;
                accountInfo.UserID = ret.userInfo.userId;
                if (ret.userInfo.connectInfos != null)
                {
                    foreach (var info in ret.userInfo.connectInfos)
                    {
                        UserDetailInfo detail = Convert(info);
                        accountInfo.BindedUserInfoCollection.Add(detail);
                    }
                }
            }

            return accountInfo;
        }

        // 没有登录态，所以OpenID和Token都是空的
        public static AccountInfo Convert(GMSDK.GMUserInfo ret)
        {
            AccountInfo accountInfo = new AccountInfo
            {
                AccountType = Convert(ret.userType),
                UserID = ret.userId,
                IsGuest = ret.isGuest,
                LoginTime = ret.loginTime,
                BindedUserInfoCollection = new List<UserDetailInfo>()
            };

            if (ret.connectInfos != null)
            {
                foreach (var info in ret.connectInfos)
                {
                    UserDetailInfo detail = Convert(info);
                    accountInfo.BindedUserInfoCollection.Add(detail);
                }
            }

            return accountInfo;
        }

        public static UserDetailInfo Convert(GMSDK.GMConnectInfo info)
        {
            return new UserDetailInfo
            {
                AccountType = Convert(info.user_type),
                PassportUserID = info.puid,
                NickName = info.nickname,
                AvatarUrl = info.avatar_url,
            };
        }

        internal static void ConvertBindAccountType(
            int type,
            ref AccountType bindAccountType,
            ref BindOrUnbindOperation operation
        )
        {
            bindAccountType = AccountType.None;
            operation = BindOrUnbindOperation.Bind;
            switch (type)
            {
                // 绑定事件
                case 0:
                    bindAccountType = AccountType.Phone;
                    operation = BindOrUnbindOperation.Bind;
                    break;
                case 1:
                    bindAccountType = AccountType.Awe;
                    operation = BindOrUnbindOperation.Bind;
                    break;
                case 3:
                    bindAccountType = AccountType.TouTiao;
                    operation = BindOrUnbindOperation.Bind;
                    break;
                case 5:
                    bindAccountType = AccountType.Apple;
                    operation = BindOrUnbindOperation.Bind;
                    break;
                case 11:
                    bindAccountType = AccountType.HuoShan;
                    operation = BindOrUnbindOperation.Bind;
                    break;
                case 13:
                    bindAccountType = AccountType.XiGua;
                    operation = BindOrUnbindOperation.Bind;
                    break;
                case 17:
                    bindAccountType = AccountType.TapTap;
                    operation = BindOrUnbindOperation.Bind;
                    break;

                // 解绑事件
                case 2:
                    bindAccountType = AccountType.Awe;
                    operation = BindOrUnbindOperation.Unbind;
                    break;
                case 4:
                    bindAccountType = AccountType.TouTiao;
                    operation = BindOrUnbindOperation.Unbind;
                    break;
                case 6:
                    bindAccountType = AccountType.Apple;
                    operation = BindOrUnbindOperation.Unbind;
                    break;
                case 12:
                    bindAccountType = AccountType.HuoShan;
                    operation = BindOrUnbindOperation.Unbind;
                    break;
                case 14:
                    bindAccountType = AccountType.XiGua;
                    operation = BindOrUnbindOperation.Unbind;
                    break;
                case 18:
                    bindAccountType = AccountType.TapTap;
                    operation = BindOrUnbindOperation.Unbind;
                    break;

                default:
                    break;
            }
        }


        public static RealNameAuthLevel Convert(GMSDK.RealNameAuthLevel level)
        {
            switch (level)
            {
                case GMSDK.RealNameAuthLevel.RealNameAuthLevelDefault:
                    return RealNameAuthLevel.Default;
                case GMSDK.RealNameAuthLevel.RealNameAuthLevelLow:
                    return RealNameAuthLevel.Low;
                case GMSDK.RealNameAuthLevel.RealNameAuthLevelMedium:
                    return RealNameAuthLevel.Medium;
                case GMSDK.RealNameAuthLevel.RealNameAuthLevelHigh:
                    return RealNameAuthLevel.High;
                default:
                    return RealNameAuthLevel.Default;
            }
        }

        public static RealNameAuthResult Convert(GMSDK.RealNameAuthRet ret)
        {
            return new RealNameAuthResult()
            {
                AuthLevel = Convert(ret.authLevel),
                Age = ret.age,
                AntiAdditionTips = ret.anti_addiction_tips
            };
        }
        
        private static AccountType GetLinkType(int type)
        {
            switch (type)
            {
                case 4:
                    return AccountType.Awe;
                default:
                    return AccountType.None;
            }
        }

        public static List<LinkInfo> Convert(JsonData jsonData)
        {
            var linkInfos = new List<LinkInfo>();
            if (jsonData.ContainsKey("resultList") && jsonData["resultList"] != null)
            {
                foreach (JsonData linkInfo in jsonData["resultList"])
                {
                    linkInfos.Add(new LinkInfo
                    {
                        Platform = GetLinkType(int.Parse(linkInfo.ContainsKey("platformCode") ? linkInfo["platformCode"].ToString() : "0")),
                        Nickname = linkInfo.ContainsKey("nickname") ? linkInfo["nickname"].ToString() : "",
                        AvatarUrl = linkInfo.ContainsKey("avatarUrl") ? linkInfo["avatarUrl"].ToString() : "",
                        OpenId = linkInfo.ContainsKey("openId") ? linkInfo["openId"].ToString() : "",
                        AccessToken = linkInfo.ContainsKey("accessToken") ? linkInfo["accessToken"].ToString() : "",
                        ScopeList = linkInfo.ContainsKey("scopeList") ? linkInfo["scopeList"].ToString() : ""
                    });
                }
            }
            return linkInfos;
        }
        
        

    }
}