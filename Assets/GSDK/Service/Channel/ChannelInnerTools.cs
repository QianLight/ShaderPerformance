using GMSDK;
using GSDK;

#if UNITY_ANDROID
namespace GSDK
{
    public class ChannelInnerTools
    {
        public static CpUploadInfo Convert(GameUploadInfo gameUploadInfo)
        {
            CpUploadInfo cpUploadInfo = new CpUploadInfo();
            if (gameUploadInfo == null)
            {
                return cpUploadInfo;
            }

            cpUploadInfo.balanceids = gameUploadInfo.BalanceIds;
            cpUploadInfo.balancenames = gameUploadInfo.BalanceNames;
            cpUploadInfo.balancenums = gameUploadInfo.BalanceNums;
            cpUploadInfo.roleids = gameUploadInfo.RoleIds;
            cpUploadInfo.intimacys = gameUploadInfo.Intimacys;
            cpUploadInfo.nexusids = gameUploadInfo.NexusIds;
            cpUploadInfo.nexusnames = gameUploadInfo.NexusNames;
            cpUploadInfo.listids = gameUploadInfo.ListIds;
            cpUploadInfo.listnames = gameUploadInfo.ListNames;
            cpUploadInfo.nums = gameUploadInfo.Nums;
            cpUploadInfo.coins = gameUploadInfo.Coins;
            cpUploadInfo.costs = gameUploadInfo.Costs;
            cpUploadInfo.type = gameUploadInfo.Type;
            cpUploadInfo.zoneid = gameUploadInfo.ZoneId;
            cpUploadInfo.zonename = gameUploadInfo.ZoneName;
            cpUploadInfo.roleid = gameUploadInfo.RoleId;
            cpUploadInfo.rolename = gameUploadInfo.RoleName;
            cpUploadInfo.professionid = gameUploadInfo.ProfessionId;
            cpUploadInfo.profession = gameUploadInfo.Profession;
            cpUploadInfo.gender = gameUploadInfo.Gender;
            cpUploadInfo.professionroleid = gameUploadInfo.ProfessionRoleId;
            cpUploadInfo.professionrolename = gameUploadInfo.ProfessionRoleName;
            cpUploadInfo.rolelevel = gameUploadInfo.RoleLevel;
            cpUploadInfo.power = gameUploadInfo.Power;
            cpUploadInfo.vip = gameUploadInfo.Vip;
            cpUploadInfo.balance = gameUploadInfo.Balance;
            cpUploadInfo.partyid = gameUploadInfo.PartyId;
            cpUploadInfo.partyname = gameUploadInfo.PartyName;
            cpUploadInfo.partyroleid = gameUploadInfo.PartyRoleId;
            cpUploadInfo.partyrolename = gameUploadInfo.PartyRoleName;
            cpUploadInfo.friendlist = gameUploadInfo.FriendList;
            cpUploadInfo.ranking = gameUploadInfo.Ranking;
            cpUploadInfo.chapter = gameUploadInfo.Chapter;
            cpUploadInfo.serverId = gameUploadInfo.ServerId;
            cpUploadInfo.serverName = gameUploadInfo.ServerName;

            return cpUploadInfo;
        }

        public static ExitGameInfo Convert(OnExitResult ret)
        {
            ExitGameInfo exitGameInfo = new ExitGameInfo();
            if (ret == null)
            {
                return exitGameInfo;
            }

            exitGameInfo.HasDialog = ret.hasDialog;

            return exitGameInfo;
        }

        public static LoginResult Convert(LoginResultRet ret)
        {
            LoginResult loginResultRet = new LoginResult();
            if (ret == null)
            {
                return loginResultRet;
            }

            loginResultRet.Token = ret.token;
            loginResultRet.AccountInfo = AccountInnerTools.Convert(ret);

            return loginResultRet;
        }

        public static ExitResult Convert(GMSDK.ExitResult ret)
        {
            ExitResult exitResult = new ExitResult();
            if (ret == null)
            {
                return exitResult;
            }

            exitResult.IsExit = ret.isExit;
            return exitResult;
        }

        public static GeneralResult Convert(ChannelGeneralCallbackResult ret)
        {
            var generalResult = new GeneralResult();
            if (ret == null)
            {
                return generalResult;
            }

            generalResult.ExtraData = ret.extraData.ToString();

            return generalResult;
        }
    }
}
#endif
